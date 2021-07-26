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
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
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
        private IResourceHandle<SkiaRenderContext> renderContextHandle;
        private Device1 d1;
        private readonly SerialDisposable inputSubscription = new SerialDisposable();
        private IInputSource lastInputSource;
        private Int2 lastRenderTargetSize;
        private DeviceContextState contextStateForAngle;
        private readonly InViewportUpstream viewportLayer = new InViewportUpstream();

        public SkiaRenderer()
        {
            //var glesContext = new GlesContext();
            //glesContext.MakeCurrent(IntPtr.Zero);
            //var skiaContext = GRContext.CreateGl(GRGlInterface.CreateAngle());
            //renderContextHandle = ResourceProvider.Return(new SkiaRenderContext(glesContext, skiaContext), x => x.Dispose()).GetHandle();
        }
        
        public ILayer Layer { get; set; }

        protected override void InitializeCore()
        {
            var nativeDevice = SharpDXInterop.GetNativeDevice(GraphicsDevice) as SharpDX.Direct3D11.Device;
            var angleDevice = Egl.eglCreateDeviceANGLE(Egl.EGL_D3D11_DEVICE_ANGLE, nativeDevice.NativePointer, null);
            renderContextHandle = SkiaRenderContext.ForDevice(angleDevice);

            d1 = nativeDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
            contextStateForAngle = d1.CreateDeviceContextState<SharpDX.Direct3D11.Device1>(SharpDX.Direct3D11.CreateDeviceContextStateFlags.None, new[] { nativeDevice.FeatureLevel }, out var chosen);

            base.InitializeCore();
        }

        protected override void Destroy()
        {
            renderContextHandle.Dispose();
            base.Destroy();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            if (Layer is null)
                return;

            var commandList = context.CommandList;
            var renderTarget = commandList.RenderTarget;

            // Fetch the skia render context (uses ANGLE -> DirectX11)

            var skiaRenderContext = renderContextHandle.Resource;

            var glesContext = skiaRenderContext.GlesContext;

            // Subscribe to input events - in case we have many sinks we assume that there's only one input source active
            var renderTargetSize = new Int2(renderTarget.Width, renderTarget.Height);
            var inputSource = context.RenderContext.GetWindowInputSource();
            if (inputSource != lastInputSource || renderTargetSize != lastRenderTargetSize)
            {
                lastInputSource = inputSource;
                lastRenderTargetSize = renderTargetSize;
                inputSubscription.Disposable = SubscribeToInputSource(inputSource, context, canvas: null, skiaRenderContext.SkiaContext);
            }

            var nativeDevice = SharpDXInterop.GetNativeDevice(GraphicsDevice) as SharpDX.Direct3D11.Device;
            var nativeTempRenderTarget = SharpDXInterop.GetNativeResource(renderTarget) as Texture2D;
            //PrintCount($"Device ({nativeDevice.NativePointer}) enter", nativeDevice.NativePointer);
            //PrintCount($"Render target ({nativeTempRenderTarget.NativePointer}) enter", nativeTempRenderTarget.NativePointer);

            d1.ImmediateContext1.SwapDeviceContextState(contextStateForAngle, out var previous);

            // Only working solution was to use shared handles
            var eglSurface = glesContext.CreateSurfaceFromClientBuffer(nativeTempRenderTarget.NativePointer);
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
                    var viewport = context.RenderContext.ViewportState.Viewport0;
                    canvas.ClipRect(SKRect.Create(viewport.X, viewport.Y, viewport.Width, viewport.Height));
                    viewportLayer.Update(Layer, SKRect.Create(viewport.X, viewport.Y, viewport.Width, viewport.Height), CommonSpace.PixelTopLeft, out var layer);

                    layer.Render(CallerInfo.InRenderer(renderTarget.Width, renderTarget.Height, canvas, skiaRenderContext.SkiaContext));
                }

                // Flush before drawing on our render target
                surface.Flush();
            }
            finally
            {
                glesContext.DestroySurface(eglSurface);
                glesContext.MakeCurrent(default);
                d1.ImmediateContext1.SwapDeviceContextState(previous, out contextStateForAngle);
            }

            //PrintCount("Device exit", nativeDevice.NativePointer);
            //PrintCount("Render target exit", nativeTempRenderTarget.NativePointer);
        }

        static void PrintCount(string name, IntPtr unkown)
        {
            Marshal.AddRef(unkown);
            var count = Marshal.Release(unkown);
            Trace.TraceInformation($"{name}: {count}");
        }

        static SKSurface CreateSkSurface(GRContext context, Texture texture)
        {
            var colorType = SKColorType.Rgba8888;
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

            return SKSurface.Create(context, renderTarget, GRSurfaceOrigin.TopLeft, colorType, colorspace: SKColorSpace.CreateSrgbLinear());
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
