using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Utilities for working with smoothing groups and hard / soft edges.
	/// </summary>
	public static class Smoothing
	{
		/// <summary>
		/// Faces with smoothingGroup = 0 are hard edges. Historically negative values were sometimes also written as hard edges.
		/// </summary>
		public const int smoothingGroupNone = 0;

		/// <summary>
		/// Smoothing groups 1-24 are smooth.
		/// </summary>
		public const int smoothRangeMin = 1;

		/// <summary>
		/// Smoothing groups 1-24 are smooth.
		/// </summary>
		public const int smoothRangeMax = 24;

		/// <summary>
		/// Smoothing groups 25-42 are hard. Note that this is obsolete, and generally hard faces should be marked SMOOTHING_GROUP_NONE.
		/// </summary>
		public const int hardRangeMin = 25;

		/// <summary>
		/// Smoothing groups 25-42 are hard. Note that this is soon to be obsolete, and generally hard faces should be marked SMOOTHING_GROUP_NONE.
		/// </summary>
		public const int hardRangeMax = 42;

		/// <summary>
		/// Get the first available unused smoothing group.
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public static int GetUnusedSmoothingGroup(ProBuilderMesh mesh)
		{
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            return GetNextUnusedSmoothingGroup(smoothRangeMin, new HashSet<int>(mesh.facesInternal.Select(x => x.smoothingGroup)));
		}

		/// <summary>
		/// Get the first available smooth group after start.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="used"></param>
		/// <returns></returns>
		static int GetNextUnusedSmoothingGroup(int start, HashSet<int> used)
		{
			while(used.Contains(start) && start < int.MaxValue - 1)
			{
				start++;

				if(start > smoothRangeMax && start < hardRangeMax)
					start = hardRangeMax + 1;
			}

			return start;
		}

		/// <summary>
		/// Is the smooth group index considered smooth?
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool IsSmooth(int index)
		{
			return (index > smoothingGroupNone && (index < hardRangeMin || index > hardRangeMax));
		}

		/// <summary>
		/// Group together adjacent faces with normal differences less than angleThreshold (in degrees).
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="faces"></param>
		/// <param name="angleThreshold"></param>
		/// <param name="normals"></param>
		public static void ApplySmoothingGroups(ProBuilderMesh mesh, IEnumerable<Face> faces, float angleThreshold, Vector3[] normals = null)
		{
            if (mesh == null || faces == null)
                throw new System.ArgumentNullException("mesh");

            // Reset the selected faces to no smoothing group
            bool anySmoothed = false;

			foreach(Face face in faces)
			{
				if(face.smoothingGroup != smoothingGroupNone)
					anySmoothed = true;

				face.smoothingGroup = Smoothing.smoothingGroupNone;
			}

			// if a set of normals was not supplied, get a new set of normals
			// with no prior smoothing groups applied.
			if(normals == null)
			{
				if(anySmoothed)
					mesh.mesh.normals = null;
				normals = mesh.GetNormals();
			}

			float threshold = Mathf.Abs(Mathf.Cos(Mathf.Clamp(angleThreshold, 0f, 89.999f) * Mathf.Deg2Rad));
			HashSet<int> used = new HashSet<int>(mesh.facesInternal.Select(x => x.smoothingGroup));
			int group = GetNextUnusedSmoothingGroup(1, used);
			HashSet<Face> processed = new HashSet<Face>();
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh, faces, true);

			foreach(WingedEdge wing in wings)
			{
				// Already part of a group
				if(!processed.Add(wing.face))
					continue;

				wing.face.smoothingGroup = group;

				if( FindSoftEdgesRecursive(normals, wing, threshold, processed) )
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

			foreach(WingedEdge border in wing)
			{
				if(border.opposite == null)
					continue;

				if( border.opposite.face.smoothingGroup == Smoothing.smoothingGroupNone
				    && IsSoftEdge(normals, border.edge, border.opposite.edge, angleThreshold) )
				{
					if(processed.Add(border.opposite.face))
					{
						foundSmoothEdge = true;
						border.opposite.face.smoothingGroup = wing.face.smoothingGroup;
						FindSoftEdgesRecursive(normals, border.opposite, angleThreshold, processed);
					}
				}
			}

			return foundSmoothEdge;
		}

		static bool IsSoftEdge(Vector3[] normals, EdgeLookup left, EdgeLookup right, float threshold)
		{
			Vector3 lx = normals[left.local.x];
			Vector3 ly = normals[left.local.y];
			Vector3 rx = normals[right.common.x == left.common.x ? right.local.x : right.local.y];
			Vector3 ry = normals[right.common.y == left.common.y ? right.local.y : right.local.x];
			lx.Normalize();
			ly.Normalize();
			rx.Normalize();
			ry.Normalize();
			return Mathf.Abs(Vector3.Dot(lx, rx)) > threshold && Mathf.Abs(Vector3.Dot(ly, ry)) > threshold;
		}
	}
}
