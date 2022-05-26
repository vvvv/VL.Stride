using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride Sphere model mesh using geometry3Sharp
    /// </summary>
    [DataContract("BoxSphereMesh")]
    [Display("BoxSphereMesh")] // This name shows up in the procedural model dropdown list
    public class BoxSphereMesh : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Sphere's radius
        /// </summary>
        [DataMember(10)]
        public float Radius { get; set; } = 0.5f;

        /// <summary>
        /// Sphere's amount of vertices
        /// </summary>
        [DataMember(11)]
        public int Tessellation { get; set; } = 8;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(12)]
        public bool SharedVertices { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(13)]
        public bool Clockwise { get; set; } = true;

        /// <summary>
        /// Uses the DMesh3 instance generated from a Sphere3Generator_NormalizedCube to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the Sphere generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new Sphere3Generator_NormalizedCube()
            {
                EdgeVertices = Tessellation,
                Radius = Radius,
                NoSharedVertices = !SharedVertices,
                Clockwise = Clockwise
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "BoxSphereMesh");
        }
    }
}
