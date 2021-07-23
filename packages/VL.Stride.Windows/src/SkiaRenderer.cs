using OpenTK.Graphics.ES20;
using SharpDX.Direct3D11;
using SkiaSharp;
using SkiaSharp.Views.GlesInterop;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Input;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using VL.Lib.Basics.Resources;
using VL.Skia;
using VL.Stride.Input;
using VL.Stride.Windows.WglInterop;
using PixelFormat = Stride.Graphics.PixelFormat;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;
using SkiaRenderContext = VL.Skia.RenderContext;

namespace VL.Stride.Windows
{
    /// <summary>
    /// Renders into a temporary texture which will then get drawn on the render target coming from downstream.
    /// </summary>
    public partial class SkiaRenderer : RendererBase
    {
        private readonly IResourceHandle<SkiaRenderContext> renderContextHandle;

        private readonly SerialDisposable inputSubscription = new SerialDisposable();
        private IInputSource lastInputSource;
        private Int2 lastRenderTargetSize;
        private Texture tempRenderTarget;
        private IntPtr? eglSurface;

        private readonly InViewportUpstream viewportLayer = new InViewportUpstream();

        public SkiaRenderer()
        {
            renderContextHandle = SkiaRenderContext.ForCurrentThread();

            //var glesContext = new GlesContext();
            //glesContext.MakeCurrent(IntPtr.Zero);
            //var skiaContext = GRContext.CreateGl(GRGlInterface.CreateAngle());
            //renderContextHandle = ResourceProvider.Return(new SkiaRenderContext(glesContext, skiaContext), x => x.Dispose()).GetHandle();
        }
        
        public ILayer Layer { get; set; }

        protected override void Destroy()
        {
            // TODO Destroy the EGL surface!

            renderContextHandle.Dispose();
            tempRenderTarget?.Dispose();
            base.Destroy();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            if (Layer is null)
                return;

            // Fetch the skia render context (uses ANGLE -> DirectX11)
            var skiaRenderContext = renderContextHandle?.Resource;
            if (skiaRenderContext is null)
                return;

            var commandList = context.CommandList;
            var renderTarget = commandList.RenderTarget;
            var glesContext = skiaRenderContext.GlesContext;
            var allocator = context.GraphicsContext.Allocator;

            // Subscribe to input events - in case we have many sinks we assume that there's only one input source active
            var renderTargetSize = new Int2(renderTarget.Width, renderTarget.Height);
            var inputSource = context.RenderContext.GetWindowInputSource();
            if (inputSource != lastInputSource || renderTargetSize != lastRenderTargetSize)
            {
                lastInputSource = inputSource;
                lastRenderTargetSize = renderTargetSize;
                inputSubscription.Disposable = SubscribeToInputSource(inputSource, context, canvas: null, skiaRenderContext.SkiaContext);

                this.tempRenderTarget?.Dispose();
                this.tempRenderTarget = null;
                glesContext.DestroySurface(this.eglSurface.Value);
                this.eglSurface = default;
            }

            var renderTargetDescription = TextureDescription.New2D(
                renderTarget.Width,
                renderTarget.Height,
                PixelFormat.B8G8R8A8_UNorm, // Typeless as well as R8G8B8A8 won't work for CreateSurfaceFromSharedHandle :(
                textureFlags: TextureFlags.ShaderResource | TextureFlags.RenderTarget,
                textureOptions: TextureOptions.Shared);

            var tempRenderTarget = this.tempRenderTarget ??= Texture.New(context.GraphicsDevice, renderTargetDescription);

            // This code path works, simple drawing commands work but SkSurface.Flush causes device lost :(
            //var nativeTempRenderTarget = SharpDXInterop.GetNativeResource(tempRenderTarget) as Texture2D;
            //var eglSurface = this.eglSurface ??= glesContext.CreateSurfaceFromClientBuffer(nativeTempRenderTarget.NativePointer);

            // Only working solution was to use shared handles
            var eglSurface = this.eglSurface ??= glesContext.CreateSurfaceFromSharedHandle(tempRenderTarget.Width, tempRenderTarget.Height, tempRenderTarget.SharedHandle);
            try
            {
                // Make the surface current (becomes default FBO)
                skiaRenderContext.MakeCurrent(eglSurface);

                // Uncomment for debugging
                // SimpleStupidTestRendering();

                // Setup a skia surface around the currently set render target
                using var surface = CreateSkSurface(skiaRenderContext.SkiaContext, renderTarget);

                // Render
                var canvas = surface.Canvas;
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Clear();

                    var viewport = context.RenderContext.ViewportState.Viewport0;
                    canvas.ClipRect(SKRect.Create(viewport.X, viewport.Y, viewport.Width, viewport.Height));
                    viewportLayer.Update(Layer, SKRect.Create(viewport.X, viewport.Y, viewport.Width, viewport.Height), CommonSpace.PixelTopLeft, out var layer);

                    layer.Render(CallerInfo.InRenderer(renderTarget.Width, renderTarget.Height, canvas, skiaRenderContext.SkiaContext));
                }

                // Flush before drawing on our render target
                surface.Flush();
                Gles.glFlush();

                if (context.GraphicsDevice.ColorSpace == ColorSpace.Linear)
                {
                    // Sadly we can't declare our temp target as Typeless - CreateSurfaceFromSharedHandle will fail
                    // Therefor we need to adjust the color space manually
                    // Would it be cheaper to draw with shader doing the conversion manually?
                    var description = tempRenderTarget.Description;
                    // Switch the color space
                    description.Format = description.Format.ToSRgb();
                    // Ensure we're not asking for a shared texture
                    description.Options = default;
                    var tempRenderTarget2 = PushScopedResource(allocator.GetTemporaryTexture2D(description));
                    commandList.Copy(tempRenderTarget, tempRenderTarget2);
                    context.GraphicsContext.DrawTexture(tempRenderTarget2, BlendStates.AlphaBlend);
                }
                else
                {
                    context.GraphicsContext.DrawTexture(tempRenderTarget, BlendStates.AlphaBlend);
                }
            }
            finally
            {
                //glesContext.DestroySurface(eglSurface);
            }
        }

        static SKSurface CreateSkSurface(GRContext context, Texture texture)
        {
            var colorType = SKColorType.Bgra8888;
            Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
            Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
            Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
            var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
            if (samples > maxSamples)
                samples = maxSamples;

            var glInfo = new GRGlFramebufferInfo(
                fboId: (uint)framebuffer,
                format: colorType.ToGlSizedFormat());

            using var renderTarget = new GRBackendRenderTarget(
                width: texture.Width,
                height: texture.Height,
                sampleCount: samples,
                stencilBits: stencil,
                glInfo: glInfo);

            return SKSurface.Create(context, renderTarget, GRSurfaceOrigin.TopLeft, colorType);
        }

        //static void SimpleStupidTestRendering()
        //{
        //    if (++i % 2 == 0)
        //        Gles.glClearColor(1, 0, 0, 1);
        //    else
        //        Gles.glClearColor(1, 1, 0, 1);

        //    Gles.glClear(Gles.GL_COLOR_BUFFER_BIT);
        //    Gles.glFlush();
        //}
        //int i = 0;

        // Works, also simple Gles drawing commands work but SkSurface.Flush causes device lost :(
        static SkiaRenderContext GetSkiaRenderContext(GraphicsDevice graphicsDevice)
        {
            return graphicsDevice.GetOrCreateSharedData("VL.Stride.Skia.RenderContext", gd =>
            {
                if (SharpDXInterop.GetNativeDevice(gd) is SharpDX.Direct3D11.Device device)
                {
                    // https://github.com/google/angle/blob/master/src/tests/egl_tests/EGLDeviceTest.cpp#L272
                    var angleDevice = Egl.eglCreateDeviceANGLE(Egl.EGL_D3D11_DEVICE_ANGLE, device.NativePointer, null);
                    if (angleDevice != default)
                        return SkiaRenderContext.ForDevice(angleDevice);
                }
                return null;
            })?.Resource;
        }
    }
}
