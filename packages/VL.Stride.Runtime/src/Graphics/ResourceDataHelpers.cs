using Stride.Core;
using Stride.Graphics;
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using VL.Lib.Basics.Imaging;
using VL.Lib.Collections;

namespace VL.Stride.Graphics
{
    public interface IStrideGraphicsDataProvider
    {
        int SizeInBytes { get; }
        int ElementSizeInBytes { get; }
        int RowSizeInBytes { get; }
        int SliceSizeInBytes { get; }
        IPinnedGraphicsData Pin();
    }

    public interface IPinnedGraphicsData : IDisposable
    {
        IntPtr Pointer { get; }
    }

    public class ImageDataProvider : IStrideGraphicsDataProvider
    {
        private IImage image;

        public ImageDataProvider(IImage image)
        {
            this.image = image;
            SizeInBytes = image.Info.ImageSize;
            ElementSizeInBytes = image.Info.Format.GetPixelSize();
            RowSizeInBytes = image.Info.ScanSize;
            SliceSizeInBytes = RowSizeInBytes * image.Info.Height;
        }

        public int SizeInBytes { get; set; }

        public int ElementSizeInBytes { get; set; }

        public int RowSizeInBytes { get; set; }

        public int SliceSizeInBytes { get; set; }


        public IPinnedGraphicsData Pin()
        {
            return new PinnedImageData(image);
        }
    }

    public struct PinnedImageData : IPinnedGraphicsData
    {
        IImageData imageData;
        MemoryHandle memoryHandle;

        public PinnedImageData(IImage image)
        {
            imageData = image.GetData();
            memoryHandle = imageData.Bytes.Pin();
        }

        public unsafe IntPtr Pointer => new IntPtr(memoryHandle.Pointer);

        public void Dispose()
        {
            memoryHandle.Dispose();
            imageData.Dispose();
        }
    }

    public class MemoryDataProvider<T> : IStrideGraphicsDataProvider where T : struct
    {
        private ReadOnlyMemory<T> memory;

        public MemoryDataProvider(ReadOnlyMemory<T> memory)
        {
            this.memory = memory;
            SizeInBytes = memory.Length;
            ElementSizeInBytes = Utilities.SizeOf<T>();
        }

        public int SizeInBytes { get; set; }

        public int ElementSizeInBytes { get; set; }

        public int RowSizeInBytes { get; set; }

        public int SliceSizeInBytes { get; set; }


        public IPinnedGraphicsData Pin()
        {
            return new PinnedMemoryHandle<T>(memory);
        }
    }

    public struct PinnedMemoryHandle<T> : IPinnedGraphicsData
    {
        MemoryHandle memoryHandle;

        public PinnedMemoryHandle(ReadOnlyMemory<T> memory)
        {
            memoryHandle = memory.Pin();
        }

        public unsafe IntPtr Pointer => new IntPtr(memoryHandle.Pointer);

        public void Dispose()
        {
            memoryHandle.Dispose();
        }
    }

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
