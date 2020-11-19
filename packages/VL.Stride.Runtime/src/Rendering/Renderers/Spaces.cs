using System;
using Stride.Core.Mathematics;
using Stride.Rendering;
using VL.Lib.Mathematics;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// Allows to adjust the units of the coordinate space 
    /// </summary>
    internal enum Sizing
    {
        /// <summary>
        /// Pixel space. One unit equals 100 acutal pixels.  
        /// </summary>
        Pixels,
        /// <summary>
        /// Device Independant Pixels are like pixels, but respect the scaling factor of the display. One unit equals 100 actual DIP.
        /// </summary>
        DIP,
        /// <summary>
        /// Adjust with and height manually. 
        /// Setting either width or height to 0 results in computing width depending on height or vice versa, 
        /// while respecting the aspect ratio of the renderer or viewport.
        /// </summary>
        ManualSize,
    }

    /// <summary>
    /// Objects are placed inside a space. Setting a space results in setting View and Projection matrices.
    /// </summary>
    public enum CommonSpace
    {
        /// <summary>
        /// The Space objects normally are placed within. 
        /// </summary>
        World,

        /// <summary>
        /// Place objects relative to the camera. (downstream View Matrix get ignored)
        /// </summary>
        View,

        /// <summary>
        /// Place objects relative to the projection. (downstream View and Projection Matrices get ignored)
        /// </summary>
        Projection,

        /// <summary>
        /// Height goes from 1 Top to -1 Bottom. The origin is located in the center. 
        /// </summary>
        Normalized,

        /// <summary>
        /// Works with device independant pixels. One unit equals 100 actual DIP. The origin is located in the center. Y-Axis points upwards.
        /// </summary>
        DIP,

        /// <summary>
        /// Works with device independant pixels. One unit equals 100 actual DIP. The origin is located at the top left. Y-Axis points upwards.
        /// </summary>
        DIPTopLeft,

        /// <summary>
        /// Works with pixels. One unit equals 100 actual pixels. The origin is located at the top left. Y-Axis points upwards.
        /// </summary>
        PixelTopLeft,
    };


    internal static class Spaces
    {
        public const float NearDefault = -100f;
        public const float FarDefault  = 100f;

        static float FDIPFactor = -1;
        public static float DIPFactor
        {
            get
            {
                if (FDIPFactor == -1)
                    using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                        FDIPFactor = g.DpiX / 96;
                return FDIPFactor;
            }
        }


        /// <summary>
        /// Applies the space by resetting the Transformation.
        /// Further upstream you may use cameras and other transformations and thus invent your own space.
        /// </summary>
        internal static void GetWithinScreenSpaceTransformation(Rectangle viewportBounds, Sizing sizing,
            float width, float height, RectangleAnchor origin, float near, float far, out Matrix transformation)
        {
            transformation = Matrix.Identity;
            switch (sizing)
            {
                case Sizing.ManualSize:
                    if (width == 0)
                    {
                        if (height == 0)
                        {
                            width = 1f;
                            height = 1f;
                        }
                        else
                            width = height * ((float)viewportBounds.Width / viewportBounds.Height);
                    }
                    else
                    if (height == 0)
                        height = width * ((float)viewportBounds.Height / viewportBounds.Width);

                    transformation.M11 = 2f / width;
                    transformation.M22 = 2f / height;
                    break;
                case Sizing.Pixels:
                    transformation.M11 = 200f / viewportBounds.Width;
                    transformation.M22 = 200f / viewportBounds.Height;
                    break;
                case Sizing.DIP:
                    transformation.M11 = DIPFactor * 200f / viewportBounds.Width;
                    transformation.M22 = DIPFactor * 200f / viewportBounds.Height;
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (origin.Horizontal())
            {
                case RectangleAnchorHorizontal.Left:
                    transformation.M41 = -1; 
                    break;
                case RectangleAnchorHorizontal.Center:
                    transformation.M41 = 0; 
                    break;
                case RectangleAnchorHorizontal.Right:
                    transformation.M41 = 1;
                    break;
                default:
                    break;
            }

            switch (origin.Vertical())
            {
                case RectangleAnchorVertical.Top:
                    transformation.M42 = 1; 
                    break;
                case RectangleAnchorVertical.Center:
                    transformation.M42 = 0;
                    break;
                case RectangleAnchorVertical.Bottom:
                    transformation.M42 = -1;
                    break;
                default:
                    break;
            }

            var depth = near - far;
            transformation.M33 = 1f / depth; // D3DXMatrixOrthoRH 
            transformation.M43 = near / depth;
        }

        internal static void GetWithinCommonSpaceTransformation(Rectangle viewportBounds, CommonSpace space, out Matrix transformation)
        {
            switch (space)
            {
                case CommonSpace.World:
                case CommonSpace.View:
                case CommonSpace.Projection:
                    throw new NotImplementedException();

                case CommonSpace.Normalized:
                    GetWithinScreenSpaceTransformation(viewportBounds, Sizing.ManualSize, 0, 2, RectangleAnchor.Center, NearDefault, FarDefault, out transformation);
                    break;
                case CommonSpace.DIP:
                    GetWithinScreenSpaceTransformation(viewportBounds, Sizing.DIP, 0, 2, RectangleAnchor.Center, NearDefault, FarDefault, out transformation);
                    break;
                case CommonSpace.DIPTopLeft:
                    GetWithinScreenSpaceTransformation(viewportBounds, Sizing.DIP, 0, 2, RectangleAnchor.TopLeft, NearDefault, FarDefault, out transformation);
                    break;
                case CommonSpace.PixelTopLeft:
                    GetWithinScreenSpaceTransformation(viewportBounds, Sizing.Pixels, 0, 2, RectangleAnchor.TopLeft, NearDefault, FarDefault, out transformation);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        internal struct ViewAndProjectionRestore : IDisposable
        {
            private readonly RenderView renderView;
            private readonly Matrix previousView;
            private readonly Matrix previousProjection;
            private readonly Matrix previousViewProjection;

            public ViewAndProjectionRestore(RenderView renderView)
            {
                this.renderView = renderView;
                this.previousView = renderView.View;
                this.previousProjection = renderView.Projection;
                this.previousViewProjection = renderView.ViewProjection;
            }

            public void Dispose()
            {
                renderView.View = previousView;
                renderView.Projection = previousProjection;
                renderView.ViewProjection = previousViewProjection;
            }
        }

        public static ViewAndProjectionRestore PushScreenSpace(this RenderView renderView, 
            ref Matrix view, ref Matrix projection, 
            bool ignoreExistingView, bool ignoreExistingProjection)
        {
            var result = new ViewAndProjectionRestore(renderView);

            if (ignoreExistingView)
                renderView.View = view;
            else
                Matrix.Multiply(ref renderView.View, ref view, out renderView.View);

            if (ignoreExistingProjection)
                renderView.Projection = projection;
            else
                Matrix.Multiply(ref renderView.Projection, ref projection, out renderView.Projection);
                
            Matrix.Multiply(ref renderView.View, ref renderView.Projection, out renderView.ViewProjection);

            return result;
        }
    }


    public abstract class AbstractSpaceNode : RendererBase
    {
        public bool IgnoreExistingView { get; set; } = true;
        public bool IgnoreExistingProjection { get; set; } = true;
        protected Matrix Identity = Matrix.Identity;
    }


    public class WithinCommonSpace : AbstractSpaceNode
    {
        public CommonSpace CommonScreenSpace { get; set; } = CommonSpace.DIPTopLeft;

        protected override void DrawInternal(RenderDrawContext context)
        {
            switch (CommonScreenSpace)
            {
                case CommonSpace.World:
                    DrawInput(context);
                    break;
                case CommonSpace.View:
                case CommonSpace.Projection:
                    Matrix proj;
                    if (CommonScreenSpace == CommonSpace.View)
                        proj = context.RenderContext.RenderView.Projection;
                    else
                        proj = Identity;

                    using (context.RenderContext.RenderView.PushScreenSpace(
                        ref Identity, ref proj,
                        true, true))
                    {
                        DrawInput(context);
                    }
                    break;
                case CommonSpace.Normalized:
                case CommonSpace.DIP:
                case CommonSpace.DIPTopLeft:
                case CommonSpace.PixelTopLeft:
                    Spaces.GetWithinCommonSpaceTransformation(context.RenderContext.ViewportState.Viewport0.Bounds,
                        CommonScreenSpace, out var m);
                    using (context.RenderContext.RenderView.PushScreenSpace(
                        ref Identity, ref m,
                        true, true))
                    {
                        DrawInput(context);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public enum ScreenSpaceUnits { Pixels, DIP }

    public enum AspectRatioCorrectionMode { NoCorrection, AutoWidth, AutoHeight }

    public class WithinPhysicalScreenSpace : AbstractSpaceNode
    {
        public ScreenSpaceUnits Units { get; set; } = ScreenSpaceUnits.DIP;
        public float Scale { get; set; } = 1f;
        public RectangleAnchor Anchor { get; set; } = RectangleAnchor.Center;

        protected override void DrawInternal(RenderDrawContext context)
        {
            Sizing sizing;
            switch (Units)
            {
                case ScreenSpaceUnits.Pixels:
                    sizing = Sizing.Pixels;
                    break;
                case ScreenSpaceUnits.DIP:
                    sizing = Sizing.DIP;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Spaces.GetWithinScreenSpaceTransformation(
                context.RenderContext.ViewportState.Viewport0.Bounds, sizing, 1f, 1f, Anchor, Spaces.NearDefault, Spaces.FarDefault, out var m);

            m.ScaleVector *= new Vector3(Scale, Scale, 1f); 

            using (context.RenderContext.RenderView.PushScreenSpace(
                ref Identity, ref m,
                IgnoreExistingView, IgnoreExistingProjection))
            {
                DrawInput(context);
            }
        }
    }

    public class WithinVirtualScreenSpace : AbstractSpaceNode
    {
        public float Width { get; set; } = 2f;
        public float Height { get; set; } = 2f;
        public RectangleAnchor Anchor { get; set; } = RectangleAnchor.Center;
        public AspectRatioCorrectionMode AspectRatioCorrectionMode { get; set; } = AspectRatioCorrectionMode.NoCorrection;

        protected override void DrawInternal(RenderDrawContext context)
        {
            var width = Width;
            var height = Height;

            switch (AspectRatioCorrectionMode)
            {
                case AspectRatioCorrectionMode.NoCorrection:
                    break;
                case AspectRatioCorrectionMode.AutoWidth:
                    width = 0;
                    break;
                case AspectRatioCorrectionMode.AutoHeight:
                    height = 0;
                    break;
                default:
                    break;
            }

            Spaces.GetWithinScreenSpaceTransformation(
                context.RenderContext.ViewportState.Viewport0.Bounds, Sizing.ManualSize, width, height, Anchor, Spaces.NearDefault, Spaces.FarDefault, out var m);

            using (context.RenderContext.RenderView.PushScreenSpace(
                ref Identity, ref m,
                IgnoreExistingView, IgnoreExistingProjection))
            {
                DrawInput(context);
            }
        }
    }
}
