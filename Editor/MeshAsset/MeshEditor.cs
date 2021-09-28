
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public class MeshEditor
{
    [MenuItem("GameObject/ProBuilder Mesh")]
    static void Init()
    {
        var gameObject = ObjectFactory.CreateGameObject("ProBuilder Mesh", typeof(PMeshFilter));
        var filter = gameObject.GetComponent<PMeshFilter>();
        filter.mesh = CreateSceneAsset<PMesh>();

        var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var cube = temp.GetComponent<MeshFilter>().sharedMesh;
        var renderer = filter.GetComponent<MeshRenderer>();
        
        renderer.sharedMaterials = new[] { temp.GetComponent<MeshRenderer>().sharedMaterial };
        filter.mesh.positions = cube.vertices;
        filter.mesh.indices = cube.triangles;
        Object.DestroyImmediate(temp);
        
        ObjectFactory.PlaceGameObject(gameObject);
    }

    [CustomEditor(typeof(PMeshFilter))]
    class PMeshFilterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var filter = (target as PMeshFilter);

            if (filter == null)
                return;

            if (GUILayout.Button("expand"))
                ScaleMesh(filter, 1.5f);
            
            if (GUILayout.Button("contract"))
                ScaleMesh(filter, .5f);
        }
    }

    static void ScaleMesh(PMeshFilter filter, float scale)
    {
        Undo.RecordObject(filter.mesh, $"Scale Mesh {scale}");
        var mesh = filter.mesh;
        var vertices = mesh.positions;
        for (int i = 0, c = vertices.Length; i < c; i++)
            vertices[i] = vertices[i] * scale;
        mesh.positions = vertices;
        filter.SyncMeshFilter();
    }

    static T CreateSceneAsset<T>() where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        if(!Directory.Exists("Assets/temp"))
            Directory.CreateDirectory("Assets/temp");
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath("Assets/temp/mesh.asset"));
        return asset;
    }
}

