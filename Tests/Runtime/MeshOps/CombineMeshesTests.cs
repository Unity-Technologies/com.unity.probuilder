using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.ProBuilder.MeshOperations;

class CombineMeshesTests
{
    ProBuilderMesh m_mesh1;
    ProBuilderMesh m_mesh2;
    ProBuilderMesh m_mesh3;
    Vector3 m_meshScale;

    [SetUp]
    public void SetUp()
    {
        m_mesh1 = ShapeGenerator.CreateShape(ShapeType.Cube);
        m_mesh2 = ShapeGenerator.CreateShape(ShapeType.Cone);
        m_mesh3 = ShapeGenerator.CreateShape(ShapeType.Cylinder);
        m_mesh1.gameObject.AddComponent<BoxCollider>();
        m_meshScale = new Vector3(2.0f, 2.0f, 2.0f);
        m_mesh1.gameObject.transform.localScale = m_meshScale;;
    }

    [TearDown]
    public void Cleanup()
    {
        UObject.DestroyImmediate(m_mesh1);
        UObject.DestroyImmediate(m_mesh2);
        UObject.DestroyImmediate(m_mesh3);
    }

    [Test]
    public void CombineMeshes_RetainObjectProperties()
    {
        var meshes = new List<ProBuilderMesh>();
        meshes.Add(m_mesh1);
        meshes.Add(m_mesh2);
        meshes.Add(m_mesh3);

        var newMeshes = CombineMeshes.Combine(meshes, m_mesh1);
        Assert.That(newMeshes.Count, Is.EqualTo(1));
        Assert.That(newMeshes[0], Is.EqualTo(m_mesh1));
        Assert.That(newMeshes[0].gameObject.GetComponent<BoxCollider>, !Is.EqualTo(null));
        Assert.That(newMeshes[0].transform.localScale, Is.EqualTo(m_meshScale));
    }
}
