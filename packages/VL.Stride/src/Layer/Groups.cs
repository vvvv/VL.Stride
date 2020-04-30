using System;
using System.Collections.Generic;
using VL.Lib.Collections;
using VL.Stride.Rendering;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;

namespace VL.Stride.Layer
{
    /// <summary>
    /// Groups a sequence of layers into one.
    /// </summary>
    public sealed class LowLevelAPISpectralGroup : ILowLevelAPIRender
    {
        Spread<ILowLevelAPIRender> FUpstreamLayers = Spread<ILowLevelAPIRender>.Empty;

        public ILowLevelAPIRender Update(Spread<ILowLevelAPIRender> input)
        {
            FUpstreamLayers = input;
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

        void ILowLevelAPIRender.SetEntityWorldMatrix(Matrix entityWorld)
        {
            foreach (var upstreamLayer in FUpstreamLayers)
                upstreamLayer?.SetEntityWorldMatrix(entityWorld);
        }
    }
}
