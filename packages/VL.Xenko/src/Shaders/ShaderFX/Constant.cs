using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Xenko.Core.Mathematics;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class Constant<T> : ComputeValue<T>
    {
        private readonly T Value;

        public Constant(T value)
        {
            ShaderName = "Constant";
            Value = value;
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            return GetShaderSourceForType<T>(ShaderName, MaterialUtility.GetAsShaderString((dynamic)Value));
        }
    }

    /// <summary>
    /// Class MaterialUtility.
    /// </summary>
    internal static class MaterialUtility
    {
        public const string BackgroundCompositionName = "color1";

        public const string ForegroundCompositionName = "color2";

        public static int GetTextureIndex(TextureCoordinate texcoord)
        {
            switch (texcoord)
            {
                case TextureCoordinate.Texcoord0:
                    return 0;
                case TextureCoordinate.Texcoord1:
                    return 1;
                case TextureCoordinate.Texcoord2:
                    return 2;
                case TextureCoordinate.Texcoord3:
                    return 3;
                case TextureCoordinate.Texcoord4:
                    return 4;
                case TextureCoordinate.Texcoord5:
                    return 5;
                case TextureCoordinate.Texcoord6:
                    return 6;
                case TextureCoordinate.Texcoord7:
                    return 7;
                case TextureCoordinate.Texcoord8:
                    return 8;
                case TextureCoordinate.Texcoord9:
                    return 9;
                case TextureCoordinate.TexcoordNone:
                default:
                    throw new ArgumentOutOfRangeException("texcoord");
            }
        }

        public static string GetAsShaderString(ColorChannel channel)
        {
            return channel.ToString().ToLowerInvariant();
        }

        public static string GetAsShaderString(Vector2 v)
        {
            return string.Format(CultureInfo.InvariantCulture, "float2({0}, {1})", v.X, v.Y);
        }

        public static string GetAsShaderString(Vector3 v)
        {
            return string.Format(CultureInfo.InvariantCulture, "float3({0}, {1}, {2})", v.X, v.Y, v.Z);
        }

        public static string GetAsShaderString(Vector4 v)
        {
            return string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, {3})", v.X, v.Y, v.Z, v.W);
        }

        public static string GetAsShaderString(Color4 c)
        {
            return string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, {3})", c.R, c.G, c.B, c.A);
        }

        public static string GetAsShaderString(float f)
        {
            return string.Format(CultureInfo.InvariantCulture, "float4({0}, {0}, {0}, {0})", f);
        }

        public static string GetAsShaderString(int f)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", f);
        }

        public static string GetAsShaderString(uint f)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", f);
        }

        public static string GetAsShaderString(object obj)
        {
            return obj.ToString();
        }

        /// <summary>
        /// Build a encapsulating ShaderMixinSource if necessary.
        /// </summary>
        /// <param name="shaderSource">The input ShaderSource.</param>
        /// <returns>A ShaderMixinSource</returns>
        public static ShaderMixinSource GetShaderMixinSource(ShaderSource shaderSource)
        {
            if (shaderSource is ShaderClassSource)
            {
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add((ShaderClassSource)shaderSource);
                return mixin;
            }
            if (shaderSource is ShaderMixinSource)
                return (ShaderMixinSource)shaderSource;

            return null;
        }
    }
}
