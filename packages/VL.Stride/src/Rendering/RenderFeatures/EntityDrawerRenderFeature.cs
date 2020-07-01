using System;
using System.Collections.Generic;
using VL.Core;
using Stride.Core.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;

namespace VL.Stride.Rendering
{

    /// <summary>
    /// The render feature redirects low level rendering calls to the <see cref="IEntityDrawer"/> 
    /// </summary>
    public /*sealed*/ class EntityDrawerRenderFeature : RootRenderFeature
    {
        private readonly List<IGraphicsRendererBase> singleCallRenderers = new List<IGraphicsRendererBase>();
        private readonly List<IGraphicsRendererBase> renderers = new List<IGraphicsRendererBase>();
        private int lastFrameNr;
        private IVLRuntime runtime;

        public EntityDrawerRenderFeature()
        {
            // Pre adjust render priority, low numer is early, high number is late (advantage of backbuffer culling)
            SortKey = 190;
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            if (Context.Services.GetService<InSceneLayerRenderFeature>() == null)
                Context.Services.AddService(this);
        }

        public override void Prepare(RenderDrawContext context)
        {
            base.Prepare(context);
            runtime ??= context.RenderContext.Services.GetService<IVLRuntime>();
        }

        public override Type SupportedRenderObjectType => typeof(RenderDrawer);

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            //CPU and GPU profiling
            using (Profiler.Begin(VLProfilerKeys.InSceneRenderProfilingKey))
            using (context.QueryManager.BeginProfile(Color.Green, VLProfilerKeys.InSceneRenderProfilingKey))
            {
                // Do not call into VL if not running
                if (runtime != null && !runtime.IsRunning)
                    return;

                // Build the list of drawers to render
                singleCallRenderers.Clear();
                renderers.Clear();
                for (var index = startIndex; index < endIndex; index++)
                {
                    var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                    var renderNode = GetRenderNode(renderNodeReference);
                    var renderElement = (RenderDrawer)renderNode.RenderObject;

                    if (renderElement.Enabled)
                    {
                        if (renderElement.SingleCallPerFrame)
                            singleCallRenderers.Add(renderElement.Renderer);
                        else
                            renderers.Add(renderElement.Renderer);
                    }
                }

                // Call renderers which want to get invoked only once per frame first
                var currentFrameNr = context.RenderContext.Time.FrameCount;
                if (lastFrameNr != currentFrameNr)
                {
                    lastFrameNr = currentFrameNr;
                    foreach (var renderer in singleCallRenderers)
                    {
                        try
                        {
                            renderer?.Draw(context);
                        }
                        catch (Exception e)
                        {
                            RuntimeGraph.ReportException(e);
                        }
                    }
                }

                // Call renderers which can get invoked twice per frame (for each eye)
                foreach (var renderer in renderers)
                {
                    try
                    {
                        renderer?.Draw(context);
                    }
                    catch (Exception e)
                    {
                        RuntimeGraph.ReportException(e);
                    }
                }
            }
        }
    }
}
