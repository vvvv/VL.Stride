using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Generates a Cylinder mesh
    /// </summary>
    [DataContract("CylinderMesh2")]
    [Display("CylinderMesh2")] // This name shows up in the procedural model dropdown list
    public class CylinderMesh2 : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Boolean value indicating if the cylinder should have caps
        /// </summary>
        [DataMember(10)]
        public bool Capped { get; set; } = true;

        /// <summary>
        /// Cylinder's base radius
        /// </summary>
        [DataMember(11)]
        public float BaseRadius { get; set; } = 0.5f;

        /// <summary>
        /// Cylinder's top radius
        /// </summary>
        [DataMember(12)]
        public float TopRadius { get; set; } = 0.5f;

        /// <summary>
        /// Cylinder's initial angle in cycles 
        /// </summary>
        [DataMember(13)]
        public float FromAngle { get; set; } = 0f;

        /// <summary>
        /// Cylinder's final angle in cycles
        /// </summary>
        [DataMember(14)]
        public float ToAngle { get; set; } = 1f;

        /// <summary>
        /// Cylinder's height
        /// </summary>
        [DataMember(15)]
        public float Height { get; set; } = 1;

        /// <summary>
        /// Cylinder's tessellation (amount of radial slices to split the cylinder into). Higher values result in smoother surfaces
        /// </summary>
        [DataMember(16)]
        public int Tessellation { get; set; } = 16;

        /// <summary>
        /// Cylinder's vertical tessellation (amount of vertical slices to split the cylinder into)
        /// </summary>
        [DataMember(17)]
        public int VTessellation { get; set; } = 2;

        [DataMember(18)]
        public bool SharedVertices { get; set; } = false;

        [DataMember(19)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a OpenCylinderGenerator or CappedCylinderGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the Cylinder generated with the public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            bool closed = ((1 - FromAngle) - (1 - ToAngle)) > 0.99f;
            MeshGenerator generator;
            if (Capped)
            {
                generator = new CappedCylinderGenerator
                {
                    BaseRadius = BaseRadius,
                    TopRadius = TopRadius,
                    StartAngleDeg = (1 - ToAngle) * 360,
                    EndAngleDeg = (1 - FromAngle) * 360,
                    Height = Height,
                    Slices = closed ? Tessellation : Tessellation + 1,
                    Rings = VTessellation,
                    NoSharedVertices = !SharedVertices,
                    Clockwise = Clockwise
                };
            }
            else
            {
                generator = new OpenCylinderGenerator
                {
                    BaseRadius = BaseRadius,
                    TopRadius = TopRadius,
                    StartAngleDeg = (1 - ToAngle) * 360,
                    EndAngleDeg = (1 - FromAngle) * 360,
                    Height = Height,
                    Slices = closed ? Tessellation : Tessellation + 1,
                    Rings = VTessellation,
                    NoSharedVertices = !SharedVertices,
                    Clockwise = Clockwise
                };
            }

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "CylinderMesh2");
        }
    }
}
