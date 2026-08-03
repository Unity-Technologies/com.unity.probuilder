using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UObject = UnityEngine.Object;

static class ProBuilderMeshEventsTests
{
    static void CallToMesh(ProBuilderMesh mesh) => mesh.ToMesh();
    static void CallRefresh(ProBuilderMesh mesh) => mesh.Refresh();
    static void CallClear(ProBuilderMesh mesh) => mesh.Clear();

    static IEnumerable<TestCaseData> MeshMutatingOperations()
    {
        yield return new TestCaseData((Action<ProBuilderMesh>)CallToMesh).SetName("ToMesh");
        yield return new TestCaseData((Action<ProBuilderMesh>)CallRefresh).SetName("Refresh");
        yield return new TestCaseData((Action<ProBuilderMesh>)CallClear).SetName("Clear");
    }

    [TestCaseSource(nameof(MeshMutatingOperations))]
    public static void VersionChanged_IsInvokedWithMesh_WhenMeshVersionIsIncremented(Action<ProBuilderMesh> mutate)
    {
        var pb = ShapeFactory.Instantiate<Cube>();

        try
        {
            ProBuilderMesh received = null;
            int invokeCount = 0;
            void Handler(ProBuilderMesh m)
            {
                received = m;
                invokeCount++;
            }

            ProBuilderMesh.versionChanged += Handler;
            try
            {
                mutate(pb);
            }
            finally
            {
                ProBuilderMesh.versionChanged -= Handler;
            }

            Assert.That(invokeCount, Is.GreaterThan(0));
            Assert.That(received, Is.SameAs(pb));
        }
        finally
        {
            UObject.DestroyImmediate(pb.gameObject);
        }
    }

    [Test]
    public static void VersionChanged_IsNotInvoked_AfterUnsubscribing()
    {
        var pb = ShapeFactory.Instantiate<Cube>();

        try
        {
            int invokeCount = 0;
            void Handler(ProBuilderMesh m) => invokeCount++;

            ProBuilderMesh.versionChanged += Handler;
            ProBuilderMesh.versionChanged -= Handler;

            pb.ToMesh();
            pb.Refresh();

            Assert.That(invokeCount, Is.EqualTo(0));
        }
        finally
        {
            UObject.DestroyImmediate(pb.gameObject);
        }
    }

    [Test]
    public static void VersionChanged_PassesTheModifiedMesh_WhenMultipleMeshesExist()
    {
        var pbA = ShapeFactory.Instantiate<Cube>();
        var pbB = ShapeFactory.Instantiate<Cube>();

        try
        {
            ProBuilderMesh received = null;
            void Handler(ProBuilderMesh m) => received = m;

            ProBuilderMesh.versionChanged += Handler;
            try
            {
                pbB.ToMesh();
            }
            finally
            {
                ProBuilderMesh.versionChanged -= Handler;
            }

            Assert.That(received, Is.SameAs(pbB));
            Assert.That(received, Is.Not.SameAs(pbA));
        }
        finally
        {
            UObject.DestroyImmediate(pbA.gameObject);
            UObject.DestroyImmediate(pbB.gameObject);
        }
    }
}
