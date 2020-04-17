using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders.ShaderFX.Control
{
    public class Do<T> : ComputeValue<T>
    {
        public Do(IComputeVoid before, IComputeValue<T> value)
        {
            Before = before;
            Value = value;
        }

        public IComputeVoid Before { get; }
        public IComputeValue<T> Value { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderClassSource = GetShaderSourceForType<T>("Do");

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);
            mixin.AddComposition(Before, "Before", context, baseKeys);

            return mixin;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Before, Value);
        }
    }
}
