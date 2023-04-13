using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    static class SelectPathFaces
    {
        static int[] s_cachedPredecessors;
        static int s_cachedStart;
        static ProBuilderMesh s_cachedMesh;
        static int s_cachedFacesCount;
        static List<WingedEdge> s_cachedWings;
        static Dictionary<Face, int> s_cachedFacesIndex = new Dictionary<Face, int>();

        /// <summary>
        /// Calculates the indexes of all faces in the shortest path between start and end
        /// </summary>
        /// <param name="start">The index of the starting face</param>
        /// <param name="end">The index of the ending face</param>
        /// <param name="mesh">The mesh of the object</param>
        /// <returns>The indexes of all faces </returns>
        public static List<int> GetPath(ProBuilderMesh mesh, int start, int end)
        {
            if (mesh == null)
                throw new System.ArgumentException("Parameter cannot be null", "mesh");
            if (start < 0 || start > mesh.faceCount - 1)
                throw new System.ArgumentException("Parameter is out of bounds", "start");
            if (end < 0 || end > mesh.faceCount - 1)
                throw new System.ArgumentException("Parameter is out of bounds", "end");

            List<int> path;

            if (start == s_cachedStart && mesh == s_cachedMesh &&  mesh.faceCount == s_cachedFacesCount)
                return GetMinimalPath(s_cachedPredecessors, start, end);

            var predecessors = Dijkstra(mesh, start);
            path = GetMinimalPath(predecessors, start, end);
            s_cachedPredecessors = predecessors;
            s_cachedStart = start;
            s_cachedMesh = mesh;

            return path;
        }

        /// <summary>
        /// Builds a list of predecessors from a given face index to all other faces
        /// Uses the Djikstra pathfinding algorithm
        /// </summary>
        /// <param name="mesh">The mesh of the object</param>
        /// <param name="start">The index of the starting face</param>
        /// <returns>A list of predecessors from a face index to all other faces</returns>
        static int[] Dijkstra(ProBuilderMesh mesh, int start)
        {
            HashSet<int> visited = new HashSet<int>();
            HashSet<int> toVisit = new HashSet<int>();
            if (s_cachedMesh != mesh || s_cachedFacesCount != mesh.faceCount)
            {
                s_cachedWings = WingedEdge.GetWingedEdges(mesh, true);
                s_cachedFacesIndex.Clear();
                s_cachedFacesCount = mesh.faceCount;

                for (int i = 0; i < mesh.facesInternal.Length; i++)
                {
                    s_cachedFacesIndex.Add(mesh.facesInternal[i], i);
                }
            }
            int wingCount = s_cachedWings.Count;

            float[] weights = new float[wingCount];
            int[] predecessors = new int[wingCount];

            for (int i = 0; i < wingCount; i++)
            {
                weights[i] = float.MaxValue;
                predecessors[i] = -1;
            }

            int current = start;
            weights[current] = 0;
            visited.Add(current);

            // Construct the paths between the start face and every other faces
            while (visited.Count < wingCount)
            {
                var currentWing = s_cachedWings[current];
                var otherWing = currentWing;
                // Update the weight array for each face next to the current one
                do
                {
                    var opposite = otherWing.opposite;
                    if (opposite == null)
                    {
                        otherWing = otherWing.next;
                        continue;
                    }

                    var idx = s_cachedFacesIndex[opposite.face];
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

                // This means there is an isolated face
                if (toVisit.Count == 0)
                {
                    return predecessors;
                }
                // Look for the next face to visit, choosing the one with less weight
                float min = float.MaxValue;
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

        static float GetWeight(int face1, int face2, ProBuilderMesh mesh)
        {
            const float baseCost = 10f;
            const float normalMult = 2f;
            const float distMult = 1f;

            // Calculates the difference between the normals of the faces
            var n1 = Math.Normal(mesh, mesh.facesInternal[face1]);
            var n2 = Math.Normal(mesh, mesh.facesInternal[face2]);
            float normalCost = (1f - Vector3.Dot(n1.normalized, n2.normalized)) * normalMult;

            // Calculates the distance between the center of the faces
            Vector3 p1 = Vector3.zero;
            Vector3 p2 = Vector3.zero;
            foreach (var point in mesh.facesInternal[face1].indexesInternal)
            {
                p1 += mesh.positionsInternal[point] / mesh.facesInternal[face1].indexesInternal.Length;
            }
            foreach (var point in mesh.facesInternal[face2].indexesInternal)
            {
                p2 += mesh.positionsInternal[point] / mesh.facesInternal[face2].indexesInternal.Length;
            }

            float distCost = (p2 - p1).magnitude * distMult;

            return baseCost + distCost + normalCost;
        }

        static List<int> GetMinimalPath(int[] predecessors, int start, int end)
        {
            if (predecessors[end] == -1)
            {
                return null;
            }
            Stack<int> list = new Stack<int>();
            int a = end;
            while (a != start)
            {
                list.Push(a);
                a = predecessors[a];
            }
            return list.ToList();
        }
    }
}
