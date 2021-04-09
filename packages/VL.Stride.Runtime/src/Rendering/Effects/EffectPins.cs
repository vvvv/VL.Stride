using System;
using System.Collections.Generic;
using System.Reflection;
using VL.Core;
using Stride.Rendering;
using Stride.Core.Mathematics;
using Stride.Graphics;
using System.Runtime.CompilerServices;
using Stride.Rendering.Materials;
using VL.Stride.Shaders.ShaderFX;
using VL.Stride.Shaders.ShaderFX.Control;
using Stride.Shaders;
using Stride.Rendering.Materials.ComputeColors;
using System.Linq;

namespace VL.Stride.Rendering
{
    abstract class EffectPinDescription : IVLPinDescription, IInfo, IVLPinDescriptionWithVisibility
    {
        public abstract string Name { get; }
        public abstract Type Type { get; }
        public abstract object DefaultValueBoxed { get; }

        public string Summary { get; set; }
        public string Remarks { get; set; }

        public bool IsVisible { get; set; } = true;

        object IVLPinDescription.DefaultValue => DefaultValueBoxed;

        public abstract IVLPin CreatePin(GraphicsDevice graphicsDevice, ParameterCollection parameters);
    }

    class PinDescription<T> : EffectPinDescription
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

        public override IVLPin CreatePin(GraphicsDevice graphicsDevice, ParameterCollection parameters) => new Pin<T>(Name, DefaultValue);
    }

    class ParameterPinDescription : EffectPinDescription
    {
        public readonly ParameterKey Key;
        public readonly int Count;
        public readonly bool IsPermutationKey;

        public ParameterPinDescription(HashSet<string> usedNames, ParameterKey key, int count = 1, object defaultValue = null, bool isPermutationKey = false, string name = null, Type typeInPatch = null)
        {
            Key = key;
            IsPermutationKey = isPermutationKey;
            Count = count;
            Name = name ?? key.GetPinName(usedNames);
            var elementType = typeInPatch ?? key.PropertyType;
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
        public override IVLPin CreatePin(GraphicsDevice graphicsDevice, ParameterCollection parameters) => EffectPins.CreatePin(graphicsDevice, parameters, Key, Count, IsPermutationKey, DefaultValueBoxed, Type);
    }

    static class EffectPins
    {
        public static IVLPin CreatePin(GraphicsDevice graphicsDevice, ParameterCollection parameters, ParameterKey key, int count, bool isPermutationKey, object value, Type typeInPatch)
        {
            if (key is ValueParameterKey<Color4> colorKey)
                return new ColorParameterPin(parameters, colorKey, graphicsDevice.ColorSpace, (Color4)value);

            var argument = key.GetType().GetGenericArguments()[0];

            if (typeInPatch.IsEnum)
            {
                var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateEnumPin), BindingFlags.Static | BindingFlags.Public);
                return createPinMethod.MakeGenericMethod(argument, typeInPatch).Invoke(null, new object[] { parameters, key, value }) as IVLPin;
            }

            if (argument == typeof(ShaderSource))
            {
                if (typeInPatch.IsGenericType && typeInPatch.GetGenericTypeDefinition() == typeof(SetVar<>))
                {
                    var typeParam = typeInPatch.GetGenericArguments()[0];
                    var createSetVarMethod = typeof(ShaderFXUtils).GetMethods().First(m => m.Name == nameof(ShaderFXUtils.DeclAndSetVar) && m.GetParameters().Length == 1);
                    value = createSetVarMethod.MakeGenericMethod(typeParam).Invoke(null, new[] { value });
                    var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateGPUValueSinkPin), BindingFlags.Static | BindingFlags.Public);
                    return createPinMethod.MakeGenericMethod(typeParam).Invoke(null, new[] { parameters, key, value }) as IVLPin;
                }
                else
                {
                    var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateShaderFXPin), BindingFlags.Static | BindingFlags.Public);
                    return createPinMethod.MakeGenericMethod(typeof(IComputeNode)).Invoke(null, new object[] { parameters, key, value }) as IVLPin;
                }
            }

            if (isPermutationKey)
            {
                var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreatePermutationPin), BindingFlags.Static | BindingFlags.Public);
                return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key, value }) as IVLPin;
            }
            else if (argument.IsValueType)
            {
                if (count > 1)
                {
                    var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateArrayPin), BindingFlags.Static | BindingFlags.Public);
                    return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key, value }) as IVLPin;
                }
                else
                {
                    var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateValuePin), BindingFlags.Static | BindingFlags.Public);
                    return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key, value }) as IVLPin;
                }
            }
            else
            {
                var createPinMethod = typeof(EffectPins).GetMethod(nameof(CreateResourcePin), BindingFlags.Static | BindingFlags.Public);
                return createPinMethod.MakeGenericMethod(argument).Invoke(null, new object[] { parameters, key }) as IVLPin;
            }
        }

        public static IVLPin CreatePermutationPin<T>(ParameterCollection parameters, PermutationParameterKey<T> key, T value)
        {
            return new PermutationParameterPin<T>(parameters, key, value);
        }

        public static IVLPin CreateEnumPin<T, TEnum>(ParameterCollection parameters, ValueParameterKey<T> key, TEnum value) where T : unmanaged where TEnum : unmanaged
        {
            return new EnumParameterPin<T, TEnum>(parameters, key, value);
        }

        public static IVLPin CreateShaderFXPin<T>(ParameterCollection parameters, PermutationParameterKey<ShaderSource> key, T value) where T : class, IComputeNode
        {
            return new ShaderFXPin<T>(parameters, key, value);
        }

        public static IVLPin CreateGPUValueSinkPin<T>(ParameterCollection parameters, PermutationParameterKey<ShaderSource> key, SetVar<T> value)
        {
            return new GPUValueSinkPin<T>(parameters, key, value);
        }

        public static IVLPin CreateValuePin<T>(ParameterCollection parameters, ValueParameterKey<T> key, T value) where T : struct
        {
            return new ValueParameterPin<T>(parameters, key, value);
        }

        public static IVLPin CreateArrayPin<T>(ParameterCollection parameters, ValueParameterKey<T> key, T[] value) where T : struct
        {
            return new ArrayValueParameterPin<T>(parameters, key, value);
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

    class PermutationParameterPin<T> : PermutationParameterPin, IVLPin<T>
    {
        public readonly PermutationParameterKey<T> Key;
        readonly EqualityComparer<T> comparer = EqualityComparer<T>.Default;

        public PermutationParameterPin(ParameterCollection parameters, PermutationParameterKey<T> key, T value) 
            : base(parameters, key)
        {
            this.Key = key;
            this.Value = value;
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

    class ValueParameterPin<T> : ParameterPin, IVLPin<T> where T : struct
    {
        public readonly ValueParameterKey<T> Key;

        public ValueParameterPin(ParameterCollection parameters, ValueParameterKey<T> key, T value)
            : base(parameters, key)
        {
            this.Key = key;
            this.Value = value;
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

    class EnumParameterPin<T, TEnum> : ParameterPin, IVLPin<TEnum> where T : unmanaged where TEnum : unmanaged
    {
        public readonly ValueParameterKey<T> Key;

        public EnumParameterPin(ParameterCollection parameters, ValueParameterKey<T> key, TEnum value)
            : base(parameters, key)
        {
            this.Key = key;
            this.Value = value;
        }

        public TEnum Value
        {
            get
            {
                T val = Parameters.Get(Key);
                return Unsafe.As<T, TEnum>(ref val);
            }
            set => Parameters.Set(Key, Unsafe.As<TEnum, T>(ref value));
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (TEnum)value;
        }
    }

    class ColorParameterPin : ParameterPin, IVLPin<Color4>
    {
        public readonly ValueParameterKey<Color4> Key;
        public readonly ColorSpace ColorSpace;

        public ColorParameterPin(ParameterCollection parameters, ValueParameterKey<Color4> key, ColorSpace colorSpace, Color4 value)
            : base(parameters, key)
        {
            this.Key = key;
            this.ColorSpace = colorSpace;
            this.Value = value;
        }

        public Color4 Value
        {
            get => Parameters.Get(Key);
            set => Parameters.Set(Key, value.ToColorSpace(ColorSpace));
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (Color4)value;
        }
    }

    class ArrayValueParameterPin<T> : ParameterPin, IVLPin<T[]> where T : struct
    {
        public readonly ValueParameterKey<T> Key;

        public ArrayValueParameterPin(ParameterCollection parameters, ValueParameterKey<T> key, T[] value) 
            : base(parameters, key)
        {
            this.Key = key;
            this.Value = value;
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

    class ResourceParameterPin<T> : ParameterPin, IVLPin<T> where T : class
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

    abstract class ShaderFXPin : ParameterPin
    {
        public readonly PermutationParameterKey<ShaderSource> Key;

        public ShaderFXPin(ParameterCollection parameters, PermutationParameterKey<ShaderSource> key)
            : base(parameters, key)
        {
            Key = key;
        }

        public bool ShaderSourceChanged { get; set; } = true;

        public void GenerateAndSetShaderSource(ShaderMixinSource mixin, ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSource(context, baseKeys);
            Parameters.Set(Key, shaderSource);
            mixin.Compositions[Key.Name] = shaderSource;
            ShaderSourceChanged = false;
        }

        protected abstract ShaderSource GetShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys);

        public abstract IComputeNode GetValueOrDefaultValue();
    }

    class ShaderFXPin<TShaderClass> : ShaderFXPin, IVLPin<TShaderClass> where TShaderClass : class, IComputeNode
    {
        private TShaderClass internalValue;
        protected TShaderClass defaultValue;

        public ShaderFXPin(ParameterCollection parameters, PermutationParameterKey<ShaderSource> key, TShaderClass value)
            : base(parameters, key)
        {
            Value = value;
            defaultValue = value;
        }

        public TShaderClass Value
        {
            get => internalValue;
            set
            {
                if (internalValue != value)
                {
                    internalValue = value;
                    ShaderSourceChanged = true;
                }
            }
        }

        public override IComputeNode GetValueOrDefaultValue()
        {
            return internalValue ?? defaultValue;
        }

        object IVLPin.Value
        {
            get => Value;
            set => Value = (TShaderClass)value;
        }

        protected override ShaderSource GetShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            return Value?.GenerateShaderSource(context, baseKeys) ?? defaultValue?.GenerateShaderSource(context, baseKeys);
        }
    }

    sealed class GPUValueSinkPin<T> : ShaderFXPin<SetVar<T>>
    {
        public GPUValueSinkPin(ParameterCollection parameters, PermutationParameterKey<ShaderSource> key, SetVar<T> value) 
            : base(parameters, key, value)
        {
        }

        protected override ShaderSource GetShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            if (Value != null)
            {
                var input = Value;
                var getter = ShaderFXUtils.GetVarValue(input);
                var graph = ShaderGraph.BuildFinalShaderGraph(getter);
                var finalVar = new Do<T>(graph, getter);
                return finalVar.GenerateShaderSource(context, baseKeys);
            }

            return base.GetShaderSource(context, baseKeys);
        }
    }

    abstract class Pin : IVLPin
    {
        public Pin(string name)
        {
            Name = name;
        }

        public abstract object Value { get; set; }

        public string Name { get; }
    }

    class Pin<T> : Pin, IVLPin<T>
    {
        public Pin(string name, T initialValue) : base(name)
        {
            Value = initialValue;
        }

        T IVLPin<T>.Value { get; set; }

        public sealed override object Value
        {
            get => ((IVLPin<T>)this).Value;
            set => ((IVLPin<T>)this).Value = (T)value;
        }
    }
}
