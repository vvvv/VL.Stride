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
            return new ShaderClassSource(shaderName + GetNameForType<T>(), genericArguments);
        }

        public static ShaderClassSource GetShaderSourceForType2<T1, T2>(string shaderName, params object[] genericArguments)
        {
            return new ShaderClassSource(shaderName + GetNameForType<T1>() + GetNameForType<T2>(), genericArguments);
        }

        static Dictionary<Type, string> KnownTypes = new Dictionary<Type, string>();

        static ShaderFXUtils()
        {
            KnownTypes.Add(typeof(float), "Float");
            KnownTypes.Add(typeof(Vector2), "Float2");
            KnownTypes.Add(typeof(Vector3), "Float3");
            KnownTypes.Add(typeof(Vector4), "Float4");
            KnownTypes.Add(typeof(Matrix), "Matrix");
            KnownTypes.Add(typeof(int), "Int");
            KnownTypes.Add(typeof(uint), "UInt");
            KnownTypes.Add(typeof(bool), "Bool");
        }

        public static string GetNameForType<T>()
        {
            if (KnownTypes.TryGetValue(typeof(T), out var result))
                return result;

            throw new NotImplementedException("No shader defined for type: " + typeof(T).Name);            
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

        public static IEnumerable<IComputeNode> ReturnIfNotNull(params IComputeNode[] children)
        {
            foreach (var child in children)
                if (child != null)
                    yield return child;
        }
    }
}
