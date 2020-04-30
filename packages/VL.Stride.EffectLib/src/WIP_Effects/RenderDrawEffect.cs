using VL.Stride.RootRenderFeatures;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// The render object used by the draw effect render feature.
    /// </summary>
    public class RenderDrawEffect : RenderObject
    {
        public string EffectName;
        public ILowLevelAPIRender Layer { get; set; }
        public bool SingleCallPerFrame { get; set; }
    }
}
