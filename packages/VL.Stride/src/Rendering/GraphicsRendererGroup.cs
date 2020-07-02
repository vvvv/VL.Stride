using System;
using System.Collections.Generic;
using VL.Lib.Collections;
using VL.Stride.Rendering;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;

namespace VL.Stride.Layer
{
    /// <summary>
    /// Groups a sequence of <see cref="IGraphicsRendererBase"/> into one.
    /// </summary>
    public sealed class GraphicsRendererGroup : IGraphicsRendererBase
    {
        Spread<IGraphicsRendererBase> upstreamRenderers = Spread<IGraphicsRendererBase>.Empty;

        public IGraphicsRendererBase Update(Spread<IGraphicsRendererBase> input)
        {
            upstreamRenderers = input;
            return this;
        }

        void IGraphicsRendererBase.Draw(RenderDrawContext context)
        {
            foreach (var upstreamLayer in upstreamRenderers)
                upstreamLayer?.Draw(context);
        }
    }
}
