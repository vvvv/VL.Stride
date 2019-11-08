using System.Collections.Generic;
using Xenko.Rendering;
using Xenko.Rendering.Materials;

namespace VL.Xenko.Shaders.ShaderFX
{
    static class ContextKeyMap<T>
    {
        static ulong ObjectKeyIDCounter;

        //shader compiler context to object key
        static Dictionary<ShaderGeneratorContext, Dictionary<T, ObjectParameterKey<T>>> KeyValuesPerContext = new Dictionary<ShaderGeneratorContext, Dictionary<T, ObjectParameterKey<T>>>();

        public static ObjectParameterKey<T> GetParameterKey(ShaderGeneratorContext context, T value)
        {
            var keyMap = GetBufferMap(context);
            if (keyMap.TryGetValue(value, out ObjectParameterKey<T> key))
            {
                return key;
            }

            var newObjectKey = ParameterKeys.NewObject<T>(default(T), "Object_fx" + (++ObjectKeyIDCounter));
            keyMap[value] = newObjectKey;

            return newObjectKey;
        }

        static Dictionary<T, ObjectParameterKey<T>> GetBufferMap(ShaderGeneratorContext context)
        {
            if (KeyValuesPerContext.TryGetValue(context, out Dictionary<T, ObjectParameterKey<T>> bufferMap))
            {
                return bufferMap;
            }

            var newBufferMap = new Dictionary<T, ObjectParameterKey<T>>();
            KeyValuesPerContext[context] = newBufferMap;

            return newBufferMap;
        }

        public static bool RemoveContext(ShaderGeneratorContext context)
        {
            return KeyValuesPerContext.Remove(context);
        }
    }
}
