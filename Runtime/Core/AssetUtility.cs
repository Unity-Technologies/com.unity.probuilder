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
    
        public static T CreateSceneAsset<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
            var dir = GetActiveSceneAssetDirectory();
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath($"{dir}/mesh.asset"));
#endif
            return asset;
        }
    }
}

