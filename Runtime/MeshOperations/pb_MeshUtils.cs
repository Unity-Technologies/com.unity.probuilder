using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using pb_Object = UnityEngine.ProBuilder.pb_Object;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Used to query pb_Objects for more detailed information than what would belong in the pbObejct class
	/// <remarks>These functions are generally superceded by other more specific classes (ex, pb_FaceLoop), or very slow and in need of a refactor. Prefer not to use this class where possible.</remarks>
	/// </summary>
	static class pb_MeshUtils
	{
		/// <summary>
		/// Returns all faces that share an edge with originFace.  If calling multiple times, use the variation that accepts a dictionary lookup to  save to the cost of generating it each call.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="originFace"></param>
		/// <param name="lookup"></param>
		/// <param name="mask"></param>
		/// <returns></returns>
		public static List<Face> GetNeighborFaces(pb_Object pb, Face originFace, Dictionary<int, int> lookup = null, IEnumerable<Face> mask = null)
		{
			if(lookup == null)
				lookup = pb.sharedIndices.ToDictionary();

			List<Face> faces = new List<Face>();

			HashSet<Edge> sharedEdges = new HashSet<Edge>();

			for(int i = 0; i < originFace.edges.Length; i++)
			{
				sharedEdges.Add(new Edge(lookup[originFace.edges[i].x], lookup[originFace.edges[i].y]));
			}

			Edge edge_s = new Edge(-1,-1);

			for(int i = 0; i < pb.faces.Length; i++)
			{
				foreach(Edge edge in pb.faces[i].edges)
				{
					edge_s.x = lookup[edge.x];
					edge_s.y = lookup[edge.y];

					bool contains = sharedEdges.Contains(edge_s);

					if( contains && (mask == null || !mask.Contains(pb.faces[i])) )
					{

						faces.Add(pb.faces[i]);
						break;
					}
				}
			}

			return faces;
		}

		/**
		 * Generates a Dictionary where each face is a key, and its value is a list of all faces adjacent.
		 */
		public static Dictionary<Face, List<Face>> GenerateNeighborLookup(pb_Object pb, IList<Face> InFaces)
		{
			Dictionary<int, int> sharedLookup = pb.sharedIndices.ToDictionary();
			Dictionary<Face, List<Face>> faceLookup = new Dictionary<Face, List<Face>>();

			IList<Face> faces = InFaces;
			int faceCount = faces.Count();
			List<Face> list;

			HashSet<Edge>[] universal = new HashSet<Edge>[faceCount];

			for(int i = 0; i < faceCount; i++)
				universal[i] = new HashSet<Edge>(EdgeExtension.GetUniversalEdges(faces[i].edges, sharedLookup));

			for(int i = 0; i < faceCount-1; i++)
			{
				if( !faceLookup.ContainsKey(faces[i]) )
					faceLookup.Add(faces[i], new List<Face>());

				for(int n = i+1; n < faceCount; n++)
				{
					bool overlaps = universal[i].Overlaps(universal[n]);

					if( overlaps )
					{
						faceLookup[faces[i]].Add(faces[n]);

						if( faceLookup.TryGetValue(faces[n], out list) )
							list.Add(faces[i]);
						else
							faceLookup.Add(faces[n], new List<Face>() {faces[i]});
					}
				}
			}

			return faceLookup;
		}

		/**
		 * \brief Returns faces that share an edge with any of @c selFcaes.
		 */
		public static Face[] GetNeighborFaces(pb_Object pb, Dictionary<int, int> sharedIndicesLookup, Face[] selFaces)
		{
			List<Face> perimeterFaces = new List<Face>();

			Edge[] perimeterEdges = pb_MeshUtils.GetPerimeterEdges(sharedIndicesLookup, selFaces).ToArray();
			Edge[] universalEdges = new Edge[perimeterEdges.Length];

			for(int i = 0; i < perimeterEdges.Length; i++)
				universalEdges[i] = new Edge( sharedIndicesLookup[perimeterEdges[i].x],
												 sharedIndicesLookup[perimeterEdges[i].y]);

			Edge edge_u = new Edge(-1, -1);

			HashSet<Face> skip = new HashSet<Face>(selFaces);

			foreach(Face face in pb.faces)
			{
				if(skip.Contains(face))
				{
					skip.Remove(face);
					continue;
				}

				foreach(Edge edge in face.edges)
				{
					edge_u.x = sharedIndicesLookup[edge.x];
					edge_u.y = sharedIndicesLookup[edge.y];

					if(Enumerable.Contains(universalEdges, edge_u))
					{
						perimeterFaces.Add(face);
						break;
					}
				}
			}

			return perimeterFaces.ToArray();
		}

		/**
		 *	Returns a list of pb_Tuple<pb_Face, pb_Edge> where each face is connected to the passed edge.
		 */
		public static List<pb_Tuple<Face, Edge>> GetNeighborFaces(pb_Object pb, Edge edge, Dictionary<int, int> lookup = null)
		{
			if(lookup == null)
				lookup = pb.sharedIndices.ToDictionary();

			List<pb_Tuple<Face, Edge>> faces = new List<pb_Tuple<Face, Edge>>();

			Edge uni = new Edge(lookup[edge.x], lookup[edge.y]);
			Edge e = new Edge(0,0);

			for(int i = 0; i < pb.faces.Length; i++)
			{
				Edge[] edges = pb.faces[i].edges;
				for(int n = 0; n < edges.Length; n++)
				{
					e.x = edges[n].x;
					e.y = edges[n].y;

					if( (uni.x == lookup[e.x] && uni.y == lookup[e.y]) ||
						(uni.x == lookup[e.y] && uni.y == lookup[e.x]))
					{
						faces.Add(new pb_Tuple<Face, Edge>(pb.faces[i], edges[n]));
						break;
					}
				}
			}
			return faces;
		}

		// todo update this and ^ this with faster variation below
		public static Face[] GetNeighborFaces(pb_Object pb, Edge[] edges)
		{
			List<Face> faces = new List<Face>();
			Dictionary<int, int> sharedIndices = pb.sharedIndices.ToDictionary();
			foreach(Face f in pb.faces)
			{
				foreach(Edge e in edges)
					if(f.edges.IndexOf(e, sharedIndices) > -1)
						faces.Add(f);
			}

			return faces.Distinct().ToArray();
		}

		static List<Face>[][] GetNeighborFacesJagged(pb_Object pb, Edge[][] selEdges)
		{
			int len = selEdges.Length;

			List<Face>[][] faces = new List<Face>[len][];
			for(int j = 0; j < len; j++)
			{
				faces[j] = new List<Face>[selEdges[j].Length];
				for(int i = 0; i < selEdges[j].Length; i++)
					faces[j][i] = new List<Face>();
			}

			IntArray[] sharedIndices = pb.sharedIndices;

			Edge[][] sharedEdges = new Edge[len][];
			for(int i = 0; i < len; i++)
				sharedEdges[i] = EdgeExtension.GetUniversalEdges(selEdges[i], sharedIndices).Distinct().ToArray();

			for(int i = 0; i < pb.faces.Length; i++)
			{
				Edge[] faceEdges = EdgeExtension.GetUniversalEdges(pb.faces[i].edges, sharedIndices).Distinct().ToArray();

				for(int j = 0; j < len; j++)
				{
					int ind = -1;
					for(int t = 0; t < sharedEdges[j].Length; t++)
					{
						if(Enumerable.Contains(faceEdges, sharedEdges[j][t]))
						{
							ind = t;
							break;
						}
					}

					if(ind > -1)
						faces[j][ind].Add(pb.faces[i]);
				}
			}

			return faces;
		}

		/**
		 *	Returns all faces connected to the passed vertex index.
		 */
		public static List<Face> GetNeighborFaces(pb_Object pb, int index)
		{
			List<Face> faces = new List<Face>();
			IntArray[] sharedIndices = pb.sharedIndices;
			int i = sharedIndices.IndexOf(index);

			foreach(Face f in pb.faces)
			{
				if(f.distinctIndices.ContainsMatch((int[])sharedIndices[i]))
					faces.Add(f);
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
		public static List<Face> GetNeighborFaces(pb_Object pb, int[] indices, Dictionary<int, int> lookup)
		{
			List<Face> neighboring = new List<Face>();
			HashSet<int> shared = new HashSet<int>();

			foreach(int tri in indices)
				shared.Add(lookup[tri]);

			for(int i = 0; i < pb.faces.Length; i++)
			{
				int[] dist = pb.faces[i].distinctIndices;

				for(int n = 0; n < dist.Length; n++)
				{
					if( shared.Contains(lookup[dist[n]]))
					{
						neighboring.Add(pb.faces[i]);
						break;
					}
				}
			}

			return neighboring;
		}

		/**
		 * Returns a unique array of Edges connected to the passed vertex indices.
		 */
		public static Edge[] GetConnectedEdges(pb_Object pb, int[] indices)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			List<Edge> connectedEdges = new List<Edge>();

			HashSet<int> shared = new HashSet<int>();
			for(int i = 0; i < indices.Length; i++)
				shared.Add(lookup[indices[i]]);

			Edge[] edges = EdgeExtension.AllEdges(pb.faces);
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

		/**
		 * Get all edges that are on the perimeter of this face group selection.
		 */
		public static IEnumerable<Edge> GetPerimeterEdges(pb_Object pb, IEnumerable<Face> faces)
		{
			return GetPerimeterEdges(pb.sharedIndices.ToDictionary(), faces);
		}

		/**
		 * Get all edges that are on the perimeter of this face group selection.
		 */
		public static IEnumerable<Edge> GetPerimeterEdges(Dictionary<int, int> sharedIndicesLookup, IEnumerable<Face> faces)
		{
			List<Edge> faceEdges = faces.SelectMany(x => x.edges).ToList();	// actual edges
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

		/**
		 * Returns the indices of perimeter edges in a given element group.
		 * todo - to speed this up, we could just use the distinct in GetUniversalEdges() - but that would
		 * break this method's usefullness in other situations.
		 */
		public static int[] GetPerimeterEdges(pb_Object pb, Edge[] edges)
		{
			if(edges.Length == EdgeExtension.AllEdges(pb.faces).Length || edges.Length < 3)
				return new int[] {};

			// Figure out how many connections each edge has to other edges in the selection
			Edge[] universal = EdgeExtension.GetUniversalEdges(edges, pb.sharedIndices.ToDictionary());
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

			return perimeter.Count != edges.Length ? perimeter.ToArray() : new int[] {};
		}

		/**
		 * Returns an array of faces where each face has at least one non-shared edge.
		 */
		public static IEnumerable<Face> GetPerimeterFaces(pb_Object pb, IEnumerable<Face> faces)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<Edge, List<Face>> sharedEdges = new Dictionary<Edge, List<Face>>();

			/**
			 * To be considered a perimeter face, at least one edge must not share
			 * any boundary with another face.
			 */

			foreach(Face face in faces)
			{
				foreach(Edge e in face.edges)
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

		/**
		 * Returns the indices of perimeter vertices in selection.
		 */
		public static int[] GetPerimeterVertices(pb_Object pb, int[] indices, Edge[] universal_edges_all)
		{
			int len = indices.Length;
			IntArray[] sharedIndices = pb.sharedIndices;
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

		private static pb_WingedEdge EdgeRingNext(pb_WingedEdge edge)
		{
			if(edge == null)
				return null;

			pb_WingedEdge next = edge.next, prev = edge.previous;
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

		/**
		 * Iterates through face edges and builds a list using the opposite edge.
		 */
		public static IEnumerable<Edge> GetEdgeRing(pb_Object pb, Edge[] edges)
		{
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
			List<EdgeLookup> edge_lookup = EdgeLookup.GetEdgeLookup(edges, pb.sharedIndices.ToDictionary()).ToList();
			edge_lookup.Distinct();

			Dictionary<Edge, pb_WingedEdge> wings_dic = new Dictionary<Edge, pb_WingedEdge>();

			for(int i = 0; i < wings.Count; i++)
				if(!wings_dic.ContainsKey(wings[i].edge.common))
					wings_dic.Add(wings[i].edge.common, wings[i]);

			HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

			for(int i = 0; i < edge_lookup.Count; i++)
			{
				pb_WingedEdge we;

				if(!wings_dic.TryGetValue(edge_lookup[i].common, out we) || used.Contains(we.edge))
					continue;

				pb_WingedEdge cur = we;

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

		/**
		 * Attempts to find edges along an Edge loop.
		 *
		 * 	http://wiki.blender.org/index.php/Doc:2.4/Manual/Modeling/Meshes/Selecting/Edges says:
		 * 	First check to see if the selected element connects to only 3 other edges.
		 * 	If the edge in question has already been added to the list, the selection ends.
		 * 	Of the 3 edges that connect to the current edge, the ones that share a face with the current edge are eliminated and the remaining edge is added to the list and is made the current edge.
		 */
		public static bool GetEdgeLoop(pb_Object pb, Edge[] edges, out Edge[] loop)
		{
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
			IEnumerable<EdgeLookup> m_edgeLookup = EdgeLookup.GetEdgeLookup(edges, pb.sharedIndices.ToDictionary());
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

		private static bool GetEdgeLoopInternal(pb_WingedEdge start, int startIndex, HashSet<EdgeLookup> used)
		{
			int ind = startIndex;
			pb_WingedEdge cur = start;

			do
			{
				used.Add(cur.edge);

				List<pb_WingedEdge> spokes = GetSpokes(cur, ind, true).DistinctBy(x => x.edge.common).ToList();

				cur = null;

				if(spokes != null && spokes.Count == 4)
				{
					cur = spokes[2];
					ind = cur.edge.common.x == ind ? cur.edge.common.y : cur.edge.common.x;
				}
			} while(cur != null && !used.Contains(cur.edge));

			return cur != null;
		}

		private static pb_WingedEdge NextSpoke(pb_WingedEdge wing, int pivot, bool opp)
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

		/**
		 * Return all edges connected to @wing with @sharedIndex as the pivot point.  The first entry in the list is always
		 * the queried wing.
		 */
		public static List<pb_WingedEdge> GetSpokes(pb_WingedEdge wing, int sharedIndex, bool allowHoles = false)
		{
			List<pb_WingedEdge> spokes = new List<pb_WingedEdge>();
			pb_WingedEdge cur = wing;
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
			List<pb_WingedEdge> fragment = new List<pb_WingedEdge>();

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
