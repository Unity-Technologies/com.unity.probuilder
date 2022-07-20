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

        public static Mesh GetOrCreateMesh(string id)
        {
            if (!s_MeshCache.TryGetValue(id, out MeshCache cache))
            {
                cache = new MeshCache
                {
                    instanceCount = 1,
                    mesh = new Mesh()
                };
                s_MeshCache.Add(id, cache);
            }
            else
            {
                cache.instanceCount++;
            }

            return cache.mesh;
        }
        public static void ReleaseMesh(string id)
        {
            if (s_MeshCache.TryGetValue(id, out MeshCache cache))
            {
                cache.instanceCount--;
                if (cache.instanceCount <= 0)
                {
                    s_MeshCache.Remove(id);
                    Object.DestroyImmediate(cache.mesh);
                }
            }
        }
    }
}
