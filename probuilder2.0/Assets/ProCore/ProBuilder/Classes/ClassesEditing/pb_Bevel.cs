using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for beveling edges.
	/// </summary>
	static class pb_Bevel
	{
		public static pb_ActionResult BevelEdges(pb_Object pb, IList<pb_Edge> edges, float amount, out List<pb_Face> createdFaces)
		{
			createdFaces = null;

			Dictionary<int, int> 		lookup 		= pb.sharedIndices.ToDictionary();
			List<pb_Vertex> 			vertices 	= new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );
			List<pb_EdgeLookup> 		m_edges 	= pb_EdgeLookup.GetEdgeLookup(edges, lookup).Distinct().ToList();
			List<pb_WingedEdge> 		wings 		= pb_WingedEdge.GetWingedEdges(pb);
			List<pb_FaceRebuildData> 	appendFaces = new List<pb_FaceRebuildData>();

			Dictionary<pb_Face, List<int>> 	ignore 	= new Dictionary<pb_Face, List<int>>();
			HashSet<int> 					slide 	= new HashSet<int>();
			int beveled = 0;

			Dictionary<int, List<pb_Tuple<pb_FaceRebuildData, List<int>>>> holes = new Dictionary<int, List<pb_Tuple<pb_FaceRebuildData, List<int>>>>();

			// test every edge that will be moved along to make sure the bevel distance is appropriate.  if it's not, adjust the max bevel amount
			// to suit.
			Dictionary<int, List<pb_WingedEdge>> spokes = pb_WingedEdge.GetSpokes(wings);
			HashSet<int> tested_common = new HashSet<int>();

			foreach(pb_EdgeLookup e in m_edges)
			{
				if(tested_common.Add(e.common.x))
				{
					foreach(pb_WingedEdge w in spokes[e.common.x])
					{
						pb_Edge le = w.edge.local;
						amount = Mathf.Min( Vector3.Distance(vertices[le.x].position, vertices[le.y].position) - .001f, amount );
					}
				}

				if(tested_common.Add(e.common.y))
				{
					foreach(pb_WingedEdge w in spokes[e.common.y])
					{
						pb_Edge le = w.edge.local;
						amount = Mathf.Min( Vector3.Distance(vertices[le.x].position, vertices[le.y].position) - .001f, amount );
					}
				}
			}

			if(amount < .001f)
				return new pb_ActionResult(Status.Canceled, "Bevel Distance > Available Surface");

			// iterate selected edges and move each leading edge back along it's direction
			// storing information about adjacent faces in the process
			foreach(pb_EdgeLookup lup in m_edges)
			{
				pb_WingedEdge we = wings.FirstOrDefault(x => x.edge.Equals(lup));

				if(we == null || we.opposite == null)
					continue;

				beveled++;

				ignore.AddOrAppend(we.face, we.edge.common.x);
				ignore.AddOrAppend(we.face, we.edge.common.y);
				ignore.AddOrAppend(we.opposite.face, we.edge.common.x);
				ignore.AddOrAppend(we.opposite.face, we.edge.common.y);

				// after initial slides go back and split indirect triangles at the intersecting index into two vertices
				slide.Add(we.edge.common.x);
				slide.Add(we.edge.common.y);

				SlideEdge(vertices, we, amount);
				SlideEdge(vertices, we.opposite, amount);

				appendFaces.AddRange( GetBridgeFaces(vertices, we, we.opposite, holes) );
			}

			if(beveled < 1)
			{
				createdFaces = null;
				return new pb_ActionResult(Status.Canceled, "Cannot Bevel Open Edges");
			}

			// grab the "createdFaces" array now so that the selection returned is just the bridged faces
			// then add holes later
			createdFaces = new List<pb_Face>(appendFaces.Select(x => x.face));

			Dictionary<pb_Face, List<pb_Tuple<pb_WingedEdge, int>>> sorted = new Dictionary<pb_Face, List<pb_Tuple<pb_WingedEdge, int>>>();

			// sort the adjacent but affected faces into winged edge groups where each group contains a set of
			// unique winged edges pointing to the same face
			foreach(int c in slide)
			{
				IEnumerable<pb_WingedEdge> matches = wings.Where(x => x.edge.common.Contains(c) && !(ignore.ContainsKey(x.face) && ignore[x.face].Contains(c)));

				HashSet<pb_Face> used = new HashSet<pb_Face>();

				foreach(pb_WingedEdge match in matches)
				{
					if(!used.Add(match.face))
						continue;

					sorted.AddOrAppend(match.face, new pb_Tuple<pb_WingedEdge, int>(match, c));
				}
			}

			// now go through those sorted faces and apply the vertex exploding, keeping track of any holes created
			foreach(KeyValuePair<pb_Face, List<pb_Tuple<pb_WingedEdge, int>>> kvp in sorted)
			{
				// common index & list of vertices it was split into
				Dictionary<int, List<int>> appendedVertices;

				pb_FaceRebuildData f = pb_VertexOps.ExplodeVertex(vertices, kvp.Value, amount, out appendedVertices);

				if(f == null)
					continue;

				appendFaces.Add(f);

				foreach(var apv in appendedVertices)
				{
					// organize holes by new face so that later we can compare the winding of the new face to the hole face
					// holes are sorted by key: common index value: face, vertex list
					holes.AddOrAppend(apv.Key, new pb_Tuple<pb_FaceRebuildData, List<int>>(f, apv.Value));
				}
			}

			pb_FaceRebuildData.Apply(appendFaces, pb, vertices);
			int removed = pb.DeleteFaces(sorted.Keys).Length;
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));

			// @todo don't rebuild sharedindices, keep 'em cached
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			lookup = sharedIndices.ToDictionary();
			List<HashSet<int>> holesCommonIndices = new List<HashSet<int>>();

			// offset the indices of holes and cull any potential holes that are less than 3 indices (not a hole :)
			foreach(KeyValuePair<int, List<pb_Tuple<pb_FaceRebuildData, List<int>>>> hole in holes)
			{
				// less than 3 indices in hole path; ain't a hole
				if(hole.Value.Sum(x => x.Item2.Count) < 3)
					continue;

				HashSet<int> holeCommon = new HashSet<int>();

				foreach(pb_Tuple<pb_FaceRebuildData, List<int>> path in hole.Value)
				{
					int offset = path.Item1.Offset() - removed;

					for(int i = 0; i < path.Item2.Count; i++)
						holeCommon.Add(lookup[path.Item2[i] + offset]);
				}

				holesCommonIndices.Add(holeCommon);
			}

			List<pb_WingedEdge> modified = pb_WingedEdge.GetWingedEdges(pb, appendFaces.Select(x => x.face));

			// now go through the holes and create faces for them
			vertices = new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );

			List<pb_FaceRebuildData> holeFaces = new List<pb_FaceRebuildData>();

			foreach(HashSet<int> h in holesCommonIndices)
			{
				// even if a set of hole indices made it past the initial culling, the distinct part
				// may have reduced the index count
				if(h.Count < 3)
				{
					continue;
				}
				// skip sorting the path if it's just a triangle
				if(h.Count < 4)
				{
					List<pb_Vertex> v = new List<pb_Vertex>( pb_Vertex.GetVertices(pb, h.Select(x => sharedIndices[x][0]).ToList()) );
					holeFaces.Add(pb_AppendPolygon.FaceWithVertices(v));
				}
				// if this hole has > 3 indices, it needs a tent pole triangulation, which requires sorting into the perimeter order
				else
				{
					List<int> holePath = pb_WingedEdge.SortCommonIndicesByAdjacency(modified, h);
					List<pb_Vertex> v = new List<pb_Vertex>( pb_Vertex.GetVertices(pb, holePath.Select(x => sharedIndices[x][0]).ToList()) );
					holeFaces.AddRange( pb_AppendPolygon.TentCapWithVertices(v) );
				}
			}

			pb_FaceRebuildData.Apply(holeFaces, pb, vertices);
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));

			// go through new faces and conform hole normals
			// get a hash of just the adjacent and bridge faces
			// HashSet<pb_Face> adjacent = new HashSet<pb_Face>(appendFaces.Select(x => x.face));
			// and also just the filled holes
			HashSet<pb_Face> newHoles = new HashSet<pb_Face>(holeFaces.Select(x => x.face));
			// now append filled holes to the full list of added faces
			appendFaces.AddRange(holeFaces);

			List<pb_WingedEdge> allNewFaceEdges = pb_WingedEdge.GetWingedEdges(pb, appendFaces.Select(x => x.face));

			for(int i = 0; i < allNewFaceEdges.Count && newHoles.Count > 0; i++)
			{
				pb_WingedEdge wing = allNewFaceEdges[i];

				if(newHoles.Contains(wing.face))
				{
					newHoles.Remove(wing.face);

					// find first edge whose opposite face isn't a filled hole* then
					// conform normal by that.
					// *or is a filled hole but has already been conformed
					foreach(pb_WingedEdge w in wing)
					{
						if(!newHoles.Contains(w.opposite.face))
						{
							w.face.material = w.opposite.face.material;
							w.face.uv = new pb_UV(w.opposite.face.uv);
							pb_ConformNormals.ConformOppositeNormal(w.opposite);
							break;
						}
					}
				}
			}

			pb.ToMesh();

			return new pb_ActionResult(Status.Success, "Bevel Edges");
 		}

 		private static readonly int[] BRIDGE_INDICES_NRM = new int[] { 2, 1, 0 };

 		private static List<pb_FaceRebuildData> GetBridgeFaces(
 			IList<pb_Vertex> vertices,
 			pb_WingedEdge left,
 			pb_WingedEdge right,
 			Dictionary<int, List<pb_Tuple<pb_FaceRebuildData, List<int>>>> holes)
 		{
 			List<pb_FaceRebuildData> faces = new List<pb_FaceRebuildData>();

 			pb_FaceRebuildData rf = new pb_FaceRebuildData();

 			pb_EdgeLookup a = left.edge;
 			pb_EdgeLookup b = right.edge;

 			rf.vertices = new List<pb_Vertex>()
 			{
 				vertices[a.local.x],
 				vertices[a.local.y],
 				vertices[a.common.x == b.common.x ? b.local.x : b.local.y],
 				vertices[a.common.x == b.common.x ? b.local.y : b.local.x]
 			};

 			Vector3 an = pb_Math.Normal(vertices, left.face.indices);
 			Vector3 bn = pb_Math.Normal(rf.vertices, BRIDGE_INDICES_NRM);

 			int[] triangles = new int[] { 2, 1, 0, 2, 3, 1 };

 			if( Vector3.Dot(an, bn) < 0f)
 				System.Array.Reverse(triangles);

 			rf.face = new pb_Face(
 				triangles,
 				left.face.material,
 				new pb_UV(),
 				-1,
 				-1,
 				-1,
 				false);

 			faces.Add(rf);

 			holes.AddOrAppend(a.common.x, new pb_Tuple<pb_FaceRebuildData, List<int>>(rf, new List<int>() { 0, 2 }));
 			holes.AddOrAppend(a.common.y, new pb_Tuple<pb_FaceRebuildData, List<int>>(rf, new List<int>() { 1, 3 }));

 			return faces;
 		}

 		private static void SlideEdge(IList<pb_Vertex> vertices, pb_WingedEdge we, float amount)
 		{
			we.face.manualUV = true;
			we.face.textureGroup = -1;

			pb_Edge slide_x = GetLeadingEdge(we, we.edge.common.x);
			pb_Edge slide_y = GetLeadingEdge(we, we.edge.common.y);

			if(!slide_x.IsValid() || !slide_y.IsValid())
				return;

			pb_Vertex x = (vertices[slide_x.x] - vertices[slide_x.y]);
			x.Normalize();

			pb_Vertex y = (vertices[slide_y.x] - vertices[slide_y.y]);
			y.Normalize();

			// need the pb_Vertex value to be modified, not reassigned in this array (which += does)
			vertices[we.edge.local.x].Add(x * amount);
			vertices[we.edge.local.y].Add(y * amount);
		}

		private static pb_Edge GetLeadingEdge(pb_WingedEdge wing, int common)
		{
			if(wing.previous.edge.common.x == common)
				return new pb_Edge(wing.previous.edge.local.y, wing.previous.edge.local.x);
			else if(wing.previous.edge.common.y == common)
				return new pb_Edge(wing.previous.edge.local.x, wing.previous.edge.local.y);
			else if(wing.next.edge.common.x == common)
				return new pb_Edge(wing.next.edge.local.y, wing.next.edge.local.x);
			else if(wing.next.edge.common.y == common)
				return new pb_Edge(wing.next.edge.local.x, wing.next.edge.local.y);

			return pb_Edge.Empty;
		}
	}
}
