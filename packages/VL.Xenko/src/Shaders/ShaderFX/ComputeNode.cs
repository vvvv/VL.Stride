using System;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Materials.ComputeColors;
using Xenko.Shaders;
using Xenko.Core.Mathematics;

namespace VL.Xenko.Shaders
{
    public class ComputeNode<T> : ComputeNode
    {
        public string ShaderName { get; set; }

        protected virtual ShaderClassSource GetShaderSourceForType(string shaderName, params string[] genericArguments)
        {
            switch (default(T))
            {
                case float v:
                    return new ShaderClassSource(shaderName + "Float", genericArguments);
                case Vector2 v:
                    return new ShaderClassSource(shaderName + "Float2", genericArguments);
                case Vector3 v:
                    return new ShaderClassSource(shaderName + "Float3", genericArguments);
                case Vector4 v:
                    return new ShaderClassSource(shaderName + "Float4", genericArguments);
                case Matrix v:
                    return new ShaderClassSource(shaderName + "Matrix", genericArguments);
                case int v:
                    return new ShaderClassSource(shaderName + "Int", genericArguments);
                case uint v:
                    return new ShaderClassSource(shaderName + "Uint", genericArguments);
                default:
                    throw new NotImplementedException("No shader defined for type: " + typeof(T).Name);
            }
        }

        protected static ShaderMixinSource CreateMixin(ShaderClassSource shaderClassSource)
        {
            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(shaderClassSource);
            return mixin;
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType(ShaderName);
            return shaderSource;
        }
    }
}
