using System;
using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.ProBuilder;
using UObject = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    [ScriptedImporter(1, MeshAsset.fileExtension)]
    public class MeshAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Can't use AssetDatabase.LoadAssetAtPath without .asset extension
            MeshAsset asset = ScriptableObject.CreateInstance<MeshAsset>();
            EditMesh mesh = JsonUtility.FromJson<EditMesh>(asset.meshPath);
            asset.name = asset.name;
            ctx.AddObjectToAsset("Mesh Asset", asset);
            EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(ctx.assetPath), asset);

            GameObject parent = new GameObject(asset.name);
            var component = parent.AddComponent<MeshComponent>();
            component.asset = asset;

            Rebuild(component, mesh, ctx);

            ctx.AddObjectToAsset("Main Object", parent);
            ctx.SetMainObject(parent);
        }

        public static void Apply(MeshComponent component)
        {
            var asset = component.asset;
            if(asset == null)
                throw new ArgumentException("component must reference a valid asset");
            // component.GetComponentInChildren<MeshRenderer>().sharedMaterial.color = component.mesh.color;
            var path = AssetDatabase.GetAssetPath(component.asset);
            if(string.IsNullOrEmpty(path))
                throw new Exception("no serialized asset to save to");
            File.WriteAllText(path, EditorJsonUtility.ToJson(asset));
        }

        public static EditMesh LoadMeshData(MeshAsset asset)
        {
            if(asset == null || string.IsNullOrEmpty(asset.meshPath))
                return null;
            if(Path.IsPathRooted(asset.meshPath))
                return JsonUtility.FromJson<EditMesh>(asset.meshPath);
            string path = AssetDatabase.GetAssetPath(asset);
            if(string.IsNullOrEmpty(path))
                throw new Exception("Cannot edit instance mesh data");
            var directory = Directory.GetParent(path);
            return JsonUtility.FromJson<EditMesh>(directory.FullName + asset.meshPath);
        }

        static void Rebuild(MeshComponent component, EditMesh mesh, AssetImportContext ctx)
        {
            var asset = component.asset;

            if(asset == null)
                throw new ArgumentException("component must reference a valid asset");

            Transform parent = component.transform;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var material = new Material(Shader.Find("Standard"));
            material.color = mesh.color;
            ctx.AddObjectToAsset("Main Material", material);
            cube.GetComponent<MeshRenderer>().sharedMaterial = material;
            cube.transform.SetParent(parent);
        }
    }
}
