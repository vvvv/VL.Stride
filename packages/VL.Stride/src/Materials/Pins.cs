using System;
using System.Collections.Generic;
using System.Reflection;
using VL.Core;

namespace VL.Stride.Materials
{
    abstract class PinDescription : IVLPinDescription
    {
        protected readonly PropertyInfo property;

        public PinDescription(PropertyInfo property, string name)
        {
            this.property = property;
            Name = name;
        }

        public string Name { get; }

        public abstract Type Type { get; }

        public abstract object DefaultValue { get; }

        public abstract Pin CreatePin();
    }

    abstract class PinDescription<T> : PinDescription
    {
        protected readonly T defaultValue;

        public PinDescription(PropertyInfo property, string name, T defaultValue) 
            : base(property, name)
        {
            this.defaultValue = defaultValue;
        }

        public override Type Type => typeof(T);
    }

    sealed class ClassPinDec<T> : PinDescription<T>
        where T : class
    {
        public ClassPinDec(PropertyInfo property, string name, T defaultValue) : base(property, name, defaultValue)
        {
        }

        // Called at compile time, value must be serializable
        public override object DefaultValue => null;

        public override Pin CreatePin() => new ClassPin<T>(property, defaultValue);
    }

    sealed class StructPinDec<T> : PinDescription<T> 
        where T : struct
    {
        public StructPinDec(PropertyInfo property, string name, T defaultValue) : base(property, name, defaultValue)
        {
        }

        // Called at compile time, value must be serializable
        public override object DefaultValue => defaultValue;

        public override Pin CreatePin() => new StructPin<T>(property, defaultValue);
    }

    abstract class Pin : IVLPin
    {
        public bool IsChanged;

        public abstract object Value { get; set; }

        public abstract void Apply(object feature);
    }

    abstract class Pin<T> : Pin
    {
        private static readonly EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

        protected readonly PropertyInfo property;
        protected T value;

        public Pin(PropertyInfo property, T value)
        {
            this.property = property;
            this.value = value;
        }

        public override object Value
        {
            get => value;
            set
            {
                var valueT = (T)value;
                if (!equalityComparer.Equals(valueT, this.value))
                {
                    this.value = valueT;
                    IsChanged = true;
                }
            }
        }
    }

    sealed class ClassPin<T> : Pin<T>
        where T : class
    {
        readonly T defaultValue;

        public ClassPin(PropertyInfo property, T value) : base(property, value)
        {
            defaultValue = value;
        }

        public override void Apply(object feature)
        {
            property.SetValue(feature, value ?? defaultValue);
            IsChanged = false;
        }
    }

    sealed class StructPin<T> : Pin<T>
        where T : struct
    {
        public StructPin(PropertyInfo property, T value) : base(property, value)
        {
        }

        public override void Apply(object feature)
        {
            property.SetValue(feature, value);
            IsChanged = false;
        }
    }
}
