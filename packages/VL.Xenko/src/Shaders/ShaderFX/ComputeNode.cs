using System;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Materials.ComputeColors;
using Xenko.Shaders;
using Xenko.Core.Mathematics;
using System.Collections.Generic;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders
{
    public class ComputeNode<T> : ComputeNode
    {
        public static IEnumerable<IComputeNode> ReturnIfNotNull(params IComputeNode[] children)
        {
            foreach (var child in children)
                if (child != null)
                    yield return child;
        }

        public string ShaderName { get; set; } = "Compute";

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType<T>(ShaderName);
            return shaderSource;
        }
    }
}
