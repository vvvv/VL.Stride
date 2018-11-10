using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using VL.Lib.Basics.Imaging;
using VL.Lib.Collections;
using Xenko.Graphics;
using Buffer = Xenko.Graphics.Buffer;

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
            {
                var dp = new DataPointer(data.Pointer, data.Size);
                texture.SetData(commandList, dp, arraySlice, mipSlice, region);
            }

            return texture;
        }

        public static void SaveTexture(Texture texture, CommandList commandList, string filename, ImageFileType imageFileType = ImageFileType.Png)
        {
            using (var image = texture.GetDataAsImage(commandList))
            {
                using (var resultFileStream = File.OpenWrite(filename))
                {
                    image.Save(resultFileStream, imageFileType);
                }
            }
        }
    }
}
