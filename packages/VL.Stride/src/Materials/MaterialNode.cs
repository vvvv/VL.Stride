using Stride.Rendering.Materials;
using System;
using System.Linq;
using VL.Core;

namespace VL.Stride.Materials
{
    class MaterialNode<TMaterial> : VLObject, IVLNode
        where TMaterial : new()
    {
        readonly Pin[] inputs;
        readonly MaterialNodeDescription<TMaterial> nodeDescription;

        public MaterialNode(NodeContext nodeContext, MaterialNodeDescription<TMaterial> description)
            : base(nodeContext)
        {
            Context = nodeContext;
            nodeDescription = description;

            inputs = description.Inputs.OfType<PinDescription>().Select(d => d.CreatePin()).ToArray();
            Outputs = new IVLPin[] { new StatePin() { Value = new TMaterial() } };
        }

        public IVLNodeDescription NodeDescription => nodeDescription;

        public IVLPin[] Inputs => inputs;

        public IVLPin[] Outputs { get; }

        public void Update()
        {
            if (inputs.Any(p => p.IsChanged))
            {
                TMaterial feature;
                if (nodeDescription.CopyOnWrite)
                {
                    // TODO: Causes crash in pipeline
                    //if (Outputs[0].Value is IDisposable disposable)
                    //    disposable.Dispose();
                    feature = new TMaterial();
                }
                else
                {
                    feature = (TMaterial)Outputs[0].Value;
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
