using System;
using System.Linq;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    static class PMeshCompiler
    {
        public static bool Compile(PMesh src, Mesh dst)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (dst == null)
                throw new ArgumentNullException(nameof(dst));

// #if DEVELOPER_MODE
            Assertions.Assert.IsNotNull(src.positions);
            Assertions.Assert.IsFalse(src.positions.Count < 3);
            Assertions.Assert.IsNotNull(src.faces);
            Assertions.Assert.IsFalse(src.faces.Count < 1);
// #endif

            if (src.positions == null || src.positions.Count < 3 || src.faces?.Count < 1)
                return false;

            dst.Clear();
            dst.indexFormat = src.vertexCount > ushort.MaxValue 
                ? Rendering.IndexFormat.UInt32 
                : Rendering.IndexFormat.UInt16;
            dst.SetVertices(src.positions as Vector3[]);

            int subMeshCount = src.faces.Max(x => x.submeshIndex) + 1;
            Submesh[] submeshes = Submesh.GetSubmeshes(src.faces, subMeshCount, MeshTopology.Triangles);

            dst.subMeshCount = submeshes.Length;

            for (int i = 0; i < dst.subMeshCount; i++)
            {
#if DEVELOPER_MODE
                if (i >= materialCount)
                    Log.Warning("Submesh index " + i + " is out of bounds of the MeshRenderer materials array.");
                if (submeshes[i] == null)
                    throw new Exception("Attempting to assign a null submesh. " + i + "/" + materialCount);
#endif
                dst.SetIndices(submeshes[i].m_Indexes, submeshes[i].m_Topology, i, false);
            }

            return false;
        }
    }
}
