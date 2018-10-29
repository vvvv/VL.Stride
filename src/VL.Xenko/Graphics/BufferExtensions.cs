using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using VL.Lib.Collections;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Core.IO;
using System.Buffers;
using System.IO;
using Buffer = Xenko.Graphics.Buffer;

namespace VL.Xenko.Graphics
{
    public static class BufferExtensions
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
        public static unsafe Buffer SetData<TData>(this Buffer buffer, CommandList commandList, Spread<TData> fromData, int offsetInBytes = 0) where TData : struct
        {
            var immutableArray = fromData._array;
            var array = Unsafe.As<ImmutableArray<TData>, TData[]>(ref immutableArray);
            buffer.SetData(commandList, array, offsetInBytes);
            return buffer;
        }

        public static unsafe Buffer New<TData>(GraphicsDevice device, Spread<TData> fromData, BufferFlags bufferFlags, GraphicsResourceUsage usage) where TData : struct
        {
            var immutableArray = fromData._array;
            var array = Unsafe.As<ImmutableArray<TData>, TData[]>(ref immutableArray);
            return Buffer.New(device, array, bufferFlags, usage);
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
