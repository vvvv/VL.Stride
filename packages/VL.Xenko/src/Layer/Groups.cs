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
    /// Groups a spread of entities into one.
    /// </summary>
    public sealed class SpectralEntityGroup : IDisposable
    {
        readonly EntityChildrenManager manager = new EntityChildrenManager(new Entity());

        public Entity Update(Spread<Entity> input, string name = "Spectral Group") => manager.Update(input, name);
        public void Dispose() => manager.Dispose();
    }
}
