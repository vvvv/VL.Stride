using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Rendering;
using Xenko.Rendering.Compositing;
using Xenko.Graphics;

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
