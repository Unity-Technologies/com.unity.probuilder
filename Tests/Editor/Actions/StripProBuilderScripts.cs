using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

public class StripProBuilderScripts
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

        UnityEditor.ProBuilder.Actions.StripProBuilderScripts.DoStrip(go.GetComponent<ProBuilderMesh>());

        Assert.That(go.GetComponent<ProBuilderMesh>() == null);
        Assert.That(go.GetComponent<PolyShape>() == null);

        UObject.DestroyImmediate(go);
    }
}
