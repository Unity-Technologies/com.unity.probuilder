using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.TestTools;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

public class ProBuilderizeActionTest
{
    [UnityTest]
    public IEnumerator StaticBatchedGameObjectSupported()
    {
        var ContainerGO = new GameObject("container");
        var Cubes = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Cube.transform.position = new Vector3(i, i, i);
            Cube.transform.SetParent(ContainerGO.transform, true);
            Cubes[i] = Cube;
        }
        StaticBatchingUtility.Combine(ContainerGO);
        MeshSelection.SetSelection(new List<GameObject>( new[]{ Cubes[2] }));
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ProBuilderize proBuilderize = new ProBuilderize();
        proBuilderize.PerformAction();
        bool completed = false;
        // confirm that the delaycall inside the action returns
        EditorApplication.delayCall += () =>
        {
            completed = true;
        };
        while (!completed)
        {
            EditorApplication.QueuePlayerLoopUpdate();
            yield return null;
        }
        Assert.IsNotNull(Cubes[2].GetComponent<ProBuilderMesh>());
    }
    [UnityTest]
    public IEnumerator StaticBatchedMultiMaterialPartialProbuilderize()
    {
        var ContainerGO = new GameObject("container");
        var Cubes = new GameObject[9];
        for (int i = 0; i < 9; i++)
        {
            var Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Cube.transform.position = new Vector3(i, i, i);
            Cube.transform.SetParent(ContainerGO.transform, true);
            Cubes[i] = Cube;
        }
        var combine = new List<CombineInstance>();

        for (int i = 3; i < 6; i++)
        {
            var mf = Cubes[i].GetComponent<MeshFilter>();
            combine.Add(new CombineInstance()
            {
                mesh = mf.sharedMesh,
                subMeshIndex = 0,
                transform = Cubes[i].transform.localToWorldMatrix
            });
        }
        // Create new mesh and combine
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine.ToArray(), false);

        // Create new GameObject with combined mesh
        GameObject combined = new GameObject("Combined Mesh");
        MeshFilter filter = combined.AddComponent<MeshFilter>();
        MeshRenderer renderer = combined.AddComponent<MeshRenderer>();
        filter.sharedMesh = combinedMesh;
        var baseMaterial = Cubes[0].GetComponent<Renderer>().sharedMaterial;
        renderer.sharedMaterials = new[] { new Material(baseMaterial), new Material(baseMaterial), new Material(baseMaterial)};
        combined.transform.SetParent(ContainerGO.transform);

        StaticBatchingUtility.Combine(ContainerGO);

        MeshSelection.SetSelection(new List<GameObject>( new[]{ combined }));
        ActiveEditorTracker.sharedTracker.ForceRebuild();
        ProBuilderize.DoProBuilderize(new []{combined.GetComponent<MeshFilter>()}, new MeshImportSettings()
        {
            quads = true,
            smoothing = true,
            smoothingAngle = 1
        });
        bool completed = false;
        // confirm that the delaycall inside the action returns
        EditorApplication.delayCall += () =>
        {
            completed = true;
        };
        while (!completed)
        {
            EditorApplication.QueuePlayerLoopUpdate();
            yield return null;
        }

        Assert.IsNotNull(combined.GetComponent<ProBuilderMesh>());
        Assert.AreEqual(3, combined.GetComponent<MeshRenderer>().sharedMaterials.Length);
        Assert.AreEqual(18, combined.GetComponent<ProBuilderMesh>().faces.Count);
    }
}
