using System;
using System.Runtime.CompilerServices;
using Stride.Engine;
using Stride.Graphics;
using Stride.Core.IO;
using System.Buffers;
using System.IO;
using Buffer = Stride.Graphics.Buffer;
using VL.Core;
using System.Reflection;
using Stride.Core;
using System.Diagnostics;

namespace VL.Stride.Graphics
{
    public static class BufferExtensions
    {
        public static Buffer New(GraphicsDevice graphicsDevice, BufferDescription description, BufferViewDescription viewDescription, IntPtr intialData)
        {
            var buffer = BufferCtor(graphicsDevice);
            return BufferInit(buffer, description, viewDescription, intialData);
        }

        const BindingFlags NonPunblicInst = BindingFlags.NonPublic | BindingFlags.Instance;

        static Buffer BufferCtor(GraphicsDevice graphicsDevice)
        {
            var ctor = typeof(Buffer).GetConstructor(NonPunblicInst, null, new[] { typeof(GraphicsDevice) }, null);
            return (Buffer)ctor.Invoke(new[] { graphicsDevice });
        }

        static Buffer BufferInit(Buffer buffer, BufferDescription description, BufferViewDescription viewDescription, IntPtr intialData)
        {
            var init = typeof(Buffer).GetMethod("InitializeFromImpl", NonPunblicInst, null, new[] { typeof(BufferDescription), typeof(BufferFlags), typeof(PixelFormat), typeof(IntPtr) }, null);
            return (Buffer)init.Invoke(buffer, new object[] { description, viewDescription.Flags, viewDescription.Format, intialData});
        }

        internal static readonly PropertyKey<Buffer> ParentBuffer = new PropertyKey<Buffer>(nameof(ParentBuffer), typeof(Buffer));

        public static Buffer ToBufferView(Buffer bufferView, Buffer parentBuffer, BufferViewDescription viewDescription, GraphicsDevice graphicsDevice)
        {
            SetGraphicsDevice(bufferView, graphicsDevice);

            //bufferDescription = description;
            SetField(bufferView, "bufferDescription", parentBuffer.Description);

            //nativeDescription = ConvertToNativeDescription(Description);
            SetField(bufferView, "nativeDescription", ConvertToNativeDescription(parentBuffer.Description));

            //ViewFlags = viewFlags;
            SetProp(bufferView, "ViewFlags", viewDescription.Flags);

            //InitCountAndViewFormat(out this.elementCount, ref viewFormat);
            InitCountAndViewFormat(bufferView, out var count, ref viewDescription.Format);
            SetField(bufferView, "elementCount", count);

            //ViewFormat = viewFormat;
            SetProp(bufferView, "ViewFormat", viewDescription.Format);

            //NativeDeviceChild = new SharpDX.Direct3D11.Buffer(GraphicsDevice.NativeDevice, dataPointer, nativeDescription);
            SetNativeChild(bufferView, GetNativeChild(parentBuffer));

            //if (nativeDescription.Usage != ResourceUsage.Staging)
            //    this.InitializeViews();
            InitializeViews(bufferView);

            if (parentBuffer is IReferencable referencable)
            {
                referencable.AddReference();
                bufferView.Destroyed += (e, s) => referencable.Release();
            }

            return bufferView;
        }

        static SharpDX.Direct3D11.DeviceChild GetNativeChild(GraphicsResourceBase graphicsResource)
        {
            var prop = typeof(GraphicsResourceBase).GetProperty("NativeDeviceChild", NonPunblicInst);
            return (SharpDX.Direct3D11.DeviceChild)prop.GetValue(graphicsResource);
        }

        static void SetNativeChild(GraphicsResourceBase graphicsResource, SharpDX.Direct3D11.DeviceChild deviceChild)
        {
            var iUnknownObject = deviceChild as SharpDX.IUnknown;
            if (iUnknownObject != null)
            {
                var refCountResult = iUnknownObject.AddReference();
                Debug.Assert(refCountResult > 1);
            }
            var prop = typeof(GraphicsResourceBase).GetProperty("NativeDeviceChild", NonPunblicInst);
            prop.SetValue(graphicsResource, deviceChild);
        }

        static SharpDX.Direct3D11.BufferDescription ConvertToNativeDescription(BufferDescription description)
        {
            var method = typeof(Buffer).GetMethod("ConvertToNativeDescription", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(BufferDescription) }, null);
            return (SharpDX.Direct3D11.BufferDescription)method.Invoke(null, new object[] { description });
        }

        static void SetField(Buffer buffer, string name, object arg)
        {
            var field = typeof(Buffer).GetField(name, NonPunblicInst);
            field.SetValue(buffer, arg);
        }

        static void SetGraphicsDevice(Buffer buffer, object arg)
        {
            var prop = typeof(GraphicsResourceBase).GetProperty("GraphicsDevice", BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(buffer, arg);
        }

        static void SetProp(Buffer buffer, string name, object arg)
        {
            var prop = typeof(Buffer).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(buffer, arg);
        }
        
        static void InitCountAndViewFormat(Buffer buffer, out int count, ref PixelFormat viewFormat)
        {
            var method = typeof(Buffer).GetMethod("InitCountAndViewFormat", NonPunblicInst);
            var args = new object[] { 0, viewFormat };
            method.Invoke(buffer, args);
            count = (int)args[0];
        }

        static void InitializeViews(Buffer buffer)
        {
            var method = typeof(Buffer).GetMethod("InitializeViews", NonPunblicInst);
            method.Invoke(buffer, null);
        }

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
        public static unsafe Buffer SetData<TData>(this Buffer buffer, CommandList commandList, IHasMemory<TData> fromData, int offsetInBytes = 0) where TData : struct
        {
            if (fromData.TryGetMemory(out ReadOnlyMemory<TData> memory))
                return buffer.SetData(commandList, memory, offsetInBytes);
            return buffer;
        }

        /// <summary>
        /// Copies the <paramref name="memory"/> to the given <paramref name="buffer"/> on GPU memory.
        /// </summary>
        /// <typeparam name="TData">The type of the T data.</typeparam>
        /// <param name="buffer">The <see cref="Buffer"/>.</param>
        /// <param name="commandList">The <see cref="CommandList"/>.</param>
        /// <param name="memory">The memory to copy from.</param>
        /// <param name="offsetInBytes">The offset in bytes to write to.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// See the unmanaged documentation about Map/UnMap for usage and restrictions.
        /// </remarks>
        /// <returns>The GPU buffer.</returns>
        public static unsafe Buffer SetData<TData>(this Buffer buffer, CommandList commandList, ReadOnlyMemory<TData> memory, int offsetInBytes = 0) where TData : struct
        {
            using (var handle = memory.Pin())
            {
                var elementSize = Unsafe.SizeOf<TData>();
                var dataPointer = new DataPointer(handle.Pointer, memory.Length * elementSize);
                buffer.SetData(commandList, dataPointer, offsetInBytes);
                return buffer;
            }
        }

        public static Buffer SetDataFromProvider(this Buffer buffer, CommandList commandList, IStrideGraphicsDataProvider data, int offsetInBytes = 0)
        {
            if (buffer != null && data != null)
            {
                using (var handle = data.Pin())
                {
                    buffer.SetData(commandList, new DataPointer(handle.Pointer, data.SizeInBytes), offsetInBytes);
                } 
            }

            return buffer;
        }

        /// <summary>
        /// Creates a new <see cref="Buffer"/> initialized with a copy of the given data.
        /// </summary>
        /// <typeparam name="TData">The element type.</typeparam>
        /// <param name="device">The graphics device.</param>
        /// <param name="fromData">The data to use to initialize the buffer.</param>
        /// <param name="bufferFlags">The buffer flags.</param>
        /// <param name="usage">The buffer usage.</param>
        /// <exception cref="ArgumentException">If retrieval of read-only memory failed.</exception>
        /// <returns>The newly created buffer.</returns>
        public static unsafe Buffer New<TData>(GraphicsDevice device, IHasMemory<TData> fromData, BufferFlags bufferFlags, GraphicsResourceUsage usage) where TData : struct
        {
            if (fromData.TryGetMemory(out ReadOnlyMemory<TData> memory))
                return New(device, memory, bufferFlags, usage);
            throw new ArgumentException($"Failed to create buffer because retrieval of read-only memory failed.", nameof(fromData));
        }

        /// <summary>
        /// Creates a new <see cref="Buffer"/> initialized with a copy of the given data.
        /// </summary>
        /// <typeparam name="TData">The element type.</typeparam>
        /// <param name="device">The graphics device.</param>
        /// <param name="memory">The data to use to initialize the buffer.</param>
        /// <param name="bufferFlags">The buffer flags.</param>
        /// <param name="usage">The buffer usage.</param>
        /// <exception cref="ArgumentException">If retrieval of read-only memory failed.</exception>
        /// <returns>The newly created buffer.</returns>
        public static unsafe Buffer New<TData>(GraphicsDevice device, ReadOnlyMemory<TData> memory, BufferFlags bufferFlags, GraphicsResourceUsage usage) where TData : struct
        {
            using (var handle = memory.Pin())
            {
                var elementSize = Unsafe.SizeOf<TData>();
                var dataPointer = new DataPointer(handle.Pointer, memory.Length * elementSize);
                return Buffer.New(device, dataPointer, elementSize, bufferFlags, usage);
            }
        }


        //    public static unsafe void WriteToDisk(this Buffer buffer, string filepath)
        //    {

        //    }

        //    public static unsafe void WriteToDisk(this Buffer buffer, Stream stream)
        //    {
        //        buffer.GetData()

        //        var pool = ArrayPool<byte>.Shared;
        //        var chunk = pool.Rent(Math.Min(buffer.SizeInBytes, 0x10000));
        //        try
        //        {
        //            fixed (byte* chunkPtr = chunk)
        //            {
        //                var offset = 0;
        //                while (stream.CanRead)
        //                {
        //                    var bytesRead = stream.Read(chunk, 0, chunk.Length);
        //                    if (bytesRead > 0)
        //                    {
        //                        var dp = new DataPointer(chunkPtr, bytesRead);
        //                        buffer.SetData(commandList, dp, offset);
        //                        offset += bytesRead;
        //                    }
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            pool.Return(chunk);
        //        }
        //    }
        //}

        public static unsafe Buffer SetDataFromFile(this Buffer buffer, CommandList commandList, string filepath)
        {
            using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                buffer.SetDataFromStream(commandList, stream);
            }
            return buffer;
        }

        public static unsafe Buffer SetDataFromXenkoAssetURL(this Buffer buffer, CommandList commandList, Game game, string url)
        {
            using (var stream = game.Content.OpenAsStream(url, StreamFlags.None))
            {
                buffer.SetDataFromStream(commandList, stream);
            }
            return buffer;
        }

        public static unsafe Buffer SetDataFromStream(this Buffer buffer, CommandList commandList, Stream stream)
        {
            var pool = ArrayPool<byte>.Shared;
            var chunk = pool.Rent(Math.Min(buffer.SizeInBytes, 0x10000));
            try
            {
                fixed (byte* chunkPtr = chunk)
                {
                    var offset = 0;
                    while (stream.CanRead)
                    {
                        var bytesRead = stream.Read(chunk, 0, chunk.Length);
                        if (bytesRead > 0)
                        {
                            var dp = new DataPointer(chunkPtr, bytesRead);
                            buffer.SetData(commandList, dp, offset);
                            offset += bytesRead;
                        }
                    }
                }
            }
            finally
            {
                pool.Return(chunk);
            }
            return buffer;
        }
    }
}
