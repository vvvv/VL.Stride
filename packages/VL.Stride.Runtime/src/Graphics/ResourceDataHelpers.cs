using Stride.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VL.Lib.Basics.Imaging;
using VL.Lib.Collections;

namespace VL.Stride.Graphics
{

    public class ImagePinner : IDisposable
    {
        IImageData imageData;
        MemoryHandle imageDataHandle;
        IntPtr pointer;

        public unsafe ImagePinner(IImage image)
        {
            imageData = image.GetData();
            imageDataHandle = imageData.Bytes.Pin();
            pointer = (IntPtr)imageDataHandle.Pointer;
        }

        public IntPtr Pointer
        {
            get => pointer;
        }

        public int SizeInBytes
        {
            get => imageData.Bytes.Length;
        }

        public int ScanSize
        {
            get => imageData.ScanSize;
        }

        public void Dispose()
        {
            imageDataHandle.Dispose();
            imageData.Dispose();
        }
    }

    public class GCPinner : IDisposable
    {
        GCHandle pinnedObject;

        public GCPinner(object obj)
        {
            pinnedObject = GCHandle.Alloc(obj, GCHandleType.Pinned);
        }

        public IntPtr Pointer
        {
            get => pinnedObject.AddrOfPinnedObject();
        }

        public void Dispose()
        {
            pinnedObject.Free();
        }
    }

    public static class ResourceDataHelpers
    {
        public static void PinSpread<T>(Spread<T> input, out IntPtr pointer, out int sizeInBytes, out int byteStride, out GCPinner pinner) where T : struct
        {
            pointer = IntPtr.Zero;
            sizeInBytes = 0;
            byteStride = 0;
            pinner = null;

            var count = input.Count;
            if (count > 0)
            {
                byteStride = Utilities.SizeOf<T>();
                sizeInBytes = byteStride * count;

                pinner = new GCPinner(input);
                pointer = pinner.Pointer;
            }
        }

        public static void PinArray<T>(T[] input, out IntPtr pointer, out int sizeInBytes, out int byteStride, out GCPinner pinner) where T : struct
        {
            pointer = IntPtr.Zero;
            sizeInBytes = 0;
            byteStride = 0;
            pinner = null;

            var count = input.Length;
            if (count > 0)
            {
                input.AsMemory();
                byteStride = Utilities.SizeOf<T>();
                sizeInBytes = byteStride * count;

                pinner = new GCPinner(input);
                pointer = pinner.Pointer;
            }
        }

        public static void PinImage(IImage input, out IntPtr pointer, out int sizeInBytes, out int bytePerRow, out int bytesPerPixel, out ImagePinner pinner)
        {
            pointer = IntPtr.Zero;
            sizeInBytes = 0;
            bytePerRow = 0;
            bytesPerPixel = 0;
            pinner = null;

            if (input != null)
            {
                pinner = new ImagePinner(input);
                sizeInBytes = pinner.SizeInBytes;
                bytePerRow = pinner.ScanSize;
                bytesPerPixel = input.Info.PixelSize;
                pointer = pinner.Pointer;
            }
        }
    }
}
