using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride GridRect model mesh using geometry3Sharp
    /// </summary>
    [DataContract("RoundRectModel")]
    [Display("RoundRectModel")] // This name shows up in the procedural model dropdown list
    public class RoundRectModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// RoundRect's amount of steps per corner
        /// </summary>
        [DataMember(10)]
        public int CornerSteps { get; set; } = 4;

        /// <summary>
        /// RoundRect's width
        /// </summary>
        [DataMember(11)]
        public float Width { get; set; } = 2f;

        /// <summary>
        /// RoundRect's height
        /// </summary>
        [DataMember(12)]
        public float Height { get; set; } = 1f;


        /// <summary>
        /// RoundRect's corner radius
        /// </summary>
        [DataMember(13)]
        public float Radius { get; set; } = 0.25f;

        /// <summary>
        /// RoundRect's corner radius
        /// </summary>
        [DataMember(13)]
        public Corner SharpCorners { get; set; } = Corner.None;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(15)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a RoundRectGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the RoundRect generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new RoundRectGenerator
            {
                CornerSteps = CornerSteps,
                Height = Height,
                Width = Width,
                Radius = Radius,
                SharpCorners = Utils.ToCorner(SharpCorners),
                Clockwise = Clockwise
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "RoundRectModel");
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
