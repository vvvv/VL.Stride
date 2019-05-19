using Xenko.Core.Mathematics;
using Xenko.Rendering;

namespace VL.Xenko.Rendering
{
    public interface IEffect
    {
        void SetEntityWorldMatrix(Matrix entityWorld);

        EffectInstance SetParameters(RenderView renderView, RenderDrawContext renderDrawContext);
    }
}
