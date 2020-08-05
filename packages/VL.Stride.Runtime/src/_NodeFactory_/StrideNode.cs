using System;
using System.Collections.Immutable;
using System.Linq;
using VL.Core;

namespace VL.Stride
{
    class StrideNode : VLObject
    {
        public bool needsUpdate;

        public StrideNode(NodeContext nodeContext)
            : base(nodeContext)
        {

        }
    }

    class StrideNode<TInstance> : StrideNode, IVLNode
        where TInstance : new()
    {
        readonly ImmutableArray<Pin> inputs;
        readonly StrideNodeDesc<TInstance> nodeDescription;
        readonly StatePin output;

        public StrideNode(NodeContext nodeContext, StrideNodeDesc<TInstance> description)
            : base(nodeContext)
        {
            Context = nodeContext;
            nodeDescription = description;

            inputs = description.Inputs.OfType<PinDescription>().Select(d => d.CreatePin(this)).ToImmutableArray();
            Outputs = ImmutableArray.Create<IVLPin>(output = new StatePin(this, new TInstance()));
        }

        public IVLNodeDescription NodeDescription => nodeDescription;

        public ImmutableArray<IVLPin> Inputs => ImmutableArray<IVLPin>.CastUp(inputs);

        public ImmutableArray<IVLPin> Outputs { get; }

        public void Update()
        {
            if (needsUpdate)
            {
                needsUpdate = false;

                TInstance instance;
                if (nodeDescription.CopyOnWrite)
                {
                    // TODO: Causes crash in pipeline
                    //if (Outputs[0].Value is IDisposable disposable)
                    //    disposable.Dispose();
                    instance = new TInstance();
                }
                else
                {
                    instance = output.value;
                }

                foreach (var pin in inputs)
                    pin.ApplyValue(instance);

                output.value = instance;
            }
        }

        public void Dispose()
        {
            if (!nodeDescription.CopyOnWrite && Outputs[0].Value is IDisposable disposable)
                disposable.Dispose();
        }

        class StatePin : IVLPin
        {
            readonly StrideNode<TInstance> node;
            readonly bool isFragmented;

            public StatePin(StrideNode<TInstance> node, TInstance instance)
            {
                this.node = node;
                this.isFragmented = node.nodeDescription.Fragmented;
                this.value = instance;
            }

            public object Value 
            { 
                get
                {
                    if (isFragmented && node.needsUpdate)
                        node.Update();
                    return value;
                }
                set => this.value = (TInstance)value;
            }
            public TInstance value;
        }
    }
}
