using g3;
using Stride.Core.Mathematics;
using Stride.Graphics;
using System.Collections.Generic;
using VL.Lib.Collections;

namespace VL.Stride.Rendering.Models
{
    /// <summary>
    /// Utility class to aid with G3 to Stride conversions
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Uses DMesh3's vertex, normal and UV data to generate a stride equivalent
        /// </summary>
        /// <param name="g3Mesh">A g3 DMesh3 instance</param>
        /// <param name="name">The model name</param>
        /// <returns></returns>
        public static GeometricMeshData<VertexPositionNormalTexture> ToGeometricMeshData(DMesh3 g3Mesh, string name)
        {
            if (g3Mesh is null)
                return null;

            return ToGeometricMeshData(new SimpleMesh(g3Mesh), name);
        }

        /// <summary>
        /// Uses SimpleMesh's vertex, normal and UV data to generate a stride equivalent
        /// </summary>
        /// <param name="g3Mesh">A g3 SimpleMesh instance</param>
        /// <param name="name">The model name</param>
        /// <returns></returns>
        public static GeometricMeshData<VertexPositionNormalTexture> ToGeometricMeshData(SimpleMesh g3Mesh, string name)
        {
            if (g3Mesh is null || g3Mesh.VertexCount == 0)
                return null;

            var vertexCount = g3Mesh.VertexCount;
            var vertices = new VertexPositionNormalTexture[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                var vi = g3Mesh.GetVertexAll(i);
                var normal = new Vector3(vi.n.x, vi.n.y, vi.n.z);
                var uv = new Vector2(vi.uv.x, vi.uv.y);
                vertices[i] = new VertexPositionNormalTexture(new Vector3((float)vi.v.x, (float)vi.v.y, (float)vi.v.z), normal, uv);
            }

            return new GeometricMeshData<VertexPositionNormalTexture>(vertices, g3Mesh.Triangles.ToArray(), isLeftHanded: true) { Name = name };
        }

        /// <summary>
        /// Converts a Stride Vector3 to a geomtry3Sharp Vector3d
        /// </summary>
        /// <param name="vector">Stride Vector3</param>
        /// <returns>A geomtry3Sharp Vector3d</returns>
        public static Vector3d ToVector3d(Vector3 vector)
        {
            if (vector != null)
                return new Vector3d(vector.X, vector.Y, vector.Z);
            return Vector3d.Zero;
        }

        /// <summary>
        /// Converts an IList of Stride Vector3 to an IList of geomtry3Sharp Vector3d
        /// </summary>
        /// <param name="vectors">IList of Stride Vector3</param>
        /// <returns>An equivalent IList of geomtry3Sharp Vector3d</returns>
        public static IList<Vector3d> ToVector3dList(Spread<Vector3> vectors)
        {
            if (vectors != null && vectors.Count > 0)
            {
                var result = new Vector3d[vectors.Count];
                for (int i = 0; i < vectors.Count; i++)
                {
                    result[i] = ToVector3d(vectors[i]);
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Converts a Stride Vector2 to a geomtry3Sharp Vector2d
        /// </summary>
        /// <param name="vector">Stride Vector2</param>
        /// <returns>A geomtry3Sharp Vector2d</returns>
        public static Vector2d ToVector2d(Vector2 vector)
        {
            if (vector != null)
                return new Vector2d(vector.X, vector.Y);
            return Vector2d.Zero;
        }

        /// <summary>
        /// Converts an IList of Stride Vector2 to an IList of geomtry3Sharp Vector2d
        /// </summary>
        /// <param name="vectors">IList of Stride Vector2</param>
        /// <returns>An equivalent IList of geomtry3Sharp Vector2d</returns>
        public static IList<Vector2d> ToVector2dList(Spread<Vector2> vectors)
        {
            if (vectors != null && vectors.Count > 0)
            {
                var result = new Vector2d[vectors.Count];
                for (int i = 0; i < vectors.Count; i++)
                {
                    result[i] = ToVector2d(vectors[i]);
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Converts a VL.Stride.Rendering.Models.Corner to a g3.RoundRectGenerator.Corner
        /// </summary>
        /// <param name="corner">A VL.Stride.Rendering.Models.RoundRectModel.Corner</param>
        /// <returns>A g3.RoundRectGenerator.Corner</returns>
        public static RoundRectGenerator.Corner ToCorner(RoundRectMesh.Corner corner)
        {
            return (RoundRectGenerator.Corner)((byte)corner);
        }

        /// <summary>
        /// Converts a spread of VL.Stride.Rendering.Models.VerticalGeneralizedCylinderModel.CircularSection to an array of g3.VerticalGeneralizedCylinderGenerator.CircularSection
        /// </summary>
        /// <param name="sections">A spread of VL.Stride.Rendering.Models.VerticalGeneralizedCylinderModel.CircularSection</param>
        /// <returns>An equivalent array of g3.VerticalGeneralizedCylinderGenerator.CircularSection</returns>
        public static MeshGenerator.CircularSection[] ToCircularSectionArray(Spread<VerticalGeneralizedCylinderMesh.CircularSection> sections)
        {
            if (sections != null && sections.Count > 0)
            {
                var result = new MeshGenerator.CircularSection[sections.Count];

                for (int i = 0; i < sections.Count; i++)
                {
                    result[i] = ToCircularSection(sections[i]);
                }

                return result;
            }
            return null;
        }

        /// <summary>
        /// Converts a VL.Stride.Rendering.Models.VerticalGeneralizedCylinderModel.CircularSection to a g3.VerticalGeneralizedCylinderGenerator.CircularSection
        /// </summary>
        /// <param name="section">An instance of a spread of VL.Stride.Rendering.Models.VerticalGeneralizedCylinderModel.CircularSection</param>
        /// <returns>An equivalent instance of g3.VerticalGeneralizedCylinderGenerator.CircularSection</returns>
        public static MeshGenerator.CircularSection ToCircularSection(VerticalGeneralizedCylinderMesh.CircularSection section)
        {
            if (section != null)
                return new MeshGenerator.CircularSection(section.Radius, section.SectionY);
            return new MeshGenerator.CircularSection(0, 0);
        }
    }
}
