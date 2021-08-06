using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

class SubmeshUtilityTests
{
    public class TestCase
    {
        readonly string m_Name;
        public readonly IList<Face> faces;
        public readonly int submeshes;
        public int[] emptySubmeshes = new int[0];

        public override string ToString() { return m_Name; }

        public TestCase(string name, int submeshes, params Face[] faces)
        {
            m_Name = name;
            this.faces = faces;
            this.submeshes = submeshes;
        }
    }

    static IEnumerable<TestCase> s_TestCases
    {
        get
        {
            yield return new TestCase("0 Submesh", 0);

            yield return new TestCase("1 Submesh", 1,
                new Face {submeshIndex = 0});

            yield return new TestCase("Multiple Submeshes", 3,
                new Face {submeshIndex = 0},
                new Face {submeshIndex = 1},
                new Face {submeshIndex = 2});

            yield return new TestCase("Multiple Submeshes with 1 Empty", 3,
                    new Face {submeshIndex = 0},
                    new Face {submeshIndex = 2})
                {emptySubmeshes = new[] {1}};

            yield return new TestCase("Multiple Submeshes with 3 Empty", 8,
                    new Face {submeshIndex = 0},
                    new Face {submeshIndex = 2},
                    new Face {submeshIndex = 2},
                    new Face {submeshIndex = 4},
                    new Face {submeshIndex = 7},
                    new Face {submeshIndex = 7},
                    new Face {submeshIndex = 7})
                {emptySubmeshes = new[] {1, 3, 5, 6}};
        }
    }

    Mesh m_TestMesh;

    [TearDown]
    public void TearDown()
    {
        if (m_TestMesh != null)
            Object.DestroyImmediate(m_TestMesh);
    }

    // Create a mesh with valid and empty submeshes depending on setup.
    // Actual submesh data isn't important outside of empty or not.
    void PopulateTestMesh(TestCase testCase)
    {
        m_TestMesh = new Mesh();
        m_TestMesh.vertices = new[] { Vector3.zero, Vector3.up, Vector3.right };

        List<int> indices = new List<int>();
        indices.Add(0);
        indices.Add(1);
        indices.Add(2);

        m_TestMesh.subMeshCount = testCase.submeshes;
        for (int i = 0; i < testCase.submeshes; ++i)
        {
            if (Array.IndexOf(testCase.emptySubmeshes, i) < 0)
            {
                m_TestMesh.SetIndices(indices, MeshTopology.Triangles, i);
            }
        }
    }

    [Test]
    public void GetSubmeshCount_ReturnsExpectAmountOfSubmesh(
        [ValueSource(nameof(s_TestCases))] TestCase testCase)
    {
        Assert.That(Submesh.GetSubmeshCount(testCase.faces), Is.EqualTo(testCase.submeshes));
    }

    [Test]
    public void GetEmptySubmesh_ResultArrayHasCorrectSubmeshes(
        [ValueSource(nameof(s_TestCases))] TestCase testCase)
    {
        PopulateTestMesh(testCase);

        List<int> emptySubmeshes = new List<int>();
        Submesh.GetEmptySubmeshes(m_TestMesh, emptySubmeshes);

        Assert.That(emptySubmeshes.Count, Is.EqualTo(testCase.emptySubmeshes.Length));
        foreach (var emptySubmesh in testCase.emptySubmeshes)
            Assert.That(emptySubmeshes, Does.Contain(emptySubmesh));
    }

    [Test]
    public void GetEmptySubmeshes_RemovesEmptySubmeshes_FaceArrayDoesntHaveEmptySubmeshes(
        [ValueSource(nameof(s_TestCases))] TestCase testCase)
    {
        PopulateTestMesh(testCase);

        List<int> emptySubmeshes = new List<int>();
        Submesh.GetEmptySubmeshes(m_TestMesh, emptySubmeshes);

        Assume.That(emptySubmeshes.Count, Is.EqualTo(testCase.emptySubmeshes.Length));
        foreach (var emptySubmesh in testCase.emptySubmeshes)
            Assume.That(emptySubmeshes, Does.Contain(emptySubmesh));

        List<Face> faces = new List<Face>(testCase.faces);
        Submesh.RemoveSubmeshes(faces, emptySubmeshes);

        //Check that no empty submeshes remains
        for (int i = 0, count = Submesh.GetSubmeshCount(faces); i < count; ++i)
        {
            bool found = false;
            //Look for faces with that submesh index
            foreach (var face in faces)
            {
                if (face.submeshIndex == i)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                Assert.Fail($"Submesh {0} is empty.");
        }
    }
}
