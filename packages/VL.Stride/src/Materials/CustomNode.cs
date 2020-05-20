using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VL.Core;
using VL.Core.Diagnostics;
using VL.Lib.Primitive.Object;

namespace VL.Stride.Materials
{
    class CustomNodeDesc<TInstance> : IVLNodeDescription
        where TInstance : new()
    {
        readonly List<CustomPinDesc> inputs = new List<CustomPinDesc>();
        readonly List<CustomPinDesc> outputs = new List<CustomPinDesc>();

        public CustomNodeDesc(IVLNodeDescriptionFactory factory, string name = default, string category = default, bool copyOnWrite = true)
        {
            Factory = factory;
            Name = name ?? typeof(TInstance).Name;
            Category = category ?? string.Empty;
            CopyOnWrite = copyOnWrite;

            AddOutput("Output", x => x);
        }

        public IVLNodeDescriptionFactory Factory { get; }

        public string Name { get; }

        public string Category { get; }

        public bool CopyOnWrite { get; }

        public IReadOnlyList<IVLPinDescription> Inputs => inputs;

        public IReadOnlyList<IVLPinDescription> Outputs => outputs;

        public IEnumerable<Message> Messages => Enumerable.Empty<Message>();

        public IVLNode CreateInstance(NodeContext context)
        {
            var instance = new TInstance();
            var inputs = this.inputs.Select(p => p.CreatePin()).ToArray();
            var outputs = this.outputs.Select(p => p.CreatePin()).ToArray();
            outputs[0].Value = instance;
            return new Node(context)
            {
                NodeDescription = this,
                Inputs = inputs,
                Outputs = outputs,
                updateAction = () =>
                {
                    if (inputs.Any(p => p.IsChanged(instance)))
                    {
                        if (CopyOnWrite)
                        {
                            // TODO: Causes render pipeline to crash
                            //if (instance is IDisposable disposable)
                            //    disposable.Dispose();

                            instance = new TInstance();
                        }

                        foreach (var input in inputs)
                            input.Apply(instance);
                        outputs[0].Value = instance;
                    }
                },
                disposeAction = () =>
                {
                    // TODO: Causes render pipeline to crash
                    //if (instance is IDisposable disposable)
                    //    disposable.Dispose();
                    if (!CopyOnWrite && instance is IDisposable disposable)
                        disposable.Dispose();
                }
            };
        }

        public bool OpenEditor()
        {
            return false;
        }

        public CustomNodeDesc<TInstance> AddInput<T>(string name, Func<TInstance, T> getter, Action<TInstance, T> setter, T defaultValue = default, Func<T, T, bool> equals = default)
        {
            inputs.Add(new CustomPinDesc()
            {
                Name = name.InsertSpaces(),
                Type = typeof(T),
                DefaultValue = defaultValue,
                CreatePin = () => new Pin<T>()
                {
                    getter = getter,
                    setter = setter,
                    equals = equals
                }
            });
            return this;
        }

        public CustomNodeDesc<TInstance> AddListInput<T>(string name, Func<TInstance, IList<T>> getter)
        {
            return AddInput<IReadOnlyList<T>>(name, 
                equals: (a, b) => a.SequenceEqual(b.Where(x => x != null)),
                getter: instance => (IReadOnlyList<T>)getter(instance),
                setter: (x, v) =>
                {
                    var currentItems = getter(x);
                    var newItems = v.Where(i => i != null);
                    if (!currentItems.SequenceEqual(newItems))
                    {
                        currentItems.Clear();
                        foreach (var item in newItems)
                            currentItems.Add(item);
                    }
                });
        }

        public CustomNodeDesc<TInstance> AddOutput<T>(string name, Func<TInstance, T> getter)
        {
            outputs.Add(new CustomPinDesc()
            {
                Name = name.InsertSpaces(),
                Type = typeof(T),
                CreatePin = () => new Pin<T>()
                {
                    getter = getter
                }
            });
            return this;
        }

        class CustomPinDesc : IVLPinDescription
        {
            public string Name { get; set; }

            public Type Type { get; set; }

            public object DefaultValue { get; set; }

            public Func<Pin> CreatePin { get; set; }
        }

        abstract class Pin : IVLPin
        {
            public abstract object Value { get; set; }

            public abstract bool IsChanged(TInstance instance);

            public abstract void Apply(TInstance instance);
        }

        class Pin<T> : Pin, IVLPin
        {
            static readonly EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

            public Func<TInstance, T> getter;
            public Action<TInstance, T> setter;
            public Func<T, T, bool> equals;
            public T value;

            public override object Value 
            { 
                get => this.value; 
                set => this.value = (T)value; 
            }

            public override bool IsChanged(TInstance instance)
            {
                var other = getter(instance);
                if (equals != null)
                    return !equals(other, value);
                else
                    return !equalityComparer.Equals(other, value);
            }

            public override void Apply(TInstance instance)
            {
                setter(instance, (T)Value);
            }
        }

        class Node : VLObject, IVLNode
        {
            public Action updateAction;
            public Action disposeAction;

            public Node(NodeContext nodeContext) : base(nodeContext)
            {
            }

            public IVLNodeDescription NodeDescription { get; set; }

            public IVLPin[] Inputs { get; set; }

            public IVLPin[] Outputs { get; set; }

            public void Dispose() => disposeAction?.Invoke();

            public void Update() => updateAction?.Invoke();
        }
    }
}
