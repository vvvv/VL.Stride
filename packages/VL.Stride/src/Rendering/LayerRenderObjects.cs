using Stride.Core.Mathematics;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    public class RenderLayerBase : RenderObject
    {
        public bool SingleCallPerFrame;
        public ILowLevelAPIRender Layer;
    }

    /// <summary>
    /// The render object used by the low level layer rendering system.
    /// </summary>
    public class RenderBeforeScene : RenderLayerBase
    {
    }

    /// <summary>
    /// The render object used by the low level layer rendering system.
    /// </summary>
    public class RenderInScene : RenderLayerBase
    {
    }

    /// <summary>
    /// The render object used by the low level layer rendering system.
    /// </summary>
    public class RenderAfterScene : RenderLayerBase
    {
    }
}
