using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class Delegate1In1Out<TIn, TOut> : Funk1In1Out<TIn, TOut>
    {
        public Delegate1In1Out(Var<TIn> arg, Var<TOut> result, IComputeVoid body)
            : base("Delegate", null)
        {
            Arg = arg;
            Result = result;

            if (body != null)
            {
                Inputs = new[]
                       {
                    new KeyValuePair<string, IComputeNode>("Body", body)
                }; 
            }
        }

        public IEnumerable<KeyValuePair<string, IComputeNode>> Inputs { get; private set; }
        public Var<TIn> Arg { get; }
        public Var<TOut> Result { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceFunkForType2<TIn, TOut>(ShaderName, Arg.VarName, Result.VarName);

            //compose if necessary
            if (Inputs != null && Inputs.Any())
            {
                var mixin = shaderSource.CreateMixin();

                foreach (var input in Inputs)
                {
                    mixin.AddComposition(input.Value, input.Key, context, baseKeys);
                }

                return mixin;
            }

            return shaderSource;
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
            return ShaderName;
        }
    }
}
