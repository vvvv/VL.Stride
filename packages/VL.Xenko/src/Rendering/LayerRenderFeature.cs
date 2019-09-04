using System;
using System.Collections.Generic;
using VL.Core;
using Xenko.Core.Diagnostics;
using Xenko.Core.Mathematics;
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

        void SetEntityWorldMatrix(Matrix entityWorld);

        void Draw(RenderContext renderContext, RenderDrawContext drawContext, RenderView renderView, RenderViewStage renderViewStage, CommandList commandList);
    }

    /// <summary>
    /// The layer render feature redirects low level rendering calls to the <see cref="LayerComponent.Layer"/> property 
    /// of all the layer components in the scene which have the SingleCallPerFrame set to false.
    /// </summary>
    public /*sealed*/ class LayerRenderFeature : RootRenderFeature
    {
        private readonly List<ILowLevelAPIRender> singleCallLayers = new List<ILowLevelAPIRender>();
        private readonly List<ILowLevelAPIRender> layers = new List<ILowLevelAPIRender>();
        private int lastFrameNr;
        private IVLRuntime runtime;
        public ILowLevelAPIRender Tooltip;

        public LayerRenderFeature()
        {
            // Pre adjust render priority, low numer is early, high number is late (advantage of backbuffer culling)
            SortKey = 190;
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            this.Context.Services.AddService(this);
        }

        public override Type SupportedRenderObjectType => typeof(RenderLayer);

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            //CPU and GPU profiling
            using (Profiler.Begin(VLProfilerKeys.LayerRenderProfilingKey))
            using (context.QueryManager.BeginProfile(Color.Green, VLProfilerKeys.LayerRenderProfilingKey))
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

                //render tooltip
                try
                {
                    Tooltip?.Draw(Context, context, renderView, renderViewStage, context.CommandList);
                }
                catch (Exception e)
                {
                    RuntimeGraph.ReportException(e);
                }
            }
        }
    }
}
