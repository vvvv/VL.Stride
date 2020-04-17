using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using VL.Lib.Basics.Imaging;
using VL.Lib.Collections;
using Xenko.Graphics;
using Buffer = Xenko.Graphics.Buffer;
using XenkoPixelFormat = Xenko.Graphics.PixelFormat;
using VLPixelFormat = VL.Lib.Basics.Imaging.PixelFormat;

namespace VL.Xenko.Graphics
{
    public static class TextureExtensions
    {
        /// <summary>
        /// Copies the <paramref name="fromData"/> to the given <paramref name="buffer"/> on GPU memory.
        /// </summary>
        /// <typeparam name="TData">The type of the T data.</typeparam>
        /// <param name="buffer">The <see cref="Buffer"/>.</param>
        /// <param name="commandList">The <see cref="CommandList"/>.</param>
        /// <param name="fromData">The data to copy from.</param>
        /// <param name="offsetInBytes">The offset in bytes to write to.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// See the unmanaged documentation about Map/UnMap for usage and restrictions.
        /// </remarks>
        /// <returns>The GPU buffer.</returns>
        public static unsafe Texture SetData<TData>(this Texture texture, CommandList commandList, Spread<TData> fromData, int arraySlice, int mipSlice, ResourceRegion? region) where TData : struct
        {
            var immutableArray = fromData._array;
            var array = Unsafe.As<ImmutableArray<TData>, TData[]>(ref immutableArray);
            texture.SetData(commandList, array, arraySlice, mipSlice, region);
            return texture;
        }

        public static unsafe Texture SetDataFromIImage(this Texture texture, CommandList commandList, IImage image, int arraySlice, int mipSlice, ResourceRegion? region)
        {
            using (var data = image.GetData())
            using (var handle = data.Bytes.Pin())
            {
                var dp = new DataPointer(handle.Pointer, data.Bytes.Length);
                texture.SetData(commandList, dp, arraySlice, mipSlice, region);
            }

            return texture;
        }

        public static void SaveTexture(this Texture texture, CommandList commandList, string filename, ImageFileType imageFileType = ImageFileType.Png)
        {
            using (var image = texture.GetDataAsImage(commandList))
            {
                using (var resultFileStream = File.OpenWrite(filename))
                {
                    image.Save(resultFileStream, imageFileType);
                }
            }
        }

        public static XenkoPixelFormat GetXenkoPixelFormat(ImageInfo info, bool isSRgb = true)
        {
            var format = info.Format;
            switch (format)
            {
                case VLPixelFormat.Unknown:
                    return XenkoPixelFormat.None;
                case VLPixelFormat.R8:
                    return XenkoPixelFormat.R8_UNorm;
                case VLPixelFormat.R16:
                    return XenkoPixelFormat.R16_UNorm;
                case VLPixelFormat.R32F:
                    return XenkoPixelFormat.R32_Float;
                case VLPixelFormat.R8G8B8X8:
                    return isSRgb ? XenkoPixelFormat.R8G8B8A8_UNorm_SRgb : XenkoPixelFormat.R8G8B8A8_UNorm;
                case VLPixelFormat.R8G8B8A8:
                    return isSRgb ? XenkoPixelFormat.R8G8B8A8_UNorm_SRgb : XenkoPixelFormat.R8G8B8A8_UNorm;
                case VLPixelFormat.B8G8R8X8:
                    return isSRgb ? XenkoPixelFormat.B8G8R8X8_UNorm_SRgb : XenkoPixelFormat.B8G8R8X8_UNorm;
                case VLPixelFormat.B8G8R8A8:
                    return isSRgb ? XenkoPixelFormat.B8G8R8A8_UNorm_SRgb : XenkoPixelFormat.B8G8R8A8_UNorm;
                case VLPixelFormat.R32G32F:
                    return XenkoPixelFormat.R32G32_Float;
                case VLPixelFormat.R32G32B32A32F:
                    return XenkoPixelFormat.R32G32B32A32_Float;
                default:
                    throw new UnsupportedPixelFormatException(format);
            }
        }

        public static VLPixelFormat GetVLImagePixelFormat(Texture texture, out bool isSRgb)
        {
            isSRgb = false;

            if (texture == null)
                return VLPixelFormat.Unknown;

                var format = texture.Format;
            switch (format)
            {
                case XenkoPixelFormat.None:
                    return VLPixelFormat.Unknown;
                case XenkoPixelFormat.R8_UNorm:
                    return VLPixelFormat.R8;
                case XenkoPixelFormat.R16_UNorm:
                    return VLPixelFormat.R16;
                case XenkoPixelFormat.R32_Float:
                    return VLPixelFormat.R32F;
                case XenkoPixelFormat.R8G8B8A8_UNorm:
                    return VLPixelFormat.R8G8B8A8;
                case XenkoPixelFormat.R8G8B8A8_UNorm_SRgb:
                    isSRgb = true;
                    return VLPixelFormat.R8G8B8A8;
                case XenkoPixelFormat.B8G8R8X8_UNorm:
                    return VLPixelFormat.B8G8R8X8;
                case XenkoPixelFormat.B8G8R8X8_UNorm_SRgb:
                    isSRgb = true;
                    return VLPixelFormat.B8G8R8X8;
                case XenkoPixelFormat.B8G8R8A8_UNorm:
                    return VLPixelFormat.B8G8R8A8;
                case XenkoPixelFormat.B8G8R8A8_UNorm_SRgb:
                    isSRgb = true;
                    return VLPixelFormat.B8G8R8A8;
                case XenkoPixelFormat.R32G32_Float:
                    return VLPixelFormat.R32G32F;
                case XenkoPixelFormat.R32G32B32A32_Float:
                    return VLPixelFormat.R32G32B32A32F;
                default:
                    throw new Exception("Unsupported Pixel Format");
            }
        }

    }
}
