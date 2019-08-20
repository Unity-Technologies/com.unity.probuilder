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
        public void CreateShapeFromPolygon_CreateMeshFailure_TriangulationMissingPoints()
        {
            // Test that creating the shape fails if the triangulation missed some of the points (invalid mesh)
            m_Poly.m_Points.Add(new Vector3(0.9f, 0, 3.4f));
            m_Poly.m_Points.Add(new Vector3(6.1f, 0, 5.7f));
            m_Poly.m_Points.Add(new Vector3(-1.6f, 0, 8.8f));
            m_Poly.m_Points.Add(new Vector3(7.4f, 0, -2.6f));

            var result = m_Poly.CreateShapeFromPolygon();
            Assert.That(result.status, Is.EqualTo(ActionResult.Status.Failure));
            Assert.That(result.notification, Is.EqualTo("Triangulation missing points"));

            // check that the mesh is cleared
            Assert.That(m_pb.facesInternal.Length, Is.EqualTo(0));
        }

        [Test]
        public void CreateShapeFromPolygon_CreateMeshSuccess()
        {
            // Test that creating a valid shape gives a result of Success
            m_Poly.m_Points.Add(new Vector3(0, 0, 0));
            m_Poly.m_Points.Add(new Vector3(0, 0, 2));
            m_Poly.m_Points.Add(new Vector3(2, 0, 2));
            m_Poly.m_Points.Add(new Vector3(2, 0, 0));

            var result = m_Poly.CreateShapeFromPolygon();
            Assert.That(result.status, Is.EqualTo(ActionResult.Status.Success));
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
            m_pb.CreateShapeFromPolygon(m_Poly.m_Points, m_Poly.extrude, m_Poly.flipNormals);
            Vector3 nrm = Math.Normal(m_pb, m_pb.facesInternal[0]);
            Assert.That(nrm, Is.EqualTo(Vector3.up));

            m_pb.Clear();
            //Changing the winding order should lead to a norml in the other direction
            m_Poly.m_Points.Reverse();
            m_pb.CreateShapeFromPolygon(m_Poly.m_Points, m_Poly.extrude, m_Poly.flipNormals);
            nrm = Math.Normal(m_pb, m_pb.facesInternal[0]);
            Assert.That(nrm, Is.EqualTo(Vector3.up));
        }

        [Test]
        public void CreateShapeFromPolygon_Holes_Success()
        {
            // Test that creating a valid shape with a hole gives a result of Success
            m_Poly.m_Points.Add(new Vector3(0, 0, 0));
            m_Poly.m_Points.Add(new Vector3(0, 0, 2));
            m_Poly.m_Points.Add(new Vector3(2, 0, 2));
            m_Poly.m_Points.Add(new Vector3(2, 0, 0));

            Vector3[][] holes = new Vector3[2][];
            holes[0] = new[]
            {
                new Vector3(0.1f, 0, 0.1f),
                new Vector3(0.1f, 0, 1.9f),
                new Vector3(0.9f, 0, 1.9f),
                new Vector3(0.9f, 0, 0.1f)
            };

            holes[1] = new[]
            {
                new Vector3(1.1f, 0, 0.1f),
                new Vector3(1.9f, 0, 0.1f),
                new Vector3(1.9f, 0, 1.9f),
            };

            var result = m_pb.CreateShapeFromPolygon(m_Poly.m_Points, m_Poly.extrude, m_Poly.flipNormals, holes);
            Assert.That(result.status, Is.EqualTo(ActionResult.Status.Success));            
        }

        [Test]
        public void CreateShapeFromPolygon_CreateMeshFailure_TooFewPointsInHole()
        {
            // Test that creating a shape with an invalid holes parameter does nothing and reports an error.
            m_Poly.m_Points.Add(new Vector3(0, 0, 0));
            m_Poly.m_Points.Add(new Vector3(0, 0, 2));
            m_Poly.m_Points.Add(new Vector3(2, 0, 2));
            m_Poly.m_Points.Add(new Vector3(2, 0, 0));

            Vector3[][] holes = new Vector3[2][];
            holes[0] = new[]
            {
                new Vector3(0.1f, 0, 0.1f),
                new Vector3(0.1f, 0, 1.9f),
                new Vector3(0.9f, 0, 1.9f),
                new Vector3(0.9f, 0, 0.1f)
            };

            holes[1] = new[]
            {
                new Vector3(1.1f, 0, 0.1f),
                new Vector3(1.9f, 0, 0.1f),
            };

            var result = m_pb.CreateShapeFromPolygon(m_Poly.m_Points, m_Poly.extrude, m_Poly.flipNormals, holes);
            Assert.That(result.status, Is.EqualTo(ActionResult.Status.NoChange));
            Assert.That(result.notification, Is.EqualTo("Too Few Points in hole 1"));
        }
    }
}
