using System;
using System.Collections.Generic;
using VL.Lib.Collections;
using VL.Stride.Rendering;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Core;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// Groups a sequence of <see cref="IGraphicsRendererBase"/> into one.
    /// </summary>
    public sealed class GraphicsRendererGroup : IGraphicsRendererBase
    {
        Spread<IGraphicsRendererBase> upstreamRenderers = Spread<IGraphicsRendererBase>.Empty;

        public void Update(Spread<IGraphicsRendererBase> input)
        {
            upstreamRenderers = input;
        }

        void IGraphicsRendererBase.Draw(RenderDrawContext context)
        {
            foreach (var upstreamLayer in upstreamRenderers)
                upstreamLayer?.Draw(context);
        }
    }

    public interface IRenderContextModifier
    {
        IDisposable ModifyRenderContext(RenderContext renderContext);
    }

    public class SetParentTransformation : IRenderContextModifier
    {
        private Matrix currentParentTransformation;

        public void Update (Matrix parentTransformation)
        {
            currentParentTransformation = parentTransformation;
        }

        public IDisposable ModifyRenderContext(RenderContext renderContext)
        {
            return renderContext.PushTagAndRestore(EntityRendererRenderFeature.CurrentParentTransformation, currentParentTransformation);
        }
    }


    public class RenderContextRenderer : IGraphicsRendererBase
    {
        IGraphicsRendererBase upstreamRenderer;
        IRenderContextModifier modifier;

        public void Update(IGraphicsRendererBase input, IRenderContextModifier contextModifier)
        {
            upstreamRenderer = input;
            modifier = contextModifier;
        }

        public void Draw(RenderDrawContext context)
        {
            using (modifier.ModifyRenderContext(context.RenderContext))
            {
                upstreamRenderer.Draw(context);
            }
        }
    }
}
