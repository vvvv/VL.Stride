using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Materials.ComputeColors;
using Xenko.Shaders;
using Xenko.Core.Mathematics;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class FloatToComputeFloat : ComputeFloat
    {
        public FloatToComputeFloat(IComputeValue<float> value)
        {
            this.Value = value;
        }

        public IComputeValue<float> Value { get; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = new ShaderClassSource("FloatToScalar");

            var mixin = shaderSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);

            return mixin;
        }
    }
}
