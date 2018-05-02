using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	public static partial class ElementSelection
	{
		/// <summary>
		/// Fetch a face loop.
		/// </summary>
		/// <param name="mesh">Target pb_Object.</param>
		/// <param name="faces">The faces to scan for face loops.</param>
		/// <param name="ring">Toggles between loop and face. Ring and loop are arbritary with faces, so this parameter just toggles between which gets scanned first.</param>
		/// <returns></returns>
		public static HashSet<Face> GetFaceLoop(ProBuilderMesh mesh, Face[] faces, bool ring = false)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faces == null)
                throw new ArgumentNullException("faces");

            HashSet<Face> loops = new HashSet<Face>();
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);

			foreach(Face face in faces)
				loops.UnionWith(GetFaceLoop(wings, face, ring));

			return loops;
		}

		/// <summary>
		/// Get both a face ring and loop from the selected faces.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		public static HashSet<Face> GetFaceRingAndLoop(ProBuilderMesh mesh, Face[] faces)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faces == null)
                throw new ArgumentNullException("faces");

            HashSet<Face> loops = new HashSet<Face>();
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);

			foreach (Face face in faces)
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
		static HashSet<Face> GetFaceLoop(List<WingedEdge> wings, Face face, bool ring)
		{
			HashSet<Face> loop = new HashSet<Face>();

			if(face == null)
				return loop;

			WingedEdge start = wings.FirstOrDefault(x => x.face == face);

			if(start == null)
				return loop;

			if(ring)
				start = start.next ?? start.previous;

			for (int i = 0; i < 2; i++)
			{
				WingedEdge cur = start;

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
