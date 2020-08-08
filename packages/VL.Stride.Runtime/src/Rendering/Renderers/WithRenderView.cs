using Stride.Core.Mathematics;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    public class WithRenderView : RendererBase
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
}
