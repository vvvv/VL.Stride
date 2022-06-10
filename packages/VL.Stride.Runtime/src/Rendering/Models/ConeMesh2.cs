using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Generates a Cone mesh
    /// </summary>
    [DataContract("ConeMesh2")]
    [Display("ConeMesh2")] // This name shows up in the procedural model dropdown list
    public class ConeMesh2 : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Cone's radius
        /// </summary>
        [DataMember(10)]
        public float Radius { get; set; } = 0.5f;

        /// <summary>
        /// Cone's initial angle in cycles 
        /// </summary>
        [DataMember(11)]
        public float FromAngle { get; set; } = 0f;

        /// <summary>
        /// Cone's final angle in cycles
        /// </summary>
        [DataMember(12)]
        public float ToAngle { get; set; } = 1f;

        /// <summary>
        /// Cone's height
        /// </summary>
        [DataMember(13)]
        public float Height { get; set; } = 1;

        /// <summary>
        /// Cone's tessellation (amount of radial slices to split the cone into). Higher values result in smoother surfaces
        /// </summary>
        [DataMember(14)]
        public int Tessellation { get; set; } = 16;

        /// <summary>
        /// Cone's vertical tessellation (amount of vertical slices to split the cone into)
        /// </summary>
        [DataMember(15)]
        public int VTessellation { get; set; } = 2;

        [DataMember(16)]
        public bool SharedVertices { get; set; } = false;

        [DataMember(17)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a ConeGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the Cone generated with the public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new ConeGenerator
            {
                BaseRadius = Radius,
                EndAngleDeg = ToAngle * 360,
                Height = Height,
                NoSharedVertices = !SharedVertices,
                Slices = Tessellation,
                Rings = VTessellation,
                StartAngleDeg = FromAngle * 360,
                Clockwise = Clockwise
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "ConeMesh2");
        }
    }
}
