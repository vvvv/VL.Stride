using System.Collections.Generic;
using Stride.Core;
using Stride.Rendering;
using Stride.Rendering.Materials;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Stride.Shaders.ShaderFX
{

    //TODO: have one counter per context
    public static class ContextKeyMap<T>
    {
        static ulong ObjectKeyIDCounter;

        //shader compiler context to object key
        static Dictionary<ShaderGeneratorContext, Dictionary<object, ObjectParameterKey<T>>> KeyValuesPerContext = new Dictionary<ShaderGeneratorContext, Dictionary<object, ObjectParameterKey<T>>>();

        public static ObjectParameterKey<T> GetParameterKey(ShaderGeneratorContext context, object uniqueReference)
        {
            var keyMap = GetContextMap(context);
            if (keyMap.TryGetValue(uniqueReference, out ObjectParameterKey<T> key))
            {
                return key;
            }

            var newObjectKey = ParameterKeys.NewObject<T>(default(T), "Object" + GetNameForType<T>() + "_fx" + (++ObjectKeyIDCounter));
            keyMap[uniqueReference] = newObjectKey;

            return newObjectKey;
        }

        static Dictionary<object, ObjectParameterKey<T>> GetContextMap(ShaderGeneratorContext context)
        {
            if (KeyValuesPerContext.TryGetValue(context, out Dictionary<object, ObjectParameterKey<T>> keyMap))
            {
                return keyMap;
            }

            var newKeyMap = new Dictionary<object, ObjectParameterKey<T>>();
            KeyValuesPerContext[context] = newKeyMap;

            return newKeyMap;
        }

        public static bool RemoveContext(ShaderGeneratorContext context)
        {
            return KeyValuesPerContext.Remove(context);
        }
    }
    public static class ContextValueKeyMap<T> where T : struct
    {
        static ulong ValueKeyIDCounter;

        //shader compiler context to object key
        static Dictionary<ShaderGeneratorContext, Dictionary<object, ValueParameterKey<T>>> KeyValuesPerContext = new Dictionary<ShaderGeneratorContext, Dictionary<object, ValueParameterKey<T>>>();

        public static ValueParameterKey<T> GetParameterKey(ShaderGeneratorContext context, object uniqueReference)
        {
            var keyMap = GetContextMap(context);
            if (keyMap.TryGetValue(uniqueReference, out ValueParameterKey<T> key))
            {
                return key;
            }

            var newObjectKey = ParameterKeys.NewValue<T>(default(T), "Value_fx" + (++ValueKeyIDCounter));
            keyMap[uniqueReference] = newObjectKey;

            return newObjectKey;
        }

        static Dictionary<object, ValueParameterKey<T>> GetContextMap(ShaderGeneratorContext context)
        {
            if (KeyValuesPerContext.TryGetValue(context, out Dictionary<object, ValueParameterKey<T>> keyMap))
            {
                return keyMap;
            }

            var newKeyMap = new Dictionary<object, ValueParameterKey<T>>();
            KeyValuesPerContext[context] = newKeyMap;

            return newKeyMap;
        }

        public static bool RemoveContext(ShaderGeneratorContext context)
        {
            return KeyValuesPerContext.Remove(context);
        }
    }

    public static class ContextValueKeyMap2<T> where T : struct
    {
        static ulong ValueKeyIDCounter;

        //shader compiler context to object key
        static Dictionary<object, ValueParameterKey<T>> KeyValuesPerReference = new Dictionary<object, ValueParameterKey<T>>();

        public static ValueParameterKey<T> GetParameterKey(object uniqueReference)
        {
            if (KeyValuesPerReference.TryGetValue(uniqueReference, out ValueParameterKey<T> key))
            {
                return key;
            }

            var newValueKey = ParameterKeys.NewValue<T>(default, "Value" + GetNameForType<T>() + "_fx" + (++ValueKeyIDCounter));
            KeyValuesPerReference[uniqueReference] = newValueKey;

            return newValueKey;
        }

        public static bool RemoveContext(object uniqueReference)
        {
            return KeyValuesPerReference.Remove(uniqueReference);
        }
    }

    public static class GenericValueKeys<T> where T : struct
    {
        static GenericValueKeys()
        {
            GenericValueParameter = ParameterKeys.NewValue<T>(default(T), "ShaderFX.InputValue" + GetNameForType<T>());
            ParameterKeys.Merge(GenericValueParameter, typeof(GenericValueKeys<T>), GenericValueParameter.Name);
        }

        [DataMemberIgnore]
        public static readonly ValueParameterKey<T> GenericValueParameter;
    }
}
