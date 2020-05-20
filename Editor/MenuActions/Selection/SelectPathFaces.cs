using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Linq;
using Unity.Profiling;
using System;

namespace UnityEditor.ProBuilder.Actions
{
    //public enum VisitedType
    //{
    //    Visited,
    //    Univisted,
    //    ToVisit
    //}

    public static class SelectPathFaces
    {
        private static int[] lastPredecessors;
        private static int lastStart;
        private static ProBuilderMesh lastMesh;


        public static List<int> GetPath(int start, int end, ProBuilderMesh mesh)
        {
            if (start == lastStart && mesh == lastMesh)
            {
                return GetMinimalPath(lastPredecessors, start, end);
            }
            else
            {
                var predecessors = Dijkstra(start, end, mesh);
                var path = GetMinimalPath(predecessors, start, end);
                lastPredecessors = predecessors;
                lastStart = start;
                lastMesh = mesh;
                return path;
            }
        }

        private static int[] Dijkstra(int start, int end, ProBuilderMesh mesh)
        {
            List<WingedEdge> wings;
            using (new ProfilerMarker("Select path Dijkstra WingedEdge").Auto())
            {
                wings = WingedEdge.GetWingedEdges(mesh, true);
            }
            int wingCount = wings.Count;
            HashSet<int> visited = new HashSet<int>();
            HashSet<int> unvisited = new HashSet<int>();

            float[] weights = new float[wingCount];
            int[] predecessors = new int[wingCount];

            for (int i = 0; i < wingCount; i++)
            {
                weights[i] = float.MaxValue;
                //unvisited.Add(i);
            }

            int current = start;
            weights[current] = 0;
            visited.Add(current);
            //unvisited.Remove(current);

            do
            {
                using (new ProfilerMarker("Select path Dijkstra update weights").Auto())
                {
                    var currentWing = wings[current];
                    var otherWing = currentWing;
                    do
                    {
                        var opposite = otherWing.opposite;
                        if (opposite == null)
                            continue;

                        var idx = Array.IndexOf(mesh.facesInternal, opposite.face);
                        var weight = GetWeight(current, idx, mesh);
                        if (weights[current] + weight < weights[idx])
                        {
                            weights[idx] = weights[current] + weight;
                            predecessors[idx] = current;
                        }

                        if (!unvisited.Contains(idx) && !visited.Contains(idx))
                        {
                            unvisited.Add(idx);
                        }

                        otherWing = otherWing.next;
                    } while (otherWing != currentWing);
                }

                double min = double.MaxValue;
                using (new ProfilerMarker("Select path Dijkstra select next face").Auto())
                {
                    foreach (var i in unvisited)
                    {
                        // Add unvisited for only neighbords
                        if (weights[i] < min)
                        {
                            min = weights[i];
                            current = i;
                        }
                    }
                }

                visited.Add(current);
                unvisited.Remove(current);

            } while (visited.Count < wingCount/* && !visited.Contains(end)*/);

            return predecessors;

        }

        private static float GetWeight(int face1, int face2, ProBuilderMesh mesh)
        {
            using (new ProfilerMarker("Select path GetWeight").Auto())
            {
                float baseCost = 10f;

                var n1 = UnityEngine.ProBuilder.Math.Normal(mesh, mesh.facesInternal[face1]);
                var n2 = UnityEngine.ProBuilder.Math.Normal(mesh, mesh.facesInternal[face2]);

                float normalCost = (1f - Vector3.Dot(n1.normalized, n2.normalized)) * 2f;

                Vector3 p1 = Vector3.zero;
                Vector3 p2 = Vector3.zero;
                foreach (var point in mesh.facesInternal[face1].indexesInternal)
                {
                    p1 += mesh.positionsInternal[point] / mesh.facesInternal[face1].indexesInternal.Count();
                }
                foreach (var point in mesh.facesInternal[face2].indexesInternal)
                {
                    p2 += mesh.positionsInternal[point] / mesh.facesInternal[face2].indexesInternal.Count();
                }

                float distCost = (p2 - p1).magnitude;

                return baseCost + distCost + normalCost;
            }
        }

        private static List<int> GetMinimalPath(int[] predecessors, int start, int end)
        {
            using (new ProfilerMarker("Select path MinimalPath").Auto())
            {
                List<int> list = new List<int>();
                list.Add(end);
                int a = end;
                while (a != start)
                {
                    a = predecessors[a];
                    list.Add(a);
                }
                for (int i = 0, j = list.Count - 1; i < j; i++)
                {
                    var item = list[j];
                    list.Remove(item);
                    list.Insert(i, item);
                }
                return list;
            }
        }

        private struct Node
        {
            public WingedEdge WingedEdge { get; private set; }
            public float Weight { get; private set; }

            public Node(WingedEdge wingedEdge, float weight)
            {
                WingedEdge = wingedEdge;
                Weight = weight;
            }
        }
    }
}
