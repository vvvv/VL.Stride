using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VL.Xenko.Effects.ComputeFX;
using VL.Xenko.Effects.TextureFX;
using VL.Xenko.Rendering;
using VL.Xenko.Shaders.ShaderFX.Operations;
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

            var var1 = new AssignVar<float>()
            {
                VarName = "var1",
                Value = value1
            };

            var var2 = new AssignVar<float>()
            {
                VarName = "var2",
                Value = value2
            };

            var var3 = new AssignVar<float>()
            {
                VarName = "var3",
                Value = value3
            };

            var assignVars = new ComputeOrder()
            {
                Computes = new[] { var1, var2, var3 }
            };

            var getter1 = new GetVar<float>()
            {
                Var = var1,
            };

            var getter2 = new GetVar<float>()
            {
                Var = var2,
            };

            var plus = new BinaryOperation<float>("Plus")
            {
                Left = getter1,
                Right = getter2
            };

            var last = new AssignVar<float>()
            {
                VarName = "result",
                Value = plus
            };

            var result = new ComputeOrder()
            {
                Computes = new IComputeVoid[] { assignVars, last }
            };

            var tree = result.GetChildren();

            var flat = tree.Flatten(n => n.GetChildren()).ToList();

            return result;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c => f(c).Flatten(f)).Concat(e);
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
