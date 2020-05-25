using System;
using System.Collections.Generic;
using System.Linq;
using VL.Core;
using VL.Core.Diagnostics;

namespace VL.Stride
{
    static class FactoryExtensions
    {
        public static CustomNodeDesc<TNew> Create<TNew>(this IVLNodeDescriptionFactory factory, 
            string name = default, 
            string category = default, 
            bool copyOnWrite = true,
            Action<TNew> init = default) 
            where TNew : new()
        {
            return new CustomNodeDesc<TNew>(factory, 
                ctor: ctx =>
                {
                    var instance = new TNew();
                    init?.Invoke(instance);
                    return (instance, default);
                }, 
                name: name, 
                category: category,
                copyOnWrite: copyOnWrite);
        }
    }

    class CustomNodeDesc<TInstance> : IVLNodeDescription
    {
        readonly List<CustomPinDesc> inputs = new List<CustomPinDesc>();
        readonly List<CustomPinDesc> outputs = new List<CustomPinDesc>();
        readonly Func<NodeContext, (TInstance, Action)> ctor;

        public CustomNodeDesc(IVLNodeDescriptionFactory factory, Func<NodeContext, (TInstance, Action)> ctor, string name = default, string category = default, bool copyOnWrite = true)
        {
            Factory = factory;
            this.ctor = ctor;
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
            var (instance, onDispose) = ctor(context);
            var inputs = this.inputs.Select(p => p.CreatePin()).ToArray();
            var outputs = this.outputs.Select(p => p.CreatePin()).ToArray();
            outputs[0].Value = instance;
            var node = new Node(context)
            {
                NodeDescription = this,
                Inputs = inputs,
                Outputs = outputs
            };
            if (CopyOnWrite)
            {
                node.updateAction = () =>
                {
                    if (inputs.Any(p => p.IsChanged))
                    {
                        // TODO: Causes render pipeline to crash
                        //if (instance is IDisposable disposable)
                        //    disposable.Dispose();

                        instance = ctor(context).Item1;

                        foreach (var input in inputs)
                            input.Apply(instance);

                        outputs[0].Value = instance;
                    }
                };
                node.disposeAction = () =>
                {
                    // TODO: Causes render pipeline to crash
                    //if (instance is IDisposable disposable)
                    //    disposable.Dispose();
                };
            }
            else
            {
                node.updateAction = () =>
                {
                    foreach (var input in inputs)
                        if (input.IsChanged)
                            input.Apply(instance);
                };
                node.disposeAction = () =>
                {
                    if (instance is IDisposable disposable)
                        disposable.Dispose();
                    onDispose?.Invoke();
                };
            }
            return node;
        }

        public bool OpenEditor()
        {
            return false;
        }

        public CustomNodeDesc<TInstance> AddInput<T>(string name, Func<TInstance, T> getter, Action<TInstance, T> setter, T defaultValue = default)
        {
            inputs.Add(new CustomPinDesc()
            {
                Name = name.InsertSpaces(),
                Type = typeof(T),
                DefaultValue = defaultValue,
                CreatePin = () => new Pin<T>()
                {
                    getter = getter,
                    setter = setter
                }
            });
            return this;
        }

        public CustomNodeDesc<TInstance> AddListInput<T>(string name, Func<TInstance, IList<T>> getter)
        {
            return AddInput<IReadOnlyList<T>>(name, 
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
            public bool IsChanged;

            public abstract object Value { get; set; }

            public abstract void Apply(TInstance instance);
        }

        class Pin<T> : Pin, IVLPin
        {
            static readonly EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

            public Func<TInstance, T> getter;
            public Action<TInstance, T> setter;
            public T value;

            public override object Value 
            { 
                get => this.value; 
                set
                {
                    var valueT = (T)value;
                    if (!equalityComparer.Equals(this.value, valueT))
                    {
                        this.value = valueT;
                        IsChanged = true;
                    }
                }
            }

            public override void Apply(TInstance instance)
            {
                setter(instance, (T)Value);
                IsChanged = false;
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
