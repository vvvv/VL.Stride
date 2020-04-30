using Stride.Core.Mathematics;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// The render object used by the low level layer rendering system.
    /// </summary>
    public class RenderLayer : RenderObject
    {
        public bool SingleCallPerFrame;
        public ILowLevelAPIRender Layer;
    }
}
