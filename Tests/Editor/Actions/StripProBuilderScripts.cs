using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

public class StripProBuilderScripts
{
    PolyShape m_Poly;
    GameObject m_GO;

    [Test]
    public void Strip_ProBuilder_Scripts()
    {
        m_GO = new GameObject();
        m_Poly = m_GO.AddComponent<PolyShape>();
        m_GO.AddComponent<ProBuilderMesh>();

        m_Poly.m_Points.Add(new Vector3(0, 0, 0));
        m_Poly.m_Points.Add(new Vector3(0, 0, 2));
        m_Poly.m_Points.Add(new Vector3(2, 0, 2));
        m_Poly.m_Points.Add(new Vector3(2, 0, 0));

        var result = m_Poly.CreateShapeFromPolygon();

        Assume.That(m_GO.GetComponent<ProBuilderMesh>() != null);
        Assume.That(m_GO.GetComponent<PolyShape>() != null);

        UnityEditor.ProBuilder.Actions.StripProBuilderScripts.DoStrip(m_GO.GetComponent<ProBuilderMesh>());

        Assert.That(m_GO.GetComponent<ProBuilderMesh>() == null);
        Assert.That(m_GO.GetComponent<PolyShape>() == null);

        UObject.DestroyImmediate(m_GO);
    }
}
