using g3;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering.ProceduralModels;
using VL.Lib.Collections;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Class used to generate a Stride Tube model mesh using geometry3Sharp
    /// </summary>
    [DataContract("TubeModel")]
    [Display("TubeModel")] // This name shows up in the procedural model dropdown list
    public class TubeModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Tube's path as a list of 3D vectors
        /// </summary>
        [DataMember(10)]
        public Spread<Vector3> Path { get; set; }

        /// <summary>
        /// Boolean value indicating if the tube's path should be a closed loop
        /// </summary>
        [DataMember(11)]
        public bool Closed { get; set; }

        /// <summary>
        /// Tube's shape as a list of 2D vectors
        /// </summary>
        [DataMember(12)]
        public Spread<Vector2> Shape { get; set; }

        /// <summary>
        /// Boolean value indicating if the tube should have caps
        /// </summary>
        [DataMember(13)]
        public bool Capped { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(14)]
        public bool Clockwise { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [DataMember(15)]
        public bool SharedVertices { get; set; } = false;

        /// <summary>
        /// Uses the DMesh3 instance generated from a TubeGenerator to create an equivalent Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]>
        /// </summary>
        /// <returns>A Stride GeometricMeshData<![CDATA[<VertexPositionNormalTexture>]]> equivalent to the Tube generated with the classes public property values</returns>
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            if (Path != null && Path.Count > 0)
            {
                var path = new DCurve3(Utils.ToVector3dList(Path), Closed);
                var tubeShape = new Polygon2d(Utils.ToVector2dList(Shape));

                var generator = new TubeGenerator(path, tubeShape)
                {
                    Capped = Capped,
                    Clockwise = Clockwise,
                    NoSharedVertices = !SharedVertices,
                };

                var meshGenerator = generator.Generate();

                return Utils.ToGeometricMeshData(meshGenerator.Generate().MakeDMesh(), "TubeModel");
            }
            return new GeometricMeshData<VertexPositionNormalTexture>(new VertexPositionNormalTexture[0], new int[0],false) { Name = "TubeModel" };
        }
    }
}
