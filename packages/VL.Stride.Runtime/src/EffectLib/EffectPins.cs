using System;
using System.Collections.Generic;
using System.Reflection;
using VL.Core;
using Stride.Rendering;
using Stride.Core.Mathematics;

namespace VL.Stride.EffectLib
{
    public abstract class EffectPinDescription : IVLPinDescription
    {
        public abstract string Name { get; }
        public abstract Type Type { get; }
        public abstract object DefaultValueBoxed { get; }

        object IVLPinDescription.DefaultValue => DefaultValueBoxed;

        public abstract IVLPin CreatePin(ParameterCollection parameters);
    }

    public class PinDescription<T> : EffectPinDescription
    {
        public PinDescription(string name, T defaultValue = default(T))
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public override string Name { get; }
        public override Type Type => typeof(T);
        public override object DefaultValueBoxed => DefaultValue;
        public T DefaultValue { get; }

        public override IVLPin CreatePin(ParameterCollection parameters) => new Pin<T>(Name, DefaultValue);
    }

    public class ParameterPinDescription : EffectPinDescription
    {
        public readonly ParameterKey Key;
        public readonly int Count;
        public readonly bool IsPermutationKey;

        public ParameterPinDescription(HashSet<string> usedNames, ParameterKey key, int count = 1, object defaultValue = null, bool isPermutationKey = false)
        {
            Key = key;
            IsPermutationKey = isPermutationKey;
            Count = count;
            Name = key.GetPinName(usedNames);
            var elementType = key.PropertyType;
            defaultValue = defaultValue ?? key.DefaultValueMetadata?.GetDefaultValue();
            // TODO: This should be fixed in Stride
            if (key.PropertyType == typeof(Matrix))
                defaultValue = Matrix.Identity;
            if (count > 1)
            {
                Type = elementType.MakeArrayType();
                var arr = Array.CreateInstance(elementType, count);
                for (int i = 0; i < arr.Length; i++)
                    arr.SetValue(defaultValue, i);
                DefaultValueBoxed = arr;
            }
            else
            {
                Type = elementType;
                DefaultValueBoxed = defaultValue;
            }
        }

        public override string Name { get; }
        public override Type Type { get; }
        public override object DefaultValueBoxed { get; }
        public override IVLPin CreatePin(ParameterCollection parameters) => EffectPins.CreatePin(parameters, Key, Count, IsPermutationKey);
    }

    static class EffectPins
    {
        public static IVLPin CreatePin(ParameterCollection parameters, ParameterKey key, int count, bool isPermutationKey)
        {
            var argument = key.GetType().GetGenericArguments()[0];
            if (isPermutationKey)
            {
                var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreatePermutationPin), BindingFlags.Static | BindingFlags.Public);
                return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key }) as IVLPin;
            }
            else if (argument.IsValueType)
            {
                var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateValuePin), BindingFlags.Static | BindingFlags.Public);
                return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key, count }) as IVLPin;
            }
            else
            {
                var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateResourcePin), BindingFlags.Static | BindingFlags.Public);
                return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key }) as IVLPin;
            }
        }

        public static IVLPin CreatePermutationPin<T>(ParameterCollection parameters, PermutationParameterKey<T> key)
        {
            return new PermutationParameterPin<T>(parameters, key);
        }

        public static IVLPin CreateValuePin<T>(ParameterCollection parameters, ValueParameterKey<T> key, int count) where T : struct
        {
            if (count > 1)
            {
                return new ArrayValueParameterPin<T>(parameters, key);
            }
            else
            {
                return new ValueParameterPin<T>(parameters, key);
            }
        }

        public static IVLPin CreateResourcePin<T>(ParameterCollection parameters, ObjectParameterKey<T> key) where T : class
        {
            return new ResourceParameterPin<T>(parameters, key);
        }
    }

    abstract class ParameterPin
    {
        public readonly ParameterCollection Parameters;
        public readonly ParameterKey ParameterKey;

        public ParameterPin(ParameterCollection parameters, ParameterKey key)
        {
            Parameters = parameters;
            ParameterKey = key;
        }
    }

    abstract class PermutationParameterPin : ParameterPin
    {
        public PermutationParameterPin(ParameterCollection parameters, ParameterKey key) 
            : base(parameters, key)
        {
        }

        public bool HasChanged { get; protected set; }
    }

    class PermutationParameterPin<T> : PermutationParameterPin, IVLPin
    {
        public readonly PermutationParameterKey<T> Key;
        readonly EqualityComparer<T> comparer = EqualityComparer<T>.Default;

        public PermutationParameterPin(ParameterCollection parameters, PermutationParameterKey<T> key) 
            : base(parameters, key)
        {
            this.Key = key;
        }

        public T Value
        {
            get => Parameters.Get(Key);
            set
            {
                HasChanged = !comparer.Equals(value, Value);
                Parameters.Set(Key, value);
            }
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (T)value;
        }
    }

    class ValueParameterPin<T> : ParameterPin, IVLPin where T : struct
    {
        public readonly ValueParameterKey<T> Key;

        public ValueParameterPin(ParameterCollection parameters, ValueParameterKey<T> key) 
            : base(parameters, key)
        {
            this.Key = key;
        }

        public T Value
        {
            get => Parameters.Get(Key);
            set => Parameters.Set(Key, value);
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (T)value;
        }
    }

    class ArrayValueParameterPin<T> : ParameterPin, IVLPin where T : struct
    {
        public readonly ValueParameterKey<T> Key;

        public ArrayValueParameterPin(ParameterCollection parameters, ValueParameterKey<T> key) 
            : base(parameters, key)
        {
            this.Key = key;
        }

        // TODO: Add overloads to Stride wich take accessor instead of less optimal key
        public T[] Value
        {
            get => Parameters.GetValues(Key);
            set
            {
                if (value.Length > 0)
                    Parameters.Set(Key, value);
            }
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (T[])value;
        }
    }

    class ResourceParameterPin<T> : ParameterPin, IVLPin where T : class
    {
        public readonly ObjectParameterKey<T> Key;

        public ResourceParameterPin(ParameterCollection parameters, ObjectParameterKey<T> key) 
            : base(parameters, key)
        {
            this.Key = key;
        }

        public T Value
        {
            get => Parameters.Get(Key);
            set => Parameters.Set(Key, value ?? Key.DefaultValueMetadataT?.DefaultValue);
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (T)value;
        }
    }

    abstract class Pin
    {
        public Pin(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    class Pin<T> : Pin, IVLPin
    {
        public Pin(string name, T initialValue) : base(name)
        {
            Value = initialValue;
        }

        public T Value { get; set; }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (T)value;
        }
    }
}
