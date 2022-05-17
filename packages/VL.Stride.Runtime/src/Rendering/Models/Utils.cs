using g3;
using Stride.Core.Mathematics;
using Stride.Graphics;

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
    }
}
