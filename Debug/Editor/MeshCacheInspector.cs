using System;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    public class MeshCacheInspector : EditorWindow
    {
        [MenuItem("Tools/Save MeshCache Asset", false, 1200)]
        static void SaveMeshCache()
        {
            MeshCache.Save();
        }

        [MenuItem("Tools/Remove from MeshCache", false, 1200)]
        static void RemoveMeshCache()
        {
            foreach(var m in MeshSelection.top)
                MeshCache.Remove(m);
        }

        [MenuItem("Tools/Reset MeshCache Asset", false, 1200)]
        static void ResetMeshCache()
        {
            MeshCache.InternalReset();
            MeshCache.Save();
        }

        [MenuItem("Tools/Clean MeshCache Asset", false, 1200)]
        static void CleanMeshCache()
        {
            MeshCache.CleanUp();
            MeshCache.Save();
        }

        [MenuItem("Tools/Open MeshCache Window", false, 1200)]
        static void OpenWindow()
        {
            GetWindow<MeshCacheInspector>();
        }

        void OnEnable()
        {
            autoRepaintOnSceneChange = true;
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if(GUILayout.Button("Reset", EditorStyles.toolbarButton))
                ResetMeshCache();
            if(GUILayout.Button("Save", EditorStyles.toolbarButton))
                SaveMeshCache();
            if(GUILayout.Button("Clean", EditorStyles.toolbarButton))
                CleanMeshCache();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var cache = MeshCache.instance;

            foreach (var keyValuePair in cache.m_MeshLibrary)
            {
                var asset = keyValuePair.Key;

                GUILayout.Label(asset == null ? "null" : asset.name);
                foreach(var reference in keyValuePair.Value)
                    GUILayout.Label($"\t{reference.ToString("D")}");
            }
        }
    }
}
