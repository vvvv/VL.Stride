using System;
using System.Collections.Generic;
using System.Text;
using VL.Xenko.Effects.TextureFX;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Rendering.Images;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Materials.ComputeColors;
using Xenko.Shaders;

namespace VL.Xenko.Shaders
{
    public static class ShaderGraph
    {
        public static ImageEffectShader ComposeShader(GraphicsDevice graphicsDevice)
        {
            var effectImageShader = new ImageEffectShader("EffectTest");

            var context = new ShaderGeneratorContext(graphicsDevice)
            {
                Parameters = effectImageShader.Parameters,
            };

            var computeColor = new ComputeShaderClassColor
            {
                MixinReference = "ComputeColorOne",
            };

            var computeColor1 = new ComputeColor(Color.Red);
            var computeColor2 = new ComputeColor(Color.Green);
            var computeColorT = new ComputeBinaryColor
            {
                LeftChild = computeColor1,
                RightChild = computeColor2,
                Operator = BinaryOperator.Add
            };

            var key = new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White);
            var shaderSource = computeColor.GenerateShaderSource(context, key);

            var mixin = new ShaderMixinSource();
            mixin.AddComposition("Func", shaderSource);

            effectImageShader.Parameters.Set(TextureFXKeys.Test123, mixin);
            //effectImageShader.Parameters.Set(MaterialKeys.DiffuseValue, Color.Red);
            return effectImageShader;
        }
    }
}
