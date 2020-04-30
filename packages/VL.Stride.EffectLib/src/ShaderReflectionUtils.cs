using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect;

namespace VL.Stride.EffectLib
{
    public static class ShaderReflectionUtils
    {
        public static IEnumerable<EffectPinDescription> GetInputs(DynamicEffectInstance effectInstance, string effectName, IServiceRegistry serviceRegistry, GraphicsDevice graphicsDevice, bool isCompute = false)
        {
            effectName = isCompute ? "ComputeEffectShader" : effectName;
            var dummyInstance = effectInstance ?? new DynamicEffectInstance(effectName);
            try
            {
                var parameters = dummyInstance.Parameters;
                if (isCompute)
                {
                    parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, effectName);
                    parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, new Int3(1));
                }

                if (effectInstance == null)
                {
                    dummyInstance.Initialize(serviceRegistry);
                    dummyInstance.UpdateEffect(graphicsDevice); 
                }

                var usedNames = new HashSet<string>();
                if (isCompute)
                {
                    usedNames.Add(EffectNodeDescription.ComputeDispatchCountInput.Name);
                    usedNames.Add(EffectNodeDescription.ComputeThreadNumbersInput.Name);
                    usedNames.Add(EffectNodeDescription.ComputeIterationCountInput.Name);
                    usedNames.Add(EffectNodeDescription.ComputeIterationParameterSetterInput.Name);
                    usedNames.Add(EffectNodeDescription.ProfilerNameInput.Name);
                    usedNames.Add(EffectNodeDescription.ComputeEnabledInput.Name);
                    // Thread numbers and thread group count pins
                    yield return EffectNodeDescription.ComputeThreadNumbersInput;
                    yield return EffectNodeDescription.ComputeDispatchCountInput;
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

                if (isCompute)
                {
                    yield return EffectNodeDescription.ComputeIterationCountInput;
                    yield return EffectNodeDescription.ComputeIterationParameterSetterInput;
                    yield return EffectNodeDescription.ProfilerNameInput;
                    yield return EffectNodeDescription.ComputeEnabledInput;
                }
            }
            finally
            {
                if (effectInstance == null)
                    dummyInstance?.Dispose();
            }
        }
    }
}
