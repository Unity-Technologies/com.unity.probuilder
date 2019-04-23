using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Utilities for working with smoothing groups. Smoothing groups are how ProBuilder defines hard and soft edges.
    /// ProBuilder calculates vertex normals by first calculating the normal for every face, which in turn is applied to each
    /// vertex that makes up the face. Afterwards, each vertex normal is averaged with coincident vertices belonging to the
    /// same smoothing group.
    /// </summary>
    public static class Smoothing
    {
        /// <value>
        /// Faces with smoothingGroup = 0 are hard edges. Historically negative values were sometimes also written as hard edges.
        /// </value>
        internal const int smoothingGroupNone = 0;

        /// <value>
        /// Smoothing groups 1-24 are smooth.
        /// </value>
        internal const int smoothRangeMin = 1;

        /// <value>
        /// Smoothing groups 1-24 are smooth.
        /// </value>
        internal const int smoothRangeMax = 24;

        /// <value>
        /// Smoothing groups 25-42 are hard. Note that this is obsolete, and generally hard faces should be marked smoothingGroupNone.
        /// </value>
        internal const int hardRangeMin = 25;

        /// <value>
        /// Smoothing groups 25-42 are hard. Note that this is soon to be obsolete, and generally hard faces should be marked smoothingGroupNone.
        /// </value>
        internal const int hardRangeMax = 42;

        /// <summary>
        /// Get the first available unused smoothing group.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <returns>An unused smoothing group.</returns>
        public static int GetUnusedSmoothingGroup(ProBuilderMesh mesh)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            return GetNextUnusedSmoothingGroup(smoothRangeMin, new HashSet<int>(mesh.facesInternal.Select(x => x.smoothingGroup)));
        }

        /// <summary>
        /// Get the first available smooth group after a specified index.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="used"></param>
        /// <returns></returns>
        static int GetNextUnusedSmoothingGroup(int start, HashSet<int> used)
        {
            while (used.Contains(start) && start < int.MaxValue - 1)
            {
                start++;

                if (start > smoothRangeMax && start < hardRangeMax)
                    start = hardRangeMax + 1;
            }

            return start;
        }

        /// <summary>
        /// Is the smooth group value considered smooth?
        /// </summary>
        /// <param name="index">The smoothing group to test.</param>
        /// <returns>True if the smoothing group value is smoothed, false if not.</returns>
        public static bool IsSmooth(int index)
        {
            return (index > smoothingGroupNone && (index < hardRangeMin || index > hardRangeMax));
        }

        /// <summary>
        /// Generate smoothing groups for a set of faces by comparing adjacent faces with normal differences less than angleThreshold (in degrees).
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="faces">Faces to be considered for smoothing.</param>
        /// <param name="angleThreshold">The maximum angle difference in degrees between adjacent face normals for the shared edge to be considered smooth.</param>
        public static void ApplySmoothingGroups(ProBuilderMesh mesh, IEnumerable<Face> faces, float angleThreshold)
        {
            ApplySmoothingGroups(mesh, faces, angleThreshold, null);
        }

        internal static void ApplySmoothingGroups(ProBuilderMesh mesh, IEnumerable<Face> faces, float angleThreshold, Vector3[] normals)
        {
            if (mesh == null || faces == null)
                throw new System.ArgumentNullException("mesh");

            // Reset the selected faces to no smoothing group
            bool anySmoothed = false;

            foreach (Face face in faces)
            {
                if (face.smoothingGroup != smoothingGroupNone)
                    anySmoothed = true;

                face.smoothingGroup = Smoothing.smoothingGroupNone;
            }

            // if a set of normals was not supplied, get a new set of normals
            // with no prior smoothing groups applied.
            if (normals == null)
            {
                if (anySmoothed)
                    mesh.mesh.normals = null;
                normals = mesh.GetNormals();
            }

            float threshold = Mathf.Abs(Mathf.Cos(Mathf.Clamp(angleThreshold, 0f, 89.999f) * Mathf.Deg2Rad));
            HashSet<int> used = new HashSet<int>(mesh.facesInternal.Select(x => x.smoothingGroup));
            int group = GetNextUnusedSmoothingGroup(1, used);
            HashSet<Face> processed = new HashSet<Face>();
            List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh, faces, true);

            foreach (WingedEdge wing in wings)
            {
                // Already part of a group
                if (!processed.Add(wing.face))
                    continue;

                wing.face.smoothingGroup = group;

                if (FindSoftEdgesRecursive(normals, wing, threshold, processed))
                {
                    used.Add(group);
                    group = GetNextUnusedSmoothingGroup(group, used);
                }
                else
                {
                    wing.face.smoothingGroup = Smoothing.smoothingGroupNone;
                }
            }
        }

        // Walk the perimiter of a wing looking for compatibly smooth connections. Returns true if any match was found, false if not.
        static bool FindSoftEdgesRecursive(Vector3[] normals, WingedEdge wing, float angleThreshold, HashSet<Face> processed)
        {
            bool foundSmoothEdge = false;

            using (var it = new WingedEdgeEnumerator(wing))
            {
                while (it.MoveNext())
                {
                    var border = it.Current;

                    if (border.opposite == null)
                        continue;

                    if (border.opposite.face.smoothingGroup == Smoothing.smoothingGroupNone
                        && IsSoftEdge(normals, border.edge, border.opposite.edge, angleThreshold))
                    {
                        if (processed.Add(border.opposite.face))
                        {
                            foundSmoothEdge = true;
                            border.opposite.face.smoothingGroup = wing.face.smoothingGroup;
                            FindSoftEdgesRecursive(normals, border.opposite, angleThreshold, processed);
                        }
                    }
                }
            }

            return foundSmoothEdge;
        }

        static bool IsSoftEdge(Vector3[] normals, EdgeLookup left, EdgeLookup right, float threshold)
        {
            Vector3 lx = normals[left.local.a];
            Vector3 ly = normals[left.local.b];
            Vector3 rx = normals[right.common.a == left.common.a ? right.local.a : right.local.b];
            Vector3 ry = normals[right.common.b == left.common.b ? right.local.b : right.local.a];
            lx.Normalize();
            ly.Normalize();
            rx.Normalize();
            ry.Normalize();
            return Mathf.Abs(Vector3.Dot(lx, rx)) > threshold && Mathf.Abs(Vector3.Dot(ly, ry)) > threshold;
        }
    }
}
