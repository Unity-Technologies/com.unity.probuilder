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
        var m_GO = new GameObject();
        var m_Poly = m_GO.AddComponent<PolyShape>();
        m_GO.AddComponent<ProBuilderMesh>();
        m_Poly.CreateShapeFromPolygon();

        Assume.That(m_GO.GetComponent<ProBuilderMesh>() != null);
        Assume.That(m_GO.GetComponent<PolyShape>() != null);

        UnityEditor.ProBuilder.Actions.StripProBuilderScripts.DoStrip(m_GO.GetComponent<ProBuilderMesh>());

        Assert.That(m_GO.GetComponent<ProBuilderMesh>() == null);
        Assert.That(m_GO.GetComponent<PolyShape>() == null);

        UObject.DestroyImmediate(m_GO);
    }
}
