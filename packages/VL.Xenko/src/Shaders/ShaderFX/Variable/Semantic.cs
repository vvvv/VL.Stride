using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class Semantic<T> : Var<T>
    {
        public Semantic(string semantic)
            : base(null, "SemanticValue")
        {
            SemanticName = semantic;
        }

        public string SemanticName { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderClassSource = new ShaderClassSource("ComputeVoid");

            return shaderClassSource;
        }

        public override string ToString()
        {
            return string.Format("{0}", SemanticName);
        }
    }
}
