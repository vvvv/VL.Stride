using System;
using System.Collections.Generic;
using System.Reflection;
using VL.Core;
using Stride.Rendering;

namespace VL.Stride.EffectLib
{
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
            var shaderType = typeof(T);
            var pinType = TypeConversions.ShaderToPinTypeMap.ValueOrDefault(shaderType);
            if (pinType != null)
            {
                var convertedValueParameterPinType = typeof(ConvertedPermutationParameterPin<,>).MakeGenericType(shaderType, pinType);
                return Activator.CreateInstance(convertedValueParameterPinType, parameters, key) as IVLPin;
            }
            return new PermutationParameterPin<T>(parameters, key);
        }

        public static IVLPin CreateValuePin<T>(ParameterCollection parameters, ValueParameterKey<T> key, int count) where T : struct
        {
            var shaderType = typeof(T);
            var pinType = TypeConversions.ShaderToPinTypeMap.ValueOrDefault(shaderType);
            if (pinType != null)
            {
                if (count > 1)
                {
                    var parameterPinType = typeof(ConvertedArrayValueParameterPin<,>).MakeGenericType(shaderType, pinType);
                    return Activator.CreateInstance(parameterPinType, parameters, key) as IVLPin;
                }
                else
                {
                    var parameterPinType = typeof(ConvertedValueParameterPin<,>).MakeGenericType(shaderType, pinType);
                    return Activator.CreateInstance(parameterPinType, parameters, key) as IVLPin;
                }
            }
            else
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
        }

        public static IVLPin CreateResourcePin<T>(ParameterCollection parameters, ObjectParameterKey<T> key) where T : class
        {
            return new ResourceParameterPin<T>(parameters, key);
        }
    }

    abstract class ParameterPin
    {
        public ParameterCollection Parameters;
        public readonly ParameterKey ParameterKey;

        public ParameterPin(ParameterCollection parameters, ParameterKey key)
        {
            Parameters = parameters;
            ParameterKey = key;
        }

        public abstract void Update(ParameterCollection parameters);
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
        PermutationParameter<T> accessor;

        public PermutationParameterPin(ParameterCollection parameters, PermutationParameterKey<T> key) 
            : base(parameters, key)
        {
            this.Parameters = parameters;
            this.Key = key;
            this.accessor = parameters.GetAccessor(key);
        }

        public override void Update(ParameterCollection parameters)
        {
            Parameters = parameters;
            accessor = parameters.GetAccessor(Key);
        }

        public T Value
        {
            get => Parameters.Get(accessor);
            set
            {
                HasChanged = !comparer.Equals(value, Value);
                Parameters.Set(accessor, value);
            }
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
        readonly ValueConverter<TPin, TShader> pinToShader;
        readonly ValueConverter<TShader, TPin> shaderToPin;
        PermutationParameter<TShader> accessor;

        public ConvertedPermutationParameterPin(ParameterCollection parameters, PermutationParameterKey<TShader> key) 
            : base(parameters, key)
        {
            Key = key;
            this.accessor = parameters.GetAccessor(key);
            this.pinToShader = TypeConversions.GetConverter<TPin, TShader>();
            this.shaderToPin = TypeConversions.GetConverter<TShader, TPin>();
        }

        public override void Update(ParameterCollection parameters)
        {
            Parameters = parameters;
            accessor = parameters.GetAccessor(Key);
        }

        public TShader ShaderValue => Parameters.Get(accessor);

        public TPin Value
        {
            get
            {
                var value = Parameters.Get(accessor);
                return shaderToPin(ref value);
            }
            set => Parameters.Set(accessor, pinToShader(ref value));
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
        ValueParameter<T> accessor;

        public ValueParameterPin(ParameterCollection parameters, ValueParameterKey<T> key) 
            : base(parameters, key)
        {
            this.Key = key;
            this.accessor = parameters.GetAccessor(key);
        }

        public override void Update(ParameterCollection parameters)
        {
            Parameters = parameters;
            accessor = parameters.GetAccessor(Key);
        }

        public T Value
        {
            get => Parameters.Get(accessor);
            set => Parameters.Set(accessor, value);
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

        public override void Update(ParameterCollection parameters)
        {
            Parameters = parameters;
        }

        // TODO: Add overloads to Xenko wich take accessor instead of less optimal key
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

    class ConvertedValueParameterPin<TShader, TPin> : ParameterPin, IVLPin
        where TShader : struct
        where TPin : struct
    {
        public readonly ValueParameterKey<TShader> Key;
        readonly ValueConverter<TPin, TShader> pinToShader;
        readonly ValueConverter<TShader, TPin> shaderToPin;
        ValueParameter<TShader> accessor;

        public ConvertedValueParameterPin(ParameterCollection parameters, ValueParameterKey<TShader> key) 
            : base(parameters, key)
        {
            Key = key;
            this.accessor = parameters.GetAccessor(key);
            this.pinToShader = TypeConversions.GetConverter<TPin, TShader>();
            this.shaderToPin = TypeConversions.GetConverter<TShader, TPin>();
        }

        public override void Update(ParameterCollection parameters)
        {
            Parameters = parameters;
            accessor = parameters.GetAccessor(Key);
        }

        public TShader ShaderValue
        {
            get => Parameters.Get(accessor);
            set => Parameters.Set(accessor, value);
        }

        public TPin Value
        {
            get
            {
                var value = Parameters.Get(accessor);
                return shaderToPin(ref value);
            }
            set => Parameters.Set(accessor, pinToShader(ref value));
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (TPin)value;
        }
    }

    class ConvertedArrayValueParameterPin<TShader, TPin> : ParameterPin, IVLPin
        where TShader : struct
        where TPin : struct
    {
        public readonly ValueParameterKey<TShader> Key;
        readonly ValueConverter<TPin, TShader> pinToShader;
        readonly ValueConverter<TShader, TPin> shaderToPin;

        public ConvertedArrayValueParameterPin(ParameterCollection parameters, ValueParameterKey<TShader> key) 
            : base(parameters, key)
        {
            Key = key;
            this.pinToShader = TypeConversions.GetConverter<TPin, TShader>();
            this.shaderToPin = TypeConversions.GetConverter<TShader, TPin>();
        }

        public override void Update(ParameterCollection parameters)
        {
            Parameters = parameters;
        }

        public TShader[] ShaderValue => Parameters.GetValues(Key);

        public TPin[] Value
        {
            get
            {
                var shaderValue = ShaderValue;
                var value = new TPin[shaderValue.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    // Make a local copy because matrix gets transposed in place
                    var c = shaderValue[i];
                    value[i] = shaderToPin(ref c);
                }
                return value;
            }
            set
            {
                var shaderValue = new TShader[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    // Make a local copy because matrix gets transposed in place
                    var c = value[i];
                    shaderValue[i] = pinToShader(ref c);
                }
                if (shaderValue.Length > 0)
                    Parameters.Set(Key, shaderValue);
            }
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (TPin[])value;
        }
    }

    class ResourceParameterPin<T> : ParameterPin, IVLPin where T : class
    {
        public readonly ObjectParameterKey<T> Key;
        ObjectParameterAccessor<T> accessor;

        public ResourceParameterPin(ParameterCollection parameters, ObjectParameterKey<T> key) 
            : base(parameters, key)
        {
            this.Parameters = parameters;
            this.Key = key;
            this.accessor = parameters.GetAccessor(key);
        }

        public override void Update(ParameterCollection parameters)
        {
            Parameters = parameters;
            accessor = parameters.GetAccessor(Key);
        }

        public T Value
        {
            get => Parameters.Get(accessor);
            set => Parameters.Set(accessor, value ?? Key.DefaultValueMetadataT?.DefaultValue);
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
