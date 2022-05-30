using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Generates a Box mesh
    /// </summary>
    [DataContract("BoxMesh2")]
    [Display("BoxMesh2")] // This name shows up in the procedural model dropdown list
    public class BoxMesh2 : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Box's tessellation (amount of vertices per edge)
        /// </summary>
        [DataMember(10)]
        public int Tessellation { get; set; } = 2;        

        [DataMember(11)]
        public bool SharedVertices { get; set; } = false;
        
        [DataMember(12)]
        public bool Clockwise { get; set; } = true;


        /// <summary>
        /// Uses the DMesh3 instance generated from a GridBox3Generator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the GridBox generated with the public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new GridBox3Generator
            {
                EdgeVertices = Tessellation,
                NoSharedVertices = !SharedVertices,
                Clockwise = Clockwise
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "BoxMesh2");
        }
    }
}
