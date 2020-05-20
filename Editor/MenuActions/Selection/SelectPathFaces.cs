using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using System.Linq;

namespace UnityEditor.ProBuilder.Actions
{
    public static class SelectPathFaces
    {
        private static int[] lastPredecessors;
        private static int lastStart;
        private static ProBuilderMesh lastMesh;
        private static List<WingedEdge> lastWings;
        private static Dictionary<Face, int> lastFacesIndex = new Dictionary<Face, int>();

        /// <summary>
        /// Calculates the indexes of all faces in the shortest path between start and end
        /// </summary>
        /// <param name="start">The index of the starting face</param>
        /// <param name="end">The index of the ending face</param>
        /// <param name="mesh">The mesh of the object</param>
        /// <returns>The indexes of all faces </returns>
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
            HashSet<int> visited = new HashSet<int>();
            HashSet<int> toVisit = new HashSet<int>();
            lastWings = lastMesh == mesh ? lastWings : WingedEdge.GetWingedEdges(mesh, true);
            int wingCount = lastWings.Count;

            if (mesh != lastMesh)
            {
                lastFacesIndex.Clear();

                for (int i = 0; i < mesh.facesInternal.Length; i++)
                {
                    lastFacesIndex.Add(mesh.facesInternal[i], i);
                }
            }

            float[] weights = new float[wingCount];
            int[] predecessors = new int[wingCount];

            for (int i = 0; i < wingCount; i++)
            {
                weights[i] = float.MaxValue;
            }

            int current = start;
            weights[current] = 0;
            visited.Add(current);

            // Construct the paths between the start face and every other faces
            while (visited.Count < wingCount)
            {
                var currentWing = lastWings[current];
                var otherWing = currentWing;
                // Update the weight array for each face next to the current one
                do
                {
                    var opposite = otherWing.opposite;
                    if (opposite == null)
                        continue;

                    var idx = lastFacesIndex[opposite.face];
                    var weight = GetWeight(current, idx, mesh);
                    // Change the predecessor and weight if the new path found if shorter
                    if (weights[current] + weight < weights[idx])
                    {
                        weights[idx] = weights[current] + weight;
                        predecessors[idx] = current;
                    }
                    // Add the face to the ones we can visit next, if not yet visited
                    if (!toVisit.Contains(idx) && !visited.Contains(idx))
                    {
                        toVisit.Add(idx);
                    }

                    otherWing = otherWing.next;

                } while (otherWing != currentWing);

                // Look for the next face to visit, choosing the one with less weight
                double min = double.MaxValue;
                foreach (var i in toVisit)
                {
                    if (weights[i] < min)
                    {
                        min = weights[i];
                        current = i;
                    }
                }
                visited.Add(current);
                toVisit.Remove(current);
            } 

            return predecessors;
        }

        private static float GetWeight(int face1, int face2, ProBuilderMesh mesh)
        {
            float baseCost = 10f;
            float normalMult = 2f;
            float distMult = 1f;

            // Calculates the difference between the normals of the faces
            var n1 = Math.Normal(mesh, mesh.facesInternal[face1]);
            var n2 = Math.Normal(mesh, mesh.facesInternal[face2]);
            float normalCost = (1f - Vector3.Dot(n1.normalized, n2.normalized)) * normalMult;

            // Calculates the distance between the center of the faces
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

            float distCost = (p2 - p1).magnitude * distMult;

            return baseCost + distCost + normalCost;
        }

        private static List<int> GetMinimalPath(int[] predecessors, int start, int end)
        {
            Stack<int> list = new Stack<int>();
            list.Push(end);
            int a = end;
            while (a != start)
            {
                a = predecessors[a];
                list.Push(a);
            }
            return list.ToList();
        }
    }
}
