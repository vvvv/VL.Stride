using Stride.Core;
using Stride.Core.Annotations;
using Stride.Rendering.Materials;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VL.Core;
using VL.Core.Diagnostics;

namespace VL.Stride.Materials
{
    class MaterialNodeDescription<TMaterial> : IVLNodeDescription
        where TMaterial : new()
    {
        private List<PinDescription> inputs;
        private List<StateOutput> outputs;
        internal Type stateOutputType;

        public MaterialNodeDescription(IVLNodeDescriptionFactory factory, string name, string category, Type stateOutputType = default)
        {
            Factory = factory;
            Name = name;
            Category = category;
            this.stateOutputType = stateOutputType ?? typeof(TMaterial);
        }

        public IVLNodeDescriptionFactory Factory { get; }

        public string Name { get; }

        public string Category { get; }

        public IReadOnlyList<IVLPinDescription> Inputs
        {
            get
            {
                return inputs ?? (inputs = Compute().ToList());

                IEnumerable<PinDescription> Compute()
                {
                    var categoryOrdering = typeof(TMaterial).GetCustomAttributes<CategoryOrderAttribute>()
                        .ToDictionary(a => a.Name, a => a.Order);

                    var properties = typeof(TMaterial)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.GetMethod != null && p.GetMethod.IsPublic && p.SetMethod != null && p.SetMethod.IsPublic)
                        .Select(p =>
                        {
                            // At least one of the following attributes must be set
                            var display = p.GetCustomAttribute<DisplayAttribute>();
                            var dataMember = p.GetCustomAttribute<DataMemberAttribute>();
                            var customSerializer = p.GetCustomAttribute<DataMemberCustomSerializerAttribute>();

                            if (display is null && dataMember is null && customSerializer is null)
                                return default;

                            return new
                            {
                                Property = p,
                                Order = dataMember?.Order,
                                Name = display?.Name?.UpperCaseAfterSpace() ?? p.Name.InsertSpaces(),
                                Category = display?.Category?.UpperCaseAfterSpace()
                            };
                        })
                        .Where(p => p != null)
                        .GroupBy(p => p.Category)
                        .OrderBy(g => g.Key != null ? categoryOrdering.ValueOrDefault(g.Key, 0) : 0)
                        .SelectMany(g => g.OrderBy(p => p.Order).ThenBy(p => p.Name));
                    var instance = new TMaterial();
                    foreach (var p in properties)
                    {
                        var property = p.Property;
                        var type = property.PropertyType;
                        var genericPinType = type.IsValueType ? typeof(StructPinDec<>) : typeof(ClassPinDec<>);
                        var pinType = genericPinType.MakeGenericType(type);
                        var defaultValue = property.GetValue(instance);
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
            return new MaterialNode<TMaterial>(context, this);
        }

        public bool OpenEditor()
        {
            return false;
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
