using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEngine.ProBuilder
{
    static class MeshHandles
    {
        static List<Vector3> s_Vector2List = new List<Vector3>();
        static List<Vector3> s_Vector3List = new List<Vector3>();
        static List<Vector4> s_Vector4List = new List<Vector4>();
        static List<int> s_IndexList = new List<int>();
        static List<int> s_SharedVertexIndexList = new List<int>();

        static readonly Vector2 k_Billboard0 = new Vector2(-1f, -1f);
        static readonly Vector2 k_Billboard1 = new Vector2(-1f,  1f);
        static readonly Vector2 k_Billboard2 = new Vector2( 1f, -1f);
        static readonly Vector2 k_Billboard3 = new Vector2( 1f,  1f);

        internal static void CreateFaceMesh(ProBuilderMesh mesh, Mesh target)
        {
            target.Clear();
            target.vertices = mesh.positionsInternal;
            target.triangles = mesh.selectedFacesInternal.SelectMany(x => x.indexes).ToArray();
        }

        internal static void CreateEdgeMesh(ProBuilderMesh mesh, Mesh target)
        {
            int edgeCount = 0;
            int faceCount = mesh.faceCount;

            for (int i = 0; i < faceCount; i++)
                edgeCount += mesh.facesInternal[i].edgesInternal.Length;

            s_IndexList.Clear();
            s_IndexList.Capacity = edgeCount * 2;

            int edgeIndex = 0;

            for (int i = 0; i < faceCount && edgeIndex < edgeCount; i++)
            {
                for (int n = 0; n < mesh.facesInternal[i].edgesInternal.Length && edgeIndex < edgeCount; n++)
                {
                    var edge = mesh.facesInternal[i].edgesInternal[n];
                    s_IndexList.Add(edge.a);
                    s_IndexList.Add(edge.b);

                    edgeIndex++;
                }
            }

            target.Clear();
            target.indexFormat = edgeCount * 2 > ushort.MaxValue ? Rendering.IndexFormat.UInt16 : Rendering.IndexFormat.UInt32;
            target.name = "ProBuilder::EdgeMesh" + target.GetInstanceID();
            target.vertices = mesh.positionsInternal;
            target.subMeshCount = 1;
#if UNITY_2019_3_OR_NEWER
            target.SetIndices(s_IndexList, MeshTopology.Lines, 0);
#else
            target.SetIndices(s_IndexList.ToArray(), MeshTopology.Lines, 0);
#endif
        }

        internal static void CreateEdgeMesh(ProBuilderMesh mesh, Mesh target, Edge[] edges)
        {
            int edgeCount = edges.Length;
            int vertexCount = edgeCount * 2;

            s_IndexList.Clear();
            s_IndexList.Capacity = vertexCount;

            for (int n = 0; n < edgeCount; n++)
            {
                var edge = edges[n];
                s_IndexList.Add(edge.a);
                s_IndexList.Add(edge.b);
            }

            target.Clear();
            target.indexFormat = vertexCount > ushort.MaxValue ? Rendering.IndexFormat.UInt16 : Rendering.IndexFormat.UInt32;
            target.name = "ProBuilder::EdgeMesh" + target.GetInstanceID();
            target.vertices = mesh.positionsInternal;
            target.subMeshCount = 1;
#if UNITY_2019_3_OR_NEWER
            target.SetIndices(s_IndexList, MeshTopology.Lines, 0);
#else
            target.SetIndices(s_IndexList.ToArray(), MeshTopology.Lines, 0);
#endif
        }

        internal static void CreateVertexMesh(ProBuilderMesh mesh, Mesh target)
        {
            s_SharedVertexIndexList.Clear();
            int sharedVertexCount = mesh.sharedVerticesInternal.Length;
            s_SharedVertexIndexList.Capacity = sharedVertexCount;

            for (int i = 0; i < sharedVertexCount; i++)
                s_SharedVertexIndexList.Add(mesh.sharedVerticesInternal[i][0]);

            CreateVertexMesh(mesh, target, s_SharedVertexIndexList);
        }

        internal static void CreateVertexMesh(ProBuilderMesh mesh, Mesh target, IList<int> indexes)
        {
            if (BuiltinMaterials.geometryShadersSupported)
                CreatePointMesh(mesh.positionsInternal, indexes, target);
            else
                CreatePointBillboardMesh(mesh.positionsInternal, indexes, target);
        }

        static void CreatePointMesh(Vector3[] positions, IList<int> indexes, Mesh target)
        {
            int vertexCount = positions.Length;
            target.Clear();
            target.indexFormat = vertexCount > ushort.MaxValue ? Rendering.IndexFormat.UInt16 : Rendering.IndexFormat.UInt32;
            target.name = "ProBuilder::PointMesh";
            target.vertices = positions;
            target.subMeshCount = 1;

            if(indexes is int[])
                target.SetIndices((int[]) indexes, MeshTopology.Points, 0);
#if UNITY_2019_3_OR_NEWER
            else if(indexes is List<int>)
                target.SetIndices((List<int>) indexes, MeshTopology.Points, 0);
#endif
            else
                target.SetIndices(indexes.ToArray(), MeshTopology.Points, 0);
        }

        internal static void CreatePointBillboardMesh(IList<Vector3> positions, Mesh target)
        {
            var pointCount = positions.Count;
            var vertexCount = pointCount * 4;

            s_Vector2List.Clear();
            s_Vector3List.Clear();
            s_IndexList.Clear();
            s_Vector2List.Capacity = vertexCount;
            s_Vector3List.Capacity = vertexCount;
            s_IndexList.Capacity = vertexCount;

            for (int i = 0; i < pointCount; i++)
            {
                s_Vector3List.Add(positions[i]);
                s_Vector3List.Add(positions[i]);
                s_Vector3List.Add(positions[i]);
                s_Vector3List.Add(positions[i]);

                s_Vector2List.Add(k_Billboard0);
                s_Vector2List.Add(k_Billboard1);
                s_Vector2List.Add(k_Billboard2);
                s_Vector2List.Add(k_Billboard3);

                s_IndexList.Add(i * 4 + 0);
                s_IndexList.Add(i * 4 + 1);
                s_IndexList.Add(i * 4 + 3);
                s_IndexList.Add(i * 4 + 2);
            }

            target.Clear();
            target.indexFormat = vertexCount > ushort.MaxValue ? Rendering.IndexFormat.UInt32 : Rendering.IndexFormat.UInt16;
            target.SetVertices(s_Vector3List);
            target.SetUVs(0, s_Vector2List);
            target.subMeshCount = 1;
#if UNITY_2019_3_OR_NEWER
            target.SetIndices(s_IndexList, MeshTopology.Quads, 0);
#else
            target.SetIndices(s_IndexList.ToArray(), MeshTopology.Quads, 0);
#endif
        }

        static void CreatePointBillboardMesh(IList<Vector3> positions, IList<int> indexes, Mesh target)
        {
            var pointCount = indexes.Count;
            var vertexCount = pointCount * 4;

            s_Vector2List.Clear();
            s_Vector3List.Clear();
            s_IndexList.Clear();
            s_Vector2List.Capacity = vertexCount;
            s_Vector3List.Capacity = vertexCount;
            s_IndexList.Capacity = vertexCount;

            for (int i = 0; i < pointCount; i++)
            {
                var index = indexes[i];

                s_Vector3List.Add(positions[index]);
                s_Vector3List.Add(positions[index]);
                s_Vector3List.Add(positions[index]);
                s_Vector3List.Add(positions[index]);

                s_Vector2List.Add(k_Billboard0);
                s_Vector2List.Add(k_Billboard1);
                s_Vector2List.Add(k_Billboard2);
                s_Vector2List.Add(k_Billboard3);

                s_IndexList.Add(i * 4 + 0);
                s_IndexList.Add(i * 4 + 1);
                s_IndexList.Add(i * 4 + 3);
                s_IndexList.Add(i * 4 + 2);
            }

            target.Clear();
            target.indexFormat = vertexCount > ushort.MaxValue ? Rendering.IndexFormat.UInt32 : Rendering.IndexFormat.UInt16;
            target.SetVertices(s_Vector3List);
            target.SetUVs(0, s_Vector2List);
            target.subMeshCount = 1;
#if UNITY_2019_3_OR_NEWER
            target.SetIndices(s_IndexList, MeshTopology.Quads, 0);
#else
            target.SetIndices(s_IndexList.ToArray(), MeshTopology.Quads, 0);
#endif
        }

        internal static void CreateEdgeBillboardMesh(ProBuilderMesh mesh, Mesh target)
        {
            target.Clear();
            const ushort k_MaxPointCountUShort = ushort.MaxValue / 4;
            var lineCount = mesh.edgeCount;

            target.indexFormat = lineCount > k_MaxPointCountUShort
                ? Rendering.IndexFormat.UInt32
                : Rendering.IndexFormat.UInt16;

            var vertices = mesh.positionsInternal;

            s_Vector3List.Clear();
            s_Vector4List.Clear();
            s_IndexList.Clear();
            s_Vector3List.Capacity = lineCount * 4;
            s_Vector4List.Capacity = lineCount * 4;
            s_IndexList.Capacity = lineCount * 4;

            int n = 0;

            foreach(var face in mesh.facesInternal)
            {
                foreach (var edge in face.edgesInternal)
                {
                    Vector3 a = vertices[edge.a], b = vertices[edge.b];
                    Vector3 c = b + (b - a);

                    s_Vector3List.Add(a);
                    s_Vector3List.Add(a);
                    s_Vector3List.Add(b);
                    s_Vector3List.Add(b);

                    s_Vector4List.Add(new Vector4(b.x, b.y, b.z, 1f));
                    s_Vector4List.Add(new Vector4(b.x, b.y, b.z, -1f));
                    s_Vector4List.Add(new Vector4(c.x, c.y, c.z, 1f));
                    s_Vector4List.Add(new Vector4(c.x, c.y, c.z, -1f));

                    s_IndexList.Add(n + 0);
                    s_IndexList.Add(n + 1);
                    s_IndexList.Add(n + 3);
                    s_IndexList.Add(n + 2);

                    n += 4;
                }
            }

            target.SetVertices(s_Vector3List);
            target.SetTangents(s_Vector4List);
            target.subMeshCount = 1;
#if UNITY_2019_3_OR_NEWER
            target.SetIndices(s_IndexList, MeshTopology.Quads, 0);
#else
            target.SetIndices(s_IndexList.ToArray(), MeshTopology.Quads, 0);
#endif
        }

        internal static void CreateEdgeBillboardMesh(ProBuilderMesh mesh, Mesh target, ICollection<Edge> edges)
        {
            target.Clear();

            const ushort k_MaxPointCountUShort = ushort.MaxValue / 4;

            var lineCount = edges.Count;

            target.indexFormat = lineCount > k_MaxPointCountUShort
                ? Rendering.IndexFormat.UInt32
                : Rendering.IndexFormat.UInt16;

            var vertices = mesh.positionsInternal;

            s_Vector3List.Clear();
            s_Vector4List.Clear();
            s_IndexList.Clear();
            s_Vector3List.Capacity = lineCount * 4;
            s_Vector4List.Capacity = lineCount * 4;
            s_IndexList.Capacity = lineCount * 4;

            int n = 0;

            foreach (var edge in edges)
            {
                Vector3 a = vertices[edge.a], b = vertices[edge.b];
                Vector3 c = b + (b - a);

                s_Vector3List.Add(a);
                s_Vector3List.Add(a);
                s_Vector3List.Add(b);
                s_Vector3List.Add(b);

                s_Vector4List.Add(new Vector4(b.x, b.y, b.z, 1f));
                s_Vector4List.Add(new Vector4(b.x, b.y, b.z, -1f));
                s_Vector4List.Add(new Vector4(c.x, c.y, c.z, 1f));
                s_Vector4List.Add(new Vector4(c.x, c.y, c.z, -1f));

                s_IndexList.Add(n + 0);
                s_IndexList.Add(n + 1);
                s_IndexList.Add(n + 3);
                s_IndexList.Add(n + 2);

                n += 4;
            }

            target.SetVertices(s_Vector3List);
            target.SetTangents(s_Vector4List);
            target.subMeshCount = 1;
#if UNITY_2019_3_OR_NEWER
            target.SetIndices(s_IndexList, MeshTopology.Quads, 0);
#else
            target.SetIndices(s_IndexList.ToArray(), MeshTopology.Quads, 0);
#endif
        }
    }
}
