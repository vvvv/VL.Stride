using SharpFont;
using Stride.Graphics;
using System;

namespace VL.Stride.Graphics
{
    public enum BuiltinBlendState
    {
        /// <summary>
        /// No blend at all.
        /// </summary>
        Default,
        /// <summary>
        /// Adds the destination data to the source data without using alpha.
        /// </summary>
        Additive,
        /// <summary>
        /// Blends the source and destination data using alpha.
        /// </summary>
        AlphaBlend,
        /// <summary>
        /// Blends source and destination data using alpha while assuming the color data contains no alpha information.
        /// </summary>
        NonPremultiplied,
        /// <summary>
        /// Overwrites the source with the destination data.
        /// </summary>
        Opaque,
        /// <summary>
        /// Render to depth stencil buffer only.
        /// </summary>
        ColorDisabled
    }

    public enum BuiltinRasterizerState
    {
        Default,
        /// <summary>
        /// Culling primitives with clockwise winding order
        /// </summary>
        CullFront,
        /// <summary>
        /// Culling primitives with counter-clockwise winding order.
        /// </summary>
        CullBack,
        /// <summary>
        /// Not culling any primitives.
        /// </summary>
        CullNone,
        /// <summary>
        /// Wireframe - culling primitives with clockwise winding order.
        /// </summary>
        WireframeCullFront,
        /// <summary>
        /// Wireframe - culling primitives with counter-clockwise winding order.
        /// </summary>
        WireframeCullBack,
        /// <summary>
        /// Wireframe - not culling any primitives.
        /// </summary>
        Wireframe
    }

    public enum BuiltinDepthStencilState
    {
        /// <summary>
        /// Use a depth stencil buffer.
        /// </summary>
        Default,
        /// <summary>
        /// Use greater comparison for Z.
        /// </summary>
        DefaultInverse,
        /// <summary>
        /// Read-only depth stencil buffer.
        /// </summary>
        DepthRead,
        /// <summary>
        /// No depth stencil buffer.
        /// </summary>
        None
    }

    public static class EnumExtensions
    {
        public static BlendStateDescription ToDescription(this BuiltinBlendState state)
        {
            switch (state)
            {
                case BuiltinBlendState.Default:
                    return BlendStates.Default;
                case BuiltinBlendState.Additive:
                    return BlendStates.Additive;
                case BuiltinBlendState.AlphaBlend:
                    return BlendStates.AlphaBlend;
                case BuiltinBlendState.NonPremultiplied:
                    return BlendStates.NonPremultiplied;
                case BuiltinBlendState.Opaque:
                    return BlendStates.Opaque;
                case BuiltinBlendState.ColorDisabled:
                    return BlendStates.ColorDisabled;
                default:
                    throw new NotImplementedException();
            }
        }

        public static BuiltinBlendState ToEnum(this BlendStateDescription description)
        {
            if (description == BlendStates.Default)
                return BuiltinBlendState.Default;
            if (description == BlendStates.Additive)
                return BuiltinBlendState.Additive;
            if (description == BlendStates.AlphaBlend)
                return BuiltinBlendState.AlphaBlend;
            if (description == BlendStates.NonPremultiplied)
                return BuiltinBlendState.NonPremultiplied;
            if (description == BlendStates.Opaque)
                return BuiltinBlendState.Opaque;
            if (description == BlendStates.ColorDisabled)
                return BuiltinBlendState.ColorDisabled;

            throw new NotImplementedException();
        }

        public static RasterizerStateDescription ToDescription(this BuiltinRasterizerState state)
        {
            switch (state)
            {
                case BuiltinRasterizerState.Default:
                    return RasterizerStateDescription.Default;
                case BuiltinRasterizerState.CullFront:
                    return RasterizerStates.CullFront;
                case BuiltinRasterizerState.CullBack:
                    return RasterizerStates.CullBack;
                case BuiltinRasterizerState.CullNone:
                    return RasterizerStates.CullNone;
                case BuiltinRasterizerState.WireframeCullFront:
                    return RasterizerStates.WireframeCullFront;
                case BuiltinRasterizerState.WireframeCullBack:
                    return RasterizerStates.WireframeCullBack;
                case BuiltinRasterizerState.Wireframe:
                    return RasterizerStates.Wireframe;
                default:
                    throw new NotImplementedException();
            }
        }

        public static BuiltinRasterizerState ToEnum(this RasterizerStateDescription description)
        {
            if (description == RasterizerStateDescription.Default)
                return BuiltinRasterizerState.Default;
            if (description == RasterizerStates.CullFront)
                return BuiltinRasterizerState.CullFront;
            if (description == RasterizerStates.CullBack)
                return BuiltinRasterizerState.CullBack;
            if (description == RasterizerStates.CullNone)
                return BuiltinRasterizerState.CullNone;
            if (description == RasterizerStates.WireframeCullFront)
                return BuiltinRasterizerState.WireframeCullFront;
            if (description == RasterizerStates.WireframeCullBack)
                return BuiltinRasterizerState.WireframeCullBack;
            if (description == RasterizerStates.Wireframe)
                return BuiltinRasterizerState.Wireframe;

            throw new NotImplementedException();
        }

        public static DepthStencilStateDescription ToDescription(this BuiltinDepthStencilState state)
        {
            switch (state)
            {
                case BuiltinDepthStencilState.Default:
                    return DepthStencilStates.Default;
                case BuiltinDepthStencilState.DefaultInverse:
                    return DepthStencilStates.DefaultInverse;
                case BuiltinDepthStencilState.DepthRead:
                    return DepthStencilStates.DepthRead;
                case BuiltinDepthStencilState.None:
                    return DepthStencilStates.None;
                default:
                    throw new NotImplementedException();
            }
        }

        public static BuiltinDepthStencilState ToEnum(this DepthStencilStateDescription description)
        {
            if (description == DepthStencilStates.Default)
                return BuiltinDepthStencilState.Default;
            if (description == DepthStencilStates.DefaultInverse)
                return BuiltinDepthStencilState.DefaultInverse;
            if (description == DepthStencilStates.DepthRead)
                return BuiltinDepthStencilState.DepthRead;
            if (description == DepthStencilStates.None)
                return BuiltinDepthStencilState.None;

            throw new NotImplementedException();
        }
    }
}
