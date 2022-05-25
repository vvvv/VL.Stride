using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride Disc model mesh using geometry3Sharp
    /// </summary>
    [DataContract("DiscMesh")]
    [Display("DiscMesh")] // This name shows up in the procedural model dropdown list
    public class DiscMesh : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Disc's outer radius
        /// </summary>
        [DataMember(10)]
        public float OuterRadius { get; set; } = 1f;

        /// <summary>
        /// Disc's inner radius
        /// </summary>
        [DataMember(11)]
        public float InnerRadius { get; set; } = 0.5f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(12)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// Initial angle in cycles 
        /// </summary>
        [DataMember(13)]
        public float FromAngle { get; set; } = 0f;

        /// <summary>
        /// Final angle in cycles
        /// </summary>
        [DataMember(14)]
        public float ToAngle { get; set; } = 1f;

        /// <summary>
        /// Amount of slices to split the cylinder into. Higher calues result in smoother surfaces.
        /// </summary>
        [DataMember(15)]
        public int Tessellation { get; set; } = 16;

        /// <summary>
        /// Uses the DMesh3 instance generated from a PuncturedDiscGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the PuncturedDisc generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var generator = new PuncturedDiscGenerator
            {
                Clockwise = Clockwise,
                EndAngleDeg = ToAngle * 360,
                InnerRadius = InnerRadius,
                OuterRadius = OuterRadius,
                Slices = Tessellation,
                StartAngleDeg = FromAngle * 360
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "DiscMesh");
        }
    }
}
