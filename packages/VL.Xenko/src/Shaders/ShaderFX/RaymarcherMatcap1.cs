using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders.ShaderFX
{
    public class RaymarcherMatcap : Funk1In1Out<Vector2, Vector4>
    {
        public RaymarcherMatcap(string functionName, IEnumerable<KeyValuePair<string, IComputeNode>> inputs)
            : base(functionName, inputs)
        {
        }

        /// <summary>
        /// Can be updated from mainloop
        /// </summary>
        public Texture Input
        {
            get => inputValue;

            set
            {
                if ((inputValue != value) || compiled)
                {
                    this.inputValue = value;

                    if (Parameters != null && UsedKey != null)
                        Parameters.Set(UsedKey, inputValue);

                    compiled = false;
                }
            }
        }

        public ObjectParameterKey<Texture> UsedKey { get; protected set; }
        public ObjectParameterKey<Texture> Key { get; }

        private Texture inputValue;

        ParameterCollection Parameters;
        bool compiled;


        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = new ShaderClassSource(ShaderName);

            UsedKey = Key ?? TexturingKeys.Texture0;

            context.Parameters.Set(UsedKey, Input);

            // remember parameters for updates from main loop 
            Parameters = context.Parameters;

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
