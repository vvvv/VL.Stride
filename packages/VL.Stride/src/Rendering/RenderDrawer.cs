using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// The render object used by the low level rendering system.
    /// </summary>
    public class RenderDrawer : RenderObject
    {
        public bool SingleCallPerFrame;
        public DrawerRenderStage RenderStage;
        public IGraphicsRendererBase Renderer;
    }
}
