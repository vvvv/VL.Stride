using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class GetSwizzle<TIn, TOut> : ComputeValue<TOut>
    {
        public GetSwizzle(IComputeValue<TIn> value, string swizzle)
        {
            Value = value;
            Swizzle = swizzle;
        }

        public IComputeValue<TIn> Value { get; }
        public string Swizzle { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderClassSource = GetShaderSourceForType2<TIn, TOut>("GetSwizzle", Swizzle);

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);

            return mixin;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }
    }
}
