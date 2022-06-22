using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;
using System.Collections.Generic;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Generates a Vertical Generalized Cylinder mesh, described by multiple concentric circular sections at different distances in the Y axis
    /// </summary>
    [DataContract("VerticalGeneralizedCylinderMesh")]
    [Display("VerticalGeneralizedCylinderMesh")] // This name shows up in the procedural model dropdown list
    public class VerticalGeneralizedCylinderMesh : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Boolean value indicating if the cylinder should have caps
        /// </summary>
        [DataMember(10)]
        public bool Capped { get; set; } = true;

        /// <summary>
        /// IReadOnlyList of circular sections that make up the cylinder
        /// </summary>
        [DataMember(11)]
        public IReadOnlyList<CircularSection> Sections { get; set; }

        /// <summary>
        /// Cylinder's tessellation (amount of radial slices to split the cylinder into). Higher values result in smoother surfaces
        /// </summary>
        [DataMember(12)]
        public int Tessellation { get; set; } = 16;

        [DataMember(13)]
        public bool SharedVertices { get; set; } = false;

        [DataMember(14)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a VerticalGeneralizedCylinderGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the VerticalGeneralizedCylinder generated with the public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new VerticalGeneralizedCylinderGenerator
            {
                Capped = Capped,
                Sections = Utils.ToCircularSectionArray(Sections),
                Slices = Tessellation,
                NoSharedVertices = !SharedVertices,
                Clockwise = Clockwise
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "VerticalGeneralizedCylinderMesh", UvScale);
        }

        /// <summary>
        /// Represents a circular section used to define a VerticalGeneralizedCylinderMesh.
        /// </summary>
        public class CircularSection
        {
            /// <summary>
            /// Circular section's radius
            /// </summary>
            public float Radius { get; set; }

            /// <summary>
            /// Circular section's position in the Y axis
            /// </summary>
            public float SectionY { get; set; }

            /// <summary>
            /// Basic constructor for CircularSection
            /// </summary>
            /// <param name="radius">Circular section's radius</param>
            /// <param name="sectionY">Circular section's position in the Y axis</param>
            public CircularSection(float radius, float sectionY)
            {
                Radius = radius;
                SectionY = sectionY;
            }
        }
    }
}
