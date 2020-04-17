using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Buffer = Xenko.Graphics.Buffer;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class DeclBuffer : ComputeNode<Buffer>, IComputeVoid
    {
        private Buffer buffer;

        /// <summary>
        /// Can be updated from mainloop
        /// </summary>
        public Buffer Buffer
        { 
            get => buffer;

            set
            {
                if (buffer != value || compiled)
                {
                    buffer = value;

                    if (Parameters != null && BufferKey != null)
                        Parameters.Set(BufferKey, buffer); 
                }
            }
        }

        public ObjectParameterKey<Buffer> BufferKey { get; protected set; }
        ParameterCollection Parameters;
        private bool compiled;

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            BufferKey = ContextKeyMap<Buffer>.GetParameterKey(context, this);

            context.Parameters.Set(BufferKey, Buffer);

            // remember parameters for updates from main loop 
            Parameters = context.Parameters;

            compiled = true;

            //no shader source to create here, only the key
            return new ShaderClassSource("ComputeVoid");
        }

        public override string ToString()
        {
            return string.Format("Buffer {0}", BufferKey.Name);
        }
    }
}
