using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	public static partial class ElementSelection
	{
		/**
		 *	Returns a list of pb_Tuple<pb_Face, pb_Edge> where each face is connected to the passed edge.
		 */
		internal static List<SimpleTuple<Face, Edge>> GetNeighborFaces(ProBuilderMesh pb, Edge edge, Dictionary<int, int> lookup = null)
		{
			if(lookup == null)
				lookup = pb.sharedIndicesInternal.ToDictionary();

			List<SimpleTuple<Face, Edge>> faces = new List<SimpleTuple<Face, Edge>>();

			Edge uni = new Edge(lookup[edge.x], lookup[edge.y]);
			Edge e = new Edge(0,0);

			for(int i = 0; i < pb.facesInternal.Length; i++)
			{
				Edge[] edges = pb.facesInternal[i].edgesInternal;
				for(int n = 0; n < edges.Length; n++)
				{
					e.x = edges[n].x;
					e.y = edges[n].y;

					if( (uni.x == lookup[e.x] && uni.y == lookup[e.y]) ||
						(uni.x == lookup[e.y] && uni.y == lookup[e.x]))
					{
						faces.Add(new SimpleTuple<Face, Edge>(pb.facesInternal[i], edges[n]));
						break;
					}
				}
			}
			return faces;
		}

		/// <summary>
		/// Gets all faces connected to each index taking into account shared vertices.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		/// <param name="lookup"></param>
		/// <returns></returns>
		internal static List<Face> GetNeighborFaces(ProBuilderMesh pb, int[] indices, Dictionary<int, int> lookup)
		{
			List<Face> neighboring = new List<Face>();
			HashSet<int> shared = new HashSet<int>();

			foreach(int tri in indices)
				shared.Add(lookup[tri]);

			for(int i = 0; i < pb.facesInternal.Length; i++)
			{
				int[] dist = pb.facesInternal[i].distinctIndices;

				for(int n = 0; n < dist.Length; n++)
				{
					if(shared.Contains(lookup[dist[n]]))
					{
						neighboring.Add(pb.facesInternal[i]);
						break;
					}
				}
			}

			return neighboring;
		}

		/// <summary>
		/// Returns a unique array of Edges connected to the passed vertex indices.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		internal static Edge[] GetConnectedEdges(ProBuilderMesh pb, int[] indices)
		{
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();

			List<Edge> connectedEdges = new List<Edge>();

			HashSet<int> shared = new HashSet<int>();
			for(int i = 0; i < indices.Length; i++)
				shared.Add(lookup[indices[i]]);

			Edge[] edges = EdgeExtension.AllEdges(pb.facesInternal);
			HashSet<Edge> used = new HashSet<Edge>();

			Edge uni = new Edge(0,0);

			for(int i = 0; i < edges.Length; i++)
			{
				Edge key = new Edge(lookup[edges[i].x], lookup[edges[i].y]);

				if( shared.Contains(key.x) || shared.Contains(key.y) && !used.Contains(uni) )
				{
					connectedEdges.Add(edges[i]);
					used.Add(key);
				}
			}

			return connectedEdges.ToArray();
		}

		/// <summary>
		/// Get all edges that are on the perimeter of this face group selection.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		internal static IEnumerable<Edge> GetPerimeterEdges(ProBuilderMesh pb, IEnumerable<Face> faces)
		{
			return GetPerimeterEdges(pb.sharedIndicesInternal.ToDictionary(), faces);
		}

		/// <summary>
		/// Get all edges that are on the perimeter of this face group selection.
		/// </summary>
		/// <param name="sharedIndicesLookup"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		internal static IEnumerable<Edge> GetPerimeterEdges(Dictionary<int, int> sharedIndicesLookup, IEnumerable<Face> faces)
		{
			List<Edge> faceEdges = faces.SelectMany(x => x.edgesInternal).ToList();	// actual edges
			int edgeCount = faceEdges.Count;

			// translate all face edges to universal edges
			Dictionary<Edge, List<Edge>> dup = new Dictionary<Edge, List<Edge>>();
			List<Edge> list;

			for(int i = 0; i < edgeCount; i++)
			{
				Edge uni = new Edge( sharedIndicesLookup[faceEdges[i].x], sharedIndicesLookup[faceEdges[i].y] );

				if( dup.TryGetValue(uni, out list) )
					list.Add(faceEdges[i]);
				else
					dup.Add(uni, new List<Edge>() { faceEdges[i] });
			}

			return dup.Where(x => x.Value.Count < 2).Select(x => x.Value[0]);
		}

		/// <summary>
		/// Returns the indices of perimeter edges in a given element group.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edges"></param>
		/// <returns></returns>
		internal static int[] GetPerimeterEdges(ProBuilderMesh pb, IList<Edge> edges)
		{
			int edgeCount = edges != null ? edges.Count : 0;

			if(edgeCount == EdgeExtension.AllEdges(pb.facesInternal).Length || edgeCount < 3)
				return new int[] {};

			// Figure out how many connections each edge has to other edges in the selection
			Edge[] universal = EdgeExtension.GetUniversalEdges(edges, pb.sharedIndicesInternal.ToDictionary());
			int[] connections = new int[universal.Length];

			for(int i = 0; i < universal.Length - 1; i++)
			{
				for(int n = i+1; n < universal.Length; n++)
				{
					if( universal[i].x == universal[n].x || universal[i].x == universal[n].y ||
						universal[i].y == universal[n].x || universal[i].y == universal[n].y )
					{
						connections[i]++;
						connections[n]++;
					}
				}
			}

			int min = ProBuilderMath.Min(connections);
			List<int> perimeter = new List<int>();

			for(int i = 0; i < connections.Length; i++)
			{
				if(connections[i] <= min)
					perimeter.Add(i);
			}

			return perimeter.Count != edgeCount ? perimeter.ToArray() : new int[] {};
		}

		/// <summary>
		/// Returns an array of faces where each face has at least one non-shared edge.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		internal static IEnumerable<Face> GetPerimeterFaces(ProBuilderMesh pb, IEnumerable<Face> faces)
		{
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();
			Dictionary<Edge, List<Face>> sharedEdges = new Dictionary<Edge, List<Face>>();

			/**
			 * To be considered a perimeter face, at least one edge must not share
			 * any boundary with another face.
			 */

			foreach(Face face in faces)
			{
				foreach(Edge e in face.edgesInternal)
				{
					Edge edge = new Edge( lookup[e.x], lookup[e.y]);

					if( sharedEdges.ContainsKey(edge) )
						sharedEdges[edge].Add(face);
					else
						sharedEdges.Add(edge, new List<Face>() { face } );
				}
			}

			return sharedEdges.Where(x => x.Value.Count < 2).Select(x => x.Value[0]).Distinct();
		}

		/// <summary>
		/// Returns the indices of perimeter vertices in selection.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		/// <param name="universal_edges_all"></param>
		/// <returns></returns>
		internal static int[] GetPerimeterVertices(ProBuilderMesh pb, int[] indices, Edge[] universal_edges_all)
		{
			int len = indices.Length;
			IntArray[] sharedIndices = pb.sharedIndicesInternal;
			int[] universal = new int[len];

			for(int i = 0; i < len; i++)
				universal[i] = sharedIndices.IndexOf(indices[i]);

			int[] connections = new int[indices.Length];
			for(int i = 0; i < indices.Length - 1; i++)
			{
				for(int n = i+1; n < indices.Length; n++)
				{
					if(universal_edges_all.Contains(universal[i], universal[n]))
					{
						connections[i]++;
						connections[n]++;
					}
				}
			}

			int min = ProBuilderMath.Min(connections);
			List<int> perimeter = new List<int>();
			for(int i = 0; i < len; i++)
			{
				if(connections[i] <= min)
					perimeter.Add(i);
			}

			return perimeter.Count < len ? perimeter.ToArray() : new int[] {};
		}

		static WingedEdge EdgeRingNext(WingedEdge edge)
		{
			if(edge == null)
				return null;

			WingedEdge next = edge.next, prev = edge.previous;
			int i = 0;

			while(next != prev && next != edge)
			{
				next = next.next;

				if(next == prev)
					return null;

				prev = prev.previous;

				i++;
			}

			if(i % 2 == 0 || next == edge)
				next = null;

			return next;
		}

		/// <summary>
		/// Iterates through face edges and builds a list using the opposite edge.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edges"></param>
		/// <returns></returns>
		internal static IEnumerable<Edge> GetEdgeRing(ProBuilderMesh pb, IEnumerable<Edge> edges)
		{
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb);
			List<EdgeLookup> edgeLookup = EdgeLookup.GetEdgeLookup(edges, pb.sharedIndicesInternal.ToDictionary()).ToList();
			edgeLookup = edgeLookup.Distinct().ToList();

			Dictionary<Edge, WingedEdge> wings_dic = new Dictionary<Edge, WingedEdge>();

			for(int i = 0; i < wings.Count; i++)
				if(!wings_dic.ContainsKey(wings[i].edge.common))
					wings_dic.Add(wings[i].edge.common, wings[i]);

			HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

			for(int i = 0, c = edgeLookup.Count; i < c; i++)
			{
				WingedEdge we;

				if(!wings_dic.TryGetValue(edgeLookup[i].common, out we) || used.Contains(we.edge))
					continue;

				WingedEdge cur = we;

				while(cur != null)
				{
					if(!used.Add(cur.edge)) break;
					cur = EdgeRingNext(cur);
					if(cur != null && cur.opposite != null) cur = cur.opposite;
				}

				cur = EdgeRingNext(we.opposite);
				if(cur != null && cur.opposite != null) cur = cur.opposite;

				// run in both directions
				while(cur != null)
				{
					if(!used.Add(cur.edge)) break;
					cur = EdgeRingNext(cur);
					if(cur != null && cur.opposite != null) cur = cur.opposite;
				}
			}

			return used.Select(x => x.local);
		}

		/// <summary>
		/// Attempts to find edges along an Edge loop.
		///
		/// http://wiki.blender.org/index.php/Doc:2.4/Manual/Modeling/Meshes/Selecting/Edges says:
		/// First check to see if the selected element connects to only 3 other edges.
		/// If the edge in question has already been added to the list, the selection ends.
		/// Of the 3 edges that connect to the current edge, the ones that share a face with the current edge are eliminated
		/// and the remaining edge is added to the list and is made the current edge.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edges"></param>
		/// <param name="loop"></param>
		/// <returns></returns>
		internal static bool GetEdgeLoop(ProBuilderMesh pb, IEnumerable<Edge> edges, out Edge[] loop)
		{
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb);
			IEnumerable<EdgeLookup> m_edgeLookup = EdgeLookup.GetEdgeLookup(edges, pb.sharedIndicesInternal.ToDictionary());
			HashSet<EdgeLookup> sources = new HashSet<EdgeLookup>(m_edgeLookup);
			HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

			for(int i = 0; i < wings.Count; i++)
			{
				if(used.Contains(wings[i].edge) || !sources.Contains(wings[i].edge))
					continue;

				bool completeLoop = GetEdgeLoopInternal(wings[i], wings[i].edge.common.y, used);

				// loop didn't close
				if(!completeLoop)
					GetEdgeLoopInternal(wings[i], wings[i].edge.common.x, used);
			}

			loop = used.Select(x => x.local).ToArray();

			return true;
		}

		static bool GetEdgeLoopInternal(WingedEdge start, int startIndex, HashSet<EdgeLookup> used)
		{
			int ind = startIndex;
			WingedEdge cur = start;

			do
			{
				used.Add(cur.edge);

				List<WingedEdge> spokes = GetSpokes(cur, ind, true).DistinctBy(x => x.edge.common).ToList();

				cur = null;

				if(spokes != null && spokes.Count == 4)
				{
					cur = spokes[2];
					ind = cur.edge.common.x == ind ? cur.edge.common.y : cur.edge.common.x;
				}
			} while(cur != null && !used.Contains(cur.edge));

			return cur != null;
		}

		static WingedEdge NextSpoke(WingedEdge wing, int pivot, bool opp)
		{
			if(opp)
				return wing.opposite;
			else
			if(wing.next.edge.common.Contains(pivot))
				return wing.next;
			else
			if(wing.previous.edge.common.Contains(pivot))
				return wing.previous;
			else
				return null;
		}

		/// <summary>
		/// Return all edges connected to @wing with @sharedIndex as the pivot point. The first entry in the list is always the queried wing.
		/// </summary>
		/// <param name="wing"></param>
		/// <param name="sharedIndex"></param>
		/// <param name="allowHoles"></param>
		/// <returns></returns>
		internal static List<WingedEdge> GetSpokes(WingedEdge wing, int sharedIndex, bool allowHoles = false)
		{
			List<WingedEdge> spokes = new List<WingedEdge>();
			WingedEdge cur = wing;
			bool opp = false;

			do
			{
				spokes.Add(cur);
				cur = NextSpoke(cur, sharedIndex, opp);
				opp = !opp;

				// we've looped around as far as it's gon' go
				if( cur != null && cur.edge.common.Equals(wing.edge.common) )
					return spokes;

			} while(cur != null);

			if(!allowHoles)
				return null;

			// if the first loop didn't come back, that means there was a hole in the geo
			// do the loop again using the opposite wing
			cur = wing.opposite;
			opp = false;
			List<WingedEdge> fragment = new List<WingedEdge>();

			// if mesh is non-manifold this situation could arise
			while(cur != null && !cur.edge.common.Equals(wing.edge.common))
			{
				fragment.Add(cur);
				cur = NextSpoke(cur, sharedIndex, opp);
				opp = !opp;
			}

			fragment.Reverse();
			spokes.AddRange(fragment);

			return spokes;
		}
	}
}
