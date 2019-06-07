using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.Tests.Framework;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOperations
{
    class AppendElementsTests
    {
        GameObject m_GO;
        PolyShape m_Poly;
        ProBuilderMesh m_pb;

        [SetUp]
        public void SetUp()
        {
            m_GO = new GameObject();
            m_Poly = m_GO.AddComponent<PolyShape>();
            m_pb = m_GO.AddComponent<ProBuilderMesh>();
        }

        [TearDown]
        public void Cleanup()
        {
            UObject.DestroyImmediate(m_GO);
        }

        [Test]
        public void CreateShapeFromPolygon_ExtrudeOrNot()
        {
            m_Poly.m_Points.Add(new Vector3(0,0,0));
            m_Poly.m_Points.Add(new Vector3(0,0,2));
            m_Poly.m_Points.Add(new Vector3(2,0,0));
            m_Poly.extrude = 0.0f;
            m_Poly.flipNormals = false;
            m_pb.CreateShapeFromPolygon(m_Poly.m_Points, m_Poly.extrude, m_Poly.flipNormals);

            Assert.That(m_pb.faceCount, Is.EqualTo(1));
            m_pb.Clear();
            Assert.That(m_pb.faceCount, Is.EqualTo(0));
            m_Poly.extrude = 1.0f;
            m_pb.CreateShapeFromPolygon(m_Poly.m_Points, m_Poly.extrude, m_Poly.flipNormals);
            Assert.That(m_pb.faceCount, Is.EqualTo(5));
        }

        [Test]
        public void CreateShapeFromPolygon_NormalFacing()
        {
            m_Poly.m_Points.Add(new Vector3(0, 0, 0));
            m_Poly.m_Points.Add(new Vector3(0, 0, 2));
            m_Poly.m_Points.Add(new Vector3(2, 0, 0));
            m_Poly.extrude = 0.0f;
            m_pb.CreateShapeFromPolygon(m_Poly.m_Points, m_Poly.extrude, m_Poly.flipNormals, Vector3.down);
            Vector3 nrm = Math.Normal(m_pb, m_pb.facesInternal[0]);
            Assert.That(nrm, Is.EqualTo(Vector3.up));

            m_pb.Clear();
            //Changing the winding order should lead to a norml in the other direction
            m_Poly.m_Points.Reverse();
            m_pb.CreateShapeFromPolygon(m_Poly.m_Points, m_Poly.extrude, m_Poly.flipNormals, Vector3.down);
            nrm = Math.Normal(m_pb, m_pb.facesInternal[0]);
            Assert.That(nrm, Is.EqualTo(Vector3.up));
        }
    }
}
