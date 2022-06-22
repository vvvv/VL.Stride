using g3;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Generates a Rounded Rectangle mesh
    /// </summary>
    [DataContract("RoundRectMesh")]
    [Display("RoundRectMesh")] // This name shows up in the procedural model dropdown list
    public class RoundRectMesh : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// RoundRect's amount of steps per corner
        /// </summary>
        [DataMember(10)]
        public int CornerSteps { get; set; } = 4;

        /// <summary>
        /// RoundRect's size as a 2D vector
        /// </summary>
        [DataMember(11)]
        public Vector2 Size { get; set; } = Vector2.One;


        /// <summary>
        /// RoundRect's corner radius
        /// </summary>
        [DataMember(12)]
        public float Radius { get; set; } = 0.25f;

        /// <summary>
        /// RoundRect's configurable sharp corners. Use the Corner enum's OR operator to configure multiple sharp corners at once
        /// </summary>
        [DataMember(13)]
        public Corner SharpCorners { get; set; } = Corner.None;

        [DataMember(14)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a RoundRectGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the RoundRect generated with the public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new RoundRectGenerator
            {
                CornerSteps = CornerSteps,
                Width = Size.X,
                Height = Size.Y,
                Radius = Radius,
                SharpCorners = Utils.ToCorner(SharpCorners),
                Clockwise = Clockwise
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "RoundRectMesh", UvScale);
        }

        /// <summary>
        /// Enum to address the individual corner of a RoundRectModel. Top-bottom and Left-right are inverted in respect to Stride (mesh is looking down) hence the order/value change
        /// </summary>
        public enum Corner
        {
            None = 0,
            TopLeft = 1,
            TopRight = 2,
            BottomLeft = 8,
            BottomRight = 4
        }
    }
}
