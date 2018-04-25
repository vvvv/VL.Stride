using System;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace MyGame
{
    /// <summary>
    /// Describes a custom vertex format structure that contains position, normal and texture information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VLNullVertex : IEquatable<VLNullVertex>, IVertex
    {
        /// <summary>
        /// Defines structure byte size.
        /// </summary>
        public static readonly int Size = 1;


        /// <summary>
        /// The vertex layout of this struct.
        /// </summary>
        public static readonly VertexDeclaration Layout = new VertexDeclaration(new VertexElement[0], 1, 4);


        public bool Equals(VLNullVertex other)
        {
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VLNullVertex && Equals((VLNullVertex)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {               
                return Size.GetHashCode();
            }
        }

        public VertexDeclaration GetLayout()
        {
            return Layout;
        }

        public void FlipWinding()
        {           
        }

        public static bool operator ==(VLNullVertex left, VLNullVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VLNullVertex left, VLNullVertex right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return "VL Null Vertex";
        }
    }
}
