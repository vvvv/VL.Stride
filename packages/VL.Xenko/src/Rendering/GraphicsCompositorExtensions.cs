using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Rendering;
using Xenko.Rendering.Compositing;
using XenkoMath = Xenko.Core.Mathematics;

namespace VL.Xenko.Rendering
{
    public static class GraphicsCompositorExtensions
    {
        public static GraphicsCompositor GetFirstForwardRenderer(this GraphicsCompositor compositor, out ForwardRenderer forwardRenderer)
        {
            var topChildRenderer = ((SceneCameraRenderer)compositor.Game).Child;
            forwardRenderer = (topChildRenderer as SceneRendererCollection)?.Children.OfType<ForwardRenderer>().FirstOrDefault() ?? (ForwardRenderer)topChildRenderer;
            return compositor;
        }

        public static ForwardRenderer SetClearOptions(this ForwardRenderer forwardRenderer, Color4 color, float depth = 1, byte stencilValue = 0, ClearRendererFlags clearFlags = ClearRendererFlags.ColorAndDepth)
        {
            forwardRenderer.Clear.Color = new XenkoMath.Color4(color.Red, color.Green, color.Blue, color.Alpha);
            forwardRenderer.Clear.ClearFlags = clearFlags;
            forwardRenderer.Clear.Depth = depth;
            forwardRenderer.Clear.Stencil = stencilValue;
            return forwardRenderer;
        }
    }
}
