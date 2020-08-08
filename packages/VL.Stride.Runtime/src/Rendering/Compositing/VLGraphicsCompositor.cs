using Stride.Rendering;
using Stride.Rendering.Compositing;

namespace VL.Stride.Rendering.Compositing
{
    public class VLGraphicsCompositor : GraphicsCompositor
    {
        public IGraphicsRendererBase BeforeDraw { get; set; }

        protected override void DrawCore(RenderDrawContext context)
        {
            BeforeDraw?.Draw(context);

            base.DrawCore(context);
        }
    }
}
