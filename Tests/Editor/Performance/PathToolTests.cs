#if PERFORMANCE_TESTS_ENABLED

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.PerformanceTesting;
using UnityEditor.ProBuilder.Actions;
using UnityEngine;
using UnityEngine.ProBuilder;

public class PathToolTests
{
    [TestCase("pb_path_test_small", 0, 2, Description = "Find a path between 2 faces on a small mesh, with close faces")]
    [TestCase("pb_path_test_small", 0, 77, Description = "Find a path between 2 faces on a small mesh, with close faces")]

    [TestCase("pb_path_test_medium", 1, 2, Description = "Find a path between 2 faces on a medium mesh, with close faces")]
    [TestCase("pb_path_test_medium", 1, 161, Description = "Find a path between 2 faces on a medium mesh, with far faces")]

    [TestCase("pb_path_test_large", 1, 2, Description = "Find a path between 2 faces on a large mesh, with close faces")]
    [TestCase("pb_path_test_large", 1, 905, Description = "Find a path between 2 faces on a large mesh, with far faces")]
    [Test, Performance]
    public void PathToolTest(string path, int start, int end)
    {
        // Do assume
        var go = Resources.Load<GameObject>(path);
        // Do assume
        var mesh = go.GetComponent<ProBuilderMesh>();

        Measure.Method(() => SelectPathFaces.GetPath(start, end, mesh))
            .WarmupCount(1)
            .MeasurementCount(5)
            .ProfilerMarkers("Select path Dijkstra WingedEdge", "Select path Dijkstra update weights", "Select path Dijkstra select next face", "Select path GetWeight", "Select path MinimalPath")
            .Run();
    }

}
#endif
