using Stride.Core.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using VL.Core;
using VL.Core.Diagnostics;

namespace VL.Stride
{
    class StrideNodeDesc<TMaterial> : IVLNodeDescription, IEnumerable
        where TMaterial : new()
    {
        private List<PinDescription> inputs;
        private List<StateOutput> outputs;
        internal Type stateOutputType;

        public StrideNodeDesc(IVLNodeDescriptionFactory factory, string name = default, string category = default, Type stateOutputType = default, bool isFragmented = false)
        {
            Factory = factory;
            Name = name ?? typeof(TMaterial).Name;
            Category = category ?? string.Empty;
            Fragmented = isFragmented;
            this.stateOutputType = stateOutputType ?? typeof(TMaterial);
        }

        public bool CopyOnWrite { get; set; } = true;

        public IVLNodeDescriptionFactory Factory { get; }

        public string Name { get; }

        public string Category { get; }

        public bool Fragmented { get; }

        public IReadOnlyList<IVLPinDescription> Inputs
        {
            get
            {
                return inputs ?? (inputs = Compute().ToList());

                IEnumerable<PinDescription> Compute()
                {
                    var categoryOrdering = typeof(TMaterial).GetCustomAttributes<CategoryOrderAttribute>()
                        .ToDictionary(a => a.Name, a => a.Order);

                    var properties = typeof(TMaterial).GetStrideProperties()
                        .GroupBy(p => p.Category)
                        .OrderBy(g => g.Key != null ? categoryOrdering.ValueOrDefault(g.Key, 0) : 0)
                        .SelectMany(g => g.OrderBy(p => p.Order).ThenBy(p => p.Name));
                    var instance = new TMaterial();
                    foreach (var p in properties)
                    {
                        var property = p.Property;
                        var type = property.GetPropertyType();
                        var pinType = GetPinType(type);

                        var defaultValue = property.GetValue(instance);
                        var defaultValueProperty = property.GetCustomAttribute<DefaultValueAttribute>();
                        if (defaultValueProperty != null && defaultValueProperty.Value != null && defaultValueProperty.Value.GetType() == type)
                            defaultValue = defaultValueProperty.Value;

                        var name = p.Name;
                        // Prepend the category to the name (if not already done so)
                        var category = p.Category;
                        if (category != null)
                        {
                            if (category == "Metal Flakes")
                                category = "Metal Flake";

                            if (!name.StartsWith(category))
                                name = $"{category} {name}";
                        }
                        yield return (PinDescription)Activator.CreateInstance(pinType, property, name, defaultValue);
                    }
                }
            }
        }

        static Type GetPinType(Type type)
        {
            if (type.IsValueType)
                return typeof(StructPinDec<>).MakeGenericType(type);
            if (TryGetElementType(type, out var elementType))
                return typeof(ListPinDesc<,>).MakeGenericType(type, elementType);
            return typeof(ClassPinDec<>).MakeGenericType(type);
        }

        static bool TryGetElementType(Type type, out Type elementType)
        {
            var typeArgs = type.GenericTypeArguments;
            if (typeArgs.Length == 1)
            {
                elementType = typeArgs[0];
                return typeof(IList<>).MakeGenericType(elementType).IsAssignableFrom(type);
            }
            if (type.BaseType != null)
            {
                return TryGetElementType(type.BaseType, out elementType);
            }
            else
            {
                elementType = default;
                return false;
            }
        }

        public IReadOnlyList<IVLPinDescription> Outputs
        {
            get
            {
                return outputs ?? (outputs = Compute().ToList());

                IEnumerable<StateOutput> Compute()
                {
                    yield return new StateOutput(stateOutputType);
                }
            }
        }

        public IEnumerable<Message> Messages => Enumerable.Empty<Message>();

        public IVLNode CreateInstance(NodeContext context)
        {
            return new StrideNode<TMaterial>(context, this);
        }

        public bool OpenEditor()
        {
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        class StateOutput : IVLPinDescription
        {
            public StateOutput(Type type)
            {
                Type = type;
            }

            public string Name => "Output";

            public Type Type { get; }

            public object DefaultValue => null;
        }
    }
}
