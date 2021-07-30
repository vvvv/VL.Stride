using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;
using SharpDX.Direct3D11;
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

            texture.Destroyed += Texture_Destroyed;
        }

        private void Texture_Destroyed(object sender, EventArgs e)
        {
            Dispose();
        }

        public InteropContext Context { get; }

        public Texture Texture { get; }

        public IntPtr DxTexture { get; }

        public IntPtr Handle { get; private set; }

        public uint Name { get; }

        void RegisterTexture()
        {
            if (Handle != default)
                return;

            using (Context.WithScopedContext())
            {
                Handle = Wgl.DXRegisterObjectNV(
                    Context.DeviceHandle,
                    DxTexture,
                    Name,
                    (uint)OpenTK.Graphics.OpenGL.All.Renderbuffer,
                    WGL_NV_DX_interop.AccessWriteDiscard);
            }
        }

        void UnregisterTexture()
        {
            if (Handle == default)
                return;

            using (Context.WithScopedContext())
            {
                Wgl.DXUnregisterObjectNV(Context.DeviceHandle, Handle);
                Handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Texture.Destroyed -= Texture_Destroyed;

            Context.MakeCurrent();

            Context.Remove(this);

            UnregisterTexture();

            GL.DeleteRenderbuffer(Name);
        }
    }
}
