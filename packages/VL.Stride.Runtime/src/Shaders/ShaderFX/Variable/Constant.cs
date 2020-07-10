using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Stride.Rendering.Materials;
using Stride.Shaders;
using Stride.Core.Mathematics;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Stride.Shaders.ShaderFX
{
    public class Constant<T> : Var<T>
    {
        public readonly T ConstantValue;

        public Constant(T value)
            : base(null, "Constant")
        {
            ConstantValue = value;
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            return new ShaderClassSource("ComputeVoid");
        }
    }
}
