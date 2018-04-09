using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Utility for selecting a face loop.
	/// </summary>
	public static class pb_FaceLoop
	{
		/// <summary>
		/// Fetch a face loop.
		/// </summary>
		/// <param name="pb">Target pb_Object.</param>
		/// <param name="faces">The faces to scan for face loops.</param>
		/// <param name="ring">Toggles between loop and face. Ring and loop are arbritary with faces, so this parameter just toggles between which gets scanned first.</param>
		/// <returns></returns>
		public static HashSet<pb_Face> GetFaceLoop(pb_Object pb, pb_Face[] faces, bool ring = false)
		{
			HashSet<pb_Face> loops = new HashSet<pb_Face>();
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);

			foreach(pb_Face face in faces)
				loops.UnionWith(GetFaceLoop(wings, face, ring));

			return loops;
		}

		/// <summary>
		/// Get both a face ring and loop from the selected faces.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		public static HashSet<pb_Face> GetFaceRingAndLoop(pb_Object pb, pb_Face[] faces)
		{
			HashSet<pb_Face> loops = new HashSet<pb_Face>();
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);

			foreach (pb_Face face in faces)
			{
				loops.UnionWith(GetFaceLoop(wings, face, true));
				loops.UnionWith(GetFaceLoop(wings, face, false));
			}

			return loops;
		}

		/// <summary>
		/// Get a face loop or ring from a set of winged edges.
		/// </summary>
		/// <param name="wings"></param>
		/// <param name="face"></param>
		/// <param name="ring"></param>
		/// <returns></returns>
		static HashSet<pb_Face> GetFaceLoop(List<pb_WingedEdge> wings, pb_Face face, bool ring)
		{
			HashSet<pb_Face> loop = new HashSet<pb_Face>();

			if(face == null)
				return loop;

			pb_WingedEdge start = wings.FirstOrDefault(x => x.face == face);

			if(start == null)
				return loop;

			if(ring)
				start = start.next ?? start.previous;

			for (int i = 0; i < 2; i++)
			{
				pb_WingedEdge cur = start;

				if (i == 1)
				{
					if(start.opposite != null && start.opposite.face != null)
						cur = start.opposite;
					else
						break;
				}

				do
				{
					if (!loop.Add(cur.face))
						break;

					if (cur.Count() != 4)
						break;

					// count == 4 assures us next.next is valid, but opposite can still be null
					cur = cur.next.next.opposite;
				} while (cur != null && cur.face != null);
			}

			return loop;
		}
	}
}