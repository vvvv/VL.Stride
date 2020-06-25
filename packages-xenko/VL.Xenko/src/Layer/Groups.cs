using System;
using VL.Lib.Collections;
using VL.Xenko.Rendering;
using Xenko.Core.Mathematics;
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

    /// <summary>
    /// Groups a spread of entities into one.
    /// </summary>
    public sealed class SpectralEntityGroup : IDisposable
    {
        readonly Entity self;
        readonly EntityChildrenManager manager;

        public SpectralEntityGroup()
        {
            self = new Entity();
            self.Transform.UseTRS = false;
            manager = new EntityChildrenManager(self);
        }

        public Entity Update(Spread<Entity> input, ref Matrix transform, string name = "Spectral Group")
        {
            self.Name = name;
            self.Transform.LocalMatrix = transform;
            return manager.Update(input);
        }

        public void Dispose() => manager.Dispose();
    }
}
