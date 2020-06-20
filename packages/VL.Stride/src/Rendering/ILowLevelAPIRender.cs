using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    public interface ILowLevelAPIRender
    {
        void Initialize();

        void Collect(RenderContext context);

        void Extract();

        void Prepare(RenderDrawContext context);

        void SetEntityWorldMatrix(Matrix entityWorld);

        void Draw(RenderContext renderContext, RenderDrawContext drawContext, RenderView renderView, RenderViewStage renderViewStage, CommandList commandList);
    }
}
