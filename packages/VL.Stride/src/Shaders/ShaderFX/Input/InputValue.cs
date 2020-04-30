using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Shaders;
using Buffer = Stride.Graphics.Buffer;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Stride.Shaders.ShaderFX
{
    public class InputValue<T> : ComputeValue<T> where T : struct
    {
        public InputValue(ValueParameterKey<T> key, string constantBufferName = "PerUpdate")
        {
            Key = key;
            ConstantBufferName = constantBufferName;
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
        public string ConstantBufferName { get; private set; }

        ParameterCollection Parameters;
        bool compiled;
        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            UsedKey = Key ?? UsedKey ?? ContextValueKeyMap2<T>.GetParameterKey(this);

            ShaderClassSource shaderClassSource;

            if (Key == null)
            {
                UsedKey = UsedKey ?? ContextValueKeyMap2<T>.GetParameterKey(this);
                context.Parameters.Set(UsedKey, Input);

                // remember parameters for updates from main loop 
                Parameters = context.Parameters;

                shaderClassSource = GetShaderSourceForType<T>("Input", UsedKey, ConstantBufferName);
            }
            else
            {
                UsedKey = Key;
                shaderClassSource = GetShaderSourceForType<T>("InputKey", UsedKey);
            }

            compiled = true;
            //no shader source to create here, only the key
            return shaderClassSource;
        }
    }
}
