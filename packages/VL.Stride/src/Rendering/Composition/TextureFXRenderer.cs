using Stride.Rendering;
using Stride.Rendering.Compositing;
using System;
using System.Collections.Generic;
using System.Text;
using VL.Core;

namespace VL.Stride.Rendering.Composition
{
    public class TextureFXRenderer : SingleStageRenderer
    {
        public IGraphicsRendererBase TextureFXAutoDrawer;
        private RenderViewStage renderViewStage;
        private IVLRuntime runtime;

        protected override void InitializeCore()
        {
            base.InitializeCore();
            if (Context.Services.GetService<TextureFXRenderer>() == null)
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

            // Render texture fx auto drawer
            if (TextureFXAutoDrawer != null)
            {
                try
                {
                    using (drawContext.PushRenderTargetsAndRestore())
                        TextureFXAutoDrawer.Draw(drawContext);
                }
                catch (Exception e)
                {
                    RuntimeGraph.ReportException(e);
                }
            }
        }
    }
}
