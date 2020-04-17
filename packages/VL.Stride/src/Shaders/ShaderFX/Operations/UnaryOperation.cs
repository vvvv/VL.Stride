using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using DataMemberAttribute = Xenko.Core.DataMemberAttribute;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders.ShaderFX
{
    public class UnaryOperation<T> : ComputeValue<T>
    {
        public UnaryOperation(string operatorName, IComputeValue<T> value)
        {
            ShaderName = operatorName;
            Value = value;
        }

        public IComputeValue<T> Value { get; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType<T>(ShaderName);

            var mixin = shaderSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);

            return mixin;
        }

        public override string ToString()
        {
            return string.Format("Op {0} {1}", Value, ShaderName);
        }
    }
}
