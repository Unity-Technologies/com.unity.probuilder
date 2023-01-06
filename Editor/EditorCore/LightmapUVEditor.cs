using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    sealed class LightmapUVEditor : EditorWindow
    {
        List<ProBuilderMesh> m_MissingLightmaps = new List<ProBuilderMesh>();

        GUIContent m_AutoLightmapUVContent = new GUIContent("Auto Lightmap UVs", "Automatically build the lightmap UV array when editing ProBuilder meshes. If this feature is disabled, you will need to use the 'Generate UV2' action to build lightmap UVs for meshes prior to baking lightmaps.");
        internal static readonly Rect desiredPosition = new Rect(100, 100, 348, 234);

        void OnEnable()
        {
            m_MissingLightmaps = FindMissingLightmaps();
            EditorMeshUtility.meshOptimized += MeshOptimized;
        }

        void OnGUI()
        {
            GUILayout.Label("Lightmap UV Settings", EditorStyles.boldLabel);

            Lightmapping.autoUnwrapLightmapUV = EditorGUILayout.Toggle(m_AutoLightmapUVContent, Lightmapping.autoUnwrapLightmapUV);

            if (m_MissingLightmaps.Count > 0)
            {
                EditorGUILayout.HelpBox(GetMissingLightmapText(), MessageType.Warning);

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Build Missing Lightmap UVs"))
                {
                    // copy the missing lightmaps array so that MeshOptimized does not interfere with the rebuild iterator
                    Lightmapping.RebuildMissingLightmapUVs(m_MissingLightmaps.ToArray());
                    EditorUtility.ShowNotification("Rebuild Missing Lightmap UVs");
                }

                GUILayout.EndHorizontal();
            }
        }

        void MeshOptimized(ProBuilderMesh mesh, Mesh umesh)
        {
            var missing = IsMissingLightmaps(mesh);

            if (missing)
            {
                if (!m_MissingLightmaps.Contains(mesh))
                    m_MissingLightmaps.Add(mesh);
            }
            else
            {
                if (m_MissingLightmaps.Contains(mesh))
                    m_MissingLightmaps.Remove(mesh);
            }

            Repaint();
        }

        string GetMissingLightmapText()
        {
            var count = m_MissingLightmaps.Count;

            if (count < 2)
                return "There is 1 mesh missing Lightmap UVs in the open scenes.";

            return "There are " + m_MissingLightmaps.Count + " meshes missing Lightmap UVs in the open scenes.";
        }

        static bool IsMissingLightmaps(ProBuilderMesh mesh)
        {
#if UNITY_2019_2_OR_NEWER
            return mesh.gameObject.HasStaticFlag(StaticEditorFlags.ContributeGI) && !mesh.HasArrays(MeshArrays.Lightmap);
#else
            return mesh.gameObject.HasStaticFlag(StaticEditorFlags.LightmapStatic) && !mesh.HasArrays(MeshArrays.Lightmap);
#endif
        }

        static List<ProBuilderMesh> FindMissingLightmaps()
        {
            return EditorUtility.FindObjectsByType<ProBuilderMesh>().Where(IsMissingLightmaps).ToList();
        }
    }
}
