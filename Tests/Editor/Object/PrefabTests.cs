using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
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

        MeshCollider collider = mesh.DemandComponent<MeshCollider>();
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

    [UnityTest]
    public IEnumerator Prefab_HasNoOverrides_WhenInstantiated()
    {
        var prefab = CreatePrefab();
        var instanced = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
        yield return null;
        PrintPrefabModifications(new [] { instanced });
        Assert.That(PrefabUtility.GetObjectOverrides(instanced), Is.Empty);
        Assert.That(PrefabUtility.GetPropertyModifications(instanced), Has.None.Matches<PropertyModification>(
            x => x.objectReference is ProBuilderMesh
                || x.objectReference is MeshCollider
                || x.objectReference is MeshFilter
        ));
        DestroyPrefab(prefab);
    }

    [UnityTest]
    public IEnumerator ModifyPrefabInstance_DoesNotSetDirty_MeshFilter()
    {
        var prefab = CreatePrefab();
        var instanced = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
        var mesh = instanced.GetComponent<ProBuilderMesh>();

        Undo.RecordObject(mesh, "Extrude");
        mesh.Extrude(new [] { mesh.faces.First() }, ExtrudeMethod.FaceNormal, 1f);
        mesh.ToMesh();
        mesh.Refresh();

        // Let serialization run so that things are marked dirty
        yield return null;

        Assume.That(PrefabUtility.HasPrefabInstanceAnyOverrides(instanced, false), Is.True);
        var overrides = PrefabUtility.GetObjectOverrides(instanced);
        Assume.That(overrides, Is.Not.Null);
        Assume.That(overrides, Has.Some.Matches<ObjectOverride>(x => x.instanceObject is ProBuilderMesh));

        Assert.That(overrides, Has.None.Matches<ObjectOverride>(x => x.instanceObject is MeshFilter));

        DestroyPrefab(prefab);
    }

    [UnityTest]
    public IEnumerator ModifyPrefabInstance_DoesNotSetDirty_MeshCollider_SharedMesh()
    {
        var prefab = CreatePrefab();
        var instanced = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
        var mesh = instanced.GetComponent<ProBuilderMesh>();

        Undo.RecordObject(mesh, "Extrude");
        mesh.Extrude(new [] { mesh.faces.First() }, ExtrudeMethod.FaceNormal, 1f);
        mesh.ToMesh();
        mesh.Refresh();

        yield return null;

        var overrides = PrefabUtility.GetObjectOverrides(instanced);
        Assume.That(overrides, Is.Not.Null);
        Assume.That(overrides, Has.Some.Matches<ObjectOverride>(x => x.instanceObject is ProBuilderMesh));
        Assert.That(overrides, Has.None.Matches<ObjectOverride>(x => x.instanceObject is MeshCollider));

        var modifications = PrefabUtility.GetPropertyModifications(instanced);
        Assert.That(modifications, Has.None.Matches<PropertyModification>(x => x.objectReference is MeshCollider && x.propertyPath == "m_Mesh"));

        DestroyPrefab(prefab);
    }

    static void PrintPrefabModifications(IEnumerable<GameObject> selection)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var mesh in selection)
        {
            sb.AppendLine("Object Overrides");
            foreach (var ride in PrefabUtility.GetObjectOverrides(mesh))
                sb.AppendLine(ride.instanceObject.ToString());
            sb.AppendLine("\nProperty Modifications");
            foreach (var modification in PrefabUtility.GetPropertyModifications(mesh))
                sb.AppendLine(modification.objectReference + "->" + modification.propertyPath + " = " + modification.value);
        }

        Debug.Log(sb.ToString());
    }
}
