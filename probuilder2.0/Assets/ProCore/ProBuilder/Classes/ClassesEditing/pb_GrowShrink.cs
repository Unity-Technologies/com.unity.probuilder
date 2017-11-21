using UnityEngine;
using System.Collections.Generic;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Methods for growing and shrinking element selections.
	/// </summary>
	public static class pb_GrowShrink
	{
		/// <summary>
		/// Grow `faces` to include any face touching the perimeter.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <param name="maxAngleDiff"></param>
		/// <returns></returns>
		public static HashSet<pb_Face> GrowSelection(pb_Object pb, IList<pb_Face> faces, float maxAngleDiff = -1f)
		{
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, true);
			HashSet<pb_Face> source = new HashSet<pb_Face>(faces);
			HashSet<pb_Face> neighboring = new HashSet<pb_Face>();

			Vector3 srcNormal = Vector3.zero;
			bool checkAngle = maxAngleDiff > 0f;

			for(int i = 0; i < wings.Count; i++)
			{
				if(!source.Contains(wings[i].face))
					continue;

				if(checkAngle)
					srcNormal = pb_Math.Normal(pb, wings[i].face);

				foreach(pb_WingedEdge w in wings[i])
				{
					if(w.opposite != null && !source.Contains(w.opposite.face))
					{
						if(checkAngle)
						{
							Vector3 oppNormal = pb_Math.Normal(pb, w.opposite.face);

							if(Vector3.Angle(srcNormal, oppNormal) < maxAngleDiff)
								neighboring.Add(w.opposite.face);
						}
						else
						{
							neighboring.Add(w.opposite.face);
						}
					}
				}
			}

			return neighboring;
		}

		static readonly Vector3 Vector3_Zero = new Vector3(0f, 0f, 0f);

		internal static void Flood(pb_WingedEdge wing, HashSet<pb_Face> selection)
		{
			Flood(null, wing, Vector3_Zero, -1f, selection);
		}

		internal static void Flood(pb_Object pb, pb_WingedEdge wing, Vector3 wingNrm, float maxAngle, HashSet<pb_Face> selection)
		{
			pb_WingedEdge next = wing;

			do
			{
				pb_WingedEdge opp = next.opposite;

				if(opp != null && !selection.Contains(opp.face))
				{
					if(maxAngle > 0f)
					{
						Vector3 oppNormal = pb_Math.Normal(pb, opp.face);

						if(Vector3.Angle(wingNrm, oppNormal) < maxAngle)
						{
							if( selection.Add(opp.face) )
								Flood(pb, opp, oppNormal, maxAngle, selection);
						}
					}
					else
					{
						if( selection.Add(opp.face) )
							Flood(pb, opp, wingNrm, maxAngle, selection);
					}
				}

				next = next.next;
			} while(next != wing);
		}

		/// <summary>
		/// Returns all adjacent faces as far as can be bridged within an angle.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <param name="maxAngleDiff"></param>
		/// <returns></returns>
		public static HashSet<pb_Face> FloodSelection(pb_Object pb, IList<pb_Face> faces, float maxAngleDiff)
		{
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, true);
			HashSet<pb_Face> source = new HashSet<pb_Face>(faces);
			HashSet<pb_Face> flood = new HashSet<pb_Face>();

			for(int i = 0; i < wings.Count; i++)
			{
				if(!flood.Contains(wings[i].face) && source.Contains(wings[i].face))
				{
					flood.Add(wings[i].face);
					Flood(pb, wings[i], maxAngleDiff > 0f ? pb_Math.Normal(pb, wings[i].face) : Vector3_Zero, maxAngleDiff, flood);
				}
			}
			return flood;
		}
	}
}
