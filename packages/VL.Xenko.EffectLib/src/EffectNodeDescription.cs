using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using VL.Core;
using VL.Core.Diagnostics;
using VL.Xenko.Rendering;
using VL.Xenko.Shaders;
using Xenko.Core.Diagnostics;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.ComputeEffect;
using Xenko.Shaders;
using Xenko.Shaders.Compiler;
using Buffer = Xenko.Graphics.Buffer;

namespace VL.Xenko.EffectLib
{
    class EffectNodeDescription : IVLNodeDescription
    {
        public static readonly PinDescription<IEffect> EffectMainOutput = new PinDescription<IEffect>("Output");

        public static readonly PinDescription<ILowLevelAPIRender> ComputeMainOutput = new PinDescription<ILowLevelAPIRender>("Output");

        public static readonly PinDescription<Int3> ComputeDispatchCountInput = new PinDescription<Int3>("Dispatch Count", new Int3(1));

        public static readonly PinDescription<Int3> ComputeThreadNumbersInput = new PinDescription<Int3>("Thread Numbers", new Int3(1));

        public static readonly PinDescription<int> ComputeIterationCountInput = new PinDescription<int>("Iteration Count", 1);

        public static readonly PinDescription<string> ProfilerNameInput = new PinDescription<string>("Profiler Name");

        public static readonly PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext>> ParameterSetterInput = 
            new PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext>>("Parameter Setter");

        public static readonly PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext, int>> ComputeIterationParameterSetterInput = 
            new PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext, int>>("Iteration Parameter Setter");

        public static readonly PinDescription<bool> ComputeEnabledInput = new PinDescription<bool>("Enabled", true);

        EffectPinDescription[] inputs, outputs;
        bool? isCompute;
        CompilerResults compilerResults;

        public EffectNodeDescription(EffectNodeFactory factory, string effectName)
        {
            Factory = factory;
            Name = effectName;
        }

        // Used when effect has errors - we keep the signature from the previous one but show the compiler errors
        // Because we have errors VL will dispose all previous instances of ours and not create any new ones but the type information of our pins we keep on
        // to so other parts of patch won't break due to loss of typing information
        public EffectNodeDescription(EffectNodeDescription previous, CompilerResults compilerResults)
        {
            Factory = previous.Factory;
            Name = previous.Name;
            inputs = previous.Inputs;
            outputs = previous.Outputs;
            isCompute = previous.IsCompute;
            this.compilerResults = compilerResults;
        }

        public EffectNodeFactory Factory { get; }

        public string Name { get; }

        public string Category => "Xenko.EffectLib";

        public EffectPinDescription[] Inputs => inputs ?? (inputs = GetInputsSafe());

        public EffectPinDescription[] Outputs => outputs ?? (outputs = GetOuputs().ToArray());

        public bool IsCompute
        {
            get => isCompute.HasValue ? isCompute.Value : (isCompute = GetIsCompute()).Value;
        }

        public bool IsInUse => compilerResults != null;

        public bool HasCompilerErrors => CompilerResults.HasErrors || CompilerResults.Bytecode.WaitForResult().CompilationLog.HasErrors;

        public CompilerResults CompilerResults => compilerResults ?? (compilerResults = Factory.GetCompilerResults(Name));

        public EffectBytecode Bytecode => CompilerResults.Bytecode.WaitForResult().Bytecode;

        public int Version { get; internal set; }

        public IEnumerable<Message> Messages
        {
            get
            {
                foreach (var m in CompilerResults.Messages)
                    yield return new Message(m.Type.ToMessageType(), m.Text);
                var bytecodeCompilerResults = CompilerResults.Bytecode.WaitForResult();
                foreach (var m in bytecodeCompilerResults.CompilationLog.Messages)
                    yield return new Message(m.Type.ToMessageType(), m.Text);
            }
        }

        public IVLNode CreateInstance(NodeContext context)
        {
            if (IsCompute)
                return new ComputeEffectNode(context, this);
            else
                return new EffectNode(context, this);
        }

        public IVLPin[] CreateNodeInputs(IVLNode node, ParameterCollection parameters) => Inputs.Select(p => p.CreatePin(node, parameters)).ToArray();

        public IVLPin[] CreateNodeOutputs(IVLNode node, ParameterCollection parameters)
        {
            var result = new IVLPin[Outputs.Length];
            for (int i = 0; i < Outputs.Length; i++)
            {
                result[i] = Outputs[i].CreatePin(node, parameters);
                if (i == 0)
                    result[i].Value = node; // Instance output
            }
            return result;
        }

        IVLNodeDescriptionFactory IVLNodeDescription.Factory => Factory;
        IReadOnlyList<IVLPinDescription> IVLNodeDescription.Inputs => Inputs;
        IReadOnlyList<IVLPinDescription> IVLNodeDescription.Outputs => Outputs;

        bool GetIsCompute()
        {
            try
            {
                if (CompilerResults.HasErrors)
                    return false;
                var bytecodeCompilerResults = CompilerResults.Bytecode.WaitForResult();
                if (bytecodeCompilerResults.CompilationLog.HasErrors)
                    return false;
                var bytecode = bytecodeCompilerResults.Bytecode;
                return bytecode.Stages.Any(s => s.Stage == ShaderStage.Compute);
            }
            catch (Exception)
            {
                return false;
            }
        }

        EffectPinDescription[] GetInputsSafe()
        {
            try
            {
                return GetInputs().ToArray();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                return Array.Empty<EffectPinDescription>();
            }
        }

        IEnumerable<EffectPinDescription> GetInputs()
        {
            var effectName = IsCompute ? "ComputeEffectShader" : Name;
            using (var dummyInstance = new DynamicEffectInstance(effectName))
            {
                var parameters = dummyInstance.Parameters;
                if (IsCompute)
                {
                    parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, Name);
                    parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, new Int3(1));
                }
                dummyInstance.Initialize(Factory.ServiceRegistry);
                dummyInstance.UpdateEffect(Factory.DeviceManager.GraphicsDevice);

                var usedNames = new HashSet<string>();
                usedNames.Add(ParameterSetterInput.Name);
                if (IsCompute)
                {
                    usedNames.Add(ComputeDispatchCountInput.Name);
                    usedNames.Add(ComputeThreadNumbersInput.Name);
                    usedNames.Add(ComputeIterationCountInput.Name);
                    usedNames.Add(ComputeIterationParameterSetterInput.Name);
                    usedNames.Add(ProfilerNameInput.Name);
                    usedNames.Add(ComputeEnabledInput.Name);
                    // Thread numbers and thread group count pins
                    yield return ComputeThreadNumbersInput;
                    yield return ComputeDispatchCountInput;
                }

                // Permutation parameters
                foreach (var parameter in parameters.ParameterKeyInfos)
                {
                    var key = parameter.Key;
                    if (key == ComputeEffectShaderKeys.ComputeShaderName)
                        continue;
                    if (key == ComputeEffectShaderKeys.ThreadNumbers)
                        continue;
                    yield return new ParameterPinDescription(usedNames, key, isPermutationKey: true);
                }

                // Resource and value parameters
                var byteCode = dummyInstance.Effect.Bytecode;
                var layoutNames = byteCode.Reflection.ResourceBindings.Select(x => x.ResourceGroup ?? "Globals").Distinct().ToList();
                var needsWorld = false;
                foreach (var parameter in parameters.Layout.LayoutParameterKeyInfos)
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

                    if (WellKnownParameters.PerDrawMap.ContainsKey(name))
                    {
                        // Expose World only - all other world dependent parameters we can compute on our own
                        needsWorld = true;
                        continue;
                    }

                    if (key == ComputeShaderBaseKeys.ThreadGroupCountGlobal)
                        continue; // Already handled

                    yield return new ParameterPinDescription(usedNames, key, parameter.Count);
                }

                if (needsWorld)
                    yield return new ParameterPinDescription(usedNames, TransformationKeys.World);

                yield return ParameterSetterInput;

                if (IsCompute)
                {
                    yield return ComputeIterationCountInput;
                    yield return ComputeIterationParameterSetterInput;
                    yield return ProfilerNameInput;
                    yield return ComputeEnabledInput;
                }
            }
        }

        IEnumerable<EffectPinDescription> GetOuputs()
        {
            if (IsCompute)
            {
                yield return ComputeMainOutput;
                foreach (var input in Inputs)
                    if (input.Type == typeof(Buffer))
                        yield return input;
            }
            else
            {
                yield return EffectMainOutput;
            }
        }

        public bool OpenEditor()
        {
            var path = Factory.GetPathOfXkslShader(Name);
            Process.Start(path);

            var bytecodeCompilerResults = CompilerResults.Bytecode.WaitForResult();
            foreach (var m in bytecodeCompilerResults.CompilationLog.Messages)
            {
                if (EffectNodeFactory.TryGetFilePath(m, out string p) && p != path && File.Exists(p))
                    Process.Start(p);
                break;
            }

            return true;
        }
    }

    abstract class EffectPinDescription : IVLPinDescription
    {
        public abstract string Name { get; }
        public abstract Type Type { get; }
        public abstract object DefaultValueBoxed { get; }

        object IVLPinDescription.DefaultValue => DefaultValueBoxed;

        public abstract IVLPin CreatePin(IVLNode node, ParameterCollection parameters);
    }

    class PinDescription<T> : EffectPinDescription
    {
        public PinDescription(string name, T defaultValue = default(T))
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public override string Name { get; }
        public override Type Type => typeof(T);
        public override object DefaultValueBoxed => DefaultValue;
        public T DefaultValue { get; }

        public override IVLPin CreatePin(IVLNode node, ParameterCollection parameters) => new Pin<T>(Name, DefaultValue);
    }

    class ParameterPinDescription : EffectPinDescription
    {
        public readonly ParameterKey Key;
        public readonly int Count;
        public readonly bool IsPermutationKey;

        public ParameterPinDescription(HashSet<string> usedNames, ParameterKey key, int count = 1, object defaultValue = null, bool isPermutationKey = false)
        {
            Key = key;
            IsPermutationKey = isPermutationKey;
            Count = count;
            Name = key.GetPinName(usedNames);
            var elementType = TypeConversions.ShaderToPinTypeMap.ValueOrDefault(key.PropertyType, key.PropertyType);
            defaultValue = defaultValue ?? key.DefaultValueMetadata?.GetDefaultValue();
            // TODO: This should be fixed in Xenko
            if (key.PropertyType == typeof(Matrix))
                defaultValue = Matrix.Identity;
            if (elementType != key.PropertyType)
                defaultValue = TypeConversions.ConvertShaderToPin(defaultValue, elementType);
            if (count > 1)
            {
                Type = elementType.MakeArrayType();
                var arr = Array.CreateInstance(elementType, count);
                for (int i = 0; i < arr.Length; i++)
                    arr.SetValue(defaultValue, i);
                DefaultValueBoxed = arr;
            }
            else
            {
                Type = elementType;
                DefaultValueBoxed = defaultValue;
            }
        }

        public override string Name { get; }
        public override Type Type { get; }
        public override object DefaultValueBoxed { get; }
        public override IVLPin CreatePin(IVLNode node, ParameterCollection parameters) => EffectPins.CreatePin(parameters, Key, Count, IsPermutationKey);
    }
}
