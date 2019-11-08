using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Shaders;
using Xenko.Core.Mathematics;
using Xenko.Rendering.Materials;

namespace VL.Xenko.Shaders.ShaderFX
{
    public static class ShaderFXUtils
    {
        public static ShaderClassSource GetShaderSourceForType<T>(string shaderName, params object[] genericArguments)
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
                    return new ShaderClassSource(shaderName + "UInt", genericArguments);
                default:
                    throw new NotImplementedException("No shader defined for type: " + typeof(T).Name);
            }
        }

        public static ShaderMixinSource CreateMixin(this ShaderClassSource shaderClassSource)
        {
            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(shaderClassSource);
            return mixin;
        }

        public static ShaderSource AddComposition(this ShaderMixinSource mixin, IComputeNode compute, string compositionName, ShaderGeneratorContext context, MaterialComputeColorKeys keys)
        {
            var shaderSource = compute?.GenerateShaderSource(context, keys);
            if (shaderSource != null)
                mixin.AddComposition(compositionName, shaderSource);

            return shaderSource;
        }
    }
}
