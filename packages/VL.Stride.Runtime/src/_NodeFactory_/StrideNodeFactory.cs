﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using VL.Core;
using VL.Stride.Engine;
using VL.Stride.Graphics;
using VL.Stride.Rendering.Composition;
using VL.Stride.Rendering.Lights;
using VL.Stride.Rendering.Materials;

[assembly: NodeFactory(typeof(VL.Stride.StrideNodeFactory))]

namespace VL.Stride
{
    public class StrideNodeFactory : IVLNodeDescriptionFactory
    {
        public StrideNodeFactory()
        {
            NodeDescriptions = GetNodeDescriptions().ToImmutableArray();
        }

        public ImmutableArray<IVLNodeDescription> NodeDescriptions { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        IEnumerable<IVLNodeDescription> GetNodeDescriptions()
        {
            foreach (var n in MaterialNodes.GetNodeDescriptions(this))
                yield return n;

            foreach (var n in LightNodes.GetNodeDescriptions(this))
                yield return n;

            foreach (var n in CompositionNodes.GetNodeDescriptions(this))
                yield return n;

            foreach (var n in EngineNodes.GetNodeDescriptions(this))
                yield return n;

            foreach (var n in PhysicsNodes.GetNodeDescriptions(this))
                yield return n;

            foreach (var n in GraphicNodes.GetNodeDescriptions(this))
                yield return n;
        }
    }
}