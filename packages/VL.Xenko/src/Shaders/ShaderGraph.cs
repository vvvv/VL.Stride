using System;
using System.Collections.Generic;
using System.Text;
using VL.Xenko.Effects.TextureFX;
using VL.Xenko.Rendering;
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
        public static TextureFXEffect ComposeShader(GraphicsDevice graphicsDevice, IComputeColor root)
        {
            var effectImageShader = new TextureFXEffect("CCShaderGraphEffect");

            //var computeColorShader = new ComputeShaderClassColor
            //{
            //    MixinReference = "ComputeColorOne",
            //};

            //var computeColor1 = new ComputeColor(Color.Red);
            //var computeColor2 = new ComputeColor(Color.Green);
            //var computeColor = new ComputeBinaryColor
            //{
            //    LeftChild = computeColor1,
            //    RightChild = computeColor2,
            //    Operator = BinaryOperator.Add
            //};

            if (root != null)
            {
                var context = new ShaderGeneratorContext(graphicsDevice)
                {
                    Parameters = effectImageShader.Parameters,
                };

                var key = new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White);
                var shaderSource = root.GenerateShaderSource(context, key);

                var mixin = new ShaderMixinSource();
                mixin.AddComposition("Func", shaderSource);

                effectImageShader.Parameters.Set(TextureFXKeys.ComputeColorRoot, mixin); 
            }
            return effectImageShader;
        }
    }
}
