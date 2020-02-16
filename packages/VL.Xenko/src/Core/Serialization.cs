using System;
using VL.Core;
using Xenko.Core.Mathematics;

namespace VL.Xenko.Core
{
    static partial class Serialization
    {
        public static void RegisterSerializers(IVLFactory factory)
        {
            factory.RegisterSerializer<Int2, Int2Serializer>();
            factory.RegisterSerializer<Int3, Int3Serializer>();
            factory.RegisterSerializer<Int4, Int4Serializer>();
        }

        class Int2Serializer : ISerializer<Int2>
        {
            public object Serialize(SerializationContext context, Int2 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Int2 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<int[]>(content, null);
                if (@array.Length == 2)
                    return new Int2(@array);
                return Int2.Zero;
            }
        }

        class Int3Serializer : ISerializer<Int3>
        {
            public object Serialize(SerializationContext context, Int3 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Int3 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<int[]>(content, null);
                if (@array.Length == 3)
                    return new Int3(@array);
                return Int3.Zero;
            }
        }

        class Int4Serializer : ISerializer<Int4>
        {
            public object Serialize(SerializationContext context, Int4 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Int4 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<int[]>(content, null);
                if (@array.Length == 4)
                    return new Int4(@array);
                return Int4.Zero;
            }
        }
    }
}
