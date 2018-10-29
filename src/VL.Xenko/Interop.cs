using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VL.Xenko
{
    public static class Interop
    {
        /// <summary>
        /// Returns the size of the object in bytes.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="object">The object.</param>
        /// <returns>The size of the object in bytes.</returns>
        public static int SizeOf<T>(T @object) => Unsafe.SizeOf<T>();

        /// <summary>
        /// Returns the element size of the collection in bytes.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>The size of one collection element in bytes.</returns>
        public static int SizeOfElement<T>(IReadOnlyCollection<T> collection) => Unsafe.SizeOf<T>();
    }
}
