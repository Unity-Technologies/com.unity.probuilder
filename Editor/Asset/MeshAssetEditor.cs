using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(MeshComponent))]
    class MeshComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var component = (MeshComponent) target;

            if(component == null)
                return;

            MeshAsset asset = component.asset;
            EditMesh mesh = MeshAssetImporter.LoadMeshData(asset);

            component.asset = (MeshAsset) EditorGUILayout.ObjectField("Mesh", asset, typeof(MeshAsset));

            EditorGUI.BeginChangeCheck();
            mesh.color = EditorGUILayout.ColorField("Color", mesh.color);
            if(EditorGUI.EndChangeCheck())
                MeshAssetImporter.Apply(component);
        }
    }

    public class MeshDataEditor : EditorWindow
    {
        IEnumerable<GameObject> m_Selection;

        [MenuItem("Window/MeshData Editor")]
        static void Init()
        {
            GetWindow<MeshDataEditor>();
        }

        void OnEnable()
        {
            OnSelectionChanged();
            autoRepaintOnSceneChange = true;
            wantsLessLayoutEvents = true;
            Selection.selectionChanged += OnSelectionChanged;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        void OnSelectionChanged()
        {
            m_Selection = Selection.gameObjects.Where(PrefabUtility.IsPartOfModelPrefab).Select(PrefabUtility.GetCorrespondingObjectFromOriginalSource);
            Repaint();
        }

        static void Write()
        {
            var data = CreateInstance<MeshAsset>();
            data.version = 1;
            data.name = "zoogers";
            var path = $"Assets/{data.name}.{MeshAsset.fileExtension}";
            File.WriteAllText(path, EditorJsonUtility.ToJson(data));
            AssetDatabase.ImportAsset(path);
        }

        void OnGUI()
        {
            if(GUILayout.Button("Save New"))
                Write();

            if(m_Selection == null)
                GUILayout.Label("null");

            foreach (var go in m_Selection)
            {
                EditorGUILayout.ObjectField("Mesh Data", go, typeof(MeshAsset));
            }
        }
    }
}
