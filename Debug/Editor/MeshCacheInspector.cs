using UnityEngine;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(MeshCache))]
    public class MeshCacheInspector : Editor
    {
        [MenuItem("Tools/Save MeshCache Asset")]
        static void SaveMeshCache()
        {
            MeshCache.Save();
        }

        [MenuItem("Tools/Reset MeshCache Asset")]
        static void ResetMeshCache()
        {
            MeshCache.InternalReset();
            MeshCache.Save();
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Hello!");
        }
    }
}
