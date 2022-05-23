using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;
using VL.Lib.Collections;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride VerticalGeneralizedCylinder model mesh using geometry3Sharp
    /// </summary>
    [DataContract("VerticalGeneralizedCylinderModel")]
    [Display("VerticalGeneralizedCylinderModel")] // This name shows up in the procedural model dropdown list
    public class VerticalGeneralizedCylinderModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Boolean value indicating if the cylinder should have caps
        /// </summary>
        [DataMember(10)]
        public bool Capped { get; set; } = true;

        /// <summary>
        /// Spread of CircularSection instances describing the different sections that make up the cylinder
        /// </summary>
        [DataMember(11)]
        public Spread<CircularSection> Sections { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(12)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(13)]
        public bool SharedVertices { get; set; } = false;

        /// <summary>
        /// Amount of slices to split the cylinder into. Higher calues result in smoother surfaces.
        /// </summary>
        [DataMember(14)]
        public int Slices { get; set; } = 16;

        /// <summary>
        /// Uses the DMesh3 instance generated from a VerticalGeneralizedCylinderGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the VerticalGeneralizedCylinder generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new VerticalGeneralizedCylinderGenerator
            {
                Capped = Capped,
                Sections = Utils.ToCircularSectionArray(Sections),
                Clockwise = Clockwise,
                NoSharedVertices = !SharedVertices,
                Slices = Slices
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "VerticalGeneralizedCylinderModel");
        }

        /// <summary>
        /// Represents a circular section used to define a VerticalGeneralizedCylinderModel.
        /// </summary>
        public class CircularSection
        {
            /// <summary>
            /// Section's radius
            /// </summary>
            public float Radius { get; set; }

            /// <summary>
            /// Section's position in the Y axis
            /// </summary>
            public float SectionY { get; set; }

            /// <summary>
            /// Basic constructor for CircularSection
            /// </summary>
            /// <param name="radius">Section's radius</param>
            /// <param name="sectionY">Section's position in the Y axis</param>
            public CircularSection(float radius, float sectionY)
            {
                Radius = radius;
                SectionY = sectionY;
            }
        }
    }
}
