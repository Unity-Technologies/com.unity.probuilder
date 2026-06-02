using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEngine.ProBuilder;
using UnityEngine.TestTools;

public class ProBuilderizeActionTest
{
    [UnityTest]
    public IEnumerator StaticBatchedGameObjectUnsupported()
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
        // confirm that the delaycall inside the action returns
        var completed = false;
        EditorApplication.delayCall += () =>
        {
            completed = true;
        };
        while (!completed)
        {
            EditorApplication.QueuePlayerLoopUpdate();
            yield return null;
        }
        LogAssert.Expect(LogType.Error, "Probuilderize is not supported for renderers that have `IsPartOfStaticBatch` set.\nCube will not probuilderize.");
        Assert.IsNull(Cubes[2].GetComponent<ProBuilderMesh>());
    }
}
