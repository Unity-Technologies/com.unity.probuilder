using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

public class MeshEditor
{
    [InitializeOnLoadMethod]
    static void UndoRedo()
    {
        Undo.undoRedoPerformed += () =>
        {
            foreach(var filter in Selection.GetFiltered<PMeshFilter>(SelectionMode.Deep))
                filter.SyncMeshFilter();
        };
    }

    static string GetActiveSceneAssetDirectory()
    {
        var scene = SceneManager.GetActiveScene();
        if (!File.Exists(scene.path))
            return "Assets/";
        return $"{Path.GetDirectoryName(scene.path)}/{scene.name}";
    }
    
    static T CreateSceneAsset<T>() where T : ScriptableObject
    {
        
        var asset = ScriptableObject.CreateInstance<T>();
        var dir = GetActiveSceneAssetDirectory();
        if(!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath($"{dir}/mesh.asset"));
        return asset;
    }

    static Face[] ConvertTrisToFaces(int[] tris)
    {
        var faces = new Face[tris.Length / 3];
        for (int i = 0, c = tris.Length - 2; i < c; i += 3)
            faces[i/3] = new Face(new[] { tris[i], tris[i + 1], tris[i + 2] });
        return faces;
    }

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
        filter.mesh.faces = ConvertTrisToFaces(cube.triangles);
        filter.SyncMeshFilter();

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

            var mesh = filter.mesh;
            EditorGUILayout.LabelField("m_Version", $"{mesh.version}");
            EditorGUILayout.LabelField("m_CompiledVersion", $"{mesh.compiledVersion}");

            if (GUILayout.Button("expand"))
                ScaleMesh(filter, 1.5f);
            
            if (GUILayout.Button("contract"))
                ScaleMesh(filter, .5f);
            
            if (GUILayout.Button("rebuild / upload / compile"))
                filter.SyncMeshFilter();
        }
    }

    static void ScaleMesh(PMeshFilter filter, float scale)
    {
        Undo.RecordObject(filter.mesh, $"Scale Mesh {scale}");
        var mesh = filter.mesh;
        var vertices = mesh.positions;
        for (int i = 0, c = vertices.Count; i < c; i++)
            vertices[i] = vertices[i] * scale;
        mesh.positions = vertices;
        filter.SyncMeshFilter();
    }
}

