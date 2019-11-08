using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Buffer = Xenko.Graphics.Buffer;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class InputValue<T> : ComputeValue<T> where T : struct
    {
        private T inputValue;

        /// <summary>
        /// Can be updated from mainloop
        /// </summary>
        public T Input
        { 
            get => inputValue;

            set
            {
                if (!inputValue.Equals(value))
                {
                    this.inputValue = value;

                    if (Parameters != null && ValueKey != null)
                        Parameters.Set(ValueKey, this.inputValue); 
                }
            }
        }

        public ValueParameterKey<T> ValueKey { get; protected set; }
        ParameterCollection Parameters;

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            ValueKey = ContextValueKeyMap<T>.GetParameterKey(context, this);

            context.Parameters.Set(ValueKey, Input);

            // remember parameters for updates from main loop 
            Parameters = context.Parameters;

            var shaderClassSource = GetShaderSourceForType<T>("InputValue", ValueKey, "PerDispatch");

            //no shader source to create here, only the key
            return shaderClassSource;
        }
    }
}
