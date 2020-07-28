using Stride.Core;
using Stride.Core.Annotations;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;

namespace VL.Stride.Engine
{
    public class LayerSystem : GameSystemBase
    {
        private RenderView renderView;
        private RenderContext renderContext;
        private RenderDrawContext renderDrawContext;

        public LayerSystem([NotNull] IServiceRegistry registry) : base(registry)
        {
            Enabled = true;
            Visible = true;
        }

        public IGraphicsRendererBase Layer { get; set; }

        protected override void LoadContent()
        {
            // Default render view
            renderView = new RenderView()
            {
                NearClipPlane = 0.05f,
                FarClipPlane = 1000,
            };

            // Create the drawing context
            var graphicsContext = Services.GetSafeServiceAs<GraphicsContext>();
            renderContext = RenderContext.GetShared(Services);
            renderDrawContext = new RenderDrawContext(Services, renderContext, graphicsContext);
        }

        public override void Draw(GameTime gameTime)
        {
            if (Layer != null)
            {
                using (renderContext.PushRenderViewAndRestore(renderView))
                using (renderDrawContext.PushRenderTargetsAndRestore())
                {
                    Layer.Draw(renderDrawContext);
                }
            }
        }
    }
}
