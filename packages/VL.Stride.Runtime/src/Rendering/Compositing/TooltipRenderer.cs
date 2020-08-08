using Stride.Rendering;
using Stride.Rendering.Compositing;
using System;
using System.Collections.Generic;
using System.Text;
using VL.Core;

namespace VL.Stride.Rendering.Compositing
{
    public class TooltipRenderer : SingleStageRenderer
    {
        public IGraphicsRendererBase Tooltip;
        private RenderViewStage renderViewStage;
        private IVLRuntime runtime;

        protected override void InitializeCore()
        {
            base.InitializeCore();
            if (Context.Services.GetService<TooltipRenderer>() == null)
                Context.Services.AddService(this);

            renderViewStage = RenderStage;
        }
        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            base.DrawCore(context, drawContext);

            // Do not call into VL if not running
            runtime ??= context.Services.GetService<IVLRuntime>();
            if (runtime != null && !runtime.IsRunning)
                return;

            // Render tooltip
            if (Tooltip != null)
            {
                try
                {
                    using (drawContext.PushRenderTargetsAndRestore())
                        Tooltip.Draw(drawContext);
                }
                catch (Exception e)
                {
                    RuntimeGraph.ReportException(e);
                }
            }
        }
    }
}
