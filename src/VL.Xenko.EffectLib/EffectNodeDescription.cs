using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VL.Core;
using VL.Xenko.Rendering;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.ComputeEffect;
using Xenko.Shaders;
using Buffer = Xenko.Graphics.Buffer;

namespace VL.Xenko.EffectLib
{
    class EffectNodeDescription : IVLNodeDescription
    {
        public static readonly PinDescription<IEffect> EffectMainOutput = new PinDescription<IEffect>("Output");

        public static readonly PinDescription<ILowLevelAPIRender> ComputeMainOutput = new PinDescription<ILowLevelAPIRender>("Output");

        public static readonly PinDescription<Int3> ComputeThreadNumbersInput = new PinDescription<Int3>("Thread Numbers", new Int3(1));

        public static readonly PinDescription<Int3> ComputeDispatchThreadGroupCountInput = new PinDescription<Int3>("Thread Group Count", new Int3(1));

        public static readonly PinDescription<bool> ComputeResetCounterInput = new PinDescription<bool>("Reset Counter", true);

        //public static readonly PinDescription<int> ComputeCounterValueInput = new PinDescription<int>("Counter Value");

        public static readonly PinDescription<int> ComputeIterationCountInput = new PinDescription<int>("Iteration Count", 1);

        public static readonly PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext>> ParameterSetterInput = 
            new PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext>>("Parameter Setter");

        public static readonly PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext, int>> ComputeIterationParameterSetterInput = 
            new PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext, int>>("Iteration Parameter Setter");

        EffectPinDescription[] inputs, outputs;
        Effect effect;
        bool? isCompute;

        public EffectNodeDescription(EffectNodeFactory factory, string effectName)
        {
            Factory = factory;
            Name = effectName;
        }

        public EffectNodeFactory Factory { get; }

        public string Name { get; }

        public string Category => "Xenko.EffectLib";

        public EffectPinDescription[] Inputs => inputs ?? (inputs = GetInputsSafe());

        public EffectPinDescription[] Outputs => outputs ?? (outputs = GetOuputs().ToArray());

        public Effect Effect => effect ?? (effect = Factory.EffectSystem.LoadEffect(Name).WaitForResult());

        public bool IsCompute => isCompute.HasValue ? isCompute.Value : (isCompute = GetIsCompute()).Value;

        public IVLNode CreateInstance(NodeContext context)
        {
            if (IsCompute)
                return new ComputeEffectNode(this);
            else
                return new EffectNode(this);
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
            using (var effectInstance = new DynamicEffectInstance(Name))
            {
                try
                {
                    effectInstance.Initialize(Factory.ServiceRegistry);
                    effectInstance.UpdateEffect(Factory.DeviceService.GraphicsDevice);
                    return effectInstance.Effect.Bytecode.Stages.Any(s => s.Stage == ShaderStage.Compute);
                }
                catch (Exception)
                {
                    return false;
                }
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
                dummyInstance.UpdateEffect(Factory.DeviceService.GraphicsDevice);

                var usedNames = new HashSet<string>();
                usedNames.Add(ParameterSetterInput.Name);
                if (IsCompute)
                {
                    usedNames.Add(ComputeThreadNumbersInput.Name);
                    usedNames.Add(ComputeDispatchThreadGroupCountInput.Name);
                    usedNames.Add(ComputeResetCounterInput.Name);
                    usedNames.Add(ComputeIterationCountInput.Name);
                    usedNames.Add(ComputeIterationParameterSetterInput.Name);
                    // Thread numbers and thread group count pins
                    yield return ComputeThreadNumbersInput;
                    yield return ComputeDispatchThreadGroupCountInput;
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

                    yield return new ParameterPinDescription(usedNames, key);
                }

                if (needsWorld)
                    yield return new ParameterPinDescription(usedNames, TransformationKeys.World);

                if (IsCompute)
                {
                    yield return ComputeResetCounterInput;
                    //yield return ComputeCounterValueInput;
                }

                yield return ParameterSetterInput;

                if (IsCompute)
                {
                    yield return ComputeIterationCountInput;
                    yield return ComputeIterationParameterSetterInput;
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
        public readonly bool IsPermutationKey;

        public ParameterPinDescription(HashSet<string> usedNames, ParameterKey key, object defaultValue = null, bool isPermutationKey = false)
        {
            Key = key;
            IsPermutationKey = isPermutationKey;
            DefaultValueBoxed = defaultValue ?? Key.DefaultValueMetadata?.GetDefaultValue();
            Name = Key.GetPinName(usedNames);
        }

        public override string Name { get; }
        public override Type Type => TypeConversions.ShaderToPinTypeMap.ValueOrDefault(Key.PropertyType, Key.PropertyType);
        public override object DefaultValueBoxed { get; }
        public override IVLPin CreatePin(IVLNode node, ParameterCollection parameters) => EffectPins.CreatePin(parameters, Key, IsPermutationKey);
    }
}
