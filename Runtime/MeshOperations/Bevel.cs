using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using System;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for beveling edges.
	/// </summary>
	public static class Bevel
	{
		/// <summary>
		/// Apply a bevel to a set of edges.
		/// </summary>
		/// <param name="mesh">Target mesh.</param>
		/// <param name="edges">A set of edges to apply bevelling to.</param>
		/// <param name="amount">A value from 0 (bevel not at all) to 1 (bevel entire face).</param>
		/// <returns>The new faces created to form the bevel.</returns>
		public static List<Face> BevelEdges(ProBuilderMesh mesh, IList<Edge> edges, float amount)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			Dictionary<int, int> lookup = mesh.sharedIndicesInternal.ToDictionary();
			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(mesh));
			List<EdgeLookup> m_edges = EdgeLookup.GetEdgeLookup(edges, lookup).Distinct().ToList();
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);
			List<FaceRebuildData> appendFaces = new List<FaceRebuildData>();

			Dictionary<Face, List<int>> ignore = new Dictionary<Face, List<int>>();
			HashSet<int> slide = new HashSet<int>();
			int beveled = 0;

			Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>> holes = new Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>>();

			// test every edge that will be moved along to make sure the bevel distance is appropriate.  if it's not, adjust the max bevel amount
			// to suit.
			Dictionary<int, List<WingedEdge>> spokes = WingedEdge.GetSpokes(wings);
			HashSet<int> tested_common = new HashSet<int>();

			foreach(EdgeLookup e in m_edges)
			{
				if(tested_common.Add(e.common.x))
				{
					foreach(WingedEdge w in spokes[e.common.x])
					{
						Edge le = w.edge.local;
						amount = Mathf.Min( Vector3.Distance(vertices[le.x].position, vertices[le.y].position) - .001f, amount );
					}
				}

				if(tested_common.Add(e.common.y))
				{
					foreach(WingedEdge w in spokes[e.common.y])
					{
						Edge le = w.edge.local;
						amount = Mathf.Min( Vector3.Distance(vertices[le.x].position, vertices[le.y].position) - .001f, amount );
					}
				}
			}

            if (amount < .001f)
            {
                Log.Info("Bevel Distance > Available Surface");
                return null;
            }

			// iterate selected edges and move each leading edge back along it's direction
			// storing information about adjacent faces in the process
			foreach(EdgeLookup lup in m_edges)
			{
				WingedEdge we = wings.FirstOrDefault(x => x.edge.Equals(lup));

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
				Log.Info("Cannot Bevel Open Edges");
				return null;
			}

			// grab the "createdFaces" array now so that the selection returned is just the bridged faces
			// then add holes later
			var createdFaces = new List<Face>(appendFaces.Select(x => x.face));

			Dictionary<Face, List<SimpleTuple<WingedEdge, int>>> sorted = new Dictionary<Face, List<SimpleTuple<WingedEdge, int>>>();

			// sort the adjacent but affected faces into winged edge groups where each group contains a set of
			// unique winged edges pointing to the same face
			foreach(int c in slide)
			{
				IEnumerable<WingedEdge> matches = wings.Where(x => x.edge.common.Contains(c) && !(ignore.ContainsKey(x.face) && ignore[x.face].Contains(c)));

				HashSet<Face> used = new HashSet<Face>();

				foreach(WingedEdge match in matches)
				{
					if(!used.Add(match.face))
						continue;

					sorted.AddOrAppend(match.face, new SimpleTuple<WingedEdge, int>(match, c));
				}
			}

			// now go through those sorted faces and apply the vertex exploding, keeping track of any holes created
			foreach(KeyValuePair<Face, List<SimpleTuple<WingedEdge, int>>> kvp in sorted)
			{
				// common index & list of vertices it was split into
				Dictionary<int, List<int>> appendedVertices;

				FaceRebuildData f = VertexEditing.ExplodeVertex(vertices, kvp.Value, amount, out appendedVertices);

				if(f == null)
					continue;

				appendFaces.Add(f);

				foreach(var apv in appendedVertices)
				{
					// organize holes by new face so that later we can compare the winding of the new face to the hole face
					// holes are sorted by key: common index value: face, vertex list
					holes.AddOrAppend(apv.Key, new SimpleTuple<FaceRebuildData, List<int>>(f, apv.Value));
				}
			}

			FaceRebuildData.Apply(appendFaces, mesh, vertices);
			int removed = mesh.DeleteFaces(sorted.Keys).Length;
			mesh.SetSharedIndexesUV(new IntArray[0]);
			mesh.SetSharedIndexes(IntArrayUtility.GetSharedIndexesWithPositions(mesh.positionsInternal));

			// @todo don't rebuild sharedindices, keep 'em cached
			IntArray[] sharedIndices = mesh.sharedIndicesInternal;
			lookup = sharedIndices.ToDictionary();
			List<HashSet<int>> holesCommonIndices = new List<HashSet<int>>();

			// offset the indices of holes and cull any potential holes that are less than 3 indices (not a hole :)
			foreach(KeyValuePair<int, List<SimpleTuple<FaceRebuildData, List<int>>>> hole in holes)
			{
				// less than 3 indices in hole path; ain't a hole
				if(hole.Value.Sum(x => x.item2.Count) < 3)
					continue;

				HashSet<int> holeCommon = new HashSet<int>();

				foreach(SimpleTuple<FaceRebuildData, List<int>> path in hole.Value)
				{
					int offset = path.item1.Offset() - removed;

					for(int i = 0; i < path.item2.Count; i++)
						holeCommon.Add(lookup[path.item2[i] + offset]);
				}

				holesCommonIndices.Add(holeCommon);
			}

			List<WingedEdge> modified = WingedEdge.GetWingedEdges(mesh, appendFaces.Select(x => x.face));

			// now go through the holes and create faces for them
			vertices = new List<Vertex>( Vertex.GetVertices(mesh) );

			List<FaceRebuildData> holeFaces = new List<FaceRebuildData>();

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
					List<Vertex> v = new List<Vertex>( Vertex.GetVertices(mesh, h.Select(x => sharedIndices[x][0]).ToList()) );
					holeFaces.Add(AppendElements.FaceWithVertices(v));
				}
				// if this hole has > 3 indices, it needs a tent pole triangulation, which requires sorting into the perimeter order
				else
				{
					List<int> holePath = WingedEdge.SortCommonIndexesByAdjacency(modified, h);
					List<Vertex> v = new List<Vertex>( Vertex.GetVertices(mesh, holePath.Select(x => sharedIndices[x][0]).ToList()) );
					holeFaces.AddRange( AppendElements.TentCapWithVertices(v) );
				}
			}

			FaceRebuildData.Apply(holeFaces, mesh, vertices);
			mesh.SetSharedIndexes(IntArrayUtility.GetSharedIndexesWithPositions(mesh.positionsInternal));

			// go through new faces and conform hole normals
			// get a hash of just the adjacent and bridge faces
			// HashSet<pb_Face> adjacent = new HashSet<pb_Face>(appendFaces.Select(x => x.face));
			// and also just the filled holes
			HashSet<Face> newHoles = new HashSet<Face>(holeFaces.Select(x => x.face));
			// now append filled holes to the full list of added faces
			appendFaces.AddRange(holeFaces);

			List<WingedEdge> allNewFaceEdges = WingedEdge.GetWingedEdges(mesh, appendFaces.Select(x => x.face));

			for(int i = 0; i < allNewFaceEdges.Count && newHoles.Count > 0; i++)
			{
				WingedEdge wing = allNewFaceEdges[i];

				if(newHoles.Contains(wing.face))
				{
					newHoles.Remove(wing.face);

					// find first edge whose opposite face isn't a filled hole* then
					// conform normal by that.
					// *or is a filled hole but has already been conformed
					foreach(WingedEdge w in wing)
					{
						if(!newHoles.Contains(w.opposite.face))
						{
							w.face.material = w.opposite.face.material;
							w.face.uv = new AutoUnwrapSettings(w.opposite.face.uv);
							SurfaceTopology.ConformOppositeNormal(w.opposite);
							break;
						}
					}
				}
			}

			mesh.ToMesh();

            return createdFaces;
 		}

 		static readonly int[] BRIDGE_INDICES_NRM = new int[] { 2, 1, 0 };

 		static List<FaceRebuildData> GetBridgeFaces(
 			IList<Vertex> vertices,
 			WingedEdge left,
 			WingedEdge right,
 			Dictionary<int, List<SimpleTuple<FaceRebuildData, List<int>>>> holes)
 		{
 			List<FaceRebuildData> faces = new List<FaceRebuildData>();

 			FaceRebuildData rf = new FaceRebuildData();

 			EdgeLookup a = left.edge;
 			EdgeLookup b = right.edge;

 			rf.vertices = new List<Vertex>()
 			{
 				vertices[a.local.x],
 				vertices[a.local.y],
 				vertices[a.common.x == b.common.x ? b.local.x : b.local.y],
 				vertices[a.common.x == b.common.x ? b.local.y : b.local.x]
 			};

 			Vector3 an = Math.Normal(vertices, left.face.indices);
 			Vector3 bn = Math.Normal(rf.vertices, BRIDGE_INDICES_NRM);

 			int[] triangles = new int[] { 2, 1, 0, 2, 3, 1 };

 			if( Vector3.Dot(an, bn) < 0f)
 				System.Array.Reverse(triangles);

 			rf.face = new Face(
 				triangles,
 				left.face.material,
 				new AutoUnwrapSettings(),
 				-1,
 				-1,
 				-1,
 				false);

 			faces.Add(rf);

 			holes.AddOrAppend(a.common.x, new SimpleTuple<FaceRebuildData, List<int>>(rf, new List<int>() { 0, 2 }));
 			holes.AddOrAppend(a.common.y, new SimpleTuple<FaceRebuildData, List<int>>(rf, new List<int>() { 1, 3 }));

 			return faces;
 		}

 		static void SlideEdge(IList<Vertex> vertices, WingedEdge we, float amount)
 		{
			we.face.manualUV = true;
			we.face.textureGroup = -1;

			Edge slide_x = GetLeadingEdge(we, we.edge.common.x);
			Edge slide_y = GetLeadingEdge(we, we.edge.common.y);

			if(!slide_x.IsValid() || !slide_y.IsValid())
				return;

			Vertex x = (vertices[slide_x.x] - vertices[slide_x.y]);
			x.Normalize();

			Vertex y = (vertices[slide_y.x] - vertices[slide_y.y]);
			y.Normalize();

			// need the pb_Vertex value to be modified, not reassigned in this array (which += does)
			vertices[we.edge.local.x].Add(x * amount);
			vertices[we.edge.local.y].Add(y * amount);
		}

		static Edge GetLeadingEdge(WingedEdge wing, int common)
		{
			if(wing.previous.edge.common.x == common)
				return new Edge(wing.previous.edge.local.y, wing.previous.edge.local.x);
			else if(wing.previous.edge.common.y == common)
				return new Edge(wing.previous.edge.local.x, wing.previous.edge.local.y);
			else if(wing.next.edge.common.x == common)
				return new Edge(wing.next.edge.local.y, wing.next.edge.local.x);
			else if(wing.next.edge.common.y == common)
				return new Edge(wing.next.edge.local.x, wing.next.edge.local.y);

			return Edge.Empty;
		}
	}
}
