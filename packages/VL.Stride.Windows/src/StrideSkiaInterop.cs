using SharpDX.Direct3D11;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaRenderContext = VL.Skia.RenderContext;
using CommandList = Stride.Graphics.CommandList;
using VL.Skia.Egl;
using SharpDX;

namespace VL.Stride.Windows
{
    class StrideSkiaInterop
    {
        // Works, also simple Gles drawing commands work but SkSurface.Flush causes device lost :(
        public static InteropContext GetInteropContext(GraphicsDevice graphicsDevice, int msaaSamples)
        {
            return graphicsDevice.GetOrCreateSharedData($"VL.Stride.Skia.InteropContext{msaaSamples}", gd =>
            {
                if (SharpDXInterop.GetNativeDevice(gd) is Device device)
                {
                    // https://github.com/google/angle/blob/master/src/tests/egl_tests/EGLDeviceTest.cpp#L272
                    var angleDevice = EglDevice.FromD3D11(device.NativePointer);
                    var d1 = device.QueryInterface<Device1>();
                    var contextState = d1.CreateDeviceContextState<Device1>(
                        CreateDeviceContextStateFlags.None,
                        new[] { device.FeatureLevel },
                        out _);

                    return new InteropContext(SkiaRenderContext.New(angleDevice, msaaSamples), d1.ImmediateContext1, contextState);
                }
                return null;
            });
        }

        public sealed class InteropContext : IDisposable
        {
            public readonly SkiaRenderContext SkiaRenderContext;
            public readonly DeviceContext1 DeviceContext;
            public DeviceContextState ContextState;

            public InteropContext(SkiaRenderContext skiaRenderContext, DeviceContext1 deviceContext, DeviceContextState contextState)
            {
                SkiaRenderContext = skiaRenderContext;
                DeviceContext = deviceContext;
                ContextState = contextState;
            }

            public ScopedDeviceContext Switch(CommandList commandList)
            {
                return new ScopedDeviceContext(this, commandList, DeviceContext);
            }

            public ScopedDeviceContext1 Switch2()
            {
                return new ScopedDeviceContext1(DeviceContext, ContextState);
            }

            public void Dispose()
            {
                ContextState.Dispose();
                SkiaRenderContext.Dispose();
            }
        }

        public readonly struct ScopedDeviceContext : IDisposable
        {
            [ThreadStatic]
            static int counter;

            readonly InteropContext ctx;
            readonly CommandList commandList;
            readonly DeviceContext1 deviceContext;

            public ScopedDeviceContext(InteropContext ctx, CommandList commandList, DeviceContext1 deviceContext)
            {
                counter++;

                this.ctx = ctx;
                this.commandList = commandList;
                this.deviceContext = deviceContext;
                deviceContext.SwapDeviceContextState(ctx.ContextState, out ctx.ContextState);
            }

            public void Dispose()
            {
                if (--counter == 0)
                {
                    // Ensure no references are held to the render targets (would prevent resize)
                    var currentRenderTarget = commandList.RenderTarget;
                    var currentDepthStencil = commandList.DepthStencilBuffer;
                    commandList.UnsetRenderTargets();
                    deviceContext.SwapDeviceContextState(ctx.ContextState, out ctx.ContextState);
                    commandList.SetRenderTarget(currentDepthStencil, currentRenderTarget);
                }
                else
                {
                    deviceContext.SwapDeviceContextState(ctx.ContextState, out ctx.ContextState);
                }

                //// Doesn't work - why?
                //var renderTargets = deviceContext.OutputMerger.GetRenderTargets(8, out var depthStencilView);
                //deviceContext.OutputMerger.ResetTargets();
                //deviceContext.SwapDeviceContextState(ctx.ContextState, out ctx.ContextState);
                //deviceContext.OutputMerger.SetTargets(depthStencilView, renderTargets);
            }
        }

        public readonly struct ScopedDeviceContext1 : IDisposable
        {
            readonly DeviceContext1 deviceContext;
            readonly DeviceContextState oldContextState;

            public ScopedDeviceContext1(DeviceContext1 deviceContext, DeviceContextState contextState)
            {
                this.deviceContext = deviceContext;
                deviceContext.SwapDeviceContextState(contextState, out oldContextState);
            }

            public void Dispose()
            {
                deviceContext.SwapDeviceContextState(oldContextState, out _);
            }
        }
    }
}
