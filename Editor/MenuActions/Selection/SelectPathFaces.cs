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
    public static class SelectPathFaces
    {
        // cache the nodes ?
        // cache the weight array

        public static List<int> GetPath(int start, int end, ProBuilderMesh mesh)
        {
           var path = GetMinimalPath(Dijkstra(start, end, mesh), start, end);
           return path;
        }

        private static int[] Dijkstra(int start, int end, ProBuilderMesh mesh)
        {
            Debug.Log("??");
            using (new ProfilerMarker("Select path Dijkstra").Auto())
            {
                List<int> visited = new List<int>();
                List<int> unvisited = new List<int>();
                var wings = WingedEdge.GetWingedEdges(mesh, true);
                int wingCount = wings.Count;

                float[] weights = new float[wingCount];
                int[] predecessors = new int[wingCount];
                List<Node> nodes = GetNodes(start, mesh);

                for (int i = 0; i < wingCount; i++)
                {
                    weights[i] = float.MaxValue;
                    unvisited.Add(i);
                }

                int current = start;
                weights[current] = 0;
                visited.Add(current);
                unvisited.Remove(current);

                do
                {
                    using (new ProfilerMarker("Select path Dijkstra first loop").Auto())
                    {
                        for (int other = 0; other < unvisited.Count; other++)
                        {
                            //var node = nodes.Where(x => x.Start == current && x.End == unvisited[other]).FirstOrDefault();
                            Node node = new Node();

                            foreach (var nodei in nodes)
                            {
                                //if (nodei.Start == current && nodei.End == unvisited[other])
                                //{
                                //    node = nodei;
                                //    break;
                                //}
                            }

                            if (node.Weight == 0)
                                continue;
                            if (weights[current] + node.Weight < weights[unvisited[other]])
                            {
                                weights[unvisited[other]] = weights[current] + node.Weight;
                                predecessors[unvisited[other]] = current;
                            }
                        }
                    }

                    double min = double.MaxValue;
                    using (new ProfilerMarker("Select path Dijkstra second loop").Auto())
                    {
                        for (int i = 0; i < unvisited.Count; i++)
                        {
                            if (weights[unvisited[i]] < min)
                            {
                                min = weights[unvisited[i]];
                                current = unvisited[i];
                            }
                        }
                    }

                    visited.Add(current);
                    unvisited.Remove(current);

                } while (visited.Count < wingCount && !visited.Contains(end));

                return predecessors;
            }
        }

        private static List<Node> GetNodes(int start, ProBuilderMesh mesh)
        {
            using (new ProfilerMarker("Select path GetNodes").Auto())
            {
                //var wings = WingedEdge.GetWingedEdges(mesh, true);
                //var wing = wings.First(x => x.face == mesh.facesInternal[start]);

                //List<Node> list = new List<Node>(mesh.facesInternal.Length * 4);
                //List<SimpleTuple<Face, Edge>> neighbors = new List<SimpleTuple<Face, Edge>>(8);
                //foreach (var face in mesh.facesInternal)
                //{
                //    foreach (var edge in face.edgesInternal)
                //    {
                //        neighbors = ElementSelection.GetNeighborFaces(mesh, edge);
                //        foreach (var neighbor in neighbors)
                //        {
                //            if (neighbor.item1 == face)
                //                continue;
                //            Node node = new Node(Array.IndexOf<Face>(mesh.facesInternal, face), GetWeight(face, neighbor.item1, mesh));
                //            list.Add(node);
                //        }
                //    }
                //}
                List<Node> list = new List<Node>(mesh.facesInternal.Length * 4); var wings = WingedEdge.GetWingedEdges(mesh, true);
                var wing = wings.First(x => x.face == mesh.facesInternal[start]);
                var visitedFaces = new List<Face>();
                return GetNodeRecursive(wing, ref visitedFaces);
            }
        }

        private static List<Node> GetNodeRecursive(WingedEdge current, ref List<Face> visitedFaces)
        {
            List<Node> list = new List<Node>();
            visitedFaces.Add(current.face);
            var edge = current;
            do
            {
                var node = new Node(edge, 1);
                list.Add(node);

                if (edge.opposite != null && !visitedFaces.Contains(edge.opposite.face))
                {
                    list.AddRange(GetNodeRecursive(edge.opposite, ref visitedFaces));
                }


                edge = edge.next;

            } while (edge != current);

            return list;
        }

        private static float GetWeight(Face face1, Face face2, ProBuilderMesh mesh)
        {
            double baseCost = 10.0;
            double normalCost = 2.0;
            double distCost = 3.0;

           // var n1 = UnityEngine.ProBuilder.Math.Normal(mesh, face1);
          //  var n2 = UnityEngine.ProBuilder.Math.Normal(mesh, face2);


            //foreach ()
            //{

            //}

            //var p1 = Math.Average(mesh.positionsInternal[face1.indexesInternal)

            return 1f;
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

            public Node(WingedEdge wingedEdge,float weight)
            {
                WingedEdge = wingedEdge;
                Weight = weight;
            }
        }
    }
}
