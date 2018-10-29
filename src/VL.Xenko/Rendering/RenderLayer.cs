using VL.Xenko.RootRenderFeatures;
using Xenko.Core.Mathematics;
using Xenko.Rendering;

namespace V.Xenko.Rendering
{
    /// <summary>
    /// The render object used by the low level layer rendering system.
    /// </summary>
    public class RenderLayer : RenderObject
    {
        public bool SingleCallPerFrame;
        public Matrix WorldMatrix;
        public ILowLevelAPIRender Layer;
    }
}
