using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Shaders;

namespace VL.Stride.Shaders.ShaderFX
{
    public class DeclResource<T> : ComputeNode<T>, IComputeVoid where T : class
    {
        protected T resource;
        readonly string resourceGroupName;

        public DeclResource(string resourceGroupName = null)
        {
            this.resourceGroupName = resourceGroupName;
        }

        /// <summary>
        /// Can be updated from mainloop
        /// </summary>
        public T Resource
        {
            get => resource;

            set
            {
                if (resource != value || compiled)
                {
                    resource = value;

                    if (Parameters != null && Key != null)
                        Parameters.Set(Key, resource);
                }
            }
        }

        public ObjectParameterKey<T> Key { get; protected set; }
        ParameterCollection Parameters;
        private bool compiled;

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            Key = ContextKeyMap<T>.GetParameterKey(context, this);

            context.Parameters.Set(Key, Resource);

            // remember parameters for updates from main loop 
            Parameters = context.Parameters;

            compiled = true;

            //no shader source to create here, only the key
            return new ShaderClassSource("ComputeVoid");
        }

        public virtual string GetResourceGroupName(ShaderGeneratorContext context)
        {
            if (string.IsNullOrWhiteSpace(resourceGroupName))
            {
                return context is MaterialGeneratorContext ? "PerMaterial" : "PerUpdate";
            }

            return resourceGroupName;
        }

        public override string ToString()
        {
            return $"{typeof(T).Name} {Key?.Name}";
        }
    }
}
