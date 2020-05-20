using Stride.Rendering.Materials;
using System;
using System.Linq;
using VL.Core;

namespace VL.Stride
{
    class StrideNode<TInstance> : VLObject, IVLNode
        where TInstance : new()
    {
        readonly Pin[] inputs;
        readonly StrideNodeDesc<TInstance> nodeDescription;

        public StrideNode(NodeContext nodeContext, StrideNodeDesc<TInstance> description)
            : base(nodeContext)
        {
            Context = nodeContext;
            nodeDescription = description;

            inputs = description.Inputs.OfType<PinDescription>().Select(d => d.CreatePin()).ToArray();
            Outputs = new IVLPin[] { new StatePin() { Value = new TInstance() } };
        }

        public IVLNodeDescription NodeDescription => nodeDescription;

        public IVLPin[] Inputs => inputs;

        public IVLPin[] Outputs { get; }

        public void Update()
        {
            if (inputs.Any(p => p.IsChanged))
            {
                TInstance feature;
                if (nodeDescription.CopyOnWrite)
                {
                    // TODO: Causes crash in pipeline
                    //if (Outputs[0].Value is IDisposable disposable)
                    //    disposable.Dispose();
                    feature = new TInstance();
                }
                else
                {
                    feature = (TInstance)Outputs[0].Value;
                }

                foreach (var pin in inputs)
                    pin.ApplyValue(feature);

                Outputs[0].Value = feature;
            }
        }

        public void Dispose()
        {
            if (!nodeDescription.CopyOnWrite && Outputs[0].Value is IDisposable disposable)
                disposable.Dispose();
        }

        class StatePin : IVLPin
        {
            public object Value { get; set; }
        }
    }
}
