using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Xenko.Core.Mathematics;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
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
