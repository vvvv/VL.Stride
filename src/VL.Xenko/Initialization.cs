using System;
using VL.Core;
using Xenko.Core.Mathematics;

namespace VL.Lib
{
    // Importer will look for static class VL.Lib.Initialization in each assembly
    static class Initialization
    {
        public static void RegisterServices(IVLFactory factory)
        {
            factory.RegisterSerializer<Int3, Int3Serializer>();
        }

        class Int3Serializer : ISerializer<Int3>
        {
            public object Serialize(SerializationContext context, Int3 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Int3 Deserialize(SerializationContext context, object content, Int3 defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<int>());
                if (@array.Length == 3)
                    return new Int3(@array);
                return defaultValue;
            }
        }
    }
}