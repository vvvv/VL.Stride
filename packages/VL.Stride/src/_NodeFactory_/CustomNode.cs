using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            Action<TNew> init = default,
            Action<NodeContext, TNew> update = default,
            bool hasStateOutput = true,
            bool fragmented = false) 
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
                copyOnWrite: copyOnWrite,
                hasStateOutput: hasStateOutput,
                fragmented: fragmented,
                update: update);
        }
    }

    class CustomNodeDesc<TInstance> : IVLNodeDescription
    {
        readonly List<CustomPinDesc> inputs = new List<CustomPinDesc>();
        readonly List<CustomPinDesc> outputs = new List<CustomPinDesc>();
        readonly Func<NodeContext, (TInstance, Action)> ctor;
        readonly Action<NodeContext, TInstance> update;

        public CustomNodeDesc(IVLNodeDescriptionFactory factory, Func<NodeContext, (TInstance, Action)> ctor, 
            string name = default, 
            string category = default, 
            bool copyOnWrite = true, 
            bool hasStateOutput = true,
            bool fragmented = false,
            Action<NodeContext, TInstance> update = default)
        {
            Factory = factory;
            this.ctor = ctor;
            this.update = update;

            Name = name ?? typeof(TInstance).Name;
            Category = category ?? string.Empty;
            CopyOnWrite = copyOnWrite;
            Fragmented = fragmented;

            if (hasStateOutput)
                AddOutput("Output", x => x);
        }

        public IVLNodeDescriptionFactory Factory { get; }

        public string Name { get; }

        public string Category { get; }

        public bool Fragmented { get; }

        public bool CopyOnWrite { get; }

        public IReadOnlyList<IVLPinDescription> Inputs => inputs;

        public IReadOnlyList<IVLPinDescription> Outputs => outputs;

        public IEnumerable<Message> Messages => Enumerable.Empty<Message>();

        public IVLNode CreateInstance(NodeContext context)
        {
            var (instance, onDispose) = ctor(context);
            var inputs = this.inputs.Select(p => p.CreatePin()).ToArray();
            var outputs = this.outputs.Select(p => p.CreatePin()).ToArray();

            var node = new Node(context)
            {
                NodeDescription = this,
                Inputs = inputs,
                Outputs = outputs
            };

            // Assign outputs
            foreach (var output in outputs)
                output.Value = output.ReadValueFrom(instance);

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
                            input.WriteValueTo(instance);

                        update?.Invoke(context, instance);

                        foreach (var output in outputs)
                            output.Value = output.ReadValueFrom(instance);
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
                    var anyChanged = false;
                    foreach (var input in inputs)
                    {
                        if (input.IsChanged)
                        {
                            input.WriteValueTo(instance);
                            anyChanged = true;
                        }
                    }

                    if (anyChanged)
                        update?.Invoke(context, instance);

                    foreach (var output in outputs)
                        output.Value = output.ReadValueFrom(instance);
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
                CreatePin = () => new InputPin<T>()
                {
                    getter = getter,
                    setter = setter
                }
            });
            return this;
        }

        public CustomNodeDesc<TInstance> AddInputWithRefEquality<T>(string name, Func<TInstance, T> getter, Action<TInstance, T> setter)
        {
            inputs.Add(new CustomPinDesc()
            {
                Name = name.InsertSpaces(),
                Type = typeof(T),
                CreatePin = () => new InputPinUsingReferenceEquality<T>()
                {
                    getter = getter,
                    setter = setter
                }
            });
            return this;
        }

        public CustomNodeDesc<TInstance> AddInputWithFallback<T>(string name, Func<TInstance, T> getter, Action<TInstance, T, T> setter, T defaultValue = default)
            where T : class
        {
            inputs.Add(new CustomPinDesc()
            {
                Name = name.InsertSpaces(),
                Type = typeof(T),
                DefaultValue = defaultValue,
                CreatePin = () =>
                {
                    var initial = default(T);
                    return new InputPin<T>()
                    {
                        getter = getter,
                        setter = (x, v) =>
                        {
                            if (initial is null)
                                initial = getter(x);
                            setter(x, v, initial);
                        }
                    };
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

        public CustomNodeDesc<TInstance> AddListInput<T>(string name, Func<TInstance, T[]> getter, Action<TInstance, T[]> setter)
        {
            return AddInput<IReadOnlyList<T>>(name,
                getter: instance => (IReadOnlyList<T>)getter(instance),
                setter: (x, v) =>
                {
                    var currentItems = getter(x);
                    var newItems = v.Where(i => i != null);
                    if (currentItems != null && !currentItems.SequenceEqual(newItems))
                        setter(x, v.ToArray());
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

            public abstract void WriteValueTo(TInstance instance);
            public abstract object ReadValueFrom(TInstance instance);
        }

        class Pin<T> : Pin, IVLPin
        {
            public Func<TInstance, T> getter;
            public Action<TInstance, T> setter;
            public T value;

            public override object Value 
            { 
                get => this.value;
                set => this.value = (T)value;
            }

            public override void WriteValueTo(TInstance instance)
            {
                setter(instance, value);
            }

            public override object ReadValueFrom(TInstance instance)
            {
                return getter(instance);
            }
        }

        class InputPin<T> : Pin<T>, IVLPin
        {
            static readonly EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

            public override object Value
            {
                get => base.Value;
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

            public override void WriteValueTo(TInstance instance)
            {
                base.WriteValueTo(instance);
                IsChanged = false;
            }
        }

        // Hack to workaround equality bug (https://github.com/stride3d/stride/issues/735)
        class InputPinUsingReferenceEquality<T> : Pin<T>, IVLPin
        {
            static readonly EqualityComparer<T> equalityComparer = ReferenceEqualityComparer<T>.Default;

            public override object Value
            {
                get => base.Value;
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

            public override void WriteValueTo(TInstance instance)
            {
                base.WriteValueTo(instance);
                IsChanged = false;
            }
        }

        sealed class ReferenceEqualityComparer<T> : EqualityComparer<T>
        {
            public static new EqualityComparer<T> Default { get; } = new ReferenceEqualityComparer<T>();

            public override bool Equals(T x, T y) => ReferenceEquals(x, y);
            public override int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
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
