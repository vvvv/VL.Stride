using Stride.Core.Mathematics;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    public interface IEffect
    {
        void SetEntityWorldMatrix(Matrix entityWorld);

        EffectInstance SetParameters(RenderView renderView, RenderDrawContext renderDrawContext);
    }
}
