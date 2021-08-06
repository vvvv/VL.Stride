﻿using Stride.Core;
using Stride.Core.Extensions;
using Stride.Core.IO;
using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect;
using Stride.Rendering.Images;
using Stride.Rendering.Materials;
using Stride.Shaders;
using Stride.Shaders.Compiler;
using Stride.Shaders.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using VL.Core;
using VL.Core.Diagnostics;
using VL.Model;
using VL.Stride.Core;
using VL.Stride.Graphics;
using VL.Stride.Effects;
using VL.Stride.Engine;
using VL.Stride.Rendering.ComputeEffect;
using VL.Stride.Shaders;
using VL.Stride.Shaders.ShaderFX;

namespace VL.Stride.Rendering
{
    static partial class EffectShaderNodes
    {
        static bool IsShaderFile(string file) => string.Equals(Path.GetExtension(file), ".sdsl", StringComparison.OrdinalIgnoreCase) || string.Equals(Path.GetExtension(file), ".sdfx", StringComparison.OrdinalIgnoreCase);

        static NameAndVersion GetNodeName(string effectName, string suffix)
        {
            // Levels_ClampBoth_TextureFX
            var name = effectName.Substring(0, effectName.Length - suffix.Length);
            // Levels_ClampBoth
            var nameParts = name.Split('_');
            if (nameParts.Length > 0)
            {
                name = nameParts[0];
                return new NameAndVersion(name, string.Join(" ", nameParts.Skip(1)));
            }
            return new NameAndVersion(name);
        }

        static bool IsNewOrDeletedShaderFile(FileSystemEventArgs e)
        {
            // Check for shader files only. Editor (like VS) create lot's of other temporary files.
            if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Deleted)
                return IsShaderFile(e.Name);
            // Also the old name must be a shader file. We're not interested in weired renamings by VS.
            if (e.ChangeType == WatcherChangeTypes.Renamed && e is RenamedEventArgs r)
                return IsShaderFile(e.Name) && IsShaderFile(r.OldName);
            return false;
        }

        static (DynamicEffectInstance effect, ImmutableArray<Message> messages) CreateEffectInstance(string effectName, ShaderMetadata shaderMetadata, IServiceRegistry serviceRegistry, GraphicsDevice graphicsDevice, ParameterCollection parameters = null, string baseShaderName = null)
        {
            var messages = ImmutableArray<Message>.Empty;
            if (baseShaderName is null)
                baseShaderName = effectName;

            var effect = new DynamicEffectInstance(effectName, parameters);
            if (parameters is null)
                parameters = effect.Parameters;

            try
            {
                effect.Initialize(serviceRegistry);
                effect.UpdateEffect(graphicsDevice);
            }
            catch (InvalidOperationException)
            {
                try
                {
                    // Compile manually to get detailed errors
                    var compilerParameters = new CompilerParameters() { EffectParameters = effect.EffectCompilerParameters };
                    foreach (var effectParameterKey in parameters.ParameterKeyInfos)
                        if (effectParameterKey.Key.Type == ParameterKeyType.Permutation)
                            compilerParameters.SetObject(effectParameterKey.Key, parameters.ObjectValues[effectParameterKey.BindingSlot]);
                    var compilerResult = serviceRegistry.GetService<EffectSystem>().Compiler.Compile(
                        shaderSource: GetShaderSource(effectName),
                        compilerParameters: compilerParameters);
                    messages = compilerResult.Messages.Select(m => m.ToMessage()).ToImmutableArray();
                }
                catch (Exception e)
                {
                    messages = messages.Add(new Message(MessageType.Error, $"Shader compiler crashed: {e}"));
                }
            }

            return (effect, messages);
        }

        static IEnumerable<ParameterKeyInfo> GetParameters(DynamicEffectInstance effectInstance)
        {
            var byteCode = effectInstance.Effect?.Bytecode;
            if (byteCode is null)
                yield break;

            var layoutNames = byteCode.Reflection.ResourceBindings.Select(x => x.ResourceGroup ?? "Globals").Distinct().ToList();
            var parameters = effectInstance.Parameters;
            var compositionParameters = parameters.ParameterKeyInfos.Where(pki => pki.Key.PropertyType == typeof(ShaderSource) && pki.Key.Name != "EffectNodeBase.EffectNodeBaseShader");
            foreach (var parameter in parameters.Layout.LayoutParameterKeyInfos.Concat(compositionParameters))
            {
                var key = parameter.Key;
                var name = key.Name;

                // Skip constant buffers
                if (layoutNames.Contains(name))
                    continue;

                // Skip compiler injected paddings
                if (name.Contains("_padding_"))
                    continue;

                // Skip well known parameters
                if (WellKnownParameters.PerFrameMap.ContainsKey(name)
                    || WellKnownParameters.PerViewMap.ContainsKey(name)
                    || WellKnownParameters.TexturingMap.ContainsKey(name))
                    continue;

                // Skip inputs from ShaderFX graph
                if (name.StartsWith("ShaderFX.Input"))
                    continue;

                yield return parameter;
            }
        }

        private static ParameterCollection BuildBaseMixin(string shaderName, ShaderMetadata shaderMetadata, GraphicsDevice graphicsDevice, out ShaderMixinSource effectInstanceMixin, ParameterCollection parameters = null)
        {
            effectInstanceMixin = new ShaderMixinSource();
            effectInstanceMixin.Mixins.Add(new ShaderClassSource(shaderName));

            var mixinParams = parameters ?? new ParameterCollection();
            mixinParams.Set(EffectNodeBaseKeys.EffectNodeBaseShader, effectInstanceMixin);

            var context = new ShaderGeneratorContext(graphicsDevice)
            {
                Parameters = mixinParams,
            };

            //add composition parameters to parameters
            if (shaderMetadata.ParsedShader != null)
            {
                foreach (var compKey in shaderMetadata.ParsedShader.CompositionsWithBaseShaders)
                {
                    var comp = compKey.Value;
                    var shaderSource = comp.GetDefaultShaderSource(context, baseKeys);
                    effectInstanceMixin.AddComposition(comp.Name, shaderSource);
                    mixinParams.Set(comp.Key, shaderSource);
                } 
            }

            return mixinParams;
        }

        //used for shader source generation
        static MaterialComputeColorKeys baseKeys = new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White);

        // check composition pins
        private static bool UpdateCompositions(IReadOnlyList<ShaderFXPin> compositionPins, GraphicsDevice graphicsDevice, ParameterCollection parameters, ShaderMixinSource mixin, CompositeDisposable subscriptions)
        {
            var anyChanged = false;
            for (int i = 0; i < compositionPins.Count; i++)
            {
                anyChanged |= compositionPins[i].ShaderSourceChanged;
            }

            if (anyChanged)
            {
                // Disposes all current subscriptions. So for example all data bindings between the sources and our parameter collection
                // gets removed.
                subscriptions.Clear();

                var context = ShaderGraph.NewShaderGeneratorContext(graphicsDevice, parameters, subscriptions);

                var updatedMixin = new ShaderMixinSource();
                updatedMixin.DeepCloneFrom(mixin);
                for (int i = 0; i < compositionPins.Count; i++)
                {
                    var cp = compositionPins[i];
                    cp.GenerateAndSetShaderSource(updatedMixin, context, baseKeys);
                }
                parameters.Set(EffectNodeBaseKeys.EffectNodeBaseShader, updatedMixin);

                return true;
            }

            return false;
        }

        static IVLPin ToOutput<T>(NodeBuilding.NodeInstanceBuildContext c, T value, Action getter)
        {
            return c.Output(() =>
            {
                getter();
                return value;
            });
        }

        static ShaderSource GetShaderSource(string effectName)
        {
            var isMixin = ShaderMixinManager.Contains(effectName);
            if (isMixin)
                return new ShaderMixinGeneratorSource(effectName);
            return new ShaderClassSource(effectName);
        }

        // Not used yet
        static Dictionary<string, Dictionary<string, ParameterKey>> GetCompilerParameters(string filePath, string sdfxEffectName)
        {
            // In .sdfx, shader has been renamed to effect, in order to avoid ambiguities with HLSL and .sdsl
            var macros = new[]
            {
                    new global::Stride.Core.Shaders.Parser.ShaderMacro("shader", "effect")
                };

            // Parse and collect
            var shader = StrideShaderParser.PreProcessAndParse(filePath, macros);
            var builder = new RuntimeShaderMixinBuilder(shader);
            return builder.CollectParameters();
        }

    }
}