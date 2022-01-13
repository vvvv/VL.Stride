using Stride.Rendering;
using Stride.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Lib.IO.Notifications;
using VL.Skia;
using VL.Skia.Egl;

namespace VL.Stride.Windows
{
    public sealed class StrideLayer : ILayer
    {
        public IGraphicsRendererBase Input { get; set; }

        public bool Notify(INotification notification, CallerInfo caller)
        {
            return false;
        }

        public void Render(CallerInfo caller)
        {
            var context = caller.GetRenderDrawContext();
            if (context is null)
                throw new InvalidOperationException("Downstream renderer must be driven by Stride");

            var device = context.GraphicsDevice;
            var commandList = context.CommandList;
            var renderTarget = commandList.RenderTarget;

            // Fetch the skia render context (uses ANGLE -> DirectX11)
            var interopContext = StrideSkiaInterop.GetInteropContext(device, (int)renderTarget.MultisampleCount);
            var skiaRenderContext = interopContext.SkiaRenderContext;

            // Enforce execution of queued draw commands
            caller.Canvas.Flush();

            using (interopContext.Switch(commandList))
            {
                var t = caller.Transformation;

                var rv = context.RenderContext.RenderView;
                var viewSize = rv.ViewSize;

                var p = rv.Projection;
                var v = rv.View;
                var vp = rv.ViewProjection;

                rv.Projection = Matrix.Identity;
                rv.View = new Matrix(
                    (t.ScaleX / viewSize.X) * 2f, 
                    -(t.SkewY / viewSize.Y) * 2f, 
                    0f, 
                    t.Persp0, 

                    (t.SkewX / viewSize.X) * 2f, 
                    -(t.ScaleY / viewSize.Y) * 2f, 
                    0f, 
                    t.Persp1, 
                    
                    0f, 
                    0f, 
                    1f, 
                    0f, 
                    
                    2f * ((t.TransX / viewSize.X) - 0.5f), 
                    2f * (0.5f - (t.TransY / viewSize.Y)), 
                    0f, 
                    t.Persp2);

                rv.ViewProjection = rv.View;

                try
                {
                    // TODO: Apply callerinfo matrix to renderview
                    Input?.Draw(context);
                }
                finally
                {
                    rv.Projection = p;
                    rv.View = v;
                    rv.ViewProjection = vp;
                }
            }
        }
    }

    static class CallerInfoExtensions
    {
        public static RenderDrawContext GetRenderDrawContext(this CallerInfo callerInfo)
        {
            return callerInfo.RenderDrawContext as RenderDrawContext;
        }
    }
}
