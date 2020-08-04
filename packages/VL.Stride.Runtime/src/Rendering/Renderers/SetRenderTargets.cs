using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Input;
using Stride.Rendering;
using System;
using System.Diagnostics;
using VL.Stride.Input;

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

    public class WithRenderTargetsAndViewPort : InputRenderBase
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
            else
            {
                DrawInput(context);
            }
        }
    }

    public class WithRenderView : InputRenderBase
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
            else
            {
                DrawInput(context);
            }
        }
    }

    public class WithWindowInputSource : InputRenderBase
    {
        public IInputSource InputSource { get; set; }

        protected override void DrawInternal(RenderDrawContext context)
        {

            var inputSource = InputSource;
            if (inputSource != null)
            {
                using (context.RenderContext.PushTagAndRestore(InputExtensions.WindowInputSource, inputSource))
                {
                    DrawInput(context);
                }
            }
            else
            {
                DrawInput(context);
            }
        }
    }

    public class GetWindowInputSource : IGraphicsRendererBase
    {
        public IGraphicsRendererBase Input { get; set; }
        public IInputSource InputSource { get; private set; }

        public void Draw(RenderDrawContext context)
        {
            try
            {
                var renderContext = context.RenderContext;
                renderContext.GetWindowInputSource(out var inputSource);
                InputSource = inputSource;

                Input?.Draw(context);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
