using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract("DiscModel")]
    [Display("DiscModel")] // This name shows up in the procedural model dropdown list
    public class DiscModel : PrimitiveProceduralModelBase
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
        public int Slices { get; set; } = 16;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var puncturedDiscGenerator = new PuncturedDiscGenerator();

            puncturedDiscGenerator.Clockwise = Clockwise;
            puncturedDiscGenerator.EndAngleDeg = ToAngle * 360;
            puncturedDiscGenerator.InnerRadius = InnerRadius;
            puncturedDiscGenerator.OuterRadius = OuterRadius;
            puncturedDiscGenerator.Slices = Slices;
            puncturedDiscGenerator.StartAngleDeg = FromAngle * 360;

            var meshGenerator = puncturedDiscGenerator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "DiscModel");
        }
    }
}
