using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEngine.ProBuilder.MeshOperations;

public class StripProBuilderScriptsTest
{
    [Test]
    public void StripProBuilderScripts_RemovesMeshAndPolyShapeComponents()
    {
        var go = new GameObject();
        var poly = go.AddComponent<PolyShape>();
        go.AddComponent<ProBuilderMesh>();
        poly.CreateShapeFromPolygon();

        Assume.That(go.GetComponent<ProBuilderMesh>() != null);
        Assume.That(go.GetComponent<PolyShape>() != null);

        StripProBuilderScripts.DoStrip(go.GetComponent<ProBuilderMesh>());

        Assert.That(go.GetComponent<ProBuilderMesh>() == null);
        Assert.That(go.GetComponent<PolyShape>() == null);

        UObject.DestroyImmediate(go);
    }

    [Test]
    public void StripProBuilderScripts_RemovesMeshAndBezierComponents()
    {
        var go = new GameObject();
        go.AddComponent<BezierShape>();
        go.AddComponent<ProBuilderMesh>();

        Assume.That(go.GetComponent<ProBuilderMesh>() != null);
        Assume.That(go.GetComponent<BezierShape>() != null);

        StripProBuilderScripts.DoStrip(go.GetComponent<ProBuilderMesh>());

        Assert.That(go.GetComponent<ProBuilderMesh>() == null);
        Assert.That(go.GetComponent<BezierShape>() == null);

        UObject.DestroyImmediate(go);
    }
}
