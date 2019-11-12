using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VL.Xenko.Effects.ComputeFX;
using VL.Xenko.Effects.ShaderFX;
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
using Buffer = Xenko.Graphics.Buffer;

namespace VL.Xenko.Shaders.ShaderFX
{
    public static class ShaderGraph
    {
        public static IComputeVoid SomeComputeFXGraph(Buffer buffer)
        {
            var bufferDecl = new DeclBuffer();
            bufferDecl.Buffer = buffer;
            var getItem = new GetItemBuffer<float>(bufferDecl, new ComputeValue<uint>());

            var value1 = new ComputeValue<float>();
            var value2 = new ComputeValue<float>();
            var value3 = new ComputeValue<float>();

            var var1 = new Var<float>(getItem);

            var var2 = new Var<float>(value2);

            var var3 = new Var<float>(value3);

            var assignVars = new ComputeOrder(var1, var2, var3);

            var first = BuildPlus(var1, var2);
            var second = BuildPlus(first, var1);
            var third = BuildPlus(second, var3);

            var root = new ComputeOrder(third);

            var finalOrder = BuildFinalShaderGraph(root);

            return finalOrder;
        }

        public static IComputeVoid BuildFinalShaderGraph(IComputeNode root, IEnumerable<IComputeNode> excludes = null)
        {
            var tree = root is IComputeVoid ? new[] { root } : root.GetChildren();

            var visited = excludes != null ? new HashSet<IComputeNode>(excludes) : new HashSet<IComputeNode>();
            var flat = tree.TraversePostOrder(n => n.GetChildren(), visited).ToList();

            var statements = flat.OfType<IComputeVoid>();

            var finalOrder = new ComputeOrder(statements);
            return finalOrder;
        }

        static Var<float> BuildPlus(Var<float> var1, Var<float> var2)
        {
            var getter1 = new GetVar<float>(var1);
            var getter2 = new GetVar<float>(var2);

            var plus = new BinaryOperation<float>("Plus", getter1, getter2);

            return new Var<float>(plus, null, "PlusResult");
        }

        public static IEnumerable<T> TraversePostOrder<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f, HashSet<T> visited) where T : IComputeNode
        {
            foreach (var item in e)
            {
                if (!visited.Contains(item))
                {
                    visited.Add(item);

                    var children = f(item).TraversePostOrder(f, visited);

                    foreach (var child in children)
                        yield return child;

                    yield return item;
                }
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c => f(c).Flatten(f)).Concat(e);
        }

        public static IEnumerable<T> ExpandPreOrder<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> elementSelector)
        {
            var stack = new Stack<IEnumerator<T>>();
            var e = source.GetEnumerator();
            try
            {
                while (true)
                {
                    while (e.MoveNext())
                    {
                        var item = e.Current;
                        yield return item;
                        var elements = elementSelector(item);
                        if (elements == null) continue;
                        stack.Push(e);
                        e = elements.GetEnumerator();
                    }
                    if (stack.Count == 0) break;
                    e.Dispose();
                    e = stack.Pop();
                }
            }
            finally
            {
                e.Dispose();
                while (stack.Count != 0) stack.Pop().Dispose();
            }
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

                computeEffect.Parameters.Set(ComputeFXKeys.ComputeFXRoot, mixin);
            }
            return computeEffect;
        }

        public static TextureFXEffect ComposeShader(GraphicsDevice graphicsDevice, IComputeValue<Vector4> root)
        {
            var effectImageShader = new TextureFXEffect("TextureFXGraphEffect");

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

                effectImageShader.Parameters.Set(TextureFXKeys.TextureFXRoot, mixin);
            }
            return effectImageShader;
        }

        public static DynamicEffectInstance ComposeDrawShader(GraphicsDevice graphicsDevice, IComputeValue<Vector4> vertexRoot, IComputeValue<Vector4> pixelRoot)
        {
            var effectImageShader = new DynamicEffectInstance("ShaderFXGraphEffect");

            if (vertexRoot != null && pixelRoot != null)
            {
                var context = new ShaderGeneratorContext(graphicsDevice)
                {
                    Parameters = effectImageShader.Parameters,
                };

                var key = new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White);
                var vertexShaderSource = vertexRoot.GenerateShaderSource(context, key);
                var pixelShaderSource = pixelRoot.GenerateShaderSource(context, key);

                var mixin = new ShaderMixinSource();
                mixin.AddComposition("VertexRoot", vertexShaderSource);
                mixin.AddComposition("PixelRoot", pixelShaderSource);

                effectImageShader.Parameters.Set(ShaderFXKeys.ShaderFXRoot, mixin);
            }
            return effectImageShader;
        }
    }
}
