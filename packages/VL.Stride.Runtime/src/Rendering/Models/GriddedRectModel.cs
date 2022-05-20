using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride GridRect model mesh using geometry3Sharp
    /// </summary>
    [DataContract("GriddedRectModel")]
    [Display("GriddedRectModel")] // This name shows up in the procedural model dropdown list
    public class GriddedRectModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// GriddedRect's amount of vertices per edge
        /// </summary>
        [DataMember(10)]
        public int EdgeVertices { get; set; } = 2;

        /// <summary>
        /// GriddedRect's width
        /// </summary>
        [DataMember(11)]
        public float Width { get; set; } = 1f;

        /// <summary>
        /// GriddedRect's height
        /// </summary>
        [DataMember(12)]
        public float Height { get; set; } = 0.5f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(13)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a GriddedRectGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the GriddedRect generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new GriddedRectGenerator
            {
                EdgeVertices = EdgeVertices,
                Height = Height,
                Width = Width,
                Clockwise = Clockwise
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "GriddedRectModel");
        }
    }
}
