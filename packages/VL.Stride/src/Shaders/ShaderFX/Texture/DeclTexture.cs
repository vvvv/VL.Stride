using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Shaders;
using Buffer = Stride.Graphics.Buffer;


namespace VL.Stride.Shaders.ShaderFX
{
    public class DeclTexture : ComputeNode<Texture>, IComputeVoid
    {
        private Texture texture;

        /// <summary>
        /// Can be updated from mainloop
        /// </summary>
        public Texture Texture
        { 
            get => texture;

            set
            {
                if (texture != value || compiled)
                {
                    texture = value;

                    if (Parameters != null && TextureKey != null)
                        Parameters.Set(TextureKey, texture); 
                }
            }
        }

        public ObjectParameterKey<Texture> TextureKey { get; protected set; }
        ParameterCollection Parameters;
        private bool compiled;

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            TextureKey = ContextKeyMap<Texture>.GetParameterKey(context, this);

            context.Parameters.Set(TextureKey, Texture);

            // remember parameters for updates from main loop 
            Parameters = context.Parameters;

            compiled = true;

            //no shader source to create here, only the key
            return new ShaderClassSource("ComputeVoid");
        }

        public override string ToString()
        {
            return string.Format("Texture {0}", TextureKey.Name);
        }
    }
}
