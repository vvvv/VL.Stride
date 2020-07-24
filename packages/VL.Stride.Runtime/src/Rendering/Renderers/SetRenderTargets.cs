using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Diagnostics;

namespace VL.Stride.Rendering
{
    public abstract class InputRenderBase : IGraphicsRendererBase
    {
        public IGraphicsRendererBase Input { get; set; }

        public void Draw(RenderDrawContext context)
        {
            if (Input != null)
            {
                try
                {
                    DrawInternal(context);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Gets called if the input is assigned.
        /// </summary>
        /// <param name="context">The context.</param>
        protected abstract void DrawInternal(RenderDrawContext context);

        protected void DrawInput(RenderDrawContext context)
        {
            Input.Draw(context);
        }
    }

    public class SetRenderTargetsAndViewPort : InputRenderBase
    {
        ViewportState viewportState = new ViewportState();

        public Texture RenderTarget { get; set; }

        public Texture DepthBuffer { get; set; }

        protected override void DrawInternal(RenderDrawContext context)
        {
            var renderTarget = RenderTarget;
            var depthBuffer = DepthBuffer;
            var setRenderTarget = renderTarget != null;
            var setDepthBuffer = depthBuffer != null;

            if (setRenderTarget || setDepthBuffer)
            {
                var renderContext = context.RenderContext;

                using (renderContext.SaveRenderOutputAndRestore())
                using (renderContext.SaveViewportAndRestore())
                using (context.PushRenderTargetsAndRestore())
                {
                    if (setRenderTarget)
                    {
                        renderContext.RenderOutput.RenderTargetFormat0 = renderTarget.ViewFormat;
                        renderContext.RenderOutput.RenderTargetCount = 1;

                        viewportState.Viewport0 = new Viewport(0, 0, renderTarget.ViewWidth, renderTarget.ViewHeight);
                        renderContext.ViewportState = viewportState;
                    }

                    context.CommandList.SetRenderTargetAndViewport(depthBuffer, renderTarget);
                    DrawInput(context);
                }  
            }
        }
    }

    public class SetRenderView : InputRenderBase
    {
        public RenderView RenderView  { get; set; }

        protected override void DrawInternal(RenderDrawContext context)
        {

            var renderView = RenderView;
            if (renderView != null)
            {
                var renderTarget = context.CommandList.RenderTarget;

                if (renderTarget is null)
                {
                    renderView.ViewSize = Vector2.One;
                }
                else
                {
                    renderView.ViewSize = new Vector2(renderTarget.ViewWidth, renderTarget.ViewHeight);
                }

                using (context.RenderContext.PushRenderViewAndRestore(renderView))
                {
                    DrawInput(context);
                } 
            }
        }
    }
}
