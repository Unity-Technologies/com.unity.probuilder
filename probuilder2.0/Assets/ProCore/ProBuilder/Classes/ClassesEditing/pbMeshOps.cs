// #undef PB_DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Math;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.MeshOperations
{
	public static class pbMeshOps
	{
#region Pivot Operations (Center, Freeze Transform)

	/**
	 * Center the mesh pivot at the average of passed indices.
	 */
	public static void CenterPivot(this pb_Object pb, int[] indices)
	{	
		Vector3 center = Vector3.zero;

		if(indices != null)
		{
			Vector3[] verts = pb.VerticesInWorldSpace(indices);

			foreach (Vector3 v in verts)
				center += v;
		
			center /= (float)verts.Length;
		}
		else
		{
			center = pb.transform.TransformPoint(pb.msh.bounds.center);
		}

		// if(pbUtil.SharedSnapEnabled)
		// 	center = pbUtil.SnapValue(center, pbUtil.SharedSnapValue);

		Vector3 dir = (pb.transform.position - center);

		pb.transform.position = center;

		// the last bool param force disables snapping vertices
		pb.TranslateVertices_World(pb.msh.triangles, dir, true);

		pb.ToMesh();
		pb.Refresh();
	}

	/**
	 *	\brief Scale vertices and set transform.localScale to Vector3.one.
	 */
	public static void FreezeScaleTransform(this pb_Object pb)
	{
		Vector3[] v = pb.vertices;
		for(int i = 0; i < v.Length; i++)
			v[i] = Vector3.Scale(v[i], pb.transform.localScale);

		pb.SetVertices(v);
		pb.ToMesh();
		pb.transform.localScale = new Vector3(1f, 1f, 1f);
		pb.Refresh();
	}
#endregion

#region Extrusion

	const float EXTRUDE_DISTANCE = .25f;
	public static bool Extrude(this pb_Object pb, pb_Face[] faces)
	{
		return pb.Extrude(faces, EXTRUDE_DISTANCE);
	}

	/**
	 * Extrudes the passed faces by extrudeDistance amount.  @faces will remain valid.
	 */
	public static bool Extrude(this pb_Object pb, pb_Face[] faces, float extrudeDistance)
	{
		List<pb_Face> appended;
		return Extrude(pb, faces, extrudeDistance, out appended);
	}

	/**
	 * Extrudes passed faces on their normal axis using extrudeDistance.
	 */
	public static bool Extrude(this pb_Object pb, pb_Face[] faces, float extrudeDistance, out List<pb_Face> appendedFaces)
	{
		appendedFaces = new List<pb_Face>();

		if(faces == null || faces.Length < 1)
			return false;

		pb_IntArray[] sharedIndices = pb.GetSharedIndices();

		Vector3[] localVerts = pb.vertices;

		pb_Edge[] perimeterEdges = pb.GetPerimeterEdges(faces);

		if(perimeterEdges == null || perimeterEdges.Length < 3)
		{
			Debug.LogWarning("No perimeter edges found.  Try deselecting and reselecting this object and trying again.");
			return false;
		}

		pb_Face[] edgeFaces = new pb_Face[perimeterEdges.Length];	// can't assume faces and perimiter edges will be 1:1 - so calculate perimeters then extract face information
		int[] allEdgeIndices = new int[perimeterEdges.Length * 2];
		int c = 0;
		for(int i = 0; i < perimeterEdges.Length; i++)
		{
			// gets the faces associated with each perimeter edge
			edgeFaces[i] = faces[0];
			foreach(pb_Face face in faces)
				if(face.edges.Contains(perimeterEdges[i]))
					edgeFaces[i] = face;

			allEdgeIndices[c++] = perimeterEdges[i].x;
			allEdgeIndices[c++] = perimeterEdges[i].y;
		}

		List<pb_Edge> extrudedIndices = new List<pb_Edge>();
		appendedFaces = new List<pb_Face>();

		/// build out new faces around edges
		for(int i = 0; i < perimeterEdges.Length; i++)
		{
			pb_Edge edge = perimeterEdges[i];
			pb_Face face = edgeFaces[i];

			// Averages the normals using only vertices that are on the edge
			Vector3 xnorm = Vector3.zero;
			Vector3 ynorm = Vector3.zero;

			// don't bother getting vertex normals if not auto-extruding
			if(extrudeDistance > Mathf.Epsilon)
			{
				xnorm = pb_Math.Normal( localVerts[face.indices[0]], localVerts[face.indices[1]], localVerts[face.indices[2]] );
				ynorm = xnorm;
				// xnorm = Norm( edge.x, sharedIndices, allEdgeIndices, oNormals );
				// ynorm = Norm( edge.y, sharedIndices, allEdgeIndices, oNormals );
			}

			int x_sharedIndex = sharedIndices.IndexOf(edge.x);
			int y_sharedIndex = sharedIndices.IndexOf(edge.y);

			// this could be condensed to a single call with an array of new faces
			pb_Face newFace = pb.AppendFace(
				new Vector3[4]
				{
					localVerts [ edge.x ],
					localVerts [ edge.y ],
					localVerts [ edge.x ] + xnorm.normalized * extrudeDistance,
					localVerts [ edge.y ] + ynorm.normalized * extrudeDistance
				},
				new Color[4]
				{
					pb.colors[ edge.x ],
					pb.colors[ edge.y ],
					pb.colors[ edge.x ],
					pb.colors[ edge.y ]
				},
				new Vector2[4],	// empty array for UVs
				new pb_Face
				( 
					new int[6] {0, 1, 2, 1, 3, 2},			// indices
					face.material,							// material
					new pb_UV(face.uv),						// UV material
					face.smoothingGroup,					// smoothing group
					-1,										// texture group
					-1,										// uv element group
					false),									// manualUV flag
					new int[4] { x_sharedIndex, y_sharedIndex, -1, -1 });

			extrudedIndices.Add(new pb_Edge(x_sharedIndex, newFace.indices[2]));
			extrudedIndices.Add(new pb_Edge(y_sharedIndex, newFace.indices[4]));

			appendedFaces.Add(newFace);
		}

		// merge extruded vertex indices with each other
		pb_IntArray[] si = pb.sharedIndices;	// leave the sharedIndices copy alone since we need the un-altered version later
		for(int i = 0; i < extrudedIndices.Count; i++)
		{
			int val = extrudedIndices[i].x;
			for(int n = 0; n < extrudedIndices.Count; n++)
			{
				if(n == i)
					continue;

				if(extrudedIndices[n].x == val)
				{
					pb_IntArrayUtility.MergeSharedIndices(ref si, extrudedIndices[n].y, extrudedIndices[i].y);
					break;
				}
			}
		}

		// Move extruded faces to top
		localVerts = pb.vertices;
		Dictionary<int, int> remappedTexGroups = new Dictionary<int, int>();
		foreach(pb_Face f in faces)
		{
			// Remap texture groups
			if(f.textureGroup > 0)
			{
				if(remappedTexGroups.ContainsKey(f.textureGroup))
				{
					f.textureGroup = remappedTexGroups[f.textureGroup];
				}
				else
				{
					int newTexGroup = pb.UnusedTextureGroup();
					remappedTexGroups.Add(f.textureGroup, newTexGroup);
					f.textureGroup = newTexGroup;
				}
			}

			int[] distinctIndices = f.distinctIndices;

			foreach(int ind in distinctIndices)
			{
				int oldIndex = si.IndexOf(ind);
				for(int i = 0; i < extrudedIndices.Count; i++)
				{
					if(oldIndex == extrudedIndices[i].x)
					{
						pb_IntArrayUtility.MergeSharedIndices(ref si, extrudedIndices[i].y, ind);
						break;
					}
				}
			}
		}

		pb.SplitUVs(pb_Face.AllTriangles(faces));
		
		// this is a separate loop cause the one above this must completely merge all sharedindices prior to 
		// checking the normal averages
		foreach(pb_Face f in faces)
		{
			Vector3 norm = pb_Math.Normal(localVerts[f.indices[0]], localVerts[f.indices[1]], localVerts[f.indices[2]]);

			foreach(int ind in f.distinctIndices)
			{
				// Vector3 norm = Norm( ind, si, allEdgeIndices, oNormals );

				localVerts[ind] += norm.normalized * extrudeDistance;
			}
		}

		// Test the winding of the first pulled face, and reverse if it's ccw
		if( pb.GetWindingOrder(faces[0]) == WindingOrder.CounterClockwise )
		{
			foreach(pb_Face face in appendedFaces)
				face.ReverseIndices();
		}

		pb.SetSharedIndices(si);
		pb.SetVertices(localVerts);

		return true;
	}

	/**
	 *	\brief Averages shared normals with the mask of 'all' (indices contained in perimeter edge)
	 */
	private static Vector3 Norm( int tri, pb_IntArray[] shared, int[] all, Vector3[] norm )
	{
		int sind = shared.IndexOf(tri);
		
		if(sind < 0)
			return Vector3.zero;

		int[] triGroup = shared[sind];

		Vector3 n = Vector3.zero;
		int count = 0;
		for(int i = 0; i < all.Length; i++)
		{
			// this is a point in the perimeter, add it to the average
			if( System.Array.IndexOf(triGroup, all[i]) > -1 )
			{
				n += norm[all[i]];
				count++;
			}
		}
		return n / (float)count;
	}

	/**
	 *	Edge extrusion override
	 */
	public static bool Extrude(this pb_Object pb, pb_Edge[] edges, float extrudeDistance, bool enableManifoldExtrude, out pb_Edge[] extrudedEdges)
	{
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		List<pb_Edge> validEdges = new List<pb_Edge>();
		List<pb_Face> edgeFaces = new List<pb_Face>();
		
		foreach(pb_Edge e in edges)
		{
			int faceCount = 0;
			pb_Face fa = null;
			foreach(pb_Face f in pb.faces)
			{
				if(f.edges.IndexOf(e, sharedIndices) > -1)
				{
					fa = f;
					if(++faceCount > 1)
						break;
				}

			}

			if(enableManifoldExtrude || faceCount < 2)
			{
				validEdges.Add(e);
				edgeFaces.Add(fa);
			}
		}

		if(validEdges.Count < 1)
		{
			extrudedEdges = null;
			return false;
		}

		Vector3[] localVerts = pb.vertices;
		Vector3[] oNormals = pb.msh.normals;

		int[] allEdgeIndices = new int[validEdges.Count * 2];
		int c = 0;	// har har har
		for(int i = 0; i < validEdges.Count; i++)
		{
			allEdgeIndices[c++] = validEdges[i].x;
			allEdgeIndices[c++] = validEdges[i].y;
		}

		List<pb_Edge> extrudedIndices = new List<pb_Edge>();
		List<pb_Edge> newEdges = new List<pb_Edge>();		// used to set the editor selection to the newly created edges

		/// build out new faces around validEdges

		for(int i = 0; i < validEdges.Count; i++)
		{
			pb_Edge edge = validEdges[i];
			pb_Face face = edgeFaces[i];

			// Averages the normals using only vertices that are on the edge
			Vector3 xnorm = Norm( edge.x, sharedIndices, allEdgeIndices, oNormals );
			Vector3 ynorm = Norm( edge.y, sharedIndices, allEdgeIndices, oNormals );

			int x_sharedIndex = sharedIndices.IndexOf(edge.x);
			int y_sharedIndex = sharedIndices.IndexOf(edge.y);

			pb_Face newFace = pb.AppendFace(
				new Vector3[4]
				{
					localVerts [ edge.x ],
					localVerts [ edge.y ],
					localVerts [ edge.x ] + xnorm.normalized * extrudeDistance,
					localVerts [ edge.y ] + ynorm.normalized * extrudeDistance
				},
				new Color[4] 
				{
					pb.colors[ edge.x ],
					pb.colors[ edge.y ],
					pb.colors[ edge.x ],
					pb.colors[ edge.y ]
				},
				new Vector2[4],
				new pb_Face( new int[6] {2, 1, 0, 2, 3, 1 }, face.material, new pb_UV(), 0, -1, -1, false ),
				new int[4] { x_sharedIndex, y_sharedIndex, -1, -1 });

			newEdges.Add(new pb_Edge(newFace.indices[3], newFace.indices[4]));

			extrudedIndices.Add(new pb_Edge(x_sharedIndex, newFace.indices[3]));
			extrudedIndices.Add(new pb_Edge(y_sharedIndex, newFace.indices[4]));
		}

		sharedIndices = pb.sharedIndices;

		// merge extruded vertex indices with each other
		for(int i = 0; i < extrudedIndices.Count; i++)
		{
			int val = extrudedIndices[i].x;
			for(int n = 0; n < extrudedIndices.Count; n++)
			{
				if(n == i)
					continue;

				if(extrudedIndices[n].x == val)
				{
					pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, extrudedIndices[n].y, extrudedIndices[i].y);
					break;
				}
			}
		}

		pb.SetSharedIndices(sharedIndices);
		pb.RebuildFaceCaches();
		
		extrudedEdges = newEdges.ToArray();
		return true;
	}
#endregion

#region Detach

	/**
	 * Removes the vertex associations so that this face may be moved independently of the main object.
	 */
	public static void DetachFace(this pb_Object pb, pb_Face face)
	{
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		pb_IntArrayUtility.RemoveValues(ref sharedIndices, face.indices);

		// Add these vertices back into the sharedIndices array under it's own entry
		for(int i = 0; i < face.distinctIndices.Length; i++)
		{			
			int[] arr = new int[1] { face.distinctIndices[i] };
			sharedIndices = pbUtil.Add(sharedIndices, new pb_IntArray(arr));
		}

		pb.SetSharedIndices(sharedIndices);
	}

	public static bool DetachFacesToObject(this pb_Object pb, pb_Face[] faces, out pb_Object detachedObject)
	{
		detachedObject = null;

		if(faces.Length < 1 || faces.Length == pb.faces.Length)
			return false;

		int[] primary = new int[faces.Length];
		for(int i = 0; i < primary.Length; i++)
			primary[i] = System.Array.IndexOf(pb.faces, faces[i]);
		
		int[] inverse = new int[pb.faces.Length - primary.Length];
		int n = 0;

		for(int i = 0; i < pb.faces.Length; i++)
			if(System.Array.IndexOf(primary, i) < 0)
				inverse[n++] = i;
				
		detachedObject = pb_Object.InitWithObject(pb);

		detachedObject.transform.position = pb.transform.position;
		detachedObject.transform.localScale = pb.transform.localScale;
		detachedObject.transform.localRotation = pb.transform.localRotation;

		pb.DeleteFaces(primary);
		detachedObject.DeleteFaces(inverse);

		pb.Refresh();
		detachedObject.Refresh();
	
		detachedObject.gameObject.name = pb.gameObject.name + "-detach";
		
		return true;
	}
#endregion

#region Bridge
#if !PROTOTYPE
		public static bool Bridge(this pb_Object pb, pb_Edge a, pb_Edge b) { return pb.Bridge(a, b, true); }
		public static bool Bridge(this pb_Object pb, pb_Edge a, pb_Edge b, bool enforcePerimiterEdgesOnly)
		{
			pb_IntArray[] sharedIndices = pb.GetSharedIndices();

			// Check to see if a face already exists
			if(enforcePerimiterEdgesOnly)
			{
				if( pbMeshUtils.GetNeighborFaces(pb, a).Count > 1 || pbMeshUtils.GetNeighborFaces(pb, b).Count > 1 )
				{
					return false;
				}
			}

			foreach(pb_Face face in pb.faces)
			{
				if(face.edges.IndexOf(a, sharedIndices) >= 0 && face.edges.IndexOf(b, sharedIndices) >= 0)
				{
					Debug.LogWarning("Face already exists between these two edges!");
					return false;
				}
			}
		
			Vector3[] verts = pb.vertices;
			Vector3[] v;
			Color[] c;
			int[] s;
			pb_UV uvs = new pb_UV();
			Material mat = pb_Constant.DefaultMaterial;

			// Get material and UV stuff from the first edge face 
			foreach(pb_Face face in pb.faces)
			{
				if(face.edges.Contains(a))	
				{
					uvs = new pb_UV(face.uv);
					mat = face.material;
					break;
				}
			}

			// Bridge will form a triangle
			if( a.Contains(b.x, sharedIndices) || a.Contains(b.y, sharedIndices) )
			{
				v = new Vector3[3];
				c = new Color[3];
				s = new int[3];

				bool axbx = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.x)], b.x) > -1;
				bool axby = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.x)], b.y) > -1;
				
				bool aybx = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.y)], b.x) > -1;
				bool ayby = System.Array.IndexOf(sharedIndices[sharedIndices.IndexOf(a.y)], b.y) > -1;
				
				if(axbx)
				{	
					v[0] = verts[a.x];
					c[0] = pb.colors[a.x];
					s[0] = sharedIndices.IndexOf(a.x);
					v[1] = verts[a.y];
					c[1] = pb.colors[a.y];
					s[1] = sharedIndices.IndexOf(a.y);
					v[2] = verts[b.y];
					c[2] = pb.colors[b.y];
					s[2] = sharedIndices.IndexOf(b.y);
				}
				else
				if(axby)
				{
					v[0] = verts[a.x];
					c[0] = pb.colors[a.x];
					s[0] = sharedIndices.IndexOf(a.x);
					v[1] = verts[a.y];
					c[1] = pb.colors[a.y];
					s[1] = sharedIndices.IndexOf(a.y);
					v[2] = verts[b.x];
					c[2] = pb.colors[b.x];
					s[2] = sharedIndices.IndexOf(b.x);
				}
				else
				if(aybx)
				{
					v[0] = verts[a.y];
					c[0] = pb.colors[a.y];
					s[0] = sharedIndices.IndexOf(a.y);
					v[1] = verts[a.x];
					c[1] = pb.colors[a.x];
					s[1] = sharedIndices.IndexOf(a.x);
					v[2] = verts[b.y];
					c[2] = pb.colors[b.y];
					s[2] = sharedIndices.IndexOf(b.y);
				}
				else
				if(ayby)
				{
					v[0] = verts[a.y];
					c[0] = pb.colors[a.y];
					s[0] = sharedIndices.IndexOf(a.y);
					v[1] = verts[a.x];
					c[1] = pb.colors[a.x];
					s[1] = sharedIndices.IndexOf(a.x);
					v[2] = verts[b.x];
					c[2] = pb.colors[b.x];
					s[2] = sharedIndices.IndexOf(b.x);
				}

				pb.AppendFace(
					v,
					c,
					new Vector2[v.Length],
					new pb_Face( axbx || axby ? new int[3] {2, 1, 0} : new int[3] {0, 1, 2}, mat, uvs, 0, -1, -1, false ),
					s);

				pb.RebuildFaceCaches();
				pb.Refresh();

				return true;
			}

			// Else, bridge will form a quad

			v = new Vector3[4];
			c = new Color[4];
			s = new int[4]; // shared indices index to add to

			v[0] = verts[a.x];
			c[0] = pb.colors[a.x];
			s[0] = sharedIndices.IndexOf(a.x);
			v[1] = verts[a.y];
			c[1] = pb.colors[a.y];
			s[1] = sharedIndices.IndexOf(a.y);

			Vector3 nrm = Vector3.Cross( verts[b.x]-verts[a.x], verts[a.y]-verts[a.x] ).normalized;
			Vector2[] planed = pb_Math.PlanarProject( new Vector3[4] {verts[a.x], verts[a.y], verts[b.x], verts[b.y] }, nrm );

			Vector2 ipoint = Vector2.zero;
			bool interescts = pb_Math.GetLineSegmentIntersect(planed[0], planed[2], planed[1], planed[3], ref ipoint);

			if(!interescts)
			{
				v[2] = verts[b.x];
				c[2] = pb.colors[b.x];
				s[2] = sharedIndices.IndexOf(b.x);
				v[3] = verts[b.y];
				c[3] = pb.colors[b.y];
				s[3] = sharedIndices.IndexOf(b.y);
			}
			else
			{
				v[2] = verts[b.y];
				c[2] = pb.colors[b.y];
				s[2] = sharedIndices.IndexOf(b.y);
				v[3] = verts[b.x];
				c[3] = pb.colors[b.x];
				s[3] = sharedIndices.IndexOf(b.x);
			}

			pb.AppendFace(
				v,
				c,
				new Vector2[v.Length],
				new pb_Face( new int[6] {2, 1, 0, 2, 3, 1 }, mat, uvs, 0, -1, -1, false ),
				s);

			pb.RebuildFaceCaches();

			return true;
		}
#endif
#endregion

#region Combine

	/**
	 *	\brief Given an array of "donors", this method returns a merged #pb_Object.
	 */
	 public static bool CombineObjects(pb_Object[] pbs, out pb_Object combined)
	 {
	 	combined = null;

	 	if(pbs.Length < 1) return false;

	 	List<Vector3> v = new List<Vector3>();
	 	List<Vector2> u = new List<Vector2>();
	 	List<pb_Face> f = new List<pb_Face>();
	 	List<pb_IntArray> s = new List<pb_IntArray>();
	 	List<pb_IntArray> suv = new List<pb_IntArray>();

	 	foreach(pb_Object pb in pbs)
	 	{
	 		int vertexCount = v.Count;

	 		// Vertices
	 		v.AddRange(pb.VerticesInWorldSpace());

	 		// UVs
	 		u.AddRange(pb.uv);

			// Faces
	 		pb_Face[] faces = new pb_Face[pb.faces.Length];
	 		for(int i = 0; i < faces.Length; i++)
	 		{
	 			faces[i] = new pb_Face(pb.faces[i]);
	 			faces[i].ShiftIndices(vertexCount);
	 			faces[i].RebuildCaches();
	 		}
	 		f.AddRange(faces);

	 		// Shared Indices
	 		pb_IntArray[] si = pb.GetSharedIndices();
	 		for(int i = 0; i < si.Length; i++)
	 		{
	 			for(int n = 0; n < si[i].Length; n++)
	 				si[i][n] += vertexCount;
	 		}
	 		s.AddRange(si);

	 		// Shared Indices UV
	 		{
		 		pb_IntArray[] si_uv = pb.GetSharedIndicesUV();
		 		for(int i = 0; i < si_uv.Length; i++)
		 		{
		 			for(int n = 0; n < si_uv[i].Length; n++)
		 				si_uv[i][n] += vertexCount;
		 		}

		 		suv.AddRange(si_uv);
		 	}
	 	}

	 	combined = pb_Object.CreateInstanceWithElements(v.ToArray(), u.ToArray(), f.ToArray(), s.ToArray(), suv.ToArray());
	 	
	 	combined.CenterPivot(new int[1]{0});

	 	return true;
	 }
#endregion

#region Init

	/**
	* "ProBuilder-ize function"
	*/
	public static pb_Object CreatePbObjectWithTransform(Transform t, bool preserveFaces)
	{
		Mesh m = t.GetComponent<MeshFilter>().sharedMesh;

		Vector3[] m_vertices = m.vertices;
		Color[] m_colors = m.colors;
		Vector2[] m_uvs = m.uv;

		List<Vector3> verts = preserveFaces ? new List<Vector3>(m.vertices) : new List<Vector3>();
		List<Color> cols = preserveFaces ? new List<Color>(m.colors) : new List<Color>();
		List<Vector2> uvs = preserveFaces ? new List<Vector2>(m.uv) : new List<Vector2>();
		List<pb_Face> faces = new List<pb_Face>();

		for(int n = 0; n < m.subMeshCount; n++)
		{
			int[] tris = m.GetTriangles(n);
			for(int i = 0; i < tris.Length; i+=3)
			{
				int index = -1;
				if(preserveFaces)
				{
					for(int j = 0; j < faces.Count; j++)
					{
						if(	faces[j].distinctIndices.Contains(tris[i+0]) ||
							faces[j].distinctIndices.Contains(tris[i+1]) ||
							faces[j].distinctIndices.Contains(tris[i+2]))
						{
							index = j;
							break;
						}
					}
				}

				if(index > -1 && preserveFaces)
				{
					int len = faces[index].indices.Length;
					int[] arr = new int[len + 3];
					System.Array.Copy(faces[index].indices, 0, arr, 0, len);
					arr[len+0] = tris[i+0];
					arr[len+1] = tris[i+1];
					arr[len+2] = tris[i+2];
					faces[index].SetIndices(arr);
					faces[index].RebuildCaches();
				}
				else
				{
					int[] faceTris;

					if(preserveFaces)
					{
						faceTris = new int[3]
						{
							tris[i+0],
							tris[i+1],
							tris[i+2]	
						};
					}
					else
					{
						verts.Add(m_vertices[tris[i+0]]);
						verts.Add(m_vertices[tris[i+1]]);
						verts.Add(m_vertices[tris[i+2]]);

						cols.Add(m_colors[tris[i+0]]);
						cols.Add(m_colors[tris[i+1]]);
						cols.Add(m_colors[tris[i+2]]);

						uvs.Add(m_uvs[tris[i+0]]);
						uvs.Add(m_uvs[tris[i+1]]);
						uvs.Add(m_uvs[tris[i+2]]);

						faceTris = new int[3] { i+0, i+1, i+2 };
					}

					faces.Add( 
						new pb_Face(
							faceTris,
							t.GetComponent<MeshRenderer>().sharedMaterials[n],
							new pb_UV(),
							0,		// smoothing group
							-1,		// texture group
							-1,		// element group
							true 	// manualUV 
						));					
				}
			}
		}

		GameObject go = (GameObject)GameObject.Instantiate(t.gameObject);
		go.GetComponent<MeshFilter>().sharedMesh = null;

		pb_Object pb = go.AddComponent<pb_Object>();
		pb.GeometryWithVerticesFaces(verts.ToArray(), faces.ToArray());

		pb.SetColors(cols.ToArray());
		pb.SetUV(uvs.ToArray());

		pb.SetName(t.name);
			
		go.transform.position = t.position;
		go.transform.localRotation = t.localRotation;
		go.transform.localScale = t.localScale;

		pb.CenterPivot(null);

		return pb;
	}
#endregion
	}
}