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
                int faceCount = mesh.faceCount;

                double[] weights = new double[faceCount];
                int[] predecessors = new int[faceCount];
                List<Node> nodes = GetNodes(start, mesh);

                for (int i = 0; i < faceCount; i++)
                {
                    weights[i] = double.MaxValue;
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
                                if (nodei.Start == current && nodei.End == unvisited[other])
                                    node = nodei;
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

                } while (visited.Count < faceCount && !visited.Contains(end));

                return predecessors;
            }
        }

        private static List<Node> GetNodes(int start, ProBuilderMesh mesh)
        {
            using (new ProfilerMarker("Select path GetNodes").Auto())
            {
                List<Node> list = new List<Node>(mesh.facesInternal.Length * 4);
                List<SimpleTuple<Face, Edge>> neighbors = new List<SimpleTuple<Face, Edge>>(8);
                foreach (var face in mesh.facesInternal)
                {
                    foreach (var edge in face.edgesInternal)
                    {
                        neighbors = ElementSelection.GetNeighborFaces(mesh, edge);
                        foreach (var neighbor in neighbors)
                        {
                            if (neighbor.item1 == face)
                                continue;
                            Node node = new Node(Array.IndexOf<Face>(mesh.facesInternal, face), Array.IndexOf<Face>(mesh.facesInternal, neighbor.item1), GetWeight(face, neighbor.item1, mesh));
                            list.Add(node);
                        }
                    }

                }

                return list;
            }
        }

        private static double GetWeight(Face face1, Face face2, ProBuilderMesh mesh)
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

            return 1.0;
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
            public int Start { get; private set; }
            public int End { get; private set; }
            public double Weight { get; private set; }

            public Node(int start, int end, double weight)
            {
                Start = start;
                End = end;
                Weight = weight;
            }
        }
    }
}
