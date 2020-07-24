using Stride.Core;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Games;
using Stride.Rendering;

namespace VL.Stride.Engine
{
    /// <summary>
    /// A scene system that does drawing in <see cref="IGraphicsRendererBase.Draw(RenderDrawContext)"/>.
    /// </summary>
    public class RenderSceneSystem : SceneSystem, IGraphicsRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameSystemBase" /> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <remarks>The GameSystem is expecting the following services to be registered: <see cref="IGame" /> and <see cref="IContentManager" />.</remarks>
        public RenderSceneSystem(IServiceRegistry registry)
            : base(registry)
        {
            SceneInstance = null;
            GraphicsCompositor?.Dispose();
            GraphicsCompositor = null;
        }

        public override bool BeginDraw()
        {
            // Done by IGraphicsRendererBase.Draw
            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            // Done by IGraphicsRendererBase.Draw
        }

        public override void EndDraw()
        {
            // Done by IGraphicsRendererBase.Draw
        }

        public void Draw(RenderDrawContext context)
        {
            if (base.BeginDraw())
            {
                base.Draw(context.RenderContext.Time);
                base.EndDraw();
            }
        }

        protected override void Destroy()
        {
            SceneInstance = null;
            GraphicsCompositor = null;
            base.Destroy();
        }
    }
}
