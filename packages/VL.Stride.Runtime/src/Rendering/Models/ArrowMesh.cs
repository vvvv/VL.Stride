using g3;
using Stride.Core;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride ArrowMesh using geometry3Sharp
    /// </summary>
    [DataContract("ArrowMesh")]
    [Display("ArrowMesh")] // This name shows up in the procedural model dropdown list
    public class ArrowMesh : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// ArrowMesh's stick length
        /// </summary>
        [DataMember(10)]
        public float StickLength { get; set; } = 1f;

        /// <summary>
        /// ArrowMesh's stick radius
        /// </summary>
        [DataMember(11)]
        public float StickRadius { get; set; } = 0.125f;

        /// <summary>
        /// ArrowMesh's base radius
        /// </summary>
        [DataMember(12)]
        public float HeadLength { get; set; } = 0.5f;

        /// <summary>
        /// ArrowMesh's head base radius
        /// </summary>
        [DataMember(13)]
        public float HeadBaseRadius { get; set; } = 0.25f;

        /// <summary>
        /// ArrowMesh's tip radius
        /// </summary>
        [DataMember(14)]
        public float TipRadius { get; set; } = 0f;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(15)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(16)]
        public bool SharedVertices { get; set; } = false;

        /// <summary>
        /// Amount of slices to split the ArrowMesh into. Higher calues result in smoother surfaces.
        /// </summary>
        [DataMember(17)]
        public int Tessellation { get; set; } = 16;

        /// <summary>
        /// Uses the DMesh3 instance generated from a Radial3DArrowGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the ArrowMesh generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {

        var generator = new Radial3DArrowGenerator
            {
                HeadBaseRadius = HeadBaseRadius,
                HeadLength = HeadLength,
                StickLength = StickLength,
                StickRadius = StickRadius,
                TipRadius = TipRadius,
                Clockwise = Clockwise,
                NoSharedVertices = !SharedVertices,
                Slices = Tessellation
            };

            var meshGenerator = generator.Generate();

            return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "ArrowMesh");
        }
    }
}
