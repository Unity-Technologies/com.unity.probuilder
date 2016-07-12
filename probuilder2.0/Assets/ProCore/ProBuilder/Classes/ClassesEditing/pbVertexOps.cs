using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KDTree;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.MeshOperations
{
	public static class pbVertexOps
	{
#region Merge / Split

		/**
		 *	\brief Collapses all passed indices to a single shared index.  Retains vertex normals.
		 *	
		 */
		public static bool MergeVertices(this pb_Object pb, int[] indices, out int collapsedIndex, bool collapseToFirst = false)
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);

			pb_Vertex cen = collapseToFirst ? vertices[indices[0]] : pb_Vertex.Average(vertices, indices);

			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_IntArray[] sharedIndicesUV = pb.sharedIndicesUV;

			int newIndex = pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, indices);
			pb_IntArrayUtility.MergeSharedIndices(ref sharedIndicesUV, indices);

			pb.SetSharedIndices(sharedIndices);
			pb.SetSharedIndicesUV(sharedIndicesUV);

			pb.SetSharedVertexValues(newIndex, cen);

			int[] mergedSharedIndex = pb.GetSharedIndices()[newIndex].array;
			
			int[] removedIndices;
			pb.RemoveDegenerateTriangles(out removedIndices);

			// get a non-deleted index to work with
			int ind = -1;
			for(int i = 0; i < mergedSharedIndex.Length; i++)
				if(!removedIndices.Contains(mergedSharedIndex[i]))
					ind = mergedSharedIndex[i];

			int t = ind;
			for(int i = 0; i < removedIndices.Length; i++)
				if(ind > removedIndices[i])	
					t--;

			if(t > -1)
			{
				collapsedIndex = t;
				return true;
			}
			else
			{
				collapsedIndex = -1;
				return false;
			}
		}

		/**
		 *	Similar to Merge vertices, expect that this method only collapses vertices within
		 *	a specified distance of one another (typically epsilon).  Returns true if any action
		 * 	was taken, false otherwise.  Outputs indices that have been welded in the @welds var.
		 */
		public static pb_ActionResult WeldVertices(this pb_Object pb, int[] indices, float delta, out int[] welds)
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);

			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();
			HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(lookup, indices);

			KDTree<int> tree = new KDTree<int>(3, 48);

			foreach(int i in common)
			{
				Vector3 v = vertices[sharedIndices[i][0]].position;
				tree.AddPoint( new double[] { v.x, v.y, v.z }, i);
			}

			double[] point = new double[3] { 0, 0, 0 };
			Dictionary<int, int> remapped = new Dictionary<int, int>();
			Dictionary<int, Vector3> averages = new Dictionary<int, Vector3>();
			int index = sharedIndices.Length;

			foreach(int i in common)
			{
				if(remapped.ContainsKey(i))
					continue;

				Vector3 v = vertices[sharedIndices[i][0]].position;
				point[0] = v.x;
				point[1] = v.y;
				point[2] = v.z;

				NearestNeighbour<int> neighborIterator = tree.NearestNeighbors( point, 64, delta );

				Vector3 avg = Vector3.zero;
				int count = 0;

				while(neighborIterator.MoveNext())
				{
					int c = neighborIterator.Current;
					avg += vertices[sharedIndices[c][0]].position;
					remapped.Add(c, index);
					count++;
				}

				avg /= count;
				averages.Add(index, avg);

				index++;
			}

			welds = new int[remapped.Count];
			int n = 0;

			foreach(var kvp in remapped)
			{
				int[] tris = sharedIndices[kvp.Key];
				
				welds[n++] = tris[0];

				for(int i = 0; i < tris.Length; i++)
				{
					lookup[tris[i]] = kvp.Value;
					vertices[tris[i]].position = averages[kvp.Value];
				}
			}

			pb.SetSharedIndices(lookup);
			pb.SetVertices(vertices);
			pb.ToMesh();

			return new pb_ActionResult(Status.Success, "Weld Vertices");
		}
		
		/**
		 * Creates separate entries in sharedIndices cache for all passed indices, and all indices in the shared array they belong to.
		 */
		public static bool SplitCommonVertices(this pb_Object pb, int[] indices)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			pb_IntArray[] sharedIndices = pb.sharedIndices;

			List<int> usedIndex = new List<int>();
			List<int> splits = new List<int>();

			for(int i = 0; i < indices.Length; i++)
			{
				int universal = lookup[indices[i]];

				if(!usedIndex.Contains(universal))
				{
					usedIndex.Add(universal);
					splits.AddRange(sharedIndices[universal].array);
				}
			}

			pb_IntArrayUtility.RemoveValues(ref sharedIndices, splits.ToArray());

			foreach(int i in splits)
				pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, i);

			pb.SetSharedIndices(sharedIndices);

			return true;
		}

		/**
		 *	Split individual indices from their common groups.
		 */
		public static void SplitVertices(this pb_Object pb, pb_Edge edge)
		{
			SplitVertices(pb, new int[] { edge.x, edge.y });
		}

		public static void SplitVertices(this pb_Object pb, IEnumerable<int> indices)
		{
			// ToDictionary always sets the universal indices in ascending order from 0+.
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			int max = lookup.Count();
			foreach(int i in indices)
				lookup[i] = ++max;
			pb.SetSharedIndices(lookup);
		}
#endregion

#region Add / Subtract

	/**
	 *	Given a face and a point, this will add a vertex to the pb_Object and retriangulate the face.
	 */
	public static bool AppendVerticesToFace(this pb_Object pb, pb_Face face, Vector3[] points, Color[] addColors, out pb_Face newFace)
	{
		if(!face.IsValid())
		{
			newFace = face;
			return false;
		}

		List<pb_Vertex> vertices = pb_Vertex.GetVertices(pb).ToList();
		List<pb_Face> faces = new List<pb_Face>( pb.faces );
		Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
		Dictionary<int, int> lookupUV = pb.sharedIndicesUV == null ? null : pb.sharedIndicesUV.ToDictionary();

		List<pb_Edge> wound = pb_WingedEdge.SortEdgesByAdjacency(face);

		List<pb_Vertex> n_vertices 	= new List<pb_Vertex>();
		List<int> n_shared 			= new List<int>();
		List<int> n_sharedUV 		= new List<int>();

		for(int i = 0; i < wound.Count; i++)
		{
			n_vertices.Add(vertices[wound[i].x]);
			n_shared.Add(lookup[wound[i].x]);

			if(lookupUV != null)
			{
				int uv;

				if(lookupUV.TryGetValue(wound[i].x, out uv))
					n_sharedUV.Add(uv);
				else
					n_sharedUV.Add(-1);
			}
		}

		List<int> triangles;

		try
		{
			pb_Triangulation.TriangulateVertices(n_vertices, out triangles, false);
		}
		catch
		{
			Debug.Log("Failed triangulating face after appending vertices.");
			newFace = null;
			return false;
		}

		pb_FaceRebuildData data = new pb_FaceRebuildData();

		data.face = new pb_Face(triangles.ToArray(), face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
		data.vertices 			= n_vertices;
		data.sharedIndices 		= n_shared;
		data.sharedIndicesUV 	= n_sharedUV;

		pb_FaceRebuildData.Apply(	new List<pb_FaceRebuildData>() { data },
									vertices,
									faces,
									lookup,
									lookupUV);

		newFace = data.face;

		pb.SetVertices(vertices);
		pb.SetFaces(faces.ToArray());
		pb.SetSharedIndices(lookup);
		pb.SetSharedIndicesUV(lookupUV);

		// check old normal and make sure this new face is pointing the same direction
		Vector3 oldNrm = pb_Math.Normal(pb, face);
		Vector3 newNrm = pb_Math.Normal(pb, newFace);

		if( Vector3.Dot(oldNrm, newNrm) < 0 )
			newFace.ReverseIndices();

		pb.DeleteFace(face);

		return true;
	}

	/**
	 *	Appends count number of vertices to an edge.
	 */
	public static pb_ActionResult AppendVerticesToEdge(this pb_Object pb, pb_Edge edge, int count, out List<pb_Edge> newEdges)
	{
		return AppendVerticesToEdge(pb, new pb_Edge[] { edge }, count, out newEdges);
	}

	public static pb_ActionResult AppendVerticesToEdge(this pb_Object pb, IList<pb_Edge> edges, int count, out List<pb_Edge> newEdges)
	{
		newEdges = new List<pb_Edge>();

		if(count < 1 || count > 512)
			return new pb_ActionResult(Status.Failure, "New edge vertex count is less than 1 or greater than 512.");

		List<pb_Vertex> vertices 		= new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
		Dictionary<int, int> lookup 	= pb.sharedIndices.ToDictionary();
		Dictionary<int, int> lookupUV	= pb.sharedIndicesUV.ToDictionary();
		List<int> indicesToDelete		= new List<int>();
		pb_Edge[] commonEdges	 		= pb_Edge.GetUniversalEdges(edges.ToArray(), lookup);
		List<pb_Edge> distinctEdges 	= commonEdges.Distinct().ToList();

		Dictionary<pb_Face, pb_FaceRebuildData> modifiedFaces = new Dictionary<pb_Face, pb_FaceRebuildData>();

		int originalSharedIndicesCount = lookup.Count();
		int sharedIndicesCount = originalSharedIndicesCount;

		foreach(pb_Edge edge in distinctEdges)
		{
			pb_Edge localEdge = pb_Edge.GetLocalEdgeFast(edge, pb.sharedIndices);

			// Generate the new vertices that will be inserted on this edge
			List<pb_Vertex> verticesToAppend = new List<pb_Vertex>(count);

			for(int i = 0; i < count; i++)
				verticesToAppend.Add(pb_Vertex.Mix(vertices[localEdge.x], vertices[localEdge.y], (i+1)/((float)count + 1)));

			List<pb_Tuple<pb_Face, pb_Edge>> adjacentFaces = pbMeshUtils.GetNeighborFaces(pb, localEdge);

			// foreach face attached to common edge, append vertices
			foreach(pb_Tuple<pb_Face, pb_Edge> tup in adjacentFaces)
			{
				pb_Face face = tup.Item1;

				pb_FaceRebuildData data;

				if( !modifiedFaces.TryGetValue(face, out data) )
				{
					data = new pb_FaceRebuildData();
					data.face = new pb_Face(null, face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
					data.vertices = new List<pb_Vertex>(pbUtil.ValuesWithIndices(vertices, face.distinctIndices));
					data.sharedIndices = new List<int>();
					data.sharedIndicesUV = new List<int>();

					foreach(int i in face.distinctIndices)
					{
						int shared;

						if(lookup.TryGetValue(i, out shared)) 
							data.sharedIndices.Add(shared);
						
						if(lookupUV.TryGetValue(i, out shared)) 
							data.sharedIndicesUV.Add(shared);
					}

					indicesToDelete.AddRange(face.distinctIndices);

					modifiedFaces.Add(face, data);
				}

				data.vertices.AddRange(verticesToAppend);

				for(int i = 0; i < count; i++)
				{
					data.sharedIndices.Add(sharedIndicesCount + i);		
					data.sharedIndicesUV.Add(-1);				
				}
			}

			sharedIndicesCount += count;
		}

		// now apply the changes
		List<pb_Face> dic_face = modifiedFaces.Keys.ToList();
		List<pb_FaceRebuildData> dic_data = modifiedFaces.Values.ToList();
		List<pb_EdgeLookup> appendedEdges = new List<pb_EdgeLookup>();

		for(int i = 0; i < dic_face.Count; i++)
		{
			pb_Face face = dic_face[i];
			pb_FaceRebuildData data = dic_data[i];

			Vector3 nrm = pb_Math.Normal(pb, face);
			Vector2[] projection = pb_Projection.PlanarProject(data.vertices.Select(x=>x.position).ToArray(), nrm);

			int vertexCount = vertices.Count;

			// triangulate and set new face indices to end of current vertex list
			List<int> indices;

			if(pb_Triangulation.SortAndTriangulate(projection, out indices))
				data.face.SetIndices(indices.ToArray());
			else
				continue;

			data.face.ShiftIndices(vertexCount);
			face.CopyFrom(data.face);

			for(int n = 0; n < data.vertices.Count; n++) 
				lookup.Add(vertexCount + n, data.sharedIndices[n]);

			if(data.sharedIndicesUV.Count == data.vertices.Count)
			{
				for(int n = 0; n < data.vertices.Count; n++) 
					lookupUV.Add(vertexCount + n, data.sharedIndicesUV[n]);
			}

			vertices.AddRange(data.vertices);

			foreach(pb_Edge e in face.edges)
			{
				pb_EdgeLookup el = new pb_EdgeLookup(new pb_Edge(lookup[e.x], lookup[e.y]), e);

				if(el.common.x >= originalSharedIndicesCount || el.common.y >= originalSharedIndicesCount)
					appendedEdges.Add(el);
			}
		}

		indicesToDelete = indicesToDelete.Distinct().ToList();
		int delCount = indicesToDelete.Count;

		newEdges = appendedEdges.Distinct().Select(x => x.local - delCount).ToList();

		pb.SetVertices(vertices);
		pb.SetSharedIndices(lookup.ToSharedIndices());
		pb.SetSharedIndicesUV(lookupUV.ToSharedIndices());
		pb.DeleteVerticesWithIndices(indicesToDelete);

		return new pb_ActionResult(Status.Success, "Subdivide Edges");
	}

	/**
	 * Removes vertices that no face references.
	 */
	public static int[] RemoveUnusedVertices(this pb_Object pb)
	{
		List<int> del = new List<int>();
		int[] tris = pb_Face.AllTriangles(pb.faces);

		for(int i = 0; i < pb.vertices.Length; i++)
			if(!tris.Contains(i))
				del.Add(i);
		
		pb.DeleteVerticesWithIndices(del);
		
		return del.ToArray();
	}

	/**
	 *	Deletes the vertices from the passed index array.  Handles rebuilding the sharedIndices array.  Does not retriangulate face.
	 */
	public static void DeleteVerticesWithIndices(this pb_Object pb, IEnumerable<int> distInd)
	{
		pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);

		vertices = vertices.RemoveAt(distInd);

		pb_Face[] nFaces = pb.faces;

		// shift all other face indices down to account for moved vertex positions
		for(int i = 0; i < nFaces.Length; i++)
		{
			int[] tris = nFaces[i].indices;

			for(int n = 0; n < tris.Length; n++)
			{
				int sub = 0;
				foreach(int d in distInd)
				{
					if(tris[n] > d)
						sub++;
				}
				tris[n] -= sub;
			}

			nFaces[i].SetIndices(tris);
		}

		// shift all other face indices in the shared index array down to account for moved vertex positions
		pb_IntArray[] si = pb.sharedIndices;
		pb_IntArray[] su = pb.sharedIndicesUV;
		pb_IntArrayUtility.RemoveValuesAndShift(ref si, distInd);
		pb_IntArrayUtility.RemoveValuesAndShift(ref su, distInd);
		pb.SetSharedIndices(si);
		pb.SetSharedIndicesUV(su);
		pb.SetVertices(vertices);
		pb.SetFaces(nFaces);
		pb.ToMesh();
	}

	/**
	 *	Split a common index on a face into two vertices and slide each vertex backwards along it's feeding edge by distance.
	 *	This method does not perform any input validation, so make sure commonIndices is distinct and all winged edges belong
	 *	to the same face.
	 * 
	 *	`appendedVertices` is common index and a list of vertices it was split into.
	 *
	 *	_ _ _ _          _ _ _
	 *	|              /
	 *	|         ->   |
	 *	|              |
	 */
	public static pb_FaceRebuildData ExplodeVertex(
		IList<pb_Vertex> vertices,
		IList<pb_Tuple<pb_WingedEdge, int>> edgeAndCommonIndex,
		float distance,
		out Dictionary<int, List<pb_Vertex>> appendedVertices)
	{
		// organize all common splits with directions so that they can all be executed at once
		Dictionary<int, List<pb_Vertex>> splits = new Dictionary<int, List<pb_Vertex>>();
		
		appendedVertices = new Dictionary<int, List<pb_Vertex>>();

		pb_Face face = edgeAndCommonIndex.FirstOrDefault().Item1.face;

		if(face == null)
			return null;

		int[] fi = face.indices;
		int[] di = face.distinctIndices;
		Dictionary<int, int> ci = new Dictionary<int, int>();

		Vector3 normal = pb_Math.Normal(
			vertices[fi[0]].position,
			vertices[fi[1]].position,
			vertices[fi[2]].position);

		foreach(var edgeAndIndex in edgeAndCommonIndex)
		{
			pb_WingedEdge edge = edgeAndIndex.Item1;
			int commonIndex = edgeAndIndex.Item2;

			pb_Edge ae = AlignEdgeWithDirection(edge.edge, commonIndex);
			pb_WingedEdge next = edge.next.edge.common.Contains(commonIndex) ? edge.next : edge.previous;
			pb_Edge an = AlignEdgeWithDirection(next.edge, commonIndex);

			if(ae == null || an == null)
				continue;

			pb_Vertex adir = (vertices[ae.y] - vertices[ae.x]);
			pb_Vertex bdir = (vertices[an.y] - vertices[an.x]);
			adir.Normalize();
			bdir.Normalize();
			
			if(!ci.ContainsKey(ae.x)) ci.Add(ae.x, commonIndex);
			if(!ci.ContainsKey(an.x)) ci.Add(an.x, commonIndex);

			splits.AddOrAppend<int, pb_Vertex>(ae.x, adir);
			splits.AddOrAppend<int, pb_Vertex>(an.x, bdir);
		}

		List<pb_Vertex> v = new List<pb_Vertex>();

		for(int i = 0; i < di.Length; i++)
		{
			List<pb_Vertex> split_dir;

			if( splits.TryGetValue(di[i], out split_dir) )
			{
				for(int n = 0; n < split_dir.Count; n++)
				{
					pb_Vertex nv = vertices[di[i]] + split_dir[n] * distance;
					appendedVertices.AddOrAppend(ci[di[i]], nv);
					v.Add(nv);
				}
			}
			else
			{
				v.Add(new pb_Vertex(vertices[di[i]]));
			}
		}

		Vector3[] facePoints = new Vector3[v.Count];

		for(int i = 0; i < v.Count; ++i)
			facePoints[i] = v[i].position;

		Vector2[] points2d = pb_Projection.PlanarProject(facePoints, normal);
		List<int> triangles;

		if(pb_Triangulation.SortAndTriangulate(points2d, out triangles))
		{
			pb_FaceRebuildData data = new pb_FaceRebuildData();

			data.vertices = v;
			data.face = new pb_Face(face);
			data.face.SetIndices(triangles.ToArray());
			
			return data;
		}

		return null;
	}

	static pb_Edge AlignEdgeWithDirection(pb_EdgeLookup edge, int commonIndex)
	{
		if(edge.common.x == commonIndex)
			return new pb_Edge(edge.local.x, edge.local.y);
		else
			return new pb_Edge(edge.local.y, edge.local.x);
	}
#endregion

#region Move

	/**
	 * Snap all vertices to an increment of @snapValue in world space.
	 */
	public static void Quantize(pb_Object pb, IList<int> indices, Vector3 snap)
	{
		Vector3[] verts = pb.vertices;
		
		for(int n = 0; n < indices.Count; n++)
			verts[indices[n]] = pb.transform.InverseTransformPoint(pbUtil.SnapValue(pb.transform.TransformPoint(verts[indices[n]]), snap));

	}	
#endregion
	}
}
