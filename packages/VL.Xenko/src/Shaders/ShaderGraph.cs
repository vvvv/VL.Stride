using System;
using System.Collections.Generic;
using System.Text;
using VL.Xenko.Effects.ComputeFX;
using VL.Xenko.Effects.TextureFX;
using VL.Xenko.Rendering;
using Xenko.Core;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.Images;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Materials.ComputeColors;
using Xenko.Shaders;

namespace VL.Xenko.Shaders
{
    public static class ShaderGraph
    {
        public static IComputeVoid SomeComputeFXGraph()
        {
            var value1 = new ComputeValue<float>();
            var value2 = new ComputeValue<float>();
            var value3 = new ComputeValue<float>();

            var first = new AssignVar<float>()
            {
                VarName = "var1",
                Value = value1
            };

            var second = new AssignVar<float>()
            {
                VarName = "var2",
                Value = value2
            };

            var third = new AssignVar<float>()
            {
                VarName = "var3",
                Value = value3
            };

            var order = new ComputeOrder()
            {
                Computes = new[] { first, second, third }
            };

            return order;
        }

        public static ComputeEffectDispatcher ComposeComputeShader(GraphicsDevice graphicsDevice, IServiceRegistry services, IComputeVoid root)
        {
            var computeEffect = new ComputeEffectDispatcher(RenderContext.GetShared(services), "ComputeFXGraphEffect");

            if (root != null)
            {
                var context = new ShaderGeneratorContext(graphicsDevice)
                {
                    Parameters = computeEffect.Parameters,
                };

                var key = new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White);
                var shaderSource = root.GenerateShaderSource(context, key);

                var mixin = new ShaderMixinSource();
                mixin.AddComposition("Root", shaderSource);

                computeEffect.Parameters.Set(ComputeFXKeys.ComputeRoot, mixin);
            }
            return computeEffect;
        }

        public static TextureFXEffect ComposeShader(GraphicsDevice graphicsDevice, IComputeColor root)
        {
            var effectImageShader = new TextureFXEffect("CCShaderGraphEffect");

            if (root != null)
            {
                var context = new ShaderGeneratorContext(graphicsDevice)
                {
                    Parameters = effectImageShader.Parameters,
                };

                var key = new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White);
                var shaderSource = root.GenerateShaderSource(context, key);

                var mixin = new ShaderMixinSource();
                mixin.AddComposition("Root", shaderSource);

                effectImageShader.Parameters.Set(TextureFXKeys.ComputeColorRoot, mixin); 
            }
            return effectImageShader;
        }
    }
}
