using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharpDX.Direct3D11;
using SkiaSharp;
using Stride.Core;
using Stride.Graphics;
using Stride.Input;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using VL.Skia;
using VL.Stride.Input;
using VL.Stride.Windows.WglInterop;
using PixelFormat = Stride.Graphics.PixelFormat;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;

namespace VL.Stride.Windows
{
    public partial class SkiaRenderer : IGraphicsRendererBase
    {
        private readonly SerialDisposable inputSubscription = new SerialDisposable();
        private IInputSource lastInputSource;
        private SharedSurface lastSharedSurface;
        private readonly InViewportUpstream viewportLayer = new InViewportUpstream();
        
        public ILayer Layer { get; set; }

        public unsafe void Draw(RenderDrawContext context)
        {
            if (Layer is null)
                return;

            // Try to build a skia context - will only work on DirectX11 backend
            var skiaContext = GetSkiaContext(context.GraphicsDevice);
            if (skiaContext is null)
                return;

            // Make our GL context current
            skiaContext.MakeCurrent();

            // Try to setup a skia surface around the currently set render target
            var commandList = context.CommandList;
            var sharedSurface = skiaContext.GetSharedSurface(commandList.RenderTarget, commandList.DepthStencilBuffer);
            if (sharedSurface is null)
                return;

            // Subscribe to input events - in case we have many sinks we assume that there's only one input source active
            var inputSource = context.RenderContext.GetWindowInputSource();
            if (inputSource != lastInputSource || sharedSurface != lastSharedSurface)
            {
                lastInputSource = inputSource;
                lastSharedSurface = sharedSurface;
                inputSubscription.Disposable = SubscribeToInputSource(inputSource, context, sharedSurface);
            }

            // Lock the backbuffer
            using (sharedSurface.Lock())
            {
                // For testing
                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, sharedSurface.Framebuffer);

                //GL.Begin(PrimitiveType.Triangles);
                //GL.Color3(1f, 0f, 0f);
                //GL.Vertex2(0f, -0.5f);
                //GL.Color3(0f, 1f, 0f);
                //GL.Vertex2(0.5f, 0.5f);
                //GL.Color3(0f, 0f, 1f);
                //GL.Vertex2(-0.5f, 0.5f);
                //GL.End();

                var surface = sharedSurface.Surface;
                var canvas = surface.Canvas;

                var viewport = context.RenderContext.ViewportState.Viewport0;

                canvas.Save();

                try
                {
                    canvas.ClipRect(SKRect.Create(viewport.X, viewport.Y, viewport.Width, viewport.Height));
                    viewportLayer.Update(Layer, SKRect.Create(viewport.X, viewport.Y, viewport.Width, viewport.Height), CommonSpace.PixelTopLeft, out var layer);

                    layer.Render(CallerInfo.InRenderer(sharedSurface.Width, sharedSurface.Height, sharedSurface.Surface.Canvas, sharedSurface.SkiaContext.GraphicsContext));
                }
                finally
                {
                    canvas.Restore();
                    canvas.Flush();
                }
            }
        }

        static InteropContext GetInteropContext(GraphicsDevice graphicsDevice)
        {
            return graphicsDevice.GetOrCreateSharedData("VL.Stride.Windows.WglInterop.InteropContext", gd =>
            {
                if (SharpDXInterop.GetNativeDevice(gd) is SharpDX.Direct3D11.Device device)
                {
                    Toolkit.Init(new ToolkitOptions() { Backend = PlatformBackend.PreferNative });

                    var window = new NativeWindow();
                    var glContext = new OpenTK.Graphics.GraphicsContext(GraphicsMode.Default, window.WindowInfo);
                    glContext.LoadAll();

                    return new InteropContext(glContext, window, device);
                }
                return null;
            });
        }

        static SkiaContext GetSkiaContext(GraphicsDevice graphicsDevice)
        {
            return graphicsDevice.GetOrCreateSharedData("VL.Stride.Windows.SkiaContext", gd =>
            {
                var interopContext = GetInteropContext(gd);
                if (interopContext != null)
                {
                    interopContext.MakeCurrent();
                    var grContext = GRContext.CreateGl();
                    return new SkiaContext(interopContext, grContext);
                }
                return null;
            });
        }

        sealed class SkiaContext : IDisposable
        {
            public SkiaContext(InteropContext interopContext, GRContext graphicsContext)
            {
                InteropContext = interopContext;
                GraphicsContext = graphicsContext;
            }

            public InteropContext InteropContext { get; }

            public GRContext GraphicsContext { get; }

            public void MakeCurrent()
            {
                InteropContext.MakeCurrent();
            }

            public void Dispose()
            {
                InteropContext.MakeCurrent();

                foreach (var e in surfaces.ToArray())
                    e.Value?.Dispose();
                surfaces.Clear();

                GraphicsContext.Dispose();
            }

            public SharedSurface GetSharedSurface(Texture colorTexture, Texture depthTexture)
            {
                var dxColor = SharpDXInterop.GetNativeResource(colorTexture) as Texture2D;
                if (dxColor != null)
                {
                    lock (surfaces)
                    {
                        var dxDepth = default(Texture2D);
                        if (depthTexture != null)
                            dxDepth = SharpDXInterop.GetNativeResource(depthTexture) as Texture2D;
                        var key = (dxColor.NativePointer, dxDepth?.NativePointer ?? IntPtr.Zero);
                        if (!surfaces.TryGetValue(key, out var surface))
                            surfaces.Add(key, surface = Create());
                        return surface;
                    }
                }
                return null;

                SharedSurface Create()
                {
                    var interopColor = InteropContext.GetInteropTexture(colorTexture);
                    if (interopColor is null)
                        return null;

                    var interopDepth = InteropContext.GetInteropTexture(depthTexture);
                    if (interopDepth is null && depthTexture != null)
                        return null;

                    var surface = SharedSurface.New(this, interopColor, interopDepth);
                    if (surface is null)
                    {
                        // Failed to create skia surface, get rid of the WGL interop handles
                        interopColor?.Dispose();
                        interopDepth?.Dispose();
                        return null;
                    }

                    return surface;
                }
            }
            // Use the native pointer for caching. Stride textures might stay the same while their internal pointer changes.
            readonly Dictionary<(IntPtr, IntPtr), SharedSurface> surfaces = new Dictionary<(IntPtr, IntPtr), SharedSurface>();

            internal void Remove(SharedSurface sharedSurface)
            {
                lock (surfaces)
                {
                    surfaces.Remove((sharedSurface.Color.DxTexture, sharedSurface.Depth?.DxTexture ?? IntPtr.Zero));
                }
            }
        }

        sealed class SharedSurface : IDisposable
        {
            public static SharedSurface New(SkiaContext skiaContext, InteropTexture color, InteropTexture depth)
            {
                var framebuffer = (uint)GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, color.Name);
                if (depth != null)
                {
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depth.Name);
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, depth.Name);
                }

                var pixelFormat = color.Texture.Format;
                var colorType = GetColorType(pixelFormat);
                var glInfo = new GRGlFramebufferInfo(framebuffer, colorType.ToGlSizedFormat());
                var renderTarget = new GRBackendRenderTarget(color.Texture.Width, color.Texture.Height, sampleCount: 0, stencilBits: 8, glInfo);
                var colorspace =
                    pixelFormat.IsSRgb() ? SKColorSpace.CreateSrgb() :
                    pixelFormat.IsHDR() ? SKColorSpace.CreateSrgbLinear() :
                    default;
                var skContext = skiaContext.GraphicsContext;
                var skiaSurface = SKSurface.Create(skContext, renderTarget, GRSurfaceOrigin.TopLeft, colorType, colorspace: colorspace);
                if (skiaSurface != null)
                {
                    return new SharedSurface(skiaContext, color, depth, framebuffer, skiaSurface);
                }
                else
                {
                    GL.DeleteFramebuffer(framebuffer);
                    return null;
                }
            }

            public readonly uint Framebuffer;
            private readonly InteropTexture[] Textures;
            private bool Disposed;

            SharedSurface(SkiaContext skiaContext, InteropTexture color, InteropTexture depth, uint framebuffer, SKSurface skiaSurface)
            {
                SkiaContext = skiaContext;
                if (depth != null)
                    Textures = new InteropTexture[] { color, depth };
                else
                    Textures = new InteropTexture[] { color };
                Framebuffer = framebuffer;
                Surface = skiaSurface;

                foreach (var t in Textures)
                    t.Texture.Destroyed += Texture_Destroyed;
            }

            private void Texture_Destroyed(object sender, EventArgs e)
            {
                Dispose();
            }

            ~SharedSurface()
            {
                Dispose(false);
            }

            public SkiaContext SkiaContext { get; }

            public InteropTexture Color => Textures[0];

            public InteropTexture Depth => Textures.ElementAtOrDefault(1);

            public SKSurface Surface { get; }

            public int Width => Color.Texture.Width;

            public int Height => Color.Texture.Height;

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Dispose(true);
            }

            void Dispose(bool disposing)
            {
                if (!Disposed)
                {
                    Disposed = true;

                    foreach (var t in Textures)
                        t.Texture.Destroyed -= Texture_Destroyed;

                    SkiaContext.MakeCurrent();
                    SkiaContext.Remove(this);
                    Surface.Dispose();
                    GL.DeleteFramebuffer(Framebuffer);
                }
            }

            static SKColorType GetColorType(PixelFormat format)
            {
                switch (format)
                {
                    case PixelFormat.B8G8R8A8_UNorm:
                    case PixelFormat.B8G8R8A8_UNorm_SRgb:
                        return SKColorType.Bgra8888;
                    case PixelFormat.R8G8B8A8_UNorm:
                    case PixelFormat.R8G8B8A8_UNorm_SRgb:
                        return SKColorType.Rgba8888;
                    case PixelFormat.R10G10B10A2_UNorm:
                        return SKColorType.Rgba1010102;
                    case PixelFormat.R16G16B16A16_Float:
                        return SKColorType.RgbaF16;
                    case PixelFormat.R32G32B32A32_Float:
                        return SKColorType.RgbaF32;
                    case PixelFormat.R16G16_Float:
                        return SKColorType.RgF16;
                    case PixelFormat.A8_UNorm:
                        return SKColorType.Alpha8;
                    case PixelFormat.R8_UNorm:
                        return SKColorType.Gray8;
                    case PixelFormat.B5G6R5_UNorm:
                        return SKColorType.Rgb565;
                    default:
                        return SKColorType.Unknown;
                }
            }

            public LockAndUnlock Lock()
            {
                return new LockAndUnlock(this);
            }

            public struct LockAndUnlock : IDisposable
            {
                readonly SharedSurface Surface;

                public LockAndUnlock(SharedSurface surface)
                {
                    Surface = surface;
                    surface.SkiaContext.InteropContext.Lock(surface.Textures);
                }

                public void Dispose()
                {
                    Surface.SkiaContext.InteropContext.Unlock(Surface.Textures);
                }
            }
        }
    }
}
