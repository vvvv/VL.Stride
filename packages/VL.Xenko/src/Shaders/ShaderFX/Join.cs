using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Xenko.Core.Mathematics;

namespace VL.Xenko.Shaders.ShaderFX
{
    public class JoinVector4 : Join<Vector4>
    {

    }

    public class Join<T> : ComputeValue<T>
    {
        public Join()
        {
            ShaderName = "Join";
        }

        public IComputeValue<float> X { get; set; }
        public IComputeValue<float> Y { get; set; }
        public IComputeValue<float> Z { get; set; }
        public IComputeValue<float> W { get; set; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(X, Y, Z, W);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType(ShaderName);

            var xShaderSource = X?.GenerateShaderSource(context, baseKeys);
            var yShaderSource = Y?.GenerateShaderSource(context, baseKeys);
            var zShaderSource = Z?.GenerateShaderSource(context, baseKeys);
            var wShaderSource = W?.GenerateShaderSource(context, baseKeys);

            var mixin = CreateMixin(shaderSource);

            if (xShaderSource != null)
                mixin.AddComposition("x", xShaderSource);
            if (yShaderSource != null)
                mixin.AddComposition("y", yShaderSource);
            if (zShaderSource != null)
                mixin.AddComposition("z", zShaderSource);
            if (wShaderSource != null)
                mixin.AddComposition("w", yShaderSource);

            return mixin;
        }
    }
}
