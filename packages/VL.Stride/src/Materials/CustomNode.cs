using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VL.Core;
using VL.Core.Diagnostics;

namespace VL.Stride.Materials
{
    class CustomNodeDesc<TInstance> : IVLNodeDescription
        where TInstance : new()
    {
        readonly List<CustomPinDesc> inputs = new List<CustomPinDesc>();
        readonly List<CustomPinDesc> outputs = new List<CustomPinDesc>();

        public CustomNodeDesc(IVLNodeDescriptionFactory factory, string name = default, string category = default)
        {
            Factory = factory;
            Name = name ?? typeof(TInstance).Name;
            Category = category ?? string.Empty;
        }

        public IVLNodeDescriptionFactory Factory { get; }

        public string Name { get; }

        public string Category { get; }

        public IReadOnlyList<IVLPinDescription> Inputs => inputs;

        public IReadOnlyList<IVLPinDescription> Outputs => outputs;

        public IEnumerable<Message> Messages => Enumerable.Empty<Message>();

        public IVLNode CreateInstance(NodeContext context)
        {
            var instance = new TInstance();
            return new Node(context)
            {
                NodeDescription = this,
                Inputs = inputs.Select(p => p.CreatePin(instance)).ToArray(),
                Outputs = outputs.Select(p => p.CreatePin(instance)).ToArray(),
            };
        }

        public bool OpenEditor()
        {
            return false;
        }

        public CustomNodeDesc<TInstance> AddInput<T>(string name, Action<TInstance, T> setter, T defaultValue = default)
        {
            inputs.Add(new CustomPinDesc()
            {
                Name = name.InsertSpaces(),
                Type = typeof(T),
                DefaultValue = defaultValue,
                CreatePin = (instance) => new Pin()
                {
                    Setter = v => setter(instance, (T)v)
                }
            });
            return this;
        }

        public CustomNodeDesc<TInstance> AddOutput<T>(string name, Func<TInstance, T> getter)
        {
            outputs.Add(new CustomPinDesc()
            {
                Name = name.InsertSpaces(),
                Type = typeof(T),
                CreatePin = (instance) => new Pin()
                {
                    Getter = () => getter(instance)
                }
            });
            return this;
        }

        class CustomPinDesc : IVLPinDescription
        {
            public string Name { get; set; }

            public Type Type { get; set; }

            public object DefaultValue { get; set; }

            public Func<TInstance, IVLPin> CreatePin { get; set; }
        }

        class Pin : IVLPin
        {
            public Action<object> Setter;
            public Func<object> Getter;

            public object Value
            {
                get => Getter();
                set => Setter(value);
            }
        }

        class Node : VLObject, IVLNode
        {
            public Node(NodeContext nodeContext) : base(nodeContext)
            {
            }

            public IVLNodeDescription NodeDescription { get; set; }

            public IVLPin[] Inputs { get; set; }

            public IVLPin[] Outputs { get; set; }

            public void Dispose()
            {
            }

            public void Update()
            {
            }
        }
    }
}
