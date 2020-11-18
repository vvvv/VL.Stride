using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VL.Core;

namespace VL.Stride
{
    abstract class PinDescription : IVLPinDescription, IInfo
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

        public string Summary => property.GetSummary();

        public string Remarks => property.GetRemarks();

        public abstract Pin<TInstance> CreatePin<TInstance>(StrideNode node) where TInstance : new();
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

        public override Pin<TInstance> CreatePin<TInstance>(StrideNode node) => new ClassPin<TInstance, T>(node, property, defaultValue);
    }

    sealed class StructPinDec<T> : PinDescription<T> 
        where T : struct
    {
        public StructPinDec(MemberInfo property, string name, T defaultValue) : base(property, name, defaultValue)
        {
        }

        // Called at compile time, value must be serializable
        public override object DefaultValue => defaultValue;

        public override Pin<TInstance> CreatePin<TInstance>(StrideNode node) => new StructPin<TInstance, T>(node, property, defaultValue);
    }

    sealed class ListPinDesc<TList, TElement> : PinDescription<TList>
        where TList : class, IList<TElement>
    {
        public ListPinDesc(MemberInfo property, string name, TList defaultValue) : base(property, name, defaultValue)
        {
        }

        public override object DefaultValue => null;

        public override Pin<TInstance> CreatePin<TInstance>(StrideNode node) => new ListPin<TInstance, TList, TElement>(node, property, defaultValue);
    }

    abstract class Pin<TInstance> : IVLPin where TInstance : new()
    {
        protected readonly StrideNode node;

        public Pin(StrideNode node)
        {
            this.node = node;
        }

        public abstract object Value { get; set; }

        public abstract void ApplyValue(TInstance instance);
    }

    abstract class Pin<TInstance, TValue> : Pin<TInstance>, IVLPin<TValue> where TInstance : new()
    {
        private static readonly EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;

        protected readonly Func<TInstance, TValue> getter;
        protected readonly Action<TInstance, TValue> setter;
        protected readonly bool isReadOnly;
        protected TValue value;

        public Pin(StrideNode node, MemberInfo member, TValue value) : base(node)
        {
            getter = member.BuildGetter<TInstance, TValue>();

            if (member is PropertyInfo property)
                isReadOnly = property.SetMethod is null || !property.SetMethod.IsPublic;

            if (!isReadOnly)
                setter = member.BuildSetter<TInstance, TValue>();

            this.value = value;
        }

        public override object Value
        {
            get => ((IVLPin<TValue>)this).Value;
            set => ((IVLPin<TValue>)this).Value = (TValue)value;
        }

        TValue IVLPin<TValue>.Value
        {
            get => value;
            set
            {
                var valueT = value;
                if (!ValueEquals(valueT, this.value))
                {
                    this.value = valueT;
                    node.needsUpdate = true;
                }
            }
        }

        public override sealed void ApplyValue(TInstance instance)
        {
            ApplyCore(instance, value);
        }

        protected virtual void ApplyCore(TInstance instance, TValue value)
        {
            if (isReadOnly)
            {
                var dst = getter(instance);
                CopyProperties(value, dst);
            }
            else
            {
                setter(instance, value);
            }
        }

        protected virtual bool ValueEquals(TValue a, TValue b) => equalityComparer.Equals(a, b);

        private static void CopyProperties(object src, object dst)
        {
            foreach (var x in typeof(TValue).GetStrideProperties())
            {
                var p = x.Property as PropertyInfo;
                if (p.SetMethod != null && p.SetMethod.IsPublic && p.GetMethod != null && p.GetMethod.IsPublic)
                    p.SetValue(dst, p.GetValue(src));
            }
        }
    }

    sealed class ClassPin<TInstance, TValue> : Pin<TInstance, TValue> where TInstance : new()
        where TValue : class
    {
        readonly TValue defaultValue;

        public ClassPin(StrideNode node, MemberInfo property, TValue value) : base(node, property, value)
        {
            defaultValue = value;
        }

        protected override void ApplyCore(TInstance instance, TValue value)
        {
            base.ApplyCore(instance, value ?? defaultValue);
        }
    }

    sealed class StructPin<TInstance, TValue> : Pin<TInstance, TValue> where TInstance : new()
        where TValue : struct
    {
        public StructPin(StrideNode node, MemberInfo property, TValue value) : base(node, property, value)
        {
        }

        protected override void ApplyCore(TInstance instance, TValue value)
        {
            base.ApplyCore(instance, value);
        }
    }

    sealed class ListPin<TInstance, TList, TItem> : Pin<TInstance, TList> where TInstance : new()
        where TList : class, IList<TItem>
    {
        readonly TList defaultValue;

        public ListPin(StrideNode node, MemberInfo property, TList value) : base(node, property, value)
        {
            defaultValue = value;
        }

        protected override bool ValueEquals(TList a, TList b)
        {
            if (a is null)
                return b is null;
            if (b is null)
                return false;
            return a.SequenceEqual(b);
        }

        protected override void ApplyCore(TInstance instance, TList value)
        {
            var src = value ?? defaultValue;
            if (isReadOnly)
            {
                var dst = getter(instance) as IList<TItem>;
                dst.Clear();
                foreach (var item in src)
                    dst.Add(item);
            }
            else
            {
                base.ApplyCore(instance, src);
            }
        }
    }
}
