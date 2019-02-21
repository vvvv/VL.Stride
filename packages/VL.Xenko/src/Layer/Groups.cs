using System;
using System.Collections.Generic;
using VL.Lib.Collections;
using VL.Xenko.Rendering;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Rendering;

namespace VL.Xenko.Layer
{
    /// <summary>
    /// Groups multiple layers into one. Layers are rendered from left to right.
    /// </summary>
    public sealed class LowLevelAPIGroup : ILowLevelAPIRender
    {
        ILowLevelAPIRender FUpstreamLayer1;
        ILowLevelAPIRender FUpstreamLayer2;

        public ILowLevelAPIRender Update(ILowLevelAPIRender input, ILowLevelAPIRender input2)
        {
            FUpstreamLayer1 = input;
            FUpstreamLayer2 = input2;
            return this;
        }

        void ILowLevelAPIRender.Initialize()
        {
            FUpstreamLayer1?.Initialize();
            FUpstreamLayer2?.Initialize();
        }

        void ILowLevelAPIRender.Collect(RenderContext context)
        {
            FUpstreamLayer1?.Collect(context);
            FUpstreamLayer2?.Collect(context);
        }

        void ILowLevelAPIRender.Extract()
        {
            FUpstreamLayer1?.Extract();
            FUpstreamLayer2?.Extract();
        }

        void ILowLevelAPIRender.Prepare(RenderDrawContext context)
        {
            FUpstreamLayer1?.Prepare(context);
            FUpstreamLayer2?.Prepare(context);
        }

        void ILowLevelAPIRender.Draw(RenderContext renderContext, RenderDrawContext drawContext, RenderView renderView, RenderViewStage renderViewStage, CommandList commandList)
        {
            FUpstreamLayer1?.Draw(renderContext, drawContext, renderView, renderViewStage, commandList);
            FUpstreamLayer2?.Draw(renderContext, drawContext, renderView, renderViewStage, commandList);
        }
    }

    /// <summary>
    /// Groups a sequence of layers into one.
    /// </summary>
    public sealed class LowLevelAPISpectralGroup : ILowLevelAPIRender
    {
        Spread<ILowLevelAPIRender> FUpstreamLayers = Spread<ILowLevelAPIRender>.Empty;

        public ILowLevelAPIRender Update(IEnumerable<ILowLevelAPIRender> input)
        {
            FUpstreamLayers = input.ToSpread();
            return this;
        }

        void ILowLevelAPIRender.Initialize()
        {
            foreach (var upstreamLayer in FUpstreamLayers)
                upstreamLayer?.Initialize();
        }

        void ILowLevelAPIRender.Collect(RenderContext context)
        {
            foreach (var upstreamLayer in FUpstreamLayers)
                upstreamLayer?.Collect(context);
        }

        void ILowLevelAPIRender.Extract()
        {
            foreach (var upstreamLayer in FUpstreamLayers)
                upstreamLayer?.Extract();
        }

        void ILowLevelAPIRender.Prepare(RenderDrawContext context)
        {
            foreach (var upstreamLayer in FUpstreamLayers)
                upstreamLayer?.Prepare(context);
        }

        void ILowLevelAPIRender.Draw(RenderContext renderContext, RenderDrawContext drawContext, RenderView renderView, RenderViewStage renderViewStage, CommandList commandList)
        {
            foreach (var upstreamLayer in FUpstreamLayers)
                upstreamLayer?.Draw(renderContext, drawContext, renderView, renderViewStage, commandList);
        }
    }

    /// <summary>
    /// Groups multiple entities into one.
    /// </summary>
    public sealed class EntityGroup : IDisposable
    {
        readonly EntityLink link1;
        readonly EntityLink link2;
        readonly Entity self;

        public EntityGroup()
        {
            self = new Entity();
            link1 = new EntityLink(self);
            link2 = new EntityLink(self);
        }

        public Entity Update(Entity input, Entity input2)
        {
            link1.Child = input;
            link2.Child = input2;
            return self;
        }

        public void Dispose()
        {
            link1.Dispose();
            link2.Dispose();
        }
    }

    /// <summary>
    /// Groups a spread of entities into one.
    /// </summary>
    public sealed class SpectralEntityGroup : IDisposable
    {
        readonly EntityChildrenManager manager = new EntityChildrenManager(new Entity());

        public Entity Update(Spread<Entity> input) => manager.Update(input);
        public void Dispose() => manager.Dispose();
    }
}
