using System;
using System.Collections.Generic;
using System.Text;
using Stride.Rendering.Materials;
using Stride.Shaders;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Stride.Shaders.ShaderFX
{
    public class Semantic<T> : Var<T>
    {
        public Semantic(string semantic, string name = "SemanticValue")
            : base(null, name)
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
