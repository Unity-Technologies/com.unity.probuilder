using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;

namespace ProBuilder2.MeshOperations
{
	public static class pb_AppendPolygon
	{
		/**
		 *	FillHole differs from CreatePolygon in that CreatePolygon expects vertices to be passed
		 *	with the correct winding order already applied.  FillHole projects and attempts to figure
		 *	out the winding order.
		 */
		public static pb_ActionResult FillHole(this pb_Object pb, IList<int> indices, out pb_Face face)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();
			HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(lookup, indices);
			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			List<pb_Vertex> append_vertices = new List<pb_Vertex>();

			foreach(int i in common)
			{
				int index = sharedIndices[i][0];
				append_vertices.Add(new pb_Vertex(vertices[index]));
			}

			pb_FaceRebuildData data = FaceWithVertices(append_vertices);

			if(data != null)
			{
				data.sharedIndices = common.ToList();
				List<pb_Face> faces = new List<pb_Face>(pb.faces);
				pb_FaceRebuildData.Apply(new pb_FaceRebuildData[] { data }, vertices, faces, lookup, null);
				pb.SetVertices(vertices);
				pb.SetFaces(faces.ToArray());
				pb.SetSharedIndices(lookup);
				pb.ToMesh();

				// find an adjacent faces and test that the normals are correct
				List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
				pb_WingedEdge newFace = wings.FirstOrDefault(x => x.face == data.face);
				face = newFace.face;
				pb_WingedEdge orig = newFace;

				// grab first edge with a valid opposite face
				while(newFace.opposite == null)
				{
					newFace = newFace.next;
					if(newFace == orig) break;
				}

				if(newFace.opposite != null)
				{
					if(pb_ConformNormals.ConformOppositeNormal(newFace.opposite))
						pb.ToMesh();
				}

				return new pb_ActionResult(Status.Success, "Fill Hole");
			}

			face = null;

			return new pb_ActionResult(Status.Failure, "Insufficient Points");
		}

		/**
		 *	Create a new face given a set of unordered vertices.
		 */
		public static pb_FaceRebuildData FaceWithVertices(List<pb_Vertex> vertices)
		{
			List<int> triangles;

			if(pb_Triangulation.TriangulateVertices(vertices, out triangles))
			{
				pb_FaceRebuildData data = new pb_FaceRebuildData();
				data.vertices = vertices;
				data.face = new pb_Face(triangles.ToArray());
				return data;
			}

			return null;
		}

		/**
		 *	Find any holes touching one of the passed vertex indices.
		 */
		public static List<List<pb_Edge>> FindHoles(pb_Object pb, IList<int> indices)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(lookup, indices);
			List<List<pb_Edge>> holes = new List<List<pb_Edge>>();

			foreach(List<pb_WingedEdge> hole in pb_AppendPolygon.FindHoles(pb, common))
				holes.Add( hole.Select(x => x.edge.local).ToList() );
				
			return holes;
		}

		/**
		 *	Find any holes touching one of the passed common indices.
		 */
		public static List<List<pb_WingedEdge>> FindHoles(pb_Object pb, HashSet<int> common)
		{
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
			HashSet<pb_WingedEdge> used = new HashSet<pb_WingedEdge>();
			List<List<pb_WingedEdge>> holes = new List<List<pb_WingedEdge>>();

			for(int i = 0; i < wings.Count; i++)
			{
				pb_WingedEdge c = wings[i];

				// if this edge has been added to a hole already, or the edge isn't in the approved list of indices,
				// or if there's an opposite face, this edge doesn't belong to a hole.  move along
				if(c.opposite != null || used.Contains(c) || !(common.Contains(c.edge.common.x) || common.Contains(c.edge.common.y)))
					continue;

				List<pb_WingedEdge> hole = new List<pb_WingedEdge>();
				pb_WingedEdge it = c;
				int ind = it.edge.common.x;

				int loopBreaker = 0;

				while(it != null && loopBreaker++ < 65000)
				{
					used.Add(it);
					hole.Add(it);

					ind = it.edge.common.x == ind ? it.edge.common.y : it.edge.common.x;
					it = FindNextEdgeInHole(it, ind);

					if(it == c)
						break;
				}

				List<pb_Tuple<int, int>> splits = new List<pb_Tuple<int, int>>();

				for(int n = 0; n < hole.Count; n++)
				{
					pb_WingedEdge wing = hole[n];

					// check previous wings for y == x (closed loop).
					for(int p = n - 1; p > -1; p--)
					{
						if( wing.edge.common.y == hole[p].edge.common.x )
						{
							splits.Add( new pb_Tuple<int, int>(p, n) );
							break;
						}
					}
				}

				// create new lists from each segment
				// holes paths are tiered like so coming in:
				//
				//	[2, 0]
				// 	[0, 9]
				// 	[9, 10]
				// 		[10, 7]
				// 			[7, 6]
				// 			[6, 1]
				// 			[1, 4]
				// 			[4, 7]	<- (y == x)
				// 		[7, 8]
				// 		[8, 5]
				// 		[5, 3]
				// 		[3, 11]
				// 		[11, 10]	<- (y == x)
				// [10, 2] 			<- (y == x)
				// 
				// paths may also contain multiple segments non-tiered

				int rx = 0, ry = 0, px = 0, splitCount = splits.Count;

				for(int n = 0; n < splitCount; n++)
				{
					int x = splits[n].Item1, y = splits[n].Item2;

					if(x > px)
					{
						rx += ry;
						ry = 0;
						px = x;
					}

					y = y - ry;
					int range = (y-x) + 1;
					x = x - rx;
					ry += range;

					List<pb_WingedEdge> sec = hole.GetRange(x, range);
					hole.RemoveRange(x, range);

					// verify that this path has at least one index that was asked for
					if(splitCount < 2 || sec.Any(w => common.Contains(w.edge.common.x)) || sec.Any(w => common.Contains(w.edge.common.y)))
						holes.Add( sec );
				}

				if(loopBreaker > 64999)
					Debug.LogError("find holes loop went crazy");
			}

			return holes;
		}

		private static pb_WingedEdge FindNextEdgeInHole(pb_WingedEdge wing, int common)
		{
			pb_WingedEdge next = wing.GetAdjacentEdgeWithCommonIndex(common);

			while(next != null && next != wing)
			{
				if(next.opposite == null)
					return next;

				next = next.opposite.GetAdjacentEdgeWithCommonIndex(common);
			}

			return null;
		}

		// private static void FollowNonManifoldPath(pb_WingedEdge edge, HashSet<pb_WingedEdge> path)
		// {
		// 	// came full circle
		// 	if(path.Contains(edge))
		// 		return;

		// 	if(edge.opposite == null)
		// 	{
		// 		path.Add(edge);

		// 		pb_WingedEdge next = edge.opposite.next;
		// 		pb_WingedEdge prev = edge.opposite.previous;

		// 		if(next.edge.common.Contains(edge.edge.common))	
		// 			FollowNonManifoldPath(next, path);
		// 		else if(prev.edge.opposite.Contains(edge.edge.common))
		// 			FollowNonManifoldPath(prev, path);
		// 	}
		// }
	}
}
