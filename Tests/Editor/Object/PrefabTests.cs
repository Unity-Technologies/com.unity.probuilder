using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.TestTools;

public class PrefabTests
{
    static GameObject CreatePrefab()
    {
        var prefabPath = AssetDatabase.GenerateUniqueAssetPath("Assets/PrefabTest.prefab");
        var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
        var prefab = PrefabUtility.SaveAsPrefabAsset(mesh.gameObject, prefabPath);
        Assume.That(prefab, Is.Not.Null);
        Assume.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(prefabPath));
        return prefab;
    }

    static void DestroyPrefab(GameObject prefab)
    {
        var path = AssetDatabase.GetAssetPath(prefab);
        Assume.That(!string.IsNullOrEmpty(path));
        AssetDatabase.DeleteAsset(path);
    }

    [Test]
    public void CreatePrefab_DoesNot_SerializeMeshFilter()
    {
        var prefab = CreatePrefab();
        Assert.That(prefab.GetComponent<MeshFilter>(), Is.Null);
        DestroyPrefab(prefab);
    }

    [Test]
    public void CreatePrefab_DoesNot_SerializeMeshColliderMeshProperty()
    {
        var prefabPath = AssetDatabase.GenerateUniqueAssetPath("Assets/PrefabTest.prefab");
        var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

        MeshCollider collider;
        if (!mesh.gameObject.TryGetComponent(out collider))
            collider = mesh.gameObject.AddComponent<MeshCollider>();
        mesh.Refresh(RefreshMask.Collisions);

        Assume.That(mesh.GetComponent<MeshCollider>(), Is.Not.Null);
        Assume.That(mesh.GetComponent<MeshCollider>(), Is.EqualTo(collider));
        Assume.That(collider.sharedMesh, Is.EqualTo(mesh.mesh));

        var prefab = PrefabUtility.SaveAsPrefabAsset(mesh.gameObject, prefabPath);

        Assume.That(prefab, Is.Not.Null);
        Assume.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(prefabPath));
        Assume.That(prefab.GetComponent<MeshCollider>(), Is.Not.Null);

        Assert.That(prefab.GetComponent<MeshCollider>().sharedMesh, Is.Null);

        if(prefabPath != null)
            AssetDatabase.DeleteAsset(prefabPath);
    }

    [Test]
    public void Prefab_HasNoOverrides_WhenInstantiated()
    {
        var prefab = CreatePrefab();
        var instanced = Object.Instantiate(prefab);
        Assert.That(PrefabUtility.HasPrefabInstanceAnyOverrides(instanced, true), Is.False);
        DestroyPrefab(prefab);
    }

    [UnityTest]
    public static IEnumerator ModifyPrefabInstance_DoesNotSetDirty_MeshFilter()
    {
        var prefab = CreatePrefab();
        var instanced = Object.Instantiate(prefab);
        var mesh = instanced.GetComponent<ProBuilderMesh>();

        mesh.Extrude(new [] { mesh.faces.First() }, ExtrudeMethod.FaceNormal, 1f);
        mesh.ToMesh();
        mesh.Refresh();
        EditorUtility.SetDirty(mesh);
        yield return null;

        Assume.That(PrefabUtility.HasPrefabInstanceAnyOverrides(instanced, true), Is.True);

        DestroyPrefab(prefab);
    }

    [Test]
    public void ModifyPrefabInstance_DoesNotSetDirty_MeshCollider_SharedMesh()
    {
        var prefab = CreatePrefab();
        var instanced = Object.Instantiate(prefab);
        Assert.That(PrefabUtility.HasPrefabInstanceAnyOverrides(instanced, true), Is.False);
        DestroyPrefab(prefab);
    }
}
