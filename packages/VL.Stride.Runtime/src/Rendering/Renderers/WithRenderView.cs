using Stride.Core.Mathematics;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    public class WithRenderView : RendererBase
    {
        public RenderView RenderView  { get; set; }
        public AspectRatioCorrectionMode AspectRatioCorrectionMode { get; set; }

        protected override void DrawInternal(RenderDrawContext context)
        {
            var renderView = RenderView;
            if (renderView != null)
            {
                var viewport = context?.RenderContext?.ViewportState?.Viewport0;

                if (viewport is null)
                {
                    renderView.ViewSize = Vector2.One;
                }
                else
                {
                    renderView.ViewSize = new Vector2(viewport.Value.Width, viewport.Value.Height);
                }

                if (AspectRatioCorrectionMode != AspectRatioCorrectionMode.NoCorrection)
                {
                    //var currentAspectRatio = renderView.Projection.M22 / renderView.Projection.M11;
                    //var actualAspectRatio = viewport.Value.Height / viewport.Value.Width;
                    switch (AspectRatioCorrectionMode)
                    {
                        case AspectRatioCorrectionMode.AutoWidth:
                            renderView.Projection.M11 = renderView.Projection.M22 * viewport.Value.Height / viewport.Value.Width; 
                            break;
                        case AspectRatioCorrectionMode.AutoHeight:
                            renderView.Projection.M22 = renderView.Projection.M11 * viewport.Value.Width / viewport.Value.Height;
                            break;
                        default:
                            break;
                    }
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
}
