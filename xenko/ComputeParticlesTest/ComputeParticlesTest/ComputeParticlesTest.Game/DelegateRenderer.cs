using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Graphics;

namespace ComputeParticlesTest
{
    public class DelegateRenderer : SceneRendererBase
    {
        public Action InitializeAction { get; set; }
        public Action<RenderContext> CollectAction { get; set; }
        public Action<RenderContext, RenderDrawContext, CommandList> DrawAction { get; set; }

        protected override void CollectCore(RenderContext context)
        {
            base.CollectCore(context);
            CollectAction?.Invoke(context);
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
            InitializeAction?.Invoke();
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            DrawAction?.Invoke(context, drawContext, drawContext.CommandList);
        }
    }
}
