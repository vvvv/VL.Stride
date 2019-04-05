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

            public Int3 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<int[]>(content, null);
                if (@array.Length == 3)
                    return new Int3(@array);
                return Int3.Zero;
            }
        }

        sealed class Vector2Serializer : ISerializer<Vector2>
        {
            public object Serialize(SerializationContext context, Vector2 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Vector2 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<float[]>(content, null);
                if (@array.Length == 2)
                    return new Vector2(@array);
                return Vector2.Zero;
            }
        }

        sealed class Vector3Serializer : ISerializer<Vector3>
        {
            public object Serialize(SerializationContext context, Vector3 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Vector3 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<float[]>(content, null);
                if (@array.Length == 3)
                    return new Vector3(@array);
                return Vector3.Zero;
            }
        }

        sealed class Vector4Serializer : ISerializer<Vector4>
        {
            public object Serialize(SerializationContext context, Vector4 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Vector4 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<float[]>(content, null);
                if (@array.Length == 4)
                    return new Vector4(@array);
                return Vector4.UnitW;
            }
        }

        sealed class QuaternionSerializer : ISerializer<Quaternion>
        {
            public object Serialize(SerializationContext context, Quaternion value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Quaternion Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<float[]>(content, null);
                if (@array.Length == 4)
                    return new Quaternion(@array);
                return Quaternion.Identity;
            }
        }

        sealed class MatrixSerializer : ISerializer<Matrix>
        {
            public object Serialize(SerializationContext context, Matrix value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Matrix Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<float[]>(content, null);
                if (@array.Length == 16)
                    return new Matrix(@array);
                return Matrix.Identity;
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

            public BoundingBox Deserialize(SerializationContext context, object content, Type type)
            {
                var minimum = context.Deserialize<Vector3>(content, nameof(BoundingBox.Minimum));
                var maximum = context.Deserialize<Vector3>(content, nameof(BoundingBox.Maximum));
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

            public RectangleF Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<float[]>(content, null);
                if (@array.Length == 4)
                    return new RectangleF(@array[0], @array[1], @array[2], @array[3]);
                return RectangleF.Empty;
            }
        }

        sealed class Color3Serializer : ISerializer<Color3>
        {
            public object Serialize(SerializationContext context, Color3 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Color3 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<float[]>(content, null);
                if (@array.Length == 3)
                    return new Color3(@array);
                return new Color3(0);
            }
        }

        sealed class Color4Serializer : ISerializer<Color4>
        {
            public object Serialize(SerializationContext context, Color4 value)
            {
                return context.Serialize(null, value.ToArray());
            }

            public Color4 Deserialize(SerializationContext context, object content, Type type)
            {
                var @array = context.Deserialize<float[]>(content, null);
                if (@array.Length == 4)
                    return new Color4(@array);
                return Color4.Black;
            }
        }
    }
}
