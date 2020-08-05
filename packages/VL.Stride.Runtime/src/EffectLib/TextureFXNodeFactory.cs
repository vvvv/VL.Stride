using Stride.Core.Extensions;
using Stride.Core.IO;
using Stride.Core.Serialization.Contents;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Images;
using Stride.Shaders;
using Stride.Shaders.Compiler;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using VL.Core;
using VL.Core.Diagnostics;
using VL.Stride.Core;
using VL.Stride.Engine;
using VL.Stride.Rendering;

namespace VL.Stride.EffectLib
{
    static class TextureFXNodeFactory
    {
        public static void Register(IVLFactory services)
        {
            services.RegisterNodeFactory("VL.Stride.TextureFXNodeFactory", factory =>
            {
                var nodes = GetNodeDescriptions(factory);
                return nodes.ToImmutableArray();
            });
        }

        static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var serviceRegistry = SharedServices.GetRegistry();
            var graphicsDeviceService = serviceRegistry.GetService<IGraphicsDeviceService>();
            var graphicsDevice = graphicsDeviceService.GraphicsDevice;
            var contentManager = serviceRegistry.GetService<ContentManager>();
            var effectSystem = serviceRegistry.GetService<EffectSystem>();

            // Ensure the effect system tracks the same files as we do
            var fieldInfo = typeof(EffectSystem).GetField("directoryWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            var directoryWatcher = fieldInfo.GetValue(effectSystem) as DirectoryWatcher;
            var modifications = Observable.FromEventPattern<FileEvent>(directoryWatcher, nameof(DirectoryWatcher.Modified))
                .Select(e => e.EventArgs)
                .Where(e => e.ChangeType == FileEventChangeType.Changed || e.ChangeType == FileEventChangeType.Renamed);

            // Effect system deals with its internal cache on update, so make sure its called.
            effectSystem.Update(default);

            var compiler = effectSystem.Compiler;

            const string sdslFileFilter = "*.sdsl";
            const string suffix = "_TextureFX";

            var files = contentManager.FileProvider.ListFiles(EffectCompilerBase.DefaultSourceShaderFolder, sdslFileFilter, VirtualSearchOption.AllDirectories);
            foreach (var file in files)
            {
                var effectName = Path.GetFileNameWithoutExtension(file);
                if (effectName.EndsWith(suffix))
                {
                    var name = effectName.Substring(0, effectName.Length - suffix.Length);
                    var shaderNodeName = $"{name}Shader";

                    IVLNodeDescription shaderNodeDescription;
                    yield return shaderNodeDescription = NewImageEffectShaderNode(shaderNodeName, effectName);
                    yield return NewImageEffectNode(shaderNodeDescription, name);
                }
            }

            string GetPathOfSdslShader(string effectName)
            {
                var fileProvider = contentManager.FileProvider;
                using (var pathStream = fileProvider.OpenStream(EffectCompilerBase.GetStoragePathFromShaderType(effectName) + "/path", VirtualFileMode.Open, VirtualFileAccess.Read))
                using (var reader = new StreamReader(pathStream))
                {
                    return reader.ReadToEnd();
                }
            }

            IVLNodeDescription NewImageEffectShaderNode(string name, string effectName)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.ImageShaders",
                    fragmented: true,
                    init: buildContext =>
                    {
                        var _inputs = new List<IVLPinDescription>();
                        var _outputs = new List<IVLPinDescription>() { buildContext.Pin("Output", typeof(ImageEffectShader)) };
                        var _messages = ImmutableArray<Message>.Empty;

                        using var _effect = new DynamicEffectInstance(effectName);
                        try
                        {
                            _effect.Initialize(serviceRegistry);
                            _effect.UpdateEffect(graphicsDevice);
                        }
                        catch (InvalidOperationException)
                        {
                            try
                            {
                                // Compile manually to get detailed errors
                                var compilerResult = compiler.Compile(
                                    shaderSource: new ShaderClassSource(effectName),
                                    compilerParameters: new CompilerParameters() { EffectParameters = _effect.EffectCompilerParameters });
                                _messages = compilerResult.Messages.Select(m => m.ToMessage()).ToImmutableArray();
                            }
                            catch (Exception e)
                            {
                                _messages = _messages.Add(new Message(MessageType.Error, $"Shader compiler crashed: {e}"));
                            }
                        }

                        var byteCode = _effect.Effect?.Bytecode;
                        if (byteCode != null)
                        {
                            var layoutNames = byteCode.Reflection.ResourceBindings.Select(x => x.ResourceGroup ?? "Globals").Distinct().ToList();

                            var usedNames = new HashSet<string>()
                            {
                                "Output Texture",
                                "Enabled"
                            };

                            var _textureCount = 0;
                            var parameters = _effect.Parameters;
                            foreach (var parameter in parameters.Layout.LayoutParameterKeyInfos.OrderBy(p => p.Key.Name.Contains("Texture") ? 0 : 1))
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
                                if (WellKnownParameters.PerFrameMap.ContainsKey(name) || WellKnownParameters.PerViewMap.ContainsKey(name))
                                    continue;

                                // Skip unbound parameters
                                if (parameter.BindingSlot < 0 && !name.StartsWith(effectName))
                                    continue;

                                if (key.PropertyType == typeof(Texture))
                                {
                                    var pinName = ++_textureCount == 1 ? "Texture" : $"Texture {_textureCount}";
                                    usedNames.Add(pinName);
                                    _inputs.Add(new PinDescription<Texture>(pinName));
                                }
                                else
                                    _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count));
                            }
                        }

                        IVLPinDescription _outputTextureInput, _enabledInput;
                        _inputs.Add(_outputTextureInput = new PinDescription<Texture>("Output Texture"));
                        _inputs.Add(_enabledInput = new PinDescription<bool>("Enabled", defaultValue: true));

                        return buildContext.Implementation(
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            newNode: nodeBuildContext =>
                            {
                                var effect = new TextureFXEffect(effectName);
                                var inputs = new List<IVLPin>();
                                var enabledInput = default(IVLPin);
                                var textureCount = 0;
                                foreach (var _input in _inputs)
                                {
                                    // Handle the predefined pins first
                                    if (_input == _outputTextureInput)
                                    {
                                        inputs.Add(nodeBuildContext.Input<Texture>(setter: t =>
                                        {
                                            if (t != null)
                                                effect.SetOutput(t);
                                        }));
                                    }
                                    else if (_input == _enabledInput)
                                        inputs.Add(enabledInput = nodeBuildContext.Input<bool>(v => effect.Enabled = v));
                                    else if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(effect.Parameters));
                                    else if (_input is PinDescription<Texture> textureInput)
                                    {
                                        var slot = textureCount++;
                                        inputs.Add(nodeBuildContext.Input<Texture>(setter: t =>
                                        {
                                            effect.SetInput(slot, t);
                                        }));
                                    }
                                }

                                var effectOutput = ToOutput(nodeBuildContext, effect, () =>
                                {
                                    //effect.Enabled = enabledInput.Value && effect.IsInputAssigned && effect.IsOutputAssigned;
                                });
                                return nodeBuildContext.Node(
                                    inputs: inputs,
                                    outputs: new[] { effectOutput },
                                    update: default,
                                    dispose: () => effect.Dispose());
                            },
                            invalidated: modifications.Where(e => Path.GetFileNameWithoutExtension(e.Name) == effectName),
                            openEditor: () =>
                            {
                                var path = GetPathOfSdslShader(effectName);
                                try
                                {
                                    Process.Start(path);
                                    return true;
                                }
                                catch
                                {
                                    return false;
                                }
                            }
                        );
                    });
            }

            IVLNodeDescription NewImageEffectNode(IVLNodeDescription shaderDescription, string name)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.TextureFX",
                    fragmented: true,
                    init: buildContext =>
                    {
                        return buildContext.Implementation(
                            inputs: shaderDescription.Inputs,
                            outputs: new[] { buildContext.Pin("Output", typeof(Texture)) },
                            messages: shaderDescription.Messages,
                            invalidated: shaderDescription.Invalidated,
                            newNode: nodeBuildContext =>
                            {
                                var nodeContext = nodeBuildContext.NodeContext;
                                var node = shaderDescription.CreateInstance(nodeContext);
                                var textureInput = node.Inputs.ElementAtOrDefault(shaderDescription.Inputs.IndexOf(p => p.Name == "Texture"));
                                var outputTextureInput = node.Inputs.ElementAtOrDefault(shaderDescription.Inputs.IndexOf(p => p.Name == "Output Texture"));
                                var gameHandle = nodeContext.GetGameHandle();
                                var game = gameHandle.Resource;
                                var graphicsDevice = game.GraphicsDevice;
                                var current = default((TextureDescription inputDesc, Texture outputTexture));
                                var mainOutput = nodeBuildContext.Output<Texture>(getter: () =>
                                {
                                    var inputTexture = textureInput?.Value as Texture;
                                    var outputTexture = outputTextureInput.Value as Texture;
                                    if (!(inputTexture is null) && outputTexture is null)
                                    {
                                        if (inputTexture.Description != current.inputDesc)
                                        {
                                            current.outputTexture?.Dispose();
                                            current.inputDesc = inputTexture.Description;
                                            current.outputTexture = Texture.New(graphicsDevice, TextureDescription.FromDescription(current.inputDesc, TextureFlags.RenderTarget | TextureFlags.ShaderResource, GraphicsResourceUsage.Default));
                                        }
                                        outputTexture = current.outputTexture;
                                    }

                                    // Set the final output texture
                                    outputTextureInput.Value = outputTexture;

                                    var effect = node.Outputs[0].Value as TextureFXEffect;
                                    var scheduler = game.Services.GetService<SchedulerSystem>();
                                    if (scheduler != null && effect != null && effect.IsInputAssigned && effect.IsOutputAssigned)
                                    {
                                        scheduler.Schedule(effect);
                                    }

                                    return outputTexture;
                                });
                                return nodeBuildContext.Node(
                                    inputs: node.Inputs,
                                    outputs: new[] { mainOutput },
                                    dispose: () =>
                                    {
                                        current.outputTexture?.Dispose();
                                        gameHandle.Dispose();
                                        node.Dispose();
                                    });
                            },
                            openEditor: () => shaderDescription.OpenEditor()
                        );
                    });
            }
        }

        static IVLPin ToOutput<T>(NodeBuilding.NodeInstanceBuildContext c, T value, Action getter)
        {
            return c.Output(() =>
            {
                getter();
                return value;
            });
        }
    }
}
