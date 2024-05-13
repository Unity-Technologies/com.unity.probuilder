using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PrefabTests
{
    List<string> m_CreatedAssets = new List<string>();

    private Scene m_Scene;

    [SetUp]
    public void Setup()
    {
        var window = EditorWindow.GetWindow<SceneView>();
        window.Show(false);
        window.Repaint();
        window.Focus();

        m_Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    GameObject CreatePrefab()
    {
        var prefabPath = AssetDatabase.GenerateUniqueAssetPath("Assets/PrefabTest.prefab");
        var mesh = ShapeFactory.Instantiate<Cube>();
        Assume.That(mesh.meshSyncState == MeshSyncState.InSync);
        var prefab = PrefabUtility.SaveAsPrefabAsset(mesh.gameObject, prefabPath);
        Assume.That(prefab, Is.Not.Null);
        Assume.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(prefabPath));
        Object.DestroyImmediate(mesh.gameObject);
        m_CreatedAssets.Add(prefabPath);
        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    static void DestroyPrefab(GameObject prefab)
    {
        var path = AssetDatabase.GetAssetPath(prefab);
        Assume.That(!string.IsNullOrEmpty(path));
        AssetDatabase.DeleteAsset(path);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var asset in m_CreatedAssets)
            AssetDatabase.DeleteAsset(asset);
        m_CreatedAssets.Clear();
    }

    [Test]
    public void CreateDoesNotSerializeMeshAsset()
    {
        var go = CreatePrefab();
        Assert.That(go.GetComponent<ProBuilderMesh>().meshSyncState == MeshSyncState.Null);
        DestroyPrefab(go);
    }

    [Test]
    public void InstantiateCreatesUniqueMesh()
    {
        var go = CreatePrefab();
        var instance1 = PrefabUtility.InstantiatePrefab(go) as GameObject;
        var instance2 = PrefabUtility.InstantiatePrefab(go) as GameObject;
        Assume.That(instance1, Is.Not.Null);
        Assume.That(instance2, Is.Not.Null);
        var mesh1 = instance1.GetComponent<ProBuilderMesh>().mesh;
        var mesh2 = instance2.GetComponent<ProBuilderMesh>().mesh;
        Assert.That(mesh1.GetInstanceID(), Is.Not.EqualTo(mesh2.GetInstanceID()));
    }

    [Test]
    public void ModifyPrefabInstanceDoesNotAffectOtherInstances()
    {
        var go = CreatePrefab();
        var instance1 = PrefabUtility.InstantiatePrefab(go) as GameObject;
        var instance2 = PrefabUtility.InstantiatePrefab(go) as GameObject;
        Assume.That(instance1, Is.Not.Null);
        Assume.That(instance2, Is.Not.Null);
        var mesh1 = instance1.GetComponent<ProBuilderMesh>();
        var mesh2 = instance2.GetComponent<ProBuilderMesh>();
        mesh1.Extrude(new [] { mesh1.faces[0] }, ExtrudeMethod.FaceNormal, .25f);

        // rebuild is not necessary, but it prevents other failures from bleeding into the results of this test
        mesh1.Rebuild();
        mesh2.Rebuild();

        Assert.That(mesh1.mesh.vertexCount, Is.GreaterThan(mesh2.mesh.vertexCount));
    }

    [Test]
    public void ApplyPrefabInstanceModificationIsAppliedToUnmodifiedInstances()
    {
        var go = CreatePrefab();
        var instance1 = PrefabUtility.InstantiatePrefab(go) as GameObject;
        var instance2 = PrefabUtility.InstantiatePrefab(go) as GameObject;
        Assume.That(instance1, Is.Not.Null);
        Assume.That(instance2, Is.Not.Null);
        var mesh1 = instance1.GetComponent<ProBuilderMesh>();
        var mesh2 = instance2.GetComponent<ProBuilderMesh>();

        mesh1.Extrude(new [] { mesh1.faces[0] }, ExtrudeMethod.FaceNormal, .25f);
        mesh1.Rebuild();

        PrefabUtility.ApplyPrefabInstance(instance1, InteractionMode.AutomatedAction);

        Assert.That(mesh1.mesh.vertexCount, Is.EqualTo(mesh2.mesh.vertexCount));

        Assert.That(mesh1.versionIndex, Is.Not.EqualTo(ProBuilderMesh.k_UnitializedVersionIndex));
        Assert.That(mesh2.versionIndex, Is.Not.EqualTo(ProBuilderMesh.k_UnitializedVersionIndex));
    }

    [Test, Ignore("Requires ENABLE_DRIVEN_PROPERTIES feature")]
    public void CreatePrefab_DoesNot_SerializeMeshFilter()
    {
        Assert.That(CreatePrefab().GetComponent<MeshFilter>(), Is.Null);
    }

    // this is just a smoke test to make sure that prefab behaviour hasn't changed. if this does not fail but
    // CreatePrefab_DoesNot_SerializeMeshFilter does, then it's a probuilder problem.
    [Test, Ignore("Requires ENABLE_DRIVEN_PROPERTIES feature")]
    public void CreatePrefab_FromUnityPrimitive_DoesNotInclude_ComponentsWith_HideFlagsDontSave()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mf = go.GetComponent<MeshFilter>();
        mf.hideFlags = HideFlags.DontSave;

        var prefabPath = AssetDatabase.GenerateUniqueAssetPath("Assets/PrefabTest.prefab");
        Assume.That(prefabPath, Is.Not.Empty);
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Assume.That(prefab, Is.Not.Null);
        Assume.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(prefabPath));
        Assert.That(prefab.GetComponent<MeshFilter>(), Is.Null);

        Object.DestroyImmediate(go);
        AssetDatabase.DeleteAsset(prefabPath);
    }

    [Test, Ignore("Requires ENABLE_DRIVEN_PROPERTIES feature")]
    public void CreatePrefab_DoesNot_SerializeMeshColliderMeshProperty()
    {
        var prefabPath = AssetDatabase.GenerateUniqueAssetPath("Assets/PrefabTest.prefab");
        var mesh = ShapeFactory.Instantiate<Cube>();

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

    [UnityTest, Ignore("Requires ENABLE_DRIVEN_PROPERTIES feature")]
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

    [UnityTest, Ignore("Requires ENABLE_DRIVEN_PROPERTIES feature")]
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

    [UnityTest, Ignore("Requires ENABLE_DRIVEN_PROPERTIES feature")]
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
                sb.AppendLine($"  - Object Reference: {modification.objectReference}\n  - Property Path: {modification.propertyPath}\n  - Modified Value: {modification.value}");
        }

        Debug.Log(sb.ToString());
    }
}
