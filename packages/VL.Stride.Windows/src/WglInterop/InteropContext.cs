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

        public InteropContext(OpenTK.Graphics.GraphicsContext graphicsContext, INativeWindow window, Device device)
        {
            GraphicsContext = graphicsContext;
            Window = window;
            DeviceHandle = Wgl.DXOpenDeviceNV(device.NativePointer);
        }

        public OpenTK.Graphics.GraphicsContext GraphicsContext { get; }

        public INativeWindow Window { get; }

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

            if (DeviceHandle != IntPtr.Zero)
                Wgl.DXCloseDeviceNV(DeviceHandle);

            GraphicsContext.Dispose();

            Window.Dispose();
        }

        public InteropTexture GetInteropTexture(Texture texture)
        {
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
    }
}
