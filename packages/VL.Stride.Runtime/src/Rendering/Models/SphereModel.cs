using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride Sphere model mesh using geometry3Sharp
    /// </summary>
    [DataContract("SphereModel")]
    [Display("SphereModel")] // This name shows up in the procedural model dropdown list
    public class SphereModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Sphere's radius
        /// </summary>
        [DataMember(10)]
        public float Radius { get; set; } = 0.5f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(11)]
        public bool SharedVertices { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a Sphere3Generator_NormalizedCube to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the Sphere generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new Sphere3Generator_NormalizedCube()
            {
                Radius = Radius,
                NoSharedVertices = !SharedVertices
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "SphereModel");
        }
    }
}
