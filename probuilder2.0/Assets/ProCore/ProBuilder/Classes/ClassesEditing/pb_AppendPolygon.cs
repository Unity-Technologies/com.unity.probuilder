using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;

namespace ProBuilder2.MeshOperations
{
	public static class pb_AppendPolygon
	{
		 /**
		  *	Create a new face connecting the vertices selected by indices.
		  */
		public static pb_ActionResult CreatePolygon(this pb_Object pb, IList<int> indices, bool unordered, out pb_Face face)
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

			pb_FaceRebuildData data = FaceWithVertices(append_vertices, unordered);

			if(data != null)
			{
				data.sharedIndices = common.ToList();
				List<pb_Face> faces = new List<pb_Face>(pb.faces);
				pb_FaceRebuildData.Apply(new pb_FaceRebuildData[] { data }, vertices, faces, lookup, null);
				pb.SetVertices(vertices);
				pb.SetFaces(faces.ToArray());
				pb.SetSharedIndices(lookup);
				face = data.face;

				return new pb_ActionResult(Status.Success, "Create Polygon");
			}

			face = null;

			const string INSUF_PTS = "Too Few Unique Points Selected";
			const string BAD_WINDING = "Points not ordered correctly";

			return new pb_ActionResult(Status.Failure, unordered ? INSUF_PTS : BAD_WINDING);
		}

		/**
		 *	Create a poly shape from a set of points on a plane.
		 */
		public static pb_ActionResult CreateShapeFromPolygon(this pb_PolyShape poly)
		{
			return poly.mesh.CreateShapeFromPolygon(poly.points, poly.extrude, poly.flipNormals);
		}

		public static pb_ActionResult CreateShapeFromPolygon(this pb_Object pb, IList<Vector3> points, float extrude, bool flipNormals)
		{
			if(points.Count < 3)
			{
				pb.SetVertices(new Vector3[0]);
				pb.SetFaces(new pb_Face[0]);
				pb.SetSharedIndices(new pb_IntArray[0]);
				return new pb_ActionResult(Status.NoChange, "Too Few Points");
			}

			Vector3[] vertices = points.ToArray();
			List<int> triangles;

			pb_Log.PushLogLevel(pb_LogLevel.Error);

			if(pb_Triangulation.TriangulateVertices(vertices, out triangles, false))
			{
				int[] indices = triangles.ToArray();

				if(pb_Math.PolygonArea(vertices, indices) < Mathf.Epsilon )
				{
					pb.SetVertices(new Vector3[0]);
					pb.SetFaces(new pb_Face[0]);
					pb.SetSharedIndices(new pb_IntArray[0]);
					pb_Log.PopLogLevel();
					return new pb_ActionResult(Status.Failure, "Polygon Area < Epsilon");
				}

				pb.GeometryWithVerticesFaces(vertices, new pb_Face[] { new pb_Face(indices) });

				Vector3 nrm = pb_Math.Normal(pb, pb.faces[0]);

				if(Vector3.Dot(Vector3.up, nrm) > 0f)
					pb.faces[0].ReverseIndices();

				pb.DuplicateAndFlip(pb.faces);

				pb.Extrude(new pb_Face[] { pb.faces[1] }, ExtrudeMethod.IndividualFaces, extrude);

				if((extrude < 0f && !flipNormals) || (extrude > 0f && flipNormals))
					pb.ReverseWindingOrder(pb.faces);
			}
			else
			{
				pb_Log.PopLogLevel();
				return new pb_ActionResult(Status.Failure, "Failed Triangulating Points");
			}
			
			pb_Log.PopLogLevel();

			pb.ToMesh();
			pb.Refresh();

			return new pb_ActionResult(Status.Success, "Create Polygon Shape");
		}

		/**
		 *	Create a new face given a set of unordered vertices (or ordered, if unordered param is set to false).
		 */
		public static pb_FaceRebuildData FaceWithVertices(List<pb_Vertex> vertices, bool unordered = true)
		{
			List<int> triangles;

			if(pb_Triangulation.TriangulateVertices(vertices, out triangles, unordered))
			{
				pb_FaceRebuildData data = new pb_FaceRebuildData();
				data.vertices = vertices;
				data.face = new pb_Face(triangles.ToArray());
				return data;
			}

			return null;
		}

		/**
		 * Given a path of vertices, inserts a new vertex in the center inserts triangles along the path.
		 */
		public static List<pb_FaceRebuildData> TentCapWithVertices(List<pb_Vertex> path)
		{
			int count = path.Count;
			pb_Vertex center = pb_Vertex.Average(path);
			List<pb_FaceRebuildData> faces = new List<pb_FaceRebuildData>();

			for(int i = 0; i < count; i++)
			{
				List<pb_Vertex> vertices = new List<pb_Vertex>()
				{
					path[i],
					center,
					path[(i+1)%count]
				};

				pb_FaceRebuildData data = new pb_FaceRebuildData();
				data.vertices = vertices;
				data.face = new pb_Face(new int[] {0 , 1, 2});

				faces.Add(data);
			}

			return faces;
		}

		/**
		 *	Find any holes touching one of the passed vertex indices.
		 */
		public static List<List<pb_Edge>> FindHoles(pb_Object pb, IList<int> indices)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(lookup, indices);
			List<List<pb_Edge>> holes = new List<List<pb_Edge>>();
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);

			foreach(List<pb_WingedEdge> hole in pb_AppendPolygon.FindHoles(wings, common))
				holes.Add( hole.Select(x => x.edge.local).ToList() );

			return holes;
		}

		/**
		 *	Find any holes touching one of the passed common indices.
		 */
		public static List<List<pb_WingedEdge>> FindHoles(List<pb_WingedEdge> wings, HashSet<int> common)
		{
			HashSet<pb_WingedEdge> used = new HashSet<pb_WingedEdge>();
			List<List<pb_WingedEdge>> holes = new List<List<pb_WingedEdge>>();

			for(int i = 0; i < wings.Count; i++)
			{
				pb_WingedEdge c = wings[i];

				// if this edge has been added to a hole already, or the edge isn't in the approved list of indices,
				// or if there's an opposite face, this edge doesn't belong to a hole.  move along.
				if(c.opposite != null || used.Contains(c) || !(common.Contains(c.edge.common.x) || common.Contains(c.edge.common.y)))
					continue;

				List<pb_WingedEdge> hole = new List<pb_WingedEdge>();
				pb_WingedEdge it = c;
				int ind = it.edge.common.x;

				while(it != null)
				{
					used.Add(it);
					hole.Add(it);

					ind = it.edge.common.x == ind ? it.edge.common.y : it.edge.common.x;
					it = FindNextEdgeInHole(it, ind);

					if(it == c)
						break;
				}

				List<pb_Tuple<int, int>> splits = new List<pb_Tuple<int, int>>();

				// check previous wings for y == x (closed loop).
				for(int n = 0; n < hole.Count; n++)
				{
					pb_WingedEdge wing = hole[n];

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
				// holes paths are nested, with holes
				// possibly split between multiple nested
				// holes
				//
				//	[2, 0]                                     [5, 3]
				// 	[0, 9]                                     [3, 11]
				// 	[9, 10]                                    [11, 10]
				// 		[10, 7]                                    [10, 2]
				// 			[7, 6]             or with split   	    [2, 0]
				// 			[6, 1]             nesting ->   	    [0, 9]
				// 			[1, 4]                             	    [9, 10]
				// 			[4, 7]	<- (y == x)                [10, 7]
				// 		[7, 8]                                 	    [7, 6]
				// 		[8, 5]                                 	    [6, 1]
				// 		[5, 3]                                 	    [1, 4]
				// 		[3, 11]                                	    [4, 7]
				// 		[11, 10]	<- (y == x)                [7, 8]
				// [10, 2] 			<- (y == x)                [8, 5]
				//
				// paths may also contain multiple segments non-tiered

				int splitCount = splits.Count;

				splits.Sort( (x, y) => x.Item1.CompareTo(y.Item1) );

				int[] shift = new int[splitCount];

				// Debug.Log(hole.ToString("\n") + "\n" + splits.ToString("\n"));

				for(int n = splitCount - 1; n > -1; n--)
				{
					int x = splits[n].Item1, y = splits[n].Item2 - shift[n];
					int range = (y - x) + 1;

					List<pb_WingedEdge> section = hole.GetRange(x, range);

					hole.RemoveRange(x, range);

					for(int m = n - 1; m > -1; m--)
						if(splits[m].Item2 > splits[n].Item2)
							shift[m] += range;

					// verify that this path has at least one index that was asked for
					if(splitCount < 2 || section.Any(w => common.Contains(w.edge.common.x)) || section.Any(w => common.Contains(w.edge.common.y)))
						holes.Add( section );
				}
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
	}
}
