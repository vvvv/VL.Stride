using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Stride.Graphics;

namespace VL.Stride.Spout
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureDesc
    {
        public uint SharedHandle;
        public uint Width;
        public uint Height;
        public uint Format;
        public uint Usage;
        public byte[] Description;

        public TextureDesc(Texture texture)
        {
            SharedHandle = (uint)texture.SharedHandle.ToInt64();
            Width = (uint)texture.Width;
            Height = (uint)texture.Height;
            Format = 0;
            Usage = 1;
            Description = new byte[256];
        }

        public TextureDesc(System.IO.MemoryMappedFiles.MemoryMappedViewStream mmvs)
        {
            BinaryReader br = new BinaryReader(mmvs);
            SharedHandle = br.ReadUInt32();
            Width = br.ReadUInt32();
            Height = br.ReadUInt32();
            Format = br.ReadUInt32();
            Usage = br.ReadUInt32();
            Description = br.ReadBytes(256);
        }

        public byte[] ToByteArray()
        {
            byte[] b = new byte[280];
            MemoryStream ms = new MemoryStream(b);
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(SharedHandle);
            bw.Write(Width);
            bw.Write(Height);
            bw.Write(Format);
            bw.Write(Usage);
            bw.Write(Description,0, Description.Length);
            bw.Dispose();
            ms.Dispose();
            return b;

        }

    }
}
