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

    public enum CommonScreenSpace
    {
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
        internal static Matrix GetWithinScreenSpaceTransformation(Rectangle viewportBounds, Sizing sizing = Sizing.ManualSize,
            float width = 0, float height = 2, RectangleAnchor origin = RectangleAnchor.Center)
        {
            Matrix transformation = Matrix.Identity;
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
                    transformation.M42 = 1;
                    break;
                default:
                    break;
            }

            return transformation;
        }

        internal static Matrix GetWithinCommonSpaceTransformation(Rectangle viewportBounds, CommonScreenSpace space)
        {
            switch (space)
            {
                case CommonScreenSpace.Normalized:
                    return GetWithinScreenSpaceTransformation(viewportBounds, Sizing.ManualSize, 0, 2, RectangleAnchor.Center);
                case CommonScreenSpace.DIP:
                    return GetWithinScreenSpaceTransformation(viewportBounds, Sizing.DIP, 0, 2, RectangleAnchor.Center);
                case CommonScreenSpace.DIPTopLeft:
                    return GetWithinScreenSpaceTransformation(viewportBounds, Sizing.DIP, 0, 2, RectangleAnchor.TopLeft);
                case CommonScreenSpace.PixelTopLeft:
                    return GetWithinScreenSpaceTransformation(viewportBounds, Sizing.Pixels, 0, 2, RectangleAnchor.TopLeft);
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

        public static ViewAndProjectionRestore PushScreenSpace(this RenderView renderView, Matrix screenSpaceMatrix, bool setViewToIdentity)
        {
            var result = new ViewAndProjectionRestore(renderView);
            if (setViewToIdentity)
            {
                renderView.View = Matrix.Identity;
                renderView.Projection = screenSpaceMatrix;
                renderView.ViewProjection = screenSpaceMatrix;
            }
            else
            {
                renderView.Projection = screenSpaceMatrix;
                renderView.ViewProjection = renderView.View * renderView.Projection;
            }
            return result;
        }
    }




    public class WithinCommonScreenSpace : RendererBase
    {
        public CommonScreenSpace CommonScreenSpace { get; set; } = CommonScreenSpace.Normalized;
        public bool SetViewToIdentity { get; set; } = true;

        protected override void DrawInternal(RenderDrawContext context)
        {
            var m = Spaces.GetWithinCommonSpaceTransformation(context.RenderContext.ViewportState.Viewport0.Bounds, CommonScreenSpace);
            using (context.RenderContext.RenderView.PushScreenSpace(m, SetViewToIdentity))
            {
                DrawInput(context);
            }
        }
    }

    public class WithinCustomScreenSpace : RendererBase
    {
        public bool DeviceIndependantPixels { get; set; } = true;
        public RectangleAnchor Anchor { get; set; } = RectangleAnchor.Center;
        public bool SetViewToIdentity { get; set; } = true;

        protected override void DrawInternal(RenderDrawContext context)
        {
            var sizing = DeviceIndependantPixels ? Sizing.DIP : Sizing.Pixels;

            var m = Spaces.GetWithinScreenSpaceTransformation(
                context.RenderContext.ViewportState.Viewport0.Bounds, sizing, 1f, 1f, Anchor);

            using (context.RenderContext.RenderView.PushScreenSpace(m, SetViewToIdentity))
            {
                DrawInput(context);
            }
        }
    }

    public class WithinManualScreenSpace : RendererBase
    {
        public float Width { get; set; } = 2f;
        public float Height { get; set; } = 2f;      
        public RectangleAnchor Anchor { get; set; } = RectangleAnchor.Center;
        public bool SetViewToIdentity { get; set; } = true;

        protected override void DrawInternal(RenderDrawContext context)
        {
            var m = Spaces.GetWithinScreenSpaceTransformation(
                context.RenderContext.ViewportState.Viewport0.Bounds, Sizing.ManualSize, Width, Height, Anchor);

            using (context.RenderContext.RenderView.PushScreenSpace(m, SetViewToIdentity))
            {
                DrawInput(context);
            }
        }
    }
}
