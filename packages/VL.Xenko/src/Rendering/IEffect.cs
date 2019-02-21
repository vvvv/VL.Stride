using Xenko.Rendering;

namespace VL.Xenko.Rendering
{
    public interface IEffect
    {
        EffectInstance SetParameters(RenderView renderView, RenderDrawContext renderDrawContext);
    }
}
