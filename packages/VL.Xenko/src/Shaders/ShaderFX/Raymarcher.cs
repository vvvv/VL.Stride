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
        public Raymarcher(string shaderName, Funk1In1Out<Vector3, float> sdf)
        {
            ShaderName = shaderName;
            SDF = sdf;
        }

        public Funk1In1Out<Vector3, float> SDF { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = new ShaderClassSource(ShaderName);

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
            return string.Format("{0} {1}", ShaderName, SDF);
        }
    }
}
