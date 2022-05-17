using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride CappedCylinder model mesh using geometry3Sharp
    /// </summary>
    [DataContract("CappedCylinderModel")]
    [Display("CappedCylinderModel")] // This name shows up in the procedural model dropdown list
    public class CappedCylinderModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Cylinder's base radius
        /// </summary>
        [DataMember(10)]
        public float BaseRadius { get; set; } = 0.5f;

        /// <summary>
        /// Cylinder's top radius
        /// </summary>
        [DataMember(11)]
        public float TopRadius { get; set; } = 0.5f;

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
        /// Cylinder's height
        /// </summary>
        [DataMember(15)]
        public float Height { get; set; } = 1;


        /// <summary>
        /// 
        /// </summary>
        [DataMember(16)]
        public bool SharedVertices { get; set; } = false;

        /// <summary>
        /// Amount of slices to split the cylinder into. Higher calues result in smoother surfaces.
        /// </summary>
        [DataMember(17)]
        public int Slices { get; set; } = 16;

        /// <summary>
        /// Uses the DMesh3 instance generated from a CappedCylinderGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the CappedCylinder generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            var cappedCylinderGenerator = new CappedCylinderGenerator
            {
                BaseRadius = BaseRadius,
                TopRadius = TopRadius,
                Clockwise = Clockwise,
                EndAngleDeg = ToAngle * 360,
                Height = Height,
                NoSharedVertices = !SharedVertices,
                Slices = Slices,
                StartAngleDeg = FromAngle * 360
            };

            var meshGenerator = cappedCylinderGenerator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "CappedCylinderModel");
        }
    }
}
