using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Functions for beveling edges.
	 */
	public static class pb_Bevel
	{
		public static pb_ActionResult BevelEdges(pb_Object pb, IList<pb_Edge> edges, float amount, out List<pb_Face> createdFaces)
		{
			Dictionary<int, int> 		lookup 		= pb.sharedIndices.ToDictionary();
			List<pb_Vertex> 			vertices 	= new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );
			List<pb_EdgeLookup> 		m_edges 	= pb_EdgeLookup.GetEdgeLookup(edges, lookup).Distinct().ToList();
			List<pb_WingedEdge> 		wings 		= pb_WingedEdge.GetWingedEdges(pb);
			List<pb_FaceRebuildData> 	appendFaces = new List<pb_FaceRebuildData>();

			Dictionary<pb_Face, List<int>> 	ignore 	= new Dictionary<pb_Face, List<int>>();
			HashSet<int> 					slide 	= new HashSet<int>();
			Dictionary<int, pb_Tuple<pb_Face, List<pb_Vertex>>> holes = new Dictionary<int, pb_Tuple<pb_Face, List<pb_Vertex>>>();
			int beveled = 0;
	
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

				SlideEdge(vertices, we, holes, amount);
				SlideEdge(vertices, we.opposite, holes, amount);

				appendFaces.AddRange( GetBridgeFaces(vertices, we, we.opposite) );
			}

			if(beveled < 1)
			{
				createdFaces = null;
				return new pb_ActionResult(Status.Canceled, "Cannot Bevel Open Edges");
			}
			
			// grab the "createdFaces" array now so that the selection returned is just the bridged faces
			// then add holes later
			HashSet<pb_Face> bridgedFaces = new HashSet<pb_Face>(appendFaces.Select(x => x.face));

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
				Dictionary<int, List<pb_Vertex>> appendedVertices;

				pb_FaceRebuildData f = pbVertexOps.ExplodeVertex(vertices, kvp.Value, amount, out appendedVertices);

				foreach(var apv in appendedVertices)
				{
					pb_Tuple<pb_Face, List<pb_Vertex>> entries;

					// organize holes by new face so that later we can compare the winding of the new face to the hole face
					if(holes.TryGetValue(apv.Key, out entries))
						entries.Item2.AddRange(apv.Value);
					else
						holes.Add(apv.Key, new pb_Tuple<pb_Face, List<pb_Vertex>>(f.face, apv.Value));
				}

				if(f != null)
					appendFaces.Add(f);
			}

			// iterate each common index affected and see if it contained more than 2 leading edges.  if it did,
			// a hole was created and needs to be filled.

			HashSet<pb_Face> holedFaces = new HashSet<pb_Face>();

			foreach(var k in holes)
			{		
				HashSet<int> used = new HashSet<int>();
				List<pb_Vertex> v = new List<pb_Vertex>();

				foreach(pb_Vertex vert in k.Value.Item2)
					if(used.Add(pb_Vector.GetHashCode(vert.position)))
						v.Add(vert);

				if(v.Count < 3)
					continue;

				pb_FaceRebuildData filledHole = pb_AppendPolygon.FaceWithVertices(v);
				
				if(filledHole != null)
				{
					// later when the mesh is rebuilt conform new hole normal to it's donor face
					holedFaces.Add(filledHole.face);
					appendFaces.Add(filledHole);
				}
			}

			createdFaces = new List<pb_Face>();
			createdFaces.AddRange(bridgedFaces);
			createdFaces.AddRange(holedFaces);

			pb_FaceRebuildData.Apply(appendFaces, pb, vertices);
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			pb.DeleteFaces(sorted.Keys);
			pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));

			// make sure filled holes are oriented correclty
			// using hashcode here because Contains() doesn't recognize faces for some reason?
			// @todo figure out why...
			// {
			List<pb_WingedEdge> created = pb_WingedEdge.GetWingedEdges(pb, createdFaces);
			
			foreach(pb_WingedEdge f in created)
			{
				if(bridgedFaces.Contains(f.face))
				{
					bridgedFaces.Remove(f.face);
					pb_WingedEdge n = f;

					do
					{
						if(n.opposite != null && holedFaces.Contains(n.opposite.face))
						{
							holedFaces.Remove(n.opposite.face);
							pb_ConformNormals.ConformOppositeNormal(n);
						}

						n = n.next;
					}
					while(n != f);
				}
			}
			// }

			pb.ToMesh();

			return new pb_ActionResult(Status.Success, "Bevel Edges");
 		}

 		private static readonly int[] BRIDGE_INDICES_NRM = new int[] { 2, 1, 0 };

 		private static List<pb_FaceRebuildData> GetBridgeFaces(IList<pb_Vertex> vertices, pb_WingedEdge left, pb_WingedEdge right)
 		{
 			List<pb_FaceRebuildData> faces = new List<pb_FaceRebuildData>();

 			pb_FaceRebuildData rf = new pb_FaceRebuildData();

 			pb_EdgeLookup a = left.edge;
 			pb_EdgeLookup b = right.edge;

 			rf.vertices = new List<pb_Vertex>() 
 			{
 				vertices[a.local.x],
 				vertices[a.local.y],
 				vertices[b.local.y],
 				vertices[b.local.x]
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

 			return faces;
 		}

 		private static void SlideEdge(IList<pb_Vertex> vertices, pb_WingedEdge we, Dictionary<int, pb_Tuple<pb_Face, List<pb_Vertex>>> holes, float amount)
 		{
			we.face.manualUV = true;
			we.face.textureGroup = -1;

			pb_Edge slide_x = GetLeadingEdge(we, we.edge.common.x);
			pb_Edge slide_y = GetLeadingEdge(we, we.edge.common.y);
			
			if(slide_x == null || slide_y == null) 
				return;

			pb_Vertex x = (vertices[slide_x.x] - vertices[slide_x.y]);
			x.Normalize();

			pb_Vertex y = (vertices[slide_y.x] - vertices[slide_y.y]);
			y.Normalize();

			// need the pb_Vertex value to be modified, not reassigned in this array (which += does)
			vertices[we.edge.local.x].Add(x * amount);
			vertices[we.edge.local.y].Add(y * amount);

			pb_Tuple<pb_Face, List<pb_Vertex>> tup;

			if(holes.TryGetValue(we.edge.common.x, out tup))
				tup.Item2.Add(vertices[we.edge.local.x]);
			else
				holes.Add(we.edge.common.x, new pb_Tuple<pb_Face, List<pb_Vertex>>(we.face, new List<pb_Vertex>() { vertices[we.edge.local.x] } ));

			if(holes.TryGetValue(we.edge.common.y, out tup))
				tup.Item2.Add(vertices[we.edge.local.y]);
			else
				holes.Add(we.edge.common.y, new pb_Tuple<pb_Face, List<pb_Vertex>>(we.face, new List<pb_Vertex>() { vertices[we.edge.local.y] } ));
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
			return null;
		}
	}
}
