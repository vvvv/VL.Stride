using g3;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride Grid box model mesh using geometry3Sharp
    /// </summary>
    [DataContract("GridBoxModel")]
    [Display("GridBoxModel")] // This name shows up in the procedural model dropdown list
    public class GridBoxModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// GridBox's center
        /// </summary>
        [DataMember(10)]
        public Vector3 Center { get; set; } = Vector3.Zero;

        /// <summary>
        /// GridBox's amount of vertices per edge
        /// </summary>
        [DataMember(11)]
        public int EdgeVertices { get; set; } = 2;

        /// <summary>
        /// GridBox's size or extent
        /// </summary>
        [DataMember(12)]
        public Vector3 Extent { get; set; } = Vector3.One;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(13)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(14)]
        public bool SharedVertices { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a GridBox3Generator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the GridBox generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new GridBox3Generator
            {
                Box = ToBox3d(Center, Extent),
                EdgeVertices = EdgeVertices,
                Clockwise = Clockwise,
                NoSharedVertices = !SharedVertices,
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "GridBoxModel");
        }

        /// <summary>
        /// Generates a g3 Box3d based on Stride Vector3's for the GridBoxModel's center and extent
        /// </summary>
        /// <param name="center">GridBoxModel's center</param>
        /// <param name="extent">GridBoxModel's extent</param>
        /// <returns></returns>
        private Box3d ToBox3d(Vector3 center, Vector3 extent)
        {
            return new Box3d(new Vector3d(center.X, center.Y, center.Z), new Vector3d(extent.X, extent.Y, extent.Z));
        }
    }
}
