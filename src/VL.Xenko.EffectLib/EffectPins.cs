using System;
using System.Collections.Generic;
using System.Reflection;
using VL.Core;
using Xenko.Rendering;

namespace VL.Xenko.EffectLib
{
    static class EffectPins
    {
        public static IVLPin CreatePin(ParameterCollection parameters, ParameterKey key, bool isPermutationKey)
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
                return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key }) as IVLPin;
            }
            else
            {
                var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateResourcePin), BindingFlags.Static | BindingFlags.Public);
                return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key }) as IVLPin;
            }
        }

        public static IVLPin CreatePermutationPin<T>(ParameterCollection parameters, PermutationParameterKey<T> key)
        {
            var shaderType = typeof(T);
            var pinType = TypeConversions.ShaderToPinTypeMap.ValueOrDefault(shaderType);
            if (pinType != null)
            {
                var convertedValueParameterPinType = typeof(ConvertedPermutationParameterPin<,>).MakeGenericType(shaderType, pinType);
                return Activator.CreateInstance(convertedValueParameterPinType, parameters, key) as IVLPin;
            }
            return new PermutationParameterPin<T>(parameters, key);
        }

        public static IVLPin CreateValuePin<T>(ParameterCollection parameters, ValueParameterKey<T> key) where T : struct
        {
            var shaderType = typeof(T);
            var pinType = TypeConversions.ShaderToPinTypeMap.ValueOrDefault(shaderType);
            if (pinType != null)
            {
                var convertedValueParameterPinType = typeof(ConvertedValueParameterPin<,>).MakeGenericType(shaderType, pinType);
                return Activator.CreateInstance(convertedValueParameterPinType, parameters, key) as IVLPin;
            }
            return new ValueParameterPin<T>(parameters, key);
        }

        public static IVLPin CreateResourcePin<T>(ParameterCollection parameters, ObjectParameterKey<T> key) where T : class
        {
            return new ResourceParameterPin<T>(parameters, key);
        }
    }

    abstract class ParameterPin
    {
        public readonly ParameterKey ParameterKey;

        public ParameterPin(ParameterKey key)
        {
            ParameterKey = key;
        }

        public abstract void Update(ParameterCollection parameters);
    }

    class PermutationParameterPin<T> : ParameterPin, IVLPin
    {
        public readonly PermutationParameterKey<T> Key;
        readonly ParameterCollection parameters;
        PermutationParameter<T> accessor;

        public PermutationParameterPin(ParameterCollection parameters, PermutationParameterKey<T> key) : base(key)
        {
            this.parameters = parameters;
            this.Key = key;
            this.accessor = parameters.GetAccessor(key);
        }

        public override void Update(ParameterCollection parameters)
        {
            accessor = parameters.GetAccessor(Key);
        }

        public T Value
        {
            get => parameters.Get(accessor);
            set => parameters.Set(accessor, value);
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (T)value;
        }
    }

    class ConvertedPermutationParameterPin<TShader, TPin> : ParameterPin, IVLPin
    {
        public readonly PermutationParameterKey<TShader> Key;
        readonly ParameterCollection parameters;
        readonly ValueConverter<TPin, TShader> pinToShader;
        readonly ValueConverter<TShader, TPin> shaderToPin;
        PermutationParameter<TShader> accessor;

        public ConvertedPermutationParameterPin(ParameterCollection parameters, PermutationParameterKey<TShader> key) : base(key)
        {
            Key = key;
            this.parameters = parameters;
            this.accessor = parameters.GetAccessor(key);
            this.pinToShader = TypeConversions.GetConverter<TPin, TShader>();
            this.shaderToPin = TypeConversions.GetConverter<TShader, TPin>();
        }

        public override void Update(ParameterCollection parameters)
        {
            accessor = parameters.GetAccessor(Key);
        }

        public TShader ShaderValue => parameters.Get(accessor);

        public TPin Value
        {
            get
            {
                var value = parameters.Get(accessor);
                return shaderToPin(ref value);
            }
            set => parameters.Set(accessor, pinToShader(ref value));
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (TPin)value;
        }
    }

    class ValueParameterPin<T> : ParameterPin, IVLPin where T : struct
    {
        public readonly ValueParameterKey<T> Key;
        readonly ParameterCollection parameters;
        ValueParameter<T> accessor;

        public ValueParameterPin(ParameterCollection parameters, ValueParameterKey<T> key) : base(key)
        {
            this.parameters = parameters;
            this.Key = key;
            this.accessor = parameters.GetAccessor(key);
        }

        public override void Update(ParameterCollection parameters)
        {
            accessor = parameters.GetAccessor(Key);
        }

        public T Value
        {
            get => parameters.Get(accessor);
            set => parameters.Set(accessor, value);
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (T)value;
        }
    }

    class ConvertedValueParameterPin<TShader, TPin> : ParameterPin, IVLPin
        where TShader : struct
        where TPin : struct
    {
        public readonly ValueParameterKey<TShader> Key;
        readonly ParameterCollection parameters;
        readonly ValueConverter<TPin, TShader> pinToShader;
        readonly ValueConverter<TShader, TPin> shaderToPin;
        ValueParameter<TShader> accessor;

        public ConvertedValueParameterPin(ParameterCollection parameters, ValueParameterKey<TShader> key) : base(key)
        {
            Key = key;
            this.parameters = parameters;
            this.accessor = parameters.GetAccessor(key);
            this.pinToShader = TypeConversions.GetConverter<TPin, TShader>();
            this.shaderToPin = TypeConversions.GetConverter<TShader, TPin>();
        }

        public override void Update(ParameterCollection parameters)
        {
            accessor = parameters.GetAccessor(Key);
        }

        public TShader ShaderValue => parameters.Get(accessor);

        public TPin Value
        {
            get
            {
                var value = parameters.Get(accessor);
                return shaderToPin(ref value);
            }
            set => parameters.Set(accessor, pinToShader(ref value));
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (TPin)value;
        }
    }

    class ResourceParameterPin<T> : ParameterPin, IVLPin where T : class
    {
        public readonly ObjectParameterKey<T> Key;
        readonly ParameterCollection parameters;
        ObjectParameterAccessor<T> accessor;

        public ResourceParameterPin(ParameterCollection parameters, ObjectParameterKey<T> key) : base(key)
        {
            this.parameters = parameters;
            this.Key = key;
            this.accessor = parameters.GetAccessor(key);
        }

        public override void Update(ParameterCollection parameters)
        {
            accessor = parameters.GetAccessor(Key);
        }

        public T Value
        {
            get => parameters.Get(accessor);
            set => parameters.Set(accessor, value ?? Key.DefaultValueMetadataT?.DefaultValue);
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
