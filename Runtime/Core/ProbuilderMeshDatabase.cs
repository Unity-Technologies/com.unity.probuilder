using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    class ProbuilderMeshDatabase
    {
        class MeshCache
        {
            public Mesh mesh;
            public int instanceCount;
        }

        static Dictionary<string, MeshCache> s_MeshCache = new Dictionary<string, MeshCache>();

        public static Mesh GetOrCreateMesh(string id, ProBuilderMesh mesh)
        {
            if (!s_MeshCache.TryGetValue(id, out MeshCache cache))
            {
                cache = new MeshCache
                {
                    instanceCount = 1,
                    mesh = new Mesh()
                };
                PopulateMesh(mesh, cache.mesh);
            }
            else
            {
                cache.instanceCount++;
            }

            return cache.mesh;
        }

        static void PopulateMesh(ProBuilderMesh pbMesh, Mesh mesh)
        {
            mesh.Clear();
            //Rebuild mesh
        }

        public static void UpdateMesh(string id, ProBuilderMesh mesh)
        {
            if (s_MeshCache.TryGetValue(id, out MeshCache cache))
            {
                PopulateMesh(mesh, cache.mesh);
            }
        }

        public static void ReleaseMesh(string id)
        {
            if (s_MeshCache.TryGetValue(id, out MeshCache cache))
            {
                cache.instanceCount--;
                if (cache.instanceCount <= 0)
                {
                    Object.DestroyImmediate(cache.mesh);
                    s_MeshCache.Remove(id);
                }
            }
        }
    }
}
