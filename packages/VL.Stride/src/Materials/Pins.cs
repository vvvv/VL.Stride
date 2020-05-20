using FFmpeg.AutoGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VL.Core;

namespace VL.Stride.Materials
{
    abstract class PinDescription : IVLPinDescription
    {
        protected readonly MemberInfo property;

        public PinDescription(MemberInfo property, string name)
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

        public PinDescription(MemberInfo property, string name, T defaultValue) 
            : base(property, name)
        {
            this.defaultValue = defaultValue;
        }

        public override Type Type => typeof(T);
    }

    sealed class ClassPinDec<T> : PinDescription<T>
        where T : class
    {
        public ClassPinDec(MemberInfo property, string name, T defaultValue) : base(property, name, defaultValue)
        {
        }

        // Called at compile time, value must be serializable
        public override object DefaultValue
        {
            get
            {
                if (typeof(T) == typeof(string))
                    return defaultValue;
                return null;
            }
        }

        public override Pin CreatePin() => new ClassPin<T>(property, defaultValue);
    }

    sealed class StructPinDec<T> : PinDescription<T> 
        where T : struct
    {
        public StructPinDec(MemberInfo property, string name, T defaultValue) : base(property, name, defaultValue)
        {
        }

        // Called at compile time, value must be serializable
        public override object DefaultValue => defaultValue;

        public override Pin CreatePin() => new StructPin<T>(property, defaultValue);
    }

    sealed class ListPinDesc<TList, TElment> : PinDescription<TList>
        where TList : class, IList<TElment>
    {
        public ListPinDesc(MemberInfo property, string name, TList defaultValue) : base(property, name, defaultValue)
        {
        }

        public override object DefaultValue => null;

        public override Pin CreatePin() => new ListPin<TList, TElment>(property, defaultValue);
    }

    abstract class Pin : IVLPin
    {
        public bool IsChanged;

        public abstract object Value { get; set; }

        public abstract void ApplyValue(object instance);
    }

    abstract class Pin<T> : Pin
    {
        private static readonly EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

        protected readonly MemberInfo member;
        protected readonly bool isReadOnly;
        protected T value;

        public Pin(MemberInfo member, T value)
        {
            this.member = member;
            this.value = value;
            if (member is PropertyInfo property)
                this.isReadOnly = property.SetMethod is null || !property.SetMethod.IsPublic;
        }

        public override object Value
        {
            get => value;
            set
            {
                var valueT = (T)value;
                if (!ValueEquals(valueT, this.value))
                {
                    this.value = valueT;
                    IsChanged = true;
                }
            }
        }

        public override sealed void ApplyValue(object feature)
        {
            ApplyCore(feature, value);
            IsChanged = false;
        }

        protected virtual void ApplyCore(object feature, T value)
        {
            if (isReadOnly)
            {
                var dst = member.GetValue(feature);
                CopyProperties(value, dst);
            }
            else
            {
                member.SetValue(feature, value);
            }
        }

        protected virtual bool ValueEquals(T a, T b) => equalityComparer.Equals(a, b);

        private static void CopyProperties(object src, object dst)
        {
            foreach (var x in typeof(T).GetStrideProperties())
            {
                var p = x.Property as PropertyInfo;
                if (p.SetMethod != null && p.SetMethod.IsPublic && p.GetMethod != null && p.GetMethod.IsPublic)
                    p.SetValue(dst, p.GetValue(src));
            }
        }
    }

    sealed class ClassPin<T> : Pin<T>
        where T : class
    {
        readonly T defaultValue;

        public ClassPin(MemberInfo property, T value) : base(property, value)
        {
            defaultValue = value;
        }

        protected override void ApplyCore(object feature, T value)
        {
            base.ApplyCore(feature, value ?? defaultValue);
        }
    }

    sealed class StructPin<T> : Pin<T>
        where T : struct
    {
        public StructPin(MemberInfo property, T value) : base(property, value)
        {
        }

        protected override void ApplyCore(object feature, T value)
        {
            base.ApplyCore(feature, value);
        }
    }

    sealed class ListPin<T, TItem> : Pin<T>
        where T : class, IList<TItem>
    {
        readonly T defaultValue;

        public ListPin(MemberInfo property, T value) : base(property, value)
        {
            defaultValue = value;
        }

        protected override bool ValueEquals(T a, T b)
        {
            if (a is null)
                return b is null;
            if (b is null)
                return false;
            return a.SequenceEqual(b);
        }

        protected override void ApplyCore(object feature, T value)
        {
            var src = value ?? defaultValue;
            if (isReadOnly)
            {
                var dst = member.GetValue(feature) as IList<TItem>;
                dst.Clear();
                foreach (var item in src)
                    dst.Add(item);
            }
            else
            {
                base.ApplyCore(feature, src);
            }
        }
    }
}
