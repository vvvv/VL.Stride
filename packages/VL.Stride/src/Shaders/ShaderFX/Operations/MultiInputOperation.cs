using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using DataMemberAttribute = Xenko.Core.DataMemberAttribute;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;
using System.Linq;

namespace VL.Xenko.Shaders.ShaderFX
{

    public class MultiInputOperation<TOut> : ComputeValue<TOut>
    {
        public MultiInputOperation(string operatorName, IEnumerable<ShaderInput> inputs, bool nameOnlyDependsOnOutput, params object[] genericArguments)
        {
            ShaderName = operatorName;
            Inputs = inputs.ToList();
            GenericArguments = genericArguments;
            NameOnlyDependsOnOutput = nameOnlyDependsOnOutput;
        }

        List<ShaderInput> Inputs { get; }
        public object[] GenericArguments { get; }

        public bool NameOnlyDependsOnOutput { get; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Inputs.Select(i => i.Shader));
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = NameOnlyDependsOnOutput ? 
                GetShaderSourceForType<TOut>(ShaderName, GenericArguments) : 
                GetShaderSourceForInputs<TOut>(ShaderName, Inputs.Select(i => i.Shader), GenericArguments);

            var mixin = shaderSource.CreateMixin();

            foreach (var cs in Inputs)
            {
                mixin.AddComposition(cs.Shader, cs.CompositionName, context, baseKeys);
            }

            return mixin;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", ShaderName, typeof(TOut));
        }
    }
}
