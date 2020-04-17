using VL.Xenko.RootRenderFeatures;
using Xenko.Rendering;

namespace VL.Xenko.Rendering
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
