#if PERFORMANCE_TESTS_ENABLED

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.ProBuilder;

public class PathToolTests
{
    [TestCase("pb_path_test_small", 0, 2, Description = "Find a path between 2 faces on a small mesh, with close faces")]
    [TestCase("pb_path_test_small", 0, 77, Description = "Find a path between 2 faces on a small mesh, with close faces")]

    [TestCase("pb_path_test_medium", 1, 2, Description = "Find a path between 2 faces on a medium mesh, with close faces")]
    [TestCase("pb_path_test_medium", 1, 905, Description = "Find a path between 2 faces on a medium mesh, with far faces")]

    [TestCase("pb_path_test_large", 1, 2, Description = "Find a path between 2 faces on a large mesh, with close faces")]
    [TestCase("pb_path_test_large", 1, 18570, Description = "Find a path between 2 faces on a large mesh, with far faces")]
    [Test, Performance]
    public void PathToolTest(string path, int start, int end)
    {
        var go = Resources.Load<GameObject>(path);
        Assume.That(go != null);
        var mesh = go.GetComponent<ProBuilderMesh>();
        Assume.That(mesh != null);

        Measure.Method(() => SelectPathFaces.GetPath(mesh, start, end)).Run();
    }

}
#endif
