using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;
using Stride.Graphics;
using System;

namespace VL.Stride.Windows.WglInterop
{
    sealed class InteropTexture : IDisposable
    {
        internal InteropTexture(InteropContext context, Texture texture, IntPtr dxTexture)
        {
            Context = context;
            Texture = texture;
            DxTexture = dxTexture;

            Name = (uint)GL.GenRenderbuffer();

            RegisterTexture();
        }

        public InteropContext Context { get; }

        public Texture Texture { get; }

        public IntPtr DxTexture { get; }

        public IntPtr Handle { get; private set; }

        public uint Name { get; }

        void RegisterTexture()
        {
            if (Handle == IntPtr.Zero)
            {
                Handle = Wgl.DXRegisterObjectNV(
                    Context.DeviceHandle,
                    DxTexture,
                    Name,
                    (uint)OpenTK.Graphics.OpenGL.All.Renderbuffer,
                    WGL_NV_DX_interop.AccessReadWrite);
            }
        }

        void UnregisterTexture()
        {
            if (Handle != IntPtr.Zero && Wgl.DXUnregisterObjectNV(Context.DeviceHandle, Handle))
            {
                Handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Context.MakeCurrent();

            UnregisterTexture();

            GL.DeleteRenderbuffer(Name);
        }
    }
}
