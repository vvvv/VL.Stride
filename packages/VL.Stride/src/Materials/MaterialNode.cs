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

        public MaterialNode(NodeContext nodeContext, MaterialNodeDescription<TMaterial> description)
            : base(nodeContext)
        {
            Context = nodeContext;
            NodeDescription = description;

            inputs = description.Inputs.OfType<PinDescription>().Select(d => d.CreatePin()).ToArray();
            Outputs = new IVLPin[] { new StatePin() { Value = new TMaterial() } };
        }

        public IVLNodeDescription NodeDescription { get; }

        public IVLPin[] Inputs => inputs;

        public IVLPin[] Outputs { get; }

        public void Update()
        {
            if (inputs.Any(p => p.IsChanged))
            {
                var feature = new TMaterial();
                foreach (var pin in inputs)
                    pin.ApplyValue(feature);
                Outputs[0].Value = feature;
            }
        }

        public void Dispose()
        {
        }

        class StatePin : IVLPin
        {
            public object Value { get; set; }
        }
    }
}
