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
    public class Constant<T> : ComputeValue<T>
    {
        private readonly T Value;

        public Constant(T value)
        {
            ShaderName = "Constant";
            Value = value;
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            return GetShaderSourceForType<T>(ShaderName, GetAsShaderString((dynamic)Value));
        }
    }
}
