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
using CommandList = Stride.Graphics.CommandList;
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
        private readonly SerialDisposable inputSubscription = new SerialDisposable();
        private IInputSource lastInputSource;
        private Int2 lastRenderTargetSize;
        private readonly InViewportUpstream viewportLayer = new InViewportUpstream();
        
        public ILayer Layer { get; set; }

        protected override void DrawCore(RenderDrawContext context)
        {
            if (Layer is null)
                return;

            var commandList = context.CommandList;
            var renderTarget = commandList.RenderTarget;

            // Fetch the skia render context (uses ANGLE -> DirectX11)
            var interopContext = GetInteropContext(context.GraphicsDevice);
            var skiaRenderContext = interopContext.SkiaRenderContext;

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

            //PrintCount($"Device ({nativeDevice.NativePointer}) enter", nativeDevice.NativePointer);
            //PrintCount($"Render target ({nativeTempRenderTarget.NativePointer}) enter", nativeTempRenderTarget.NativePointer);

            using (interopContext.Scoped(commandList))
            {
                var nativeTempRenderTarget = SharpDXInterop.GetNativeResource(renderTarget) as Texture2D;
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
                }
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
        static InteropContext GetInteropContext(GraphicsDevice graphicsDevice)
        {
            return graphicsDevice.GetOrCreateSharedData("VL.Stride.Skia.InteropContext", gd =>
            {
                if (SharpDXInterop.GetNativeDevice(gd) is Device device)
                {
                    // https://github.com/google/angle/blob/master/src/tests/egl_tests/EGLDeviceTest.cpp#L272
                    var angleDevice = Egl.eglCreateDeviceANGLE(Egl.EGL_D3D11_DEVICE_ANGLE, device.NativePointer, null);
                    if (angleDevice != default)
                    {
                        var d1 = device.QueryInterface<Device1>();
                        var contextState = d1.CreateDeviceContextState<Device1>(
                            CreateDeviceContextStateFlags.None, 
                            new[] { device.FeatureLevel }, 
                            out _);

                        return new InteropContext(SkiaRenderContext.New(angleDevice), d1.ImmediateContext1, contextState);
                    }
                }
                return null;
            });
        }

        sealed class InteropContext : IDisposable
        {
            public readonly SkiaRenderContext SkiaRenderContext;
            public readonly DeviceContext1 DeviceContext;
            public readonly DeviceContextState ContextState;

            public InteropContext(SkiaRenderContext skiaRenderContext, DeviceContext1 deviceContext, DeviceContextState contextState)
            {
                SkiaRenderContext = skiaRenderContext;
                DeviceContext = deviceContext;
                ContextState = contextState;
            }

            public ScopedDeviceContext Scoped(CommandList commandList)
            {
                return new ScopedDeviceContext(commandList, DeviceContext, ContextState);
            }

            public void Dispose()
            {
                ContextState.Dispose();
                SkiaRenderContext.Dispose();
            }
        }

        readonly struct ScopedDeviceContext : IDisposable
        {
            readonly CommandList commandList;
            readonly DeviceContext1 deviceContext;
            readonly DeviceContextState oldContextState;

            public ScopedDeviceContext(CommandList commandList, DeviceContext1 deviceContext, DeviceContextState contextState)
            {
                this.commandList = commandList;
                this.deviceContext = deviceContext;
                deviceContext.SwapDeviceContextState(contextState, out oldContextState);
            }

            public void Dispose()
            {
                // Ensure no references are held to the render targets (would prevent resize)
                var currentRenderTarget = commandList.RenderTarget;
                var currentDepthStencil = commandList.DepthStencilBuffer;
                commandList.UnsetRenderTargets();
                deviceContext.SwapDeviceContextState(oldContextState, out _);
                commandList.SetRenderTarget(currentDepthStencil, currentRenderTarget);

                // Doesn't work - why?
                //var renderTargets = deviceContext.OutputMerger.GetRenderTargets(8, out var depthStencilView);
                //deviceContext.OutputMerger.ResetTargets();
                //deviceContext.SwapDeviceContextState(oldContextState, out _);
                //deviceContext.OutputMerger.SetTargets(depthStencilView, renderTargets);
            }
        }
    }
}
