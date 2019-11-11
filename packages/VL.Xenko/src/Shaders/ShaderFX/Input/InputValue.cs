using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Buffer = Xenko.Graphics.Buffer;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class InputValue<T> : ComputeValue<T> where T : struct
    {
        public InputValue(ValueParameterKey<T> key)
        {
            Key = key;
        }

        private T inputValue;

        /// <summary>
        /// Can be updated from mainloop
        /// </summary>
        public T Input
        { 
            get => inputValue;

            set
            {
                if (!inputValue.Equals(value) || compiled)
                {
                    this.inputValue = value;

                    if (Parameters != null && UsedKey != null)
                        Parameters.Set(UsedKey, inputValue);

                    compiled = false;
                }
            }
        }

        public ValueParameterKey<T> UsedKey { get; protected set; }
        public ValueParameterKey<T> Key { get; }

        ParameterCollection Parameters;
        bool compiled;
        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            UsedKey = Key ?? ContextValueKeyMap2<T>.GetParameterKey(this);
            var cBuffer = "PerDraw";

            if (Key == null)
            {
                context.Parameters.Set(UsedKey, Input);

                // remember parameters for updates from main loop 
                Parameters = context.Parameters;

                cBuffer = "PerUpdate";
            }

            var shaderClassSource = GetShaderSourceForType<T>("Input", UsedKey, cBuffer);
            compiled = true;
            //no shader source to create here, only the key
            return shaderClassSource;
        }
    }
}
