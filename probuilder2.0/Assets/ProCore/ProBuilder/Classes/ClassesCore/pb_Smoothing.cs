using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	/// <summary>
	/// Utilities for working with smoothing groups and hard / soft edges.
	/// </summary>
	public static class pb_Smoothing
	{
		/// <summary>
		/// Faces with smoothingGroup = 0 are hard edges. Historically negative values were sometimes also written as hard edges.
		/// </summary>
		public const int SMOOTHING_GROUP_NONE = 0;

		/// <summary>
		/// Smoothing groups 1-24 are smooth.
		/// </summary>
		public const int SMOOTH_RANGE_MIN = 1;

		/// <summary>
		/// Smoothing groups 1-24 are smooth.
		/// </summary>
		public const int SMOOTH_RANGE_MAX = 24;

		/// <summary>
		/// Smoothing groups 25-42 are hard. Note that this is obsolete, and generally hard faces should be marked SMOOTHING_GROUP_NONE.
		/// </summary>
		public const int HARD_RANGE_MIN = 25;

		/// <summary>
		/// Smoothing groups 25-42 are hard. Note that this is obsolete, and generally hard faces should be marked SMOOTHING_GROUP_NONE.
		/// </summary>
		public const int HARD_RANGE_MAX = 42;

		/// <summary>
		/// Get the first available unused smoothing group.
		/// </summary>
		/// <param name="pb"></param>
		/// <returns></returns>
		public static int GetUnusedSmoothingGroup(pb_Object pb)
		{
			return GetNextUnusedSmoothingGroup(SMOOTH_RANGE_MIN, new HashSet<int>(pb.faces.Select(x => x.smoothingGroup)));
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

				if(start > SMOOTH_RANGE_MAX && start < HARD_RANGE_MAX)
					start = HARD_RANGE_MAX + 1;
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
			return (index > SMOOTHING_GROUP_NONE && (index < HARD_RANGE_MIN || index > HARD_RANGE_MAX));
		}

		/// <summary>
		/// Group together adjacent faces with normal differences less than angleThreshold (in degrees).
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <param name="angleThreshold"></param>
		/// <param name="normals"></param>
		public static void ApplySmoothingGroups(pb_Object pb, IEnumerable<pb_Face> faces, float angleThreshold, Vector3[] normals = null)
		{
			// Reset the selected faces to no smoothing group
			bool anySmoothed = false;

			foreach(pb_Face face in faces)
			{
				if(face.smoothingGroup != SMOOTHING_GROUP_NONE)
					anySmoothed = true;

				face.smoothingGroup = pb_Smoothing.SMOOTHING_GROUP_NONE;
			}

			// if a set of normals was not supplied, get a new set of normals
			// with no prior smoothing groups applied.
			if(normals == null)
			{
				if(anySmoothed)
					pb.msh.normals = null;
				normals = pb.GetNormals();
			}

			float threshold = Mathf.Abs(Mathf.Cos(Mathf.Clamp(angleThreshold, 0f, 89.999f) * Mathf.Deg2Rad));
			HashSet<int> used = new HashSet<int>(pb.faces.Select(x => x.smoothingGroup));
			int group = GetNextUnusedSmoothingGroup(1, used);
			HashSet<pb_Face> processed = new HashSet<pb_Face>();
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, faces, true);

			foreach(pb_WingedEdge wing in wings)
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
					wing.face.smoothingGroup = pb_Smoothing.SMOOTHING_GROUP_NONE;
				}
			}
		}

		// Walk the perimiter of a wing looking for compatibly smooth connections. Returns true if any match was found, false if not.
		static bool FindSoftEdgesRecursive(Vector3[] normals, pb_WingedEdge wing, float angleThreshold, HashSet<pb_Face> processed)
		{
			bool foundSmoothEdge = false;

			foreach(pb_WingedEdge border in wing)
			{
				if(border.opposite == null)
					continue;

				if( border.opposite.face.smoothingGroup == pb_Smoothing.SMOOTHING_GROUP_NONE
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

		static bool IsSoftEdge(Vector3[] normals, pb_EdgeLookup left, pb_EdgeLookup right, float threshold)
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
