using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform.Windows;
using SharpDX.Direct3D11;
using Stride.Core;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VL.Stride.Windows.WglInterop
{
    sealed class InteropContext : IDisposable
    {
        // Use the native pointer for caching. Stride textures might stay the same while their internal pointer changes.
        readonly Dictionary<IntPtr, InteropTexture> textures = new Dictionary<IntPtr, InteropTexture>();

        readonly DeviceContext1 deviceContext;
        readonly DeviceContextState contextState;

        public InteropContext(OpenTK.Graphics.GraphicsContext graphicsContext, INativeWindow window, Device device)
        {
            GraphicsContext = graphicsContext;
            Window = window;
            Device = device;

            // From https://hg.mozilla.org/mozilla-central/file/tip/gfx/gl/SharedSurfaceD3D11Interop.cpp to workaround AMD driver bug
            var device1 = device.QueryInterface<Device1>();
            deviceContext = device1.ImmediateContext1;
            contextState = device1.CreateDeviceContextState<SharpDX.Direct3D10.Device>(
                CreateDeviceContextStateFlags.None, 
                new [] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, 
                out _);

            DeviceHandle = Wgl.DXOpenDeviceNV(device.NativePointer);
        }

        public OpenTK.Graphics.GraphicsContext GraphicsContext { get; }

        public INativeWindow Window { get; }

        public Device Device { get; }
        public IntPtr DeviceHandle { get; }

        public void MakeCurrent()
        {
            GraphicsContext.MakeCurrent(Window.WindowInfo);
        }

        public void Dispose()
        {
            MakeCurrent();

            foreach (var t in textures.Values.ToArray())
                t?.Dispose();

            contextState.Dispose();

            if (DeviceHandle != IntPtr.Zero)
                Wgl.DXCloseDeviceNV(DeviceHandle);

            GraphicsContext.Dispose();

            Window.Dispose();
        }

        public InteropTexture GetInteropTexture(Texture texture)
        {
            if (texture is null)
                return null;
            if (DeviceHandle == IntPtr.Zero)
                return null;

            if (SharpDXInterop.GetNativeResource(texture) is Texture2D dxTexture)
            {
                lock (textures)
                {
                    if (!textures.TryGetValue(dxTexture.NativePointer, out var interopTexture))
                        textures.Add(dxTexture.NativePointer, interopTexture = new InteropTexture(this, texture, dxTexture.NativePointer));
                    return interopTexture;
                }
            }

            return null;
        }

        internal void Remove(InteropTexture interopTexture)
        {
            lock (textures)
            {
                textures.Remove(interopTexture.DxTexture);
            }
        }

        public bool Lock(InteropTexture[] textures)
        {
            unsafe
            {
                void** objects = stackalloc void*[textures.Length];

                for (int i = 0; i < textures.Length; i++)
                {
                    var handle = textures[i].Handle;
                    if (handle == IntPtr.Zero)
                        return false;

                    objects[i] = handle.ToPointer();
                }

                return Wgl.DXLockObjectsNV(DeviceHandle, textures.Length, new IntPtr(objects));
            }
        }

        public bool Unlock(InteropTexture[] textures)
        {
            unsafe
            {
                void** objects = stackalloc void*[textures.Length];

                for (int i = 0; i < textures.Length; i++)
                {
                    var handle = textures[i].Handle;
                    if (handle == IntPtr.Zero)
                        return false;

                    objects[i] = handle.ToPointer();
                }

                return Wgl.DXUnlockObjectsNV(DeviceHandle, textures.Length, new IntPtr(objects));
            }
        }

        public ScopedContextState WithScopedContext() => new ScopedContextState(deviceContext, contextState);

        public readonly struct ScopedContextState : IDisposable
        {
            private readonly DeviceContext1 deviceContext;
            private readonly DeviceContextState oldContextState;

            public ScopedContextState(DeviceContext1 deviceContext, DeviceContextState contextState)
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
