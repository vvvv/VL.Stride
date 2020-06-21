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
    /// The layer render feature redirects low level rendering calls to the <see cref="LayerComponent.Layer"/> property 
    /// of all the layer components in the scene which have the SingleCallPerFrame set to false.
    /// </summary>
    public /*sealed*/ class AfterSceneLayerRenderFeature : RootRenderFeature
    {
        private readonly List<ILowLevelAPIRender> singleCallLayers = new List<ILowLevelAPIRender>();
        private readonly List<ILowLevelAPIRender> layers = new List<ILowLevelAPIRender>();
        private int lastFrameNr;
        private IVLRuntime runtime;

        public AfterSceneLayerRenderFeature()
        {
            // Pre adjust render priority, low numer is early, high number is late (advantage of backbuffer culling)
            SortKey = 200;
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            if (Context.Services.GetService<AfterSceneLayerRenderFeature>() == null)
                Context.Services.AddService(this);
        }

        public override Type SupportedRenderObjectType => typeof(RenderAfterScene);

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            //CPU and GPU profiling
            using (Profiler.Begin(VLProfilerKeys.AfterSceneRenderProfilingKey))
            using (context.QueryManager.BeginProfile(Color.Green, VLProfilerKeys.AfterSceneRenderProfilingKey))
            {
                // Do not call into VL if not running
                var renderContext = context.RenderContext;
                runtime ??= renderContext.Services.GetService<IVLRuntime>();
                if (runtime != null && !runtime.IsRunning)
                    return;

                // Build the list of layers to render
                singleCallLayers.Clear();
                layers.Clear();
                for (var index = startIndex; index < endIndex; index++)
                {
                    var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                    var renderNode = GetRenderNode(renderNodeReference);
                    var renderElement = (RenderAfterScene)renderNode.RenderObject;

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
            }
        }
    }
}
