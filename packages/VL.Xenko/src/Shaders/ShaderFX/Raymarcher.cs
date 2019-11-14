using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Core.Mathematics;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders.ShaderFX
{
    public class Raymarcher : ComputeValue<Vector4> 
    {
        public Raymarcher(SDF3D sdf)
        {
            SDF = sdf;
        }

        public SDF3D SDF { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = new ShaderClassSource("Raymarcher");

            var mixin = shaderSource.CreateMixin();

            mixin.AddComposition(SDF, "SDF", context, baseKeys);

            return mixin;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(SDF);
        }

        public override string ToString()
        {
            return string.Format("Raymarcher {0}", SDF);
        }
    }
}
