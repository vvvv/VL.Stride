using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Shaders;

namespace VL.Stride.Shaders.ShaderFX
{
    public class GenericComputeNode<TOut> : ComputeValue<TOut>
    {
        public GenericComputeNode(Func<ShaderGeneratorContext, MaterialComputeColorKeys, ShaderClassCode> getShaderSource,
            IEnumerable<KeyValuePair<string, IComputeNode>> inputs)
        {
            Inputs = inputs?.Where(input => !string.IsNullOrWhiteSpace(input.Key) && input.Value != null).ToList();
            GetShaderSource = getShaderSource;
        }

        public Func<ShaderGeneratorContext, MaterialComputeColorKeys, ShaderClassCode> GetShaderSource { get; }

        public IEnumerable<KeyValuePair<string, IComputeNode>> Inputs { get; }

        public ParameterCollection Parameters => parameters;
        public ShaderClassCode ShaderClass => shaderClass;

        ParameterCollection parameters;
        ShaderClassCode shaderClass;

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            parameters = context.Parameters;
            shaderClass = GetShaderSource(context, baseKeys);

            //compose if necessary
            if (Inputs != null && Inputs.Any())
            {
                var mixin = shaderClass.CreateMixin();

                foreach (var input in Inputs)
                {
                    mixin.AddComposition(input.Value, input.Key, context, baseKeys);
                }

                return mixin;
            }

            return shaderClass;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            if (Inputs != null)
            {
                foreach (var item in Inputs)
                {
                    if (item.Value != null)
                        yield return item.Value;
                }
            }
        }

        public override string ToString()
        {
            return shaderClass?.ToString() ?? GetType().ToString();
        }
    }
}
