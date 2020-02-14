using System;
using VL.Core;
using Xenko.Core.Mathematics;

namespace VL.Xenko.Core
{
    static partial class Serialization
    {
        public static void RegisterSerializers(IVLFactory factory)
        {
            factory.RegisterSerializer<Int3, Int3Serializer>();
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
    }
}
