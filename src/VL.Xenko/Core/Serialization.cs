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
            factory.RegisterSerializer<BoundingBox, BoundingBoxSerializer>();
            factory.RegisterSerializer<Color3, Color3Serializer>();
            factory.RegisterSerializer<Color4, Color4Serializer>();
            factory.RegisterSerializer<Matrix, MatrixSerializer>();
            factory.RegisterSerializer<Quaternion, QuaternionSerializer>();
            factory.RegisterSerializer<RectangleF, RectangleFSerializer>();
            factory.RegisterSerializer<Vector2, Vector2Serializer>();
            factory.RegisterSerializer<Vector3, Vector3Serializer>();
            factory.RegisterSerializer<Vector4, Vector4Serializer>();
        }

        class Int3Serializer : ISerializer<Int3>
        {
            public object Serialize(SerializationContext context, Int3 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Int3 Deserialize(SerializationContext context, object content, Type type, Int3 defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<int>());
                if (@array.Length == 3)
                    return new Int3(@array);
                return defaultValue;
            }
        }

        sealed class Vector2Serializer : ISerializer<Vector2>
        {
            public object Serialize(SerializationContext context, Vector2 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Vector2 Deserialize(SerializationContext context, object content, Type type, Vector2 defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<float>());
                if (@array.Length == 2)
                    return new Vector2(@array);
                return defaultValue;
            }
        }

        sealed class Vector3Serializer : ISerializer<Vector3>
        {
            public object Serialize(SerializationContext context, Vector3 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Vector3 Deserialize(SerializationContext context, object content, Type type, Vector3 defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<float>());
                if (@array.Length == 3)
                    return new Vector3(@array);
                return defaultValue;
            }
        }

        sealed class Vector4Serializer : ISerializer<Vector4>
        {
            public object Serialize(SerializationContext context, Vector4 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Vector4 Deserialize(SerializationContext context, object content, Type type, Vector4 defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<float>());
                if (@array.Length == 4)
                    return new Vector4(@array);
                return defaultValue;
            }
        }

        sealed class QuaternionSerializer : ISerializer<Quaternion>
        {
            public object Serialize(SerializationContext context, Quaternion value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Quaternion Deserialize(SerializationContext context, object content, Type type, Quaternion defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<float>());
                if (@array.Length == 4)
                    return new Quaternion(@array);
                return defaultValue;
            }
        }

        sealed class MatrixSerializer : ISerializer<Matrix>
        {
            public object Serialize(SerializationContext context, Matrix value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Matrix Deserialize(SerializationContext context, object content, Type type, Matrix defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<float>());
                if (@array.Length == 16)
                    return new Matrix(@array);
                return defaultValue;
            }
        }

        sealed class BoundingBoxSerializer : ISerializer<BoundingBox>
        {
            public object Serialize(SerializationContext context, BoundingBox value)
            {
                return new object[]
                {
                context.Serialize(nameof(BoundingBox.Minimum), value.Minimum),
                context.Serialize(nameof(BoundingBox.Maximum), value.Maximum)
                };
            }

            public BoundingBox Deserialize(SerializationContext context, object content, Type type, BoundingBox defaultValue)
            {
                var minimum = context.Deserialize(content, nameof(BoundingBox.Minimum), defaultValue.Minimum);
                var maximum = context.Deserialize(content, nameof(BoundingBox.Maximum), defaultValue.Maximum);
                return new BoundingBox(minimum, maximum);
            }
        }

        sealed class RectangleFSerializer : ISerializer<RectangleF>
        {
            public object Serialize(SerializationContext context, RectangleF value)
            {
                var @array = new float[] { value.X, value.Y, value.Width, value.Height };
                return context.Serialize(null, @array);
            }

            public RectangleF Deserialize(SerializationContext context, object content, Type type, RectangleF defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<float>());
                if (@array.Length == 4)
                    return new RectangleF(@array[0], @array[1], @array[2], @array[3]);
                return defaultValue;
            }
        }

        sealed class Color3Serializer : ISerializer<Color3>
        {
            public object Serialize(SerializationContext context, Color3 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Color3 Deserialize(SerializationContext context, object content, Type type, Color3 defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<float>());
                if (@array.Length == 3)
                    return new Color3(@array);
                return defaultValue;
            }
        }

        sealed class Color4Serializer : ISerializer<Color4>
        {
            public object Serialize(SerializationContext context, Color4 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Color4 Deserialize(SerializationContext context, object content, Type type, Color4 defaultValue)
            {
                var @array = context.Deserialize(content, null, Array.Empty<float>());
                if (@array.Length == 4)
                    return new Color4(@array);
                return defaultValue;
            }
        }
    }
}
