using System;
using System.Collections.Generic;
using VL.Core;
using Xenko.Graphics;
using Xenko.Rendering;

namespace VL.Xenko.Rendering
{
    public interface ILowLevelAPIRender
    {
        void Initialize();

        void Collect(RenderContext context);

        void Extract();

        void Prepare(RenderDrawContext context);

        void Draw(RenderContext renderContext, RenderDrawContext drawContext, RenderView renderView, RenderViewStage renderViewStage, CommandList commandList);
    }

    /// <summary>
    /// The layer render feature redirects low level rendering calls to the <see cref="LayerComponent.Layer"/> property 
    /// of all the layer components in the scene which have the SingleCallPerFrame set to false.
    /// </summary>
    public /*sealed*/ class LayerRenderFeature : RootRenderFeature, INotifyHotSwapImminent
    {
        private readonly List<ILowLevelAPIRender> singleCallLayers = new List<ILowLevelAPIRender>();
        private readonly List<ILowLevelAPIRender> layers = new List<ILowLevelAPIRender>();
        private int lastFrameNr;
        private IVLRuntime runtime;

        public LayerRenderFeature()
        {
            // Pre adjust render priority, low numer is early, high number is late (advantage of backbuffer culling)
            SortKey = 190;
        }

        public override Type SupportedRenderObjectType => typeof(RenderLayer);

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            // Do not call into VL if not running
            var renderContext = context.RenderContext;
            var runtime = this.runtime ?? (this.runtime = renderContext.Services.GetService<IVLRuntime>());
            if (runtime != null && !runtime.IsRunning)
                return;

            // Build the list of layers to render
            singleCallLayers.Clear();
            layers.Clear();
            for (var index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);
                var renderElement = (RenderLayer)renderNode.RenderObject;

                if (renderElement.Enabled)
                {
                    if (renderElement.SingleCallPerFrame)
                        singleCallLayers.Add(renderElement.Layer);
                    else
                        layers.Add(renderElement.Layer); 
                }
            }

            // Tell VL that we're calling into it - ensures that no hotswap will be happening
            RuntimeGraph.Enter(this);
            try
            {
                // Call layers which want to get invoked only once per frame first
                var currentFrameNr = renderContext.Time.FrameCount;
                if (lastFrameNr != currentFrameNr)
                {
                    lastFrameNr = currentFrameNr;
                    foreach (var layer in singleCallLayers)
                    {
                        try
                        {
                            layer?.Draw(Context, context, renderView, renderViewStage, context.CommandList);
                        }
                        catch (Exception e)
                        {
                            RuntimeGraph.ReportException(e);
                        }
                    }
                }

                // Call layers which can get invoked twice per frame (for each eye)
                foreach (var layer in layers)
                {
                    try
                    {
                        layer?.Draw(Context, context, renderView, renderViewStage, context.CommandList);
                    }
                    catch (Exception e)
                    {
                        RuntimeGraph.ReportException(e);
                    }
                }
            }
            finally
            {
                RuntimeGraph.Exit(this);
            }
        }

        void INotifyHotSwapImminent.HotSwapImminent() { }
    }
}
