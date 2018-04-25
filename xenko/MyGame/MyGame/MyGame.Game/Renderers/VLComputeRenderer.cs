using SiliconStudio.Xenko.Rendering.Compositing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Core;

namespace MyGame 
{
    [Display("VL Compute Renderer")]
    public class VLComputeRenderer : SingleStageRenderer
    {
        protected override void CollectCore(RenderContext context)
        {
            base.CollectCore(context);
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            base.DrawCore(context, drawContext);
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
        }
    }
}
