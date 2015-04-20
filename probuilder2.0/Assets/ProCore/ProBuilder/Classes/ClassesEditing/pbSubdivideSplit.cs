using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.Math;
using ProBuilder2.Triangulator;
using ProBuilder2.Triangulator.Geometry;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.MeshOperations
{
public static class pbSubdivideSplit
{

#if !PROTOTYPE

#region Wrappers (Subdivide)

	/**
	 * Insert an point at the center of each face and split every edge.
	 */
	public static bool Subdivide(this pb_Object pb)
	{

		try
		{
			List<pb_EdgeConnection> ec = new List<pb_EdgeConnection>();
			foreach(pb_Face f in pb.faces)
				ec.Add(new pb_EdgeConnection(f, new List<pb_Edge>(f.edges)));

			pb_Face[] faces;

			ConnectEdges(pb, ec, out faces);
		}
		catch(System.Exception e)
		{
			Debug.LogWarning("Subdivide failed: \n" + e.ToString());
			return false;
		}

		return true;
	}

	public static bool SubdivideFace(this pb_Object pb, pb_Face[] faces, out pb_Face[] splitFaces)
	{
		List<pb_EdgeConnection> split = new List<pb_EdgeConnection>();
		foreach(pb_Face face in faces)
			split.Add(new pb_EdgeConnection(face, new List<pb_Edge>(face.edges)));

		return pb.ConnectEdges(split, out splitFaces);
	}
#endregion

#region Connect Edges / Vertices

	/**
	 * Attempts to insert new edges connecting the center of all passed edges.
	 */
	public static bool ConnectEdges(this pb_Object pb, pb_Edge[] edges, out pb_Edge[] newEdges)
	{
		// pb_Profiler profiler = new pb_Profiler();

		// profiler.BeginSample("Con nectEdges");

		int len = edges.Length;
		List<pb_EdgeConnection> splits = new List<pb_EdgeConnection>();

		// profiler.BeginSample("Split Edges");
		for(int i = 0; i < len; i++)
		{
			List<pb_Face> neighbors = pbMeshUtils.GetNeighborFaces(pb, edges[i]);

			foreach(pb_Face face in neighbors)
			{
				if(!splits.Contains((pb_EdgeConnection)face))
				{
					List<pb_Edge> faceEdges = new List<pb_Edge>();
					foreach(pb_Edge e in edges)
					{
						int localEdgeIndex = face.edges.IndexOf(e, pb.sharedIndices);
						if(localEdgeIndex > -1)
							faceEdges.Add(face.edges[localEdgeIndex]);
					}

					if(faceEdges.Count > 1)	
						splits.Add(new pb_EdgeConnection(face, faceEdges));
				}
			}
		}
		// profiler.EndSample();

		Vector3[] vertices = pb.GetVertices( pb_EdgeConnection.AllTriangles(splits).Distinct().ToArray() );


		pb_Face[] faces;
 		bool success = ConnectEdges(pb, splits, out faces);

		// profiler.BeginSample("Find New Edges");
		if(success)
		{
			/**
			 * Get the newly created Edges so that we can return them.
			 */
			List<pb_Edge> nedges = new List<pb_Edge>();

			for(int i = 0; i < faces.Length; i++)
			{
				for(int n = 0; n < faces[i].edges.Length; n++)
				{
					if( vertices.Contains(pb.vertices[faces[i].edges[n].x]) || vertices.Contains(pb.vertices[faces[i].edges[n].y]) )
						continue;
					else
						nedges.Add( faces[i].edges[n] );
				}
			}
			newEdges = nedges.ToArray();
		}
		else
		{
			newEdges = null;
		}
		// profiler.EndSample();

		// profiler.EndSample();
		// Debug.Log(profiler.ToString());

		return success;
	}

	struct DanglingVertex
	{
		public Vector3 position;
		public Color color;

		public DanglingVertex(Vector3 InPosition, Color InColor)
		{
			position = InPosition;
			color = InColor;
		}
	}

	private static bool ConnectEdges(this pb_Object pb, List<pb_EdgeConnection> pb_edgeConnectionsUnfiltered, out pb_Face[] faces)
	{
		List<pb_EdgeConnection> pb_edgeConnections = new List<pb_EdgeConnection>();
		foreach(pb_EdgeConnection ec in pb_edgeConnectionsUnfiltered)
			if(ec.isValid)
				pb_edgeConnections.Add(ec);

		int len = pb_edgeConnections.Count;

		if(len < 1)
		{
			faces = null;
			return false;
		}

		Vector3[] vertices = pb.vertices;
		Color[] colors = pb.colors;

		List<pb_Face> successfullySplitFaces = new List<pb_Face>();

		List<pb_Face> all_splitFaces 		= new List<pb_Face>();
		List<Vector3[]> all_splitVertices 	= new List<Vector3[]>();
		List<Color[]> all_splitColors 		= new List<Color[]>();
		List<Vector2[]> all_splitUVs	 	= new List<Vector2[]>();
		List<int[]> all_splitSharedIndices	= new List<int[]>();
		bool[] success 						= new bool[len];

		// use a nullable type because in order for the adjacent face triangulation
		// code to work, it needs to know what dangling vert belongs to which edge, 
		// if we out a vector3[] with each index corresponding to the passed edges
		// in pb_EdgeConnection, it's easy to maintain the relationship.
		DanglingVertex?[][] danglingVertices = new DanglingVertex?[len][];	

		// profiler.BeginSample("foreach(edge connection)");
		int i = 0;
		foreach(pb_EdgeConnection fc in pb_edgeConnections)
		{	
			pb_Face[] splitFaces 		= null;
			Vector3[][] splitVertices 	= null;
			Color[][] splitColors  		= null;
			Vector2[][] splitUVs 		= null;
			int[][] splitSharedIndices 	= null;
	
			if( fc.edges.Count < 3 )
			{
				Vector3 edgeACen = (vertices[fc.edges[0].x] + vertices[fc.edges[0].y]) / 2f; 
				Vector3 edgeBCen = (vertices[fc.edges[1].x] + vertices[fc.edges[1].y]) / 2f;

				Color cola = (colors[fc.edges[0].x] + colors[fc.edges[0].y]) / 2f;
				Color colb = (colors[fc.edges[1].x] + colors[fc.edges[1].y]) / 2f;

				danglingVertices[i] = new DanglingVertex?[2] { new DanglingVertex(edgeACen, cola), new DanglingVertex(edgeBCen, colb) };

				success[i] = SplitFace_Internal(
					new SplitSelection(pb, fc.face, edgeACen, edgeBCen, cola, colb, false, false, new int[]{fc.edges[0].x, fc.edges[0].y}, new int[]{fc.edges[1].x, fc.edges[1].y}),
					out splitFaces,
					out splitVertices, 
					out splitColors,
					out splitUVs,
					out splitSharedIndices);
				
				if(success[i])
					successfullySplitFaces.Add(fc.face);
			}
			else
			{
				DanglingVertex?[] appendedVertices = null;

				success[i] = SubdivideFace_Internal(pb, fc,
					out appendedVertices,
					out splitFaces,
					out splitVertices,
					out splitColors,
					out splitUVs,
					out splitSharedIndices);

				if(success[i])
					successfullySplitFaces.Add(fc.face);
	
				danglingVertices[i] = appendedVertices;
			}

			if(success[i])
			{
				int texGroup = fc.face.textureGroup < 0 ? pb.UnusedTextureGroup(i+1) : fc.face.textureGroup;
				
				for(int j = 0; j < splitFaces.Length; j++)
				{
					splitFaces[j].textureGroup = texGroup;
					all_splitFaces.Add(splitFaces[j]);
					all_splitVertices.Add(splitVertices[j]);
					all_splitColors.Add(splitColors[j]);
					all_splitUVs.Add(splitUVs[j]);
					all_splitSharedIndices.Add(splitSharedIndices[j]);
				}
			}

			i++;
		}
		// profiler.EndSample();


		// profiler.BeginSample("Retrianguate");
		/**
		 *	Figure out which faces need to be re-triangulated
		 */
		pb_Edge[][] tedges = new pb_Edge[pb_edgeConnections.Count][];
		int n = 0;
		for(i = 0; i < pb_edgeConnections.Count; i++)
			tedges[n++] = pb_edgeConnections[i].edges.ToArray();

		List<pb_Face>[][] allConnects = pbMeshUtils.GetNeighborFacesJagged(pb, tedges);		


		Dictionary<pb_Face, List<DanglingVertex>> addVertex = new Dictionary<pb_Face, List<DanglingVertex>>();
		List<pb_Face> temp = new List<pb_Face>();
		for(int j = 0; j < pb_edgeConnections.Count; j++)
		{
			if(!success[j]) continue;

			// check that this edge has a buddy that it welded it's new vertex to, and if not,
			// create one
			for(i = 0; i < pb_edgeConnections[j].edges.Count; i++)
			{
				if(danglingVertices[j][i] == null) 
					continue;

				List<pb_Face> connected = allConnects[j][i];

				foreach(pb_Face face in connected)
				{
					int ind = successfullySplitFaces.IndexOf(face);

					if(ind < 0)
					{
						if(addVertex.ContainsKey(face))
							addVertex[face].Add( (DanglingVertex)danglingVertices[j][i] );
						else
						{
							temp.Add(face);
							addVertex.Add(face, new List<DanglingVertex>(1) { (DanglingVertex)danglingVertices[j][i] });
						}
					}
				}
			}
		}
		// profiler.EndSample();

		// profiler.BeginSample("Append vertices to faces");
		pb_Face[] appendedFaces = pb.AppendFaces(all_splitVertices.ToArray(), all_splitColors.ToArray(), all_splitUVs.ToArray(), all_splitFaces.ToArray(), all_splitSharedIndices.ToArray());
		
		List<pb_Face> triangulatedFaces = new List<pb_Face>();
		foreach(KeyValuePair<pb_Face, List<DanglingVertex>> add in addVertex)
		{
			pb_Face newFace;

			if( pb.AppendVerticesToFace(add.Key, add.Value.Select(x => x.position).ToArray(), add.Value.Select(x => x.color).ToArray(), out newFace) )
				triangulatedFaces.Add(newFace);
			else
				Debug.LogError("Mesh re-triangulation failed.");//  Specifically, AppendVerticesToFace(" + add.Key + " : " + add.Value.ToFormattedString(", "));
		}
		// profiler.EndSample();

		// profiler.BeginSample("rebuild mesh");

		// Re-triangulate any faces left with dangling verts at edges
		// Weld verts, including those added in re-triangu
		int[] splitFaceTris = pb_Face.AllTriangles(appendedFaces);
		int[] triangulatedFaceTris = pb_Face.AllTriangles(triangulatedFaces);
		int[] allModifiedTris = new int[splitFaceTris.Length + triangulatedFaceTris.Length];
		
		System.Array.Copy(splitFaceTris, 0, allModifiedTris, 0, splitFaceTris.Length);
		System.Array.Copy(triangulatedFaceTris, 0, allModifiedTris, splitFaceTris.Length, triangulatedFaceTris.Length);
		
		// safe to assume that we probably didn't delete anything :/
		int[] welds;

		// profiler.BeginSample("weld vertices");
		pb.WeldVertices(allModifiedTris, Mathf.Epsilon, out welds);

		// profiler.EndSample();
		// pb.SetSharedIndices( pb_IntArrayUtility.ExtractSharedIndices(pb.vertices) );

		// Now that we're done screwing with geo, delete all the old faces (that were successfully split)		
		// profiler.BeginSample("delete faces");
		pb.DeleteFaces( successfullySplitFaces.ToArray() );
		faces = appendedFaces;
		// profiler.EndSample();
		// profiler.EndSample();
		// profiler.EndSample();
		// Debug.Log(profiler.ToString());

		return true;
	}

	/**
	 *	Splits face per vertex.
	 *	Todo - Could implement more sanity checks - namely testing for edges before sending to Split_Internal.  However,
	 *	the split method is smart enough to fail on those cases, so ignore for now.
	 */
	public static bool ConnectVertices(this pb_Object pb, List<pb_VertexConnection> vertexConnectionsUnfiltered, out int[] triangles)
	{
		List<pb_VertexConnection> vertexConnections = new List<pb_VertexConnection>();
		List<int> inds = new List<int>();

		int i = 0;

		for(i = 0; i < vertexConnectionsUnfiltered.Count; i++)
		{
			pb_VertexConnection vc = vertexConnectionsUnfiltered[i];
			vc.indices = vc.indices.Distinct().ToList();

			if(vc.isValid) 
			{
				inds.AddRange(vc.indices);
				vertexConnections.Add(vc);
			}
		}

		if(vertexConnections.Count < 1)
		{
			triangles = null;
			return false;
		}

		List<Vector3> selectedVertices = pb.GetVertices( pb_VertexConnection.AllTriangles(vertexConnections) );

		int len = vertexConnections.Count;

		// new faces will be built from successfull split ops
		List<pb_Face> successfullySplitFaces = new List<pb_Face>();
		List<pb_Face> all_splitFaces = new List<pb_Face>();

		List<Vector3[]> all_splitVertices = new List<Vector3[]>();
		List<Color[]> all_splitColors = new List<Color[]>();
		List<Vector2[]> all_splitUVs = new List<Vector2[]>();

		List<int[]> all_splitSharedIndices = new List<int[]>();
		bool[] success = new bool[len];

		pb_IntArray[] sharedIndices = pb.sharedIndices;

		i = 0;
		foreach(pb_VertexConnection vc in vertexConnections)
		{
			pb_Face[] splitFaces = null;
			Vector3[][] splitVertices = null;
			Color[][] splitColors = null;
			Vector2[][] splitUVs;
			int[][] splitSharedIndices = null;
	
			if( vc.indices.Count < 3 )
			{
				int indA = vc.face.indices.IndexOf(vc.indices[0], sharedIndices);
				int indB = vc.face.indices.IndexOf(vc.indices[1], sharedIndices);
				
				if(indA < 0 || indB < 0)
				{
					success[i] = false;
					continue;
				}

				indA = vc.face.indices[indA];
				indB = vc.face.indices[indB];

				success[i] = SplitFace_Internal(
					new SplitSelection(pb, vc.face, pb.vertices[indA], pb.vertices[indB], pb.colors[indA], pb.colors[indB], true, true, new int[] {indA}, new int[] {indB}),
					out splitFaces,
					out splitVertices, 
					out splitColors,
					out splitUVs,
					out splitSharedIndices);

				if(success[i])
					successfullySplitFaces.Add(vc.face);
			}
			else
			{
				Vector3 pokedVertex;

				success[i] = PokeFace_Internal(pb, vc.face, vc.indices.ToArray(),
					out pokedVertex,
					out splitFaces,
					out splitVertices,
					out splitColors, 
					out splitUVs, 
					out splitSharedIndices);

				if(success[i])
				{
					selectedVertices.Add(pokedVertex);
					successfullySplitFaces.Add(vc.face);
				}
			}

			if(success[i])
			{
				int texGroup = pb.UnusedTextureGroup(i+1);

				for(int j = 0; j < splitFaces.Length; j++)
				{
					splitFaces[j].textureGroup = texGroup;

					all_splitFaces.Add(splitFaces[j]);
					all_splitVertices.Add(splitVertices[j]);
					all_splitColors.Add(splitColors[j]);
					all_splitUVs.Add(splitUVs[j]);

					all_splitSharedIndices.Add(splitSharedIndices[j]);
				}
			}

			i++;
		}

		if(all_splitFaces.Count < 1)
		{
			triangles = null;
			return false;
		}

		pb_Face[] appendedFaces = pb.AppendFaces(all_splitVertices.ToArray(),
		                                         all_splitColors.ToArray(),
		                                         all_splitUVs.ToArray(),
		                                         all_splitFaces.ToArray(),
		                                         all_splitSharedIndices.ToArray());

		inds.AddRange(pb_Face.AllTriangles(appendedFaces));
		
		int[] welds;
		pb.WeldVertices(inds.ToArray(), Mathf.Epsilon, out welds);

		pb.DeleteFaces(successfullySplitFaces.ToArray());
		
		List<int> seltris = new List<int>();
		for(i = 0; i < selectedVertices.Count; i++)
		{
			int ind = System.Array.IndexOf(pb.vertices, selectedVertices[i]);
			if(ind > -1)	
				seltris.Add(ind);
		}

		triangles = seltris.Distinct().ToArray();
		return true;
	}
#endregion

#region Internal Implementation

	/**
	 *	Store information about a point on a face when to be used when splitting. ProBuilder
	 *	allows splitting faces from 2 points that can either land on any mix of vertex or edge,
	 *	so we need to tell the split face function that information.
	 */
	private class SplitSelection
	{
		public pb_Object pb;
		public pb_Face face;

		public Vector3 pointA;
		public Vector3 pointB;

		public Color colorA;
		public Color colorB;

		public bool aIsVertex;
		public bool bIsVertex;

		public int[] indexA;	// if vertex -> face relative index - cannot be a sharedIndex
								// if edge -> non-face relative.
		public int[] indexB;	// if vertex -> face relative index - cannot be a sharedIndex
								// if edge -> non-face relative.

		/**
		 *	Constructor
		 */
		public SplitSelection(pb_Object pb, pb_Face face, Vector3 pointA, Vector3 pointB, Color colA, Color colB, bool aIsVertex, bool bIsVertex, int[] indexA, int[] indexB)
		{
			this.pb = pb;
			this.face = face;
			this.pointA = pointA;
			this.pointB = pointB;
			this.colorA = colA;
			this.colorB = colB;
			this.aIsVertex = aIsVertex;
			this.bIsVertex = bIsVertex;
			this.indexA = indexA;
			this.indexB = indexB;
		}

		public override string ToString()
		{
			return "face: " + face.ToString() + "\n" + "a is vertex: " + aIsVertex + "\nb is vertex: " + bIsVertex + "\nind a, b: "+ indexA + ", " + indexB;
		}
	}

	/**
	 *	This method assumes that the split selection edges share a common face and have already been sanity checked.  Will return 
	 *	the variables necessary to compose a new face from the split, or null if the split is invalid.
	 */
	private static bool SplitFace_Internal(SplitSelection splitSelection,
		out pb_Face[] splitFaces,
		out Vector3[][] splitVertices,
		out Color[][] splitColors,
		out Vector2[][] splitUVs,
		out int[][] splitSharedIndices) 
	{
		splitFaces = null;
		splitVertices = null;
		splitColors = null;
		splitUVs = null;
		splitSharedIndices = null;

		pb_Object pb = splitSelection.pb;	// we'll be using this a lot
		pb_Face face = splitSelection.face;	// likewise

		int[] indices = face.distinctIndices;
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		int[] sharedIndex = new int[indices.Length];
		for(int i = 0; i < indices.Length; i++)
			sharedIndex[i] = sharedIndices.IndexOf(indices[i]);

		// First order of business is to translate the face to 2D plane.
		Vector3[] verts = pb.GetVertices(face.distinctIndices);
		Color[] colors = pbUtil.ValuesWithIndices(pb.colors, face.distinctIndices);
		Vector2[] uvs = pb.GetUVs(face.distinctIndices);

		Vector3 projAxis = pb_Math.ProjectionAxisToVector( pb_Math.VectorToProjectionAxis(pb_Math.Normal(pb, face) ) );
		Vector2[] plane = pb_Math.PlanarProject(verts, projAxis);

		// Split points
 		Vector3 splitPointA_3d = splitSelection.pointA;
 		Vector3 splitPointB_3d = splitSelection.pointB;

 		Vector2 splitPointA_uv = splitSelection.aIsVertex ? pb.uv[splitSelection.indexA[0]] : (pb.uv[splitSelection.indexA[0]] + pb.uv[splitSelection.indexA[1]]) /2f;
 		Vector2 splitPointB_uv = splitSelection.bIsVertex ? pb.uv[splitSelection.indexB[0]] : (pb.uv[splitSelection.indexB[0]] + pb.uv[splitSelection.indexB[1]]) /2f;

		Vector2 splitPointA_2d = pb_Math.PlanarProject( new Vector3[1] { splitPointA_3d }, projAxis )[0];
		Vector2 splitPointB_2d = pb_Math.PlanarProject( new Vector3[1] { splitPointB_3d }, projAxis )[0];

		List<Vector3> v_polyA = new List<Vector3>();	// point in object space
		List<Vector3> v_polyB = new List<Vector3>();	// point in object space

		List<Color> c_polyA = new List<Color>();
		List<Color> c_polyB = new List<Color>();
		
		List<Vector2> v_polyB_2d = new List<Vector2>();	// point in 2d space - used to triangulate
		List<Vector2> v_polyA_2d = new List<Vector2>();	// point in 2d space - used to triangulate

		List<Vector2> u_polyA = new List<Vector2>();
		List<Vector2> u_polyB = new List<Vector2>();

		List<int> i_polyA = new List<int>();			// sharedIndices array index
		List<int> i_polyB = new List<int>();			// sharedIndices array index

		List<int> nedgeA = new List<int>();
		List<int> nedgeB = new List<int>();

		// Sort points into two separate polygons
		for(int i = 0; i < indices.Length; i++)
		{
			// is this point (a) a vertex to split or (b) on the negative or positive side of this split line
			if( (splitSelection.aIsVertex && splitSelection.indexA[0] == indices[i]) ||  (splitSelection.bIsVertex && splitSelection.indexB[0] == indices[i]) )
			{
				v_polyA.Add( verts[i] );
				v_polyB.Add( verts[i] );
				
				u_polyA.Add( uvs[i] );
				u_polyB.Add( uvs[i] );

				v_polyA_2d.Add( plane[i] );
				v_polyB_2d.Add( plane[i] );

				i_polyA.Add( sharedIndex[i] );
				i_polyB.Add( sharedIndex[i] );

				c_polyA.Add(colors[i]);
				c_polyB.Add(colors[i]);
			}
			else
			{
				// split points across the division line
				Vector2 perp = pb_Math.Perpendicular(splitPointB_2d, splitPointA_2d);
				Vector2 origin = (splitPointA_2d + splitPointB_2d) / 2f;
				
				if( Vector2.Dot(perp, plane[i]-origin) > 0 )
				{
					v_polyA.Add(verts[i]);
					v_polyA_2d.Add(plane[i]);
					u_polyA.Add(uvs[i]);
					i_polyA.Add(sharedIndex[i]);
					c_polyA.Add(colors[i]);
				}
				else
				{
					v_polyB.Add(verts[i]);
					v_polyB_2d.Add(plane[i]);
					u_polyB.Add(uvs[i]);
					i_polyB.Add(sharedIndex[i]);
					c_polyB.Add(colors[i]);
				}
			}
		}

		if(!splitSelection.aIsVertex)
		{
			v_polyA.Add( splitPointA_3d );
			v_polyA_2d.Add( splitPointA_2d );
			u_polyA.Add( splitPointA_uv );
			i_polyA.Add(-1);
			c_polyA.Add(splitSelection.colorA);
			
			v_polyB.Add( splitPointA_3d );
			v_polyB_2d.Add( splitPointA_2d );
			u_polyB.Add( splitPointA_uv );
			i_polyB.Add(-1);	//	neg 1 because it's a new vertex point
			c_polyB.Add(splitSelection.colorA);

			nedgeA.Add(v_polyA.Count);
			nedgeB.Add(v_polyB.Count);
		}

		// PLACE
		if(!splitSelection.bIsVertex)
		{
			v_polyA.Add( splitPointB_3d );
			v_polyA_2d.Add( splitPointB_2d );
			u_polyA.Add( splitPointB_uv );
			i_polyA.Add(-1);
			c_polyB.Add(splitSelection.colorB);
			
			v_polyB.Add( splitPointB_3d );
			v_polyB_2d.Add( splitPointB_2d );
			u_polyB.Add( splitPointB_uv );
			i_polyB.Add(-1);	//	neg 1 because it's a new vertex point
			c_polyB.Add(splitSelection.colorB);
		
			nedgeA.Add(v_polyA.Count);
			nedgeB.Add(v_polyB.Count);
		}

		if(v_polyA_2d.Count < 3 || v_polyB_2d.Count < 3)
		{
			splitFaces = null;
			splitVertices = null;
			splitSharedIndices = null;
			return false;
		}

		// triangulate new polygons
		int[] t_polyA = Delaunay.Triangulate(v_polyA_2d).ToIntArray();
		int[] t_polyB = Delaunay.Triangulate(v_polyB_2d).ToIntArray();

		if(t_polyA.Length < 3 || t_polyB.Length < 3)
			return false;

		// figure out the face normals for the new faces and check to make sure they match the original face
		Vector2[] pln = pb_Math.PlanarProject( pb.GetVertices(face.indices), projAxis );

		Vector3 nrm = Vector3.Cross( pln[2] - pln[0], pln[1] - pln[0]);
		Vector3 nrmA = Vector3.Cross( v_polyA_2d[ t_polyA[2] ]-v_polyA_2d[ t_polyA[0] ], v_polyA_2d[ t_polyA[1] ]-v_polyA_2d[ t_polyA[0] ] );
		Vector3 nrmB = Vector3.Cross( v_polyB_2d[ t_polyB[2] ]-v_polyB_2d[ t_polyB[0] ], v_polyB_2d[ t_polyB[1] ]-v_polyB_2d[ t_polyB[0] ] );

		if(Vector3.Dot(nrm, nrmA) < 0) System.Array.Reverse(t_polyA);
		if(Vector3.Dot(nrm, nrmB) < 0) System.Array.Reverse(t_polyB);

		// triangles, material, pb_UV, smoothing group, shared index
		pb_Face faceA = new pb_Face( t_polyA, face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, face.elementGroup, face.manualUV);
		pb_Face faceB = new pb_Face( t_polyB, face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, face.elementGroup, face.manualUV);

		splitFaces = new pb_Face[2] { faceA, faceB };
		splitVertices = new Vector3[2][] { v_polyA.ToArray(), v_polyB.ToArray() };
		splitColors = new Color[2][] { c_polyA.ToArray(), c_polyB.ToArray() };
		splitUVs = new Vector2[2][] { u_polyA.ToArray(), u_polyB.ToArray() };
		splitSharedIndices = new int[2][] { i_polyA.ToArray(), i_polyB.ToArray() };

		return true;
	}

	/**
	 *	Inserts a vertex at the center of each edge, then connects the new vertices to another new
	 *	vertex placed at the center of the face.
	 */
	private static bool SubdivideFace_Internal(pb_Object pb, pb_EdgeConnection pb_edgeConnection, 
		out DanglingVertex?[] appendedVertices,	
		out pb_Face[] splitFaces,
		out Vector3[][] splitVertices,
		out Color[][] splitColors,
		out Vector2[][] splitUVs,
		out int[][] splitSharedIndices)
	{
		splitFaces 			= null;
		splitVertices 		= null;
		splitColors 		= null;
		splitUVs 			= null;
		splitSharedIndices 	= null;
		appendedVertices 	= new DanglingVertex?[pb_edgeConnection.edges.Count];

		// cache all the things
		pb_Face face = pb_edgeConnection.face;
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		Vector3[] vertices = pb.vertices;
		Vector2[] uvs = pb.uv;

		List<Vector2> edgeCentersUV = new List<Vector2>();
		List<Vector3> edgeCenters3d = new List<Vector3>();
		List<Color> edgeCentersCol = new List<Color>();
		
		// filter duplicate edges
		int u = 0;
		List<int> usedEdgeIndices = new List<int>();
		foreach(pb_Edge edge in pb_edgeConnection.edges)
		{
			int ind = face.edges.IndexOf(edge, sharedIndices);
			if(!usedEdgeIndices.Contains(ind))
			{
				Vector3 cen = (vertices[edge.x] + vertices[edge.y]) / 2f;

				appendedVertices[u] = new DanglingVertex(cen, (pb.colors[edge.x] + pb.colors[edge.y]) / 2f);
				
				edgeCenters3d.Add(cen);
				edgeCentersUV.Add( (uvs[edge.x] + uvs[edge.y])/2f );
				edgeCentersCol.Add( (pb.colors[edge.x] + pb.colors[edge.y])/2f );

				usedEdgeIndices.Add(ind);
			}
			else
			{
				appendedVertices[u] = null;
			}

			u++;
		}

		// now we have all the vertices of the old face, plus the new edge center vertices
		Vector3 nrm = pb_Math.Normal(pb.GetVertices(face.indices));

		Vector3[] verts3d = pb.GetVertices(face.distinctIndices);
		Vector2[] faceUVs = pb.GetUVs(face.distinctIndices);
		Color[] colors = pbUtil.ValuesWithIndices(pb.colors, face.distinctIndices);

		Vector2[] verts2d = pb_Math.PlanarProject(verts3d, nrm);
		Vector2[] edgeCenters2d = pb_Math.PlanarProject(edgeCenters3d.ToArray(), nrm);
		
		Vector3 cen3d = pb_Math.Average(verts3d);
		Vector2 cenUV = pb_Bounds2D.Center(faceUVs);

		Vector2 cen2d = pb_Math.PlanarProject( new Vector3[1] { cen3d }, nrm)[0];

		// Get the directions from which to segment this face
		Vector2[] dividers = new Vector2[edgeCenters2d.Length];
		for(int i = 0; i < edgeCenters2d.Length; i++)
			dividers[i] = (edgeCenters2d[i] - cen2d).normalized;

		List<Vector2>[] quadrants2d = new List<Vector2>[edgeCenters2d.Length];
		List<Vector3>[] quadrants3d = new List<Vector3>[edgeCenters2d.Length];
		List<Vector2>[] quadrantsUV = new List<Vector2>[edgeCenters2d.Length];
		List<Color>[] 	quadrantsCol = new List<Color>[edgeCenters2d.Length];

		List<int>[]		sharedIndex = new List<int>[edgeCenters2d.Length];

		for(int i = 0; i < quadrants2d.Length; i++)
		{
			quadrants2d[i] = new List<Vector2>(1) { cen2d };
			quadrants3d[i] = new List<Vector3>(1) { cen3d };			
			quadrantsUV[i] = new List<Vector2>(1) { cenUV };
			quadrantsCol[i] = new List<Color>(1) { pb_Math.Average(pbUtil.ValuesWithIndices(pb.colors, face.distinctIndices)) };

			sharedIndex[i] = new List<int>(1) { -2 };		// any negative value less than -1 will be treated as a new group
		}

		// add the divisors
		for(int i = 0; i < edgeCenters2d.Length; i++)
		{
			quadrants2d[i].Add(edgeCenters2d[i]);
			quadrants3d[i].Add(edgeCenters3d[i]);
			quadrantsUV[i].Add(edgeCentersUV[i]);
			quadrantsCol[i].Add(edgeCentersCol[i]);

			sharedIndex[i].Add(-1);

			// and add closest in the counterclockwise direction
			Vector2 dir = (edgeCenters2d[i]-cen2d).normalized;
			float largestClockwiseDistance = 0f;
			int quad = -1;
			for(int j = 0; j < dividers.Length; j++)
			{
				if(j == i) continue;	// this is a dividing vertex - ignore

				float dist = Vector2.Angle(dividers[j], dir);
				if( Vector2.Dot(pb_Math.Perpendicular(dividers[j]), dir) < 0f )
					dist = 360f - dist;

				if(dist > largestClockwiseDistance)
				{
					largestClockwiseDistance = dist;
					quad = j;
				}
			}

			quadrants2d[quad].Add(edgeCenters2d[i]);
			quadrants3d[quad].Add(edgeCenters3d[i]);
			quadrantsUV[quad].Add(edgeCentersUV[i]);
			quadrantsCol[quad].Add(edgeCentersCol[i]);

			sharedIndex[quad].Add(-1);
		}

		// distribute the existing vertices
		for(int i = 0; i < face.distinctIndices.Length; i++)
		{
			Vector2 dir = (verts2d[i]-cen2d).normalized;	// plane corresponds to distinctIndices
			float largestClockwiseDistance = 0f;
			int quad = -1;
			for(int j = 0; j < dividers.Length; j++)
			{
				float dist = Vector2.Angle(dividers[j], dir);
				if( Vector2.Dot(pb_Math.Perpendicular(dividers[j]), dir) < 0f )
					dist = 360f - dist;

				if(dist > largestClockwiseDistance)
				{
					largestClockwiseDistance = dist;
					quad = j;
				}
			}

			quadrants2d[quad].Add(verts2d[i]);
			quadrants3d[quad].Add(verts3d[i]);
			quadrantsUV[quad].Add(faceUVs[i]);
			quadrantsCol[quad].Add(colors[i]);

			sharedIndex[quad].Add(pb.sharedIndices.IndexOf(face.distinctIndices[i]));
		}

		int len = quadrants2d.Length;

		// Triangulate
		int[][] tris = new int[len][];
		for(int i = 0; i < len; i++)
		{
			if(quadrants2d[i].Count < 3)
			{
				Debug.LogError("Insufficient points to triangulate.  Exit subdivide operation.  This is probably due to a concave face.");
				return false;
			}
		
			tris[i] = Delaunay.Triangulate(quadrants2d[i]).ToIntArray();

			if(tris[i].Length < 3)	///< #521
				return false;
			
			if( Vector3.Dot(nrm, pb_Math.Normal(quadrants3d[i][tris[i][0]], quadrants3d[i][tris[i][1]], quadrants3d[i][tris[i][2]])) < 0 )
				System.Array.Reverse(tris[i]);
		}

		splitFaces 		= new pb_Face[len];
		splitVertices 	= new Vector3[len][];
		splitColors 	= new Color[len][];
		splitUVs 		= new Vector2[len][];
		splitSharedIndices 	= new int[len][];

		for(int i = 0; i < len; i++)
		{
			// triangles, material, pb_UV, smoothing group, shared index
			splitFaces[i] 			= new pb_Face(tris[i], face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, face.elementGroup, face.manualUV);
			splitVertices[i] 		= quadrants3d[i].ToArray();
			splitColors[i] 			= quadrantsCol[i].ToArray();
			splitUVs[i] 			= quadrantsUV[i].ToArray();
			
			splitSharedIndices[i] 	= sharedIndex[i].ToArray();
		}

		return true;
	}

	/**
	 *	Inserts a split from each selected vertex to the center of the face
	 */
	private static bool PokeFace_Internal(pb_Object pb, pb_Face face, int[] indices_nonFaceSpecific,
		out Vector3 pokedVertex,
		out pb_Face[] splitFaces,
		out Vector3[][] splitVertices,
		out Color[][] splitColors,
		out Vector2[][] splitUVs,
		out int[][] splitSharedIndices)
	{
		pokedVertex = Vector3.zero;
		splitFaces = null;
		splitVertices = null;
		splitColors = null;
		splitUVs = null;
		splitSharedIndices = null;

		pb_IntArray[] sharedIndices = pb.sharedIndices;

		///** Sort index array such that it only uses indices local to the passed face
		int[] dist_indices = new int[indices_nonFaceSpecific.Length];
		int[] dist_ind_si = new int[face.distinctIndices.Length];

		// figure out sharedIndices index of distinct Indices
		for(int i = 0; i < face.distinctIndices.Length; i++)
			dist_ind_si[i] = sharedIndices.IndexOf(face.distinctIndices[i]);

		// now do the same for non-face specific indices, assigning matching groups
		
		///** Sort index array such that it only uses indices local to the passed face
		for(int i = 0; i < dist_indices.Length; i++)
		{
			int ind = System.Array.IndexOf(dist_ind_si, sharedIndices.IndexOf(indices_nonFaceSpecific[i]));
			if(ind < 0) return false;

			dist_indices[i] = face.distinctIndices[ind];
		}

		int[] indices = dist_indices.Distinct().ToArray();

		// throw out splits with less than 2 vertices, or splits composed 
		// of a single edge
		switch(indices.Length)
		{
			case 0:
			case 1:
				return false;

			case 2:
				if( System.Array.IndexOf(face.edges, new pb_Edge(indices[0], indices[1]) ) > -1)
					return false;
				break;
			default:
				break;
		}
		
		// end triangle sorting

		/**
		 *	The general idea here is to project the face into 2d space,
		 *	split the 2d points into groups based on the intersecting lines,
		 *	then triangulate those groups.  once the groups have been 
		 *	triangulated, rebuild the 3d vertices using the new groups
		 *	(building new verts for seams).
		 *	
		 *	Think like you're cutting a pie... but first the pie is a basketball,
		 *	then a pie, then a basketball again.  I'm on a horse.
		 */

		Vector3[] verts 	= pb.GetVertices(face.distinctIndices);
		Vector2[] uvs 		= pb.GetUVs(face.distinctIndices);
		Color[] colors  	= pbUtil.ValuesWithIndices(pb.colors, face.distinctIndices);

		Vector2 cenUV		= pb_Bounds2D.Center(uvs);
		Vector3 cen3d 		= pb_Math.Average(verts);
		pokedVertex 		= cen3d;
		Vector3 nrm 		= pb_Math.Normal(pb.GetVertices(face.indices));
		Color cenColor 		= pb_Math.Average(colors);

		// this should be cleaned up
		Vector2[] plane 	= pb_Math.PlanarProject(verts, nrm);
		Vector2[] indPlane 	= pb_Math.PlanarProject(pb.GetVertices(indices), nrm);
		Vector2 cen2d 		= pb_Math.PlanarProject( new Vector3[1] { cen3d }, nrm)[0];

		// Get the directions from which to segment this face
		Vector2[] dividers = new Vector2[indices.Length];
		for(int i = 0; i < indices.Length; i++)
			dividers[i] = (indPlane[i] - cen2d).normalized;

		List<Vector2>[] quadrants2d 	= new List<Vector2>[indices.Length];
		List<Vector3>[] quadrants3d 	= new List<Vector3>[indices.Length];
		List<Vector2>[] quadrantsUV_2d 	= new List<Vector2>[indices.Length];
		List<Color>[] 	quadrantsCol 	= new List<Color>[indices.Length];

		List<int>[]		sharedIndex = new List<int>[indices.Length];

		for(int i = 0; i < quadrants2d.Length; i++)
		{
			quadrants2d[i] = new List<Vector2>(1) { cen2d };
			quadrants3d[i] = new List<Vector3>(1) { cen3d };
			quadrantsUV_2d[i] = new List<Vector2>(1) { cenUV };
			quadrantsCol[i] = new List<Color>(1) { cenColor };

			sharedIndex[i] = new List<int>(1) { -2 };		// any negative value less than -1 will be treated as a new group
		}

		for(int i = 0; i < face.distinctIndices.Length; i++)
		{
			// if this index is a divider, it needs to belong to the leftmost and 
			// rightmost quadrant
			int indexInPokeVerts = System.Array.IndexOf(indices, face.distinctIndices[i]);
			int ignore = -1;
			if( indexInPokeVerts > -1)
			{	
				// Add vert to this quadrant
				quadrants2d[indexInPokeVerts].Add(plane[i]);
				quadrants3d[indexInPokeVerts].Add(verts[i]);
				quadrantsUV_2d[indexInPokeVerts].Add(uvs[i]);
				quadrantsCol[indexInPokeVerts].Add(colors[i]);

				sharedIndex[indexInPokeVerts].Add(pb.sharedIndices.IndexOf(face.distinctIndices[i]));

				// And also the one closest counter clockwise
				ignore = indexInPokeVerts;
			}

			Vector2 dir = (plane[i]-cen2d).normalized;	// plane corresponds to distinctIndices
			float largestClockwiseDistance = 0f;
			int quad = -1;
			for(int j = 0; j < dividers.Length; j++)
			{
				if(j == ignore) continue;	// this is a dividing vertex - ignore

				float dist = Vector2.Angle(dividers[j], dir);
				if( Vector2.Dot(pb_Math.Perpendicular(dividers[j]), dir) < 0f )
					dist = 360f - dist;

				if(dist > largestClockwiseDistance)
				{
					largestClockwiseDistance = dist;
					quad = j;
				}
			}

			quadrants2d[quad].Add(plane[i]);
			quadrants3d[quad].Add(verts[i]);
			quadrantsUV_2d[quad].Add(uvs[i]);
			quadrantsCol[quad].Add(colors[i]);

			sharedIndex[quad].Add(pb.sharedIndices.IndexOf(face.distinctIndices[i]));
		}

		int len = quadrants2d.Length;

		// Triangulate
		int[][] tris = new int[len][];
		for(int i = 0; i < len; i++)
		{
			try {
				tris[i] = Delaunay.Triangulate(quadrants2d[i]).ToIntArray();
			
				if(tris[i] == null || tris[i].Length < 3)
				{
					Debug.Log("Fail triangulation");
					return false;
				}
			} catch (System.Exception error) {
				Debug.LogError("PokeFace internal failed triangulation. Bail!\n" + error);
				return false;
			}

			// todo - check that face normal is correct
		}

		splitFaces 			= new pb_Face[len];
		splitVertices 		= new Vector3[len][];
		splitColors 		= new Color[len][];
		splitUVs 			= new Vector2[len][];
		splitSharedIndices 	= new int[len][];

		for(int i = 0; i < len; i++)
		{
			// triangles, material, pb_UV, smoothing group, shared index
			splitFaces[i] 		= new pb_Face(tris[i], face.material, new pb_UV(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
			splitVertices[i] 	= quadrants3d[i].ToArray();
			splitColors[i] 		= quadrantsCol[i].ToArray();
			splitUVs[i] 		= quadrantsUV_2d[i].ToArray();

			splitSharedIndices[i] = sharedIndex[i].ToArray();
		}

		return true;
	}

	static string tfs(Vector3[] v)
	{
		string tx2 = v[0].ToString("F4");

		for(int i = 1; i < v.Length; i++)
		{
			tx2 += "\n" + v[i].ToString("F4");
		}
		return tx2;
	}

	static string tfs(Vector2[] v)
	{
		string tx2 = v[0].ToString("F4");

		for(int i = 1; i < v.Length; i++)
		{
			tx2 += "\n" + v[i].ToString("F4");
		}
		return tx2;
	}
#endregion
#endif
}
}