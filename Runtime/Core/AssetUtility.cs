using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEngine.ProBuilder
{
    static class AssetUtility
    {
        public static string GetActiveSceneAssetDirectory()
        {
#if UNITY_EDITOR
            var scene = SceneManager.GetActiveScene();
            if (!File.Exists(scene.path))
                return "Assets/";
            return $"{Path.GetDirectoryName(scene.path)}/{scene.name}";
#else
            return "";
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void Save<T>(T obj) where T : Object
        {
            if (obj == null)
                return;
            // todo SaveAssetIfDirty isn't saving dirty assets...
            // AssetDatabase.SaveAssetIfDirty(obj);
            AssetDatabase.SaveAssets();
        }
    
        [Conditional("UNITY_EDITOR")]
        public static void SetDirty<T>(T obj) where T : Object
        {
            EditorUtility.SetDirty(obj);
        }
    
        public static T CreateSceneAsset<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
            var dir = GetActiveSceneAssetDirectory();
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath($"{dir}/{name}.asset"));
#endif
            return asset;
        }
    }
}

