using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for working with surface topology.
	/// </summary>
	public static class Topology
	{
		/// <summary>
		/// Reverse the winding order for each passed pb_Face.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="faces"></param>
		public static void ReverseWindingOrder(this ProBuilderMesh mesh, Face[] faces)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faces == null)
                throw new ArgumentNullException("faces");

            for (int i = 0; i < faces.Length; i++)
				faces[i].Reverse();
		}

		/// <summary>
		/// Attempt to figure out the winding order the passed face.
		/// </summary>
		/// <remarks>May return WindingOrder.Unknown.</remarks>
		/// <param name="mesh"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		public static WindingOrder GetWindingOrder(this ProBuilderMesh mesh, Face face)
		{
			Vector2[] p = Projection.PlanarProject(mesh, face);
			return GetWindingOrder(p);
		}

		static WindingOrder GetWindingOrder(IList<Vertex> vertices, IList<int> indices)
		{
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            if (indices == null)
                throw new ArgumentNullException("indices");

            Vector2[] p = Projection.PlanarProject(vertices, indices);
			return GetWindingOrder(p);
		}

		/// <summary>
		/// Return the winding order of a set of ordered points.
		/// </summary>
		/// <remarks>http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order</remarks>
		/// <param name="points">A set of unordered indices.</param>
		/// <returns>The winding order if found, WindingOrder.Unknown if not.</returns>
		public static WindingOrder GetWindingOrder(IList<Vector2> points)
		{
            if (points == null)
                throw new ArgumentNullException("points");

            float sum = 0f;

			int len = points.Count;

			// http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
			for(int i = 0; i < len; i++)
			{
				Vector2 a = points[i];
				Vector2 b = i < len - 1 ? points[i+1] : points[0];

				sum += ( (b.x - a.x) * (b.y+a.y) );
			}

			return sum == 0f ? WindingOrder.Unknown : (sum > 0f ? WindingOrder.Clockwise : WindingOrder.CounterClockwise);
		}

		/// <summary>
		/// Reverses the orientation of the middle edge in a quad.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		public static bool FlipEdge(this ProBuilderMesh mesh, Face face)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (face == null)
                throw new ArgumentNullException("face");

            int[] indices = face.indices;

			if(indices.Length != 6)
				return false;

			int[] mode = ArrayUtility.FilledArray<int>(1, indices.Length);

			for(int x = 0; x < indices.Length - 1; x++)
			{
				for(int y = x+1; y < indices.Length; y++)
				{
					if(indices[x] == indices[y])
					{
						mode[x]++;
						mode[y]++;
					}
				}
			}

			if(	mode[0] + mode[1] + mode[2] != 5 ||
				mode[3] + mode[4] + mode[5] != 5 )
				return false;

			int i0 = indices[ mode[0] == 1 ? 0 : mode[1] == 1 ? 1 : 2 ];
			int i1 = indices[ mode[3] == 1 ? 3 : mode[4] == 1 ? 4 : 5 ];

			int used = -1;

			if(mode[0] == 2)
			{
				used = indices[0];
				indices[0] =  i1;
			}
			else if(mode[1] == 2)
			{
				used = indices[1];
				indices[1] = i1;
			}
			else if(mode[2] == 2)
			{
				used = indices[2];
				indices[2] = i1;
			}

			if(mode[3] == 2 && indices[3] != used)
				indices[3] = i0;
			else if(mode[4] == 2 && indices[4] != used)
				indices[4] = i0;
			else if(mode[5] == 2 && indices[5] != used)
				indices[5] = i0;

			face.InvalidateCache();

			return true;
		}

		/// <summary>
		/// Conform groups of adjacent faces.  This function supports multiple islands of interconnected faces, but it
		/// may not unify each island the same way.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		public static ActionResult ConformNormals(this ProBuilderMesh pb, IList<Face> faces)
		{
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb, faces);
			HashSet<Face> used = new HashSet<Face>();
			int count = 0;

			// this loop adds support for multiple islands of grouped selections
			for(int i = 0; i < wings.Count; i++)
			{
				if(used.Contains(wings[i].face))
					continue;

				Dictionary<Face, bool> flags = new Dictionary<Face, bool>();

				GetWindingFlags(wings[i], true, flags);

				int flip = 0;

				foreach(var kvp in flags)
					flip += kvp.Value ? 1 : -1;

				bool direction = flip > 0;

				foreach(var kvp in flags)
				{
					if(direction != kvp.Value)
					{
						count++;
						kvp.Key.Reverse();
					}
				}

				used.UnionWith(flags.Keys);
			}

			if(count > 0)
				return new ActionResult(ActionResult.Status.Success, count > 1 ? string.Format("Flipped {0} faces", count) : "Flipped 1 face");
			else
				return new ActionResult(ActionResult.Status.NoChange, "Faces Uniform");
		}

		static void GetWindingFlags(WingedEdge edge, bool flag, Dictionary<Face, bool> flags)
		{
			flags.Add(edge.face, flag);

			WingedEdge next = edge;

			do
			{
				WingedEdge opp = next.opposite;

				if(opp != null && !flags.ContainsKey(opp.face))
				{
					Edge cea = GetCommonEdgeInWindingOrder(next);
					Edge ceb = GetCommonEdgeInWindingOrder(opp);

					GetWindingFlags(opp, cea.x == ceb.x ? !flag : flag, flags);
				}

				next = next.next;

			} while(next != edge);
		}

		/// <summary>
		/// Ensure the opposite face to source matches the winding order.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		internal static ActionResult ConformOppositeNormal(WingedEdge source)
		{
			if(source == null || source.opposite == null)
				return new ActionResult(ActionResult.Status.Failure, "Source edge does not share an edge with another face.");

			Edge cea = GetCommonEdgeInWindingOrder(source);
			Edge ceb = GetCommonEdgeInWindingOrder(source.opposite);

			if( cea.x == ceb.x )
			{
				source.opposite.face.Reverse();

				return new ActionResult(ActionResult.Status.Success, "Reversed target face winding order.");
			}

			return new ActionResult(ActionResult.Status.NoChange, "Faces already unified.");
		}

		/// <summary>
		/// Iterate a face and return a new common edge where the edge indices are true to the triangle winding order.
		/// </summary>
		/// <param name="wing"></param>
		/// <returns></returns>
		static Edge GetCommonEdgeInWindingOrder(WingedEdge wing)
		{
			int[] indices = wing.face.indices;
			int len = indices.Length;

			for(int i = 0; i < len; i += 3)
			{
				Edge e = wing.edge.local;
				int a = indices[i], b = indices[i+1], c = indices[i+2];

				if(e.x == a && e.y == b)
					return wing.edge.common;
				else if(e.x == b && e.y == a)
					return new Edge(wing.edge.common.y, wing.edge.common.x);
				else if(e.x == b && e.y == c)
					return wing.edge.common;
				else if(e.x == c && e.y == b)
					return new Edge(wing.edge.common.y, wing.edge.common.x);
				else if(e.x == c && e.y == a)
					return wing.edge.common;
				else if(e.x == a && e.y == c)
					return new Edge(wing.edge.common.y, wing.edge.common.x);
			}

			return Edge.Empty;
		}

		/// <summary>
		/// Match a target face to the source face. Faces must be adjacent.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="lookup"></param>
		internal static void MatchNormal(Face source, Face target, Dictionary<int, int> lookup)
		{
			List<EdgeLookup> sourceEdges = EdgeLookup.GetEdgeLookup(source.edgesInternal, lookup).ToList();
			List<EdgeLookup> targetEdges = EdgeLookup.GetEdgeLookup(target.edgesInternal, lookup).ToList();

			bool superBreak = false;

			Edge src, tar;

			for(int i = 0; !superBreak && i < sourceEdges.Count; i++)
			{
				src = sourceEdges[i].common;

				for(int n = 0; !superBreak && n < targetEdges.Count; n++)
				{
					tar = targetEdges[n].common;

					if(src.Equals(tar))
					{
						if(src.x == tar.x)
							target.Reverse();

						superBreak = true;
					}
				}
			}
		}
	}
}
