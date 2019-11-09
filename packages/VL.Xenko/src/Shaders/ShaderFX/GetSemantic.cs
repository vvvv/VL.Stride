using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class GetSemantic<T> : ComputeValue<T>
    {
        public GetSemantic(string semantic)
        {
            Semantic = semantic;
        }

        public string Semantic { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderClassSource = GetShaderSourceForType<T>("GetSemantic", "SemanticValue", Semantic);

            return shaderClassSource;
        }
    }
}
