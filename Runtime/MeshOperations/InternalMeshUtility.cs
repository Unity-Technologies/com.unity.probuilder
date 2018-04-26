using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	// Utilities for working with pb_Object meshes.  The operations here only operate on the
	// element caches in pb_Object- they do not affect the UnityEngine.Mesh.  You should call
	// ToMesh() prior to invoking these methods, then Refresh() & optionally Optimize() post.
	//
	// The general purpose pb_MeshOps and pb_VertexOps classes are being phased out in favor of classes specific to one purpose
	static class InternalMeshUtility
	{
		/// <summary>
		/// Center the mesh pivot at the average of passed indices.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices"></param>
		public static void CenterPivot(this ProBuilderMesh pb, int[] indices)
		{
			Vector3 center = Vector3.zero;

			if(indices != null && indices.Length > 0)
			{
				Vector3[] verts = pb.VerticesInWorldSpace(indices);

				foreach (Vector3 v in verts)
					center += v;

				center /= (float)verts.Length;
			}
			else
			{
				center = pb.transform.TransformPoint(pb.mesh.bounds.center);
			}

			Vector3 dir = (pb.transform.position - center);

			pb.transform.position = center;

			pb.ToMesh();
			pb.TranslateVerticesInWorldSpace(pb.mesh.triangles, dir);
			pb.Refresh();
		}

		/// <summary>
		/// Move the object pivot to worldPosition.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="worldPosition"></param>
		public static void CenterPivot(this ProBuilderMesh pb, Vector3 worldPosition)
		{
			Vector3 offset = pb.transform.position - worldPosition;

			pb.transform.position = worldPosition;

			pb.ToMesh();
			pb.TranslateVerticesInWorldSpace(pb.mesh.triangles, offset);
			pb.Refresh();
		}

		/// <summary>
		/// Scale vertices and set transform.localScale to Vector3.one.
		/// </summary>
		/// <param name="pb"></param>
		public static void FreezeScaleTransform(this ProBuilderMesh pb)
		{
			Vector3[] v = pb.positionsInternal;

			for(var i = 0; i < v.Length; i++)
				v[i] = Vector3.Scale(v[i], pb.transform.localScale);

			pb.transform.localScale = new Vector3(1f, 1f, 1f);
		}

		/// <summary>
		/// Extrudes the passed faces by extrudeDistance amount.  @faces will remain valid.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <param name="extrudeDistance"></param>
		/// <returns></returns>
		[System.Obsolete("Please use `bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float distance)`")]
		public static bool Extrude(this ProBuilderMesh pb, Face[] faces, float extrudeDistance)
		{
			Face[] appended;
			return Extrude(pb, faces, extrudeDistance, true, out appended);
		}

		/// <summary>
		/// Extrudes passed faces on their normal axis using extrudeDistance.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <param name="extrudeDistance"></param>
		/// <param name="extrudeAsGroup"></param>
		/// <param name="appendedFaces"></param>
		/// <returns></returns>
		[System.Obsolete("Please use `bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float distance)`")]
		public static bool Extrude(this ProBuilderMesh pb, Face[] faces, float extrudeDistance, bool extrudeAsGroup, out Face[] appendedFaces)
		{
			return Extrude(pb, faces, extrudeAsGroup ? ExtrudeMethod.VertexNormal : ExtrudeMethod.IndividualFaces, extrudeDistance, out appendedFaces);
		}

		[System.Obsolete("Please use `bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float distance)`")]
		public static bool Extrude(this ProBuilderMesh pb, Face[] faces, ExtrudeMethod method, float extrudeDistance, out Face[] appendedFaces)
		{
			appendedFaces = null;

			if(faces == null || faces.Length < 1)
				return false;

			IntArray[] sharedIndices = pb.GetSharedIndices();
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();

			int vertexCount = pb.vertexCount;
			Vector3[] localVerts = pb.positionsInternal;
			bool extrudeAsGroup = method != ExtrudeMethod.IndividualFaces;

			Edge[][] perimeterEdges = extrudeAsGroup ? new Edge[1][] { ElementSelection.GetPerimeterEdges(lookup, faces).ToArray() } : faces.Select(x => x.edges).ToArray();

			if(perimeterEdges.Length < 1 || (extrudeAsGroup && perimeterEdges[0].Length < 3))
			{
				Debug.LogWarning("No perimeter edges found.  Try deselecting and reselecting this object and trying again.");
				return false;
			}

			Face[][] edgeFaces = new Face[perimeterEdges.Length][];	// can't assume faces and perimiter edges will be 1:1 - so calculate perimeters then extract face information
			int[][] allEdgeIndices = new int[perimeterEdges.Length][];
			int c = 0;

			for(int i = 0; i < perimeterEdges.Length; i++)
			{
				c = 0;
				allEdgeIndices[i] = new int[perimeterEdges[i].Length * 2];
				edgeFaces[i] = new Face[perimeterEdges[i].Length];

				for(int n = 0; n < perimeterEdges[i].Length; n++)
				{
					// gets the faces associated with each perimeter edge
					foreach(Face face in faces)
					{
						if(face.edges.Contains(perimeterEdges[i][n]))
						{
							edgeFaces[i][n] = face;
							break;
						}
					}

					allEdgeIndices[i][c++] = perimeterEdges[i][n].x;
					allEdgeIndices[i][c++] = perimeterEdges[i][n].y;
				}
			}

			List<Edge>[] extrudedIndices = new List<Edge>[perimeterEdges.Length];
			Vector3[] normals = pb.mesh.normals;
			Vector3[] extrusionPerIndex = new Vector3[vertexCount];

			List<Vector3[]> append_vertices = new List<Vector3[]>();
			List<Color[]> append_color = new List<Color[]>();
			List<Vector2[]> append_uv = new List<Vector2[]>();
			List<Face> append_face = new List<Face>();
			List<int[]> append_shared = new List<int[]>();

			// build out new faces around edges

			for(int i = 0; i < perimeterEdges.Length; i++)
			{
				extrudedIndices[i] = new List<Edge>();

				for(int n = 0; n < perimeterEdges[i].Length; n++)
				{
					Edge edge = perimeterEdges[i][n];
					Face face = edgeFaces[i][n];

					// Averages the normals using only vertices that are on the edge
					Vector3 nrm = ProBuilderMath.Normal(pb, face);

					Vector3 xnorm = Vector3.zero;
					Vector3 ynorm = Vector3.zero;

					// don't bother getting vertex normals if not auto-extruding
					if( Mathf.Abs(extrudeDistance) > Mathf.Epsilon)
					{
						if( !extrudeAsGroup )
						{
							xnorm = nrm;
							ynorm = nrm;
						}
						else
						{
							xnorm = AverageNormalWithIndices(sharedIndices[lookup[edge.x]], allEdgeIndices[i], normals );
							ynorm = AverageNormalWithIndices(sharedIndices[lookup[edge.y]], allEdgeIndices[i], normals );
						}
					}

					int x_sharedIndex = lookup[edge.x];
					int y_sharedIndex = lookup[edge.y];

					// if the centers of extruded faces should uniformly be extruded some edges will need
					// to be shortened or lengthened.
					float compensatedDistanceX = extrudeDistance, compensatedDistanceY = extrudeDistance;

					if(method == ExtrudeMethod.FaceNormal)
					{
						compensatedDistanceX = ProBuilderMath.Secant(Vector3.Angle(nrm, xnorm) * Mathf.Deg2Rad) * extrudeDistance;
						compensatedDistanceY = ProBuilderMath.Secant(Vector3.Angle(nrm, ynorm) * Mathf.Deg2Rad) * extrudeDistance;
					}

					extrusionPerIndex[edge.x] = xnorm.normalized * compensatedDistanceX;
					extrusionPerIndex[edge.y] = ynorm.normalized * compensatedDistanceY;

					append_vertices.Add( new Vector3[]
						{
							localVerts [ edge.x ],
							localVerts [ edge.y ],
							localVerts [ edge.x ] + extrusionPerIndex[edge.x],
							localVerts [ edge.y ] + extrusionPerIndex[edge.y]
						});

					append_color.Add( new Color[]
						{
							pb.colorsInternal[ edge.x ],
							pb.colorsInternal[ edge.y ],
							pb.colorsInternal[ edge.x ],
							pb.colorsInternal[ edge.y ]
						});

					append_uv.Add( new Vector2[4] );

					append_face.Add( new Face(
							new int[6] {0, 1, 2, 1, 3, 2},			// indices
							face.material,							// material
							new AutoUnwrapSettings(face.uv),						// UV material
							face.smoothingGroup,					// smoothing group
							-1,										// texture group
							-1,										// uv element group
							false)									// manualUV flag
							);

					append_shared.Add( new int[4]
						{
							x_sharedIndex,
							y_sharedIndex,
							-1,
							-1
						});

					extrudedIndices[i].Add(new Edge(x_sharedIndex, -1));
					extrudedIndices[i].Add(new Edge(y_sharedIndex, -1));
				}
			}

			appendedFaces = pb.AppendFaces( append_vertices.ToArray(), append_color.ToArray(), append_uv.ToArray(), append_face.ToArray(), append_shared.ToArray() );

			// x = shared index, y = triangle (only known once faces are appended to pb_Object)
			for(int i = 0, f = 0; i < extrudedIndices.Length; i++)
			{
				for(int n = 0; n < extrudedIndices[i].Count; n+=2)
				{
					extrudedIndices[i][n+0] = new Edge(extrudedIndices[i][n+0].x, appendedFaces[f].indices[2]);
					extrudedIndices[i][n+1] = new Edge(extrudedIndices[i][n+1].x, appendedFaces[f++].indices[4]);
				}
			}

			IntArray[] si = pb.sharedIndicesInternal;	// leave the sharedIndices copy alone since we need the un-altered version later
			Dictionary<int, int> welds = si.ToDictionary();

			// Weld side-wall top vertices together, both grouped and non-grouped need this.
			for(int f = 0; f < extrudedIndices.Length; f++)
			{
				for(int i = 0; i < extrudedIndices[f].Count-1; i++)
				{
					int val = extrudedIndices[f][i].x;
					for(int n = i+1; n < extrudedIndices[f].Count; n++)
					{
						if(extrudedIndices[f][n].x == val)
						{
							welds[extrudedIndices[f][i].y] = welds[extrudedIndices[f][n].y];
							break;
						}
					}
				}
			}

			localVerts = pb.positionsInternal;

			// Remove smoothing and texture group flags
			foreach(Face f in faces)
			{
				f.smoothingGroup = Smoothing.smoothingGroupNone;
				f.textureGroup = -1;
			}

			if(extrudeAsGroup)
			{
				foreach(Face f in faces)
				{
					int[] distinctIndices = f.distinctIndices;

					// Merge in-group face seams
					foreach(int ind in distinctIndices)
					{
						int oldIndex = si.IndexOf(ind);

						for(int n = 0; n < allEdgeIndices.Length; n++)
						{
							for(int i = 0; i < extrudedIndices[n].Count; i++)
							{
								if(oldIndex == extrudedIndices[n][i].x)
								{
									welds[ind] = welds[extrudedIndices[n][i].y];
									break;
								}
							}
						}
					}
				}
			}
			else
			// If extruding as separate faces, weld each face to the tops of the bridging faces
			{
				for(int i = 0; i < edgeFaces.Length; i++)
				{
					foreach(int n in edgeFaces[i].SelectMany(x => x.distinctIndices))
					{
						int old_si_index = lookup[n];
						int match = extrudedIndices[i].FindIndex(x => x.x == old_si_index);

						if(match < 0)
							continue;

						int match_tri_index = extrudedIndices[i][match].y;

						if(welds.ContainsKey(match_tri_index))
						{
							welds[n] = welds[match_tri_index];
						}
					}
				}
			}

			si = IntArrayUtility.ToSharedIndices(welds);

			pb.SplitUVs(Face.AllTriangles(faces));

			/**
			 * Move the inside faces to the top of the extrusion
			 *
			 * This is a separate loop cause the one above this must completely merge all sharedindices prior to
			 * checking the normal averages
			 *
			 */
			int[] allIndices = faces.SelectMany(x => x.distinctIndices).ToArray();
			float compensatedDistance = extrudeDistance;

			foreach(Face f in faces)
			{
				Vector3 faceNormal = ProBuilderMath.Normal(localVerts[f.indices[0]], localVerts[f.indices[1]], localVerts[f.indices[2]]);
				Vector3 norm = extrudeAsGroup ? Vector3.zero : faceNormal;

				foreach(int ind in f.distinctIndices)
				{
					if(extrudeAsGroup)
					{
						norm = AverageNormalWithIndices(sharedIndices[lookup[ind]], allIndices, normals);

						if(method == ExtrudeMethod.FaceNormal)
						{
							compensatedDistance = ProBuilderMath.Secant(Vector3.Angle(faceNormal, norm) * Mathf.Deg2Rad) * extrudeDistance;
						}
					}

					localVerts[ind] += norm.normalized * compensatedDistance;
				}
			}

			pb.SetSharedIndices(si);

			List<Face> allModified = new List<Face>(appendedFaces);
			allModified.AddRange(faces);
			HashSet<Face> sources = new HashSet<Face>(faces);
			List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb, allModified);

			foreach(WingedEdge wing in wings)
			{
				if(sources.Contains(wing.face))
				{
					sources.Remove(wing.face);

					foreach(WingedEdge w in wing)
						Normals.ConformOppositeNormal(w);
				}
			}

			return true;
		}

		/// <summary>
		/// Averages shared normals with the mask of 'all' (indices contained in perimeter edge)
		/// </summary>
		/// <param name="shared"></param>
		/// <param name="all"></param>
		/// <param name="norm"></param>
		/// <returns></returns>
		internal static Vector3 AverageNormalWithIndices(int[] shared, int[] all, Vector3[] norm )
		{
			Vector3 n = Vector3.zero;
			int count = 0;
			for(int i = 0; i < all.Length; i++)
			{
				// this is a point in the perimeter, add it to the average
				if( System.Array.IndexOf(shared, all[i]) > -1 )
				{
					n += norm[all[i]];
					count++;
				}
			}
			return (n / (float)count);
		}

		/// <summary>
		/// Removes the vertex associations so that this face may be moved independently of the main object.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		public static List<Face> DetachFaces(this ProBuilderMesh pb, IEnumerable<Face> faces)
		{
			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(pb));
			int sharedIndicesOffset = pb.sharedIndicesInternal.Length;
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();

			List<FaceRebuildData> detached = new List<FaceRebuildData>();

			foreach(Face face in faces)
			{
				FaceRebuildData data = new FaceRebuildData();
				data.vertices = new List<Vertex>();
				data.sharedIndices = new List<int>();
				data.face = new Face(face);

				Dictionary<int, int> match = new Dictionary<int, int>();
				int[] indices = new int[face.indices.Length];

				for(int i = 0; i < face.indices.Length; i++)
				{
					int local;

					if( match.TryGetValue(face.indices[i], out local) )
					{
						indices[i] = local;
					}
					else
					{
						local = data.vertices.Count;
						indices[i] = local;
						match.Add(face.indices[i], local);
						data.vertices.Add(vertices[face.indices[i]]);
						data.sharedIndices.Add(lookup[face.indices[i]] + sharedIndicesOffset);
					}
				}

				data.face.SetIndices(indices.ToArray());
				detached.Add(data);
			}

			FaceRebuildData.Apply(detached, pb, vertices, null, lookup);
			pb.DeleteFaces(faces);

			pb.ToMesh();

			return detached.Select(x => x.face).ToList();
		}

	#if !PROTOTYPE

		/// <summary>
		/// Insert a face between two edges.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="enforcePerimiterEdgesOnly"></param>
		/// <returns></returns>
		public static bool Bridge(this ProBuilderMesh pb, Edge a, Edge b, bool enforcePerimiterEdgesOnly = false)
			{
				IntArray[] sharedIndices = pb.GetSharedIndices();
				Dictionary<int, int> lookup = sharedIndices.ToDictionary();

				// Check to see if a face already exists
				if(enforcePerimiterEdgesOnly)
				{
					if( ElementSelection.GetNeighborFaces(pb, a).Count > 1 || ElementSelection.GetNeighborFaces(pb, b).Count > 1 )
					{
						return false;
					}
				}

				foreach(Face face in pb.facesInternal)
				{
					if(face.edges.IndexOf(a, lookup) >= 0 && face.edges.IndexOf(b, lookup) >= 0)
					{
						Debug.LogWarning("Face already exists between these two edges!");
						return false;
					}
				}

				Vector3[] verts = pb.positionsInternal;
				Vector3[] v;
				Color[] c;
				int[] s;
				AutoUnwrapSettings uvs = new AutoUnwrapSettings();
				Material mat = BuiltinMaterials.DefaultMaterial;

				// Get material and UV stuff from the first edge face
				SimpleTuple<Face, Edge> faceAndEdge = null;

				if(!EdgeExtension.ValidateEdge(pb, a, out faceAndEdge))
					EdgeExtension.ValidateEdge(pb, b, out faceAndEdge);

				if(faceAndEdge != null)
				{
					uvs = new AutoUnwrapSettings(faceAndEdge.item1.uv);
					mat = faceAndEdge.item1.material;
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
						c[0] = pb.colorsInternal[a.x];
						s[0] = sharedIndices.IndexOf(a.x);
						v[1] = verts[a.y];
						c[1] = pb.colorsInternal[a.y];
						s[1] = sharedIndices.IndexOf(a.y);
						v[2] = verts[b.y];
						c[2] = pb.colorsInternal[b.y];
						s[2] = sharedIndices.IndexOf(b.y);
					}
					else
					if(axby)
					{
						v[0] = verts[a.x];
						c[0] = pb.colorsInternal[a.x];
						s[0] = sharedIndices.IndexOf(a.x);
						v[1] = verts[a.y];
						c[1] = pb.colorsInternal[a.y];
						s[1] = sharedIndices.IndexOf(a.y);
						v[2] = verts[b.x];
						c[2] = pb.colorsInternal[b.x];
						s[2] = sharedIndices.IndexOf(b.x);
					}
					else
					if(aybx)
					{
						v[0] = verts[a.y];
						c[0] = pb.colorsInternal[a.y];
						s[0] = sharedIndices.IndexOf(a.y);
						v[1] = verts[a.x];
						c[1] = pb.colorsInternal[a.x];
						s[1] = sharedIndices.IndexOf(a.x);
						v[2] = verts[b.y];
						c[2] = pb.colorsInternal[b.y];
						s[2] = sharedIndices.IndexOf(b.y);
					}
					else
					if(ayby)
					{
						v[0] = verts[a.y];
						c[0] = pb.colorsInternal[a.y];
						s[0] = sharedIndices.IndexOf(a.y);
						v[1] = verts[a.x];
						c[1] = pb.colorsInternal[a.x];
						s[1] = sharedIndices.IndexOf(a.x);
						v[2] = verts[b.x];
						c[2] = pb.colorsInternal[b.x];
						s[2] = sharedIndices.IndexOf(b.x);
					}

					pb.AppendFace(
						v,
						c,
						new Vector2[v.Length],
						new Face( axbx || axby ? new int[3] {2, 1, 0} : new int[3] {0, 1, 2}, mat, uvs, 0, -1, -1, false ),
						s);

					return true;
				}

				// Else, bridge will form a quad

				v = new Vector3[4];
				c = new Color[4];
				s = new int[4]; // shared indices index to add to

				v[0] = verts[a.x];
				c[0] = pb.colorsInternal[a.x];
				s[0] = sharedIndices.IndexOf(a.x);
				v[1] = verts[a.y];
				c[1] = pb.colorsInternal[a.y];
				s[1] = sharedIndices.IndexOf(a.y);

				Vector3 nrm = Vector3.Cross( verts[b.x]-verts[a.x], verts[a.y]-verts[a.x] ).normalized;
				Vector2[] planed = Projection.PlanarProject( new Vector3[4] {verts[a.x], verts[a.y], verts[b.x], verts[b.y] }, nrm );

				Vector2 ipoint = Vector2.zero;
				bool intersects = ProBuilderMath.GetLineSegmentIntersect(planed[0], planed[2], planed[1], planed[3], ref ipoint);

				if(!intersects)
				{
					v[2] = verts[b.x];
					c[2] = pb.colorsInternal[b.x];
					s[2] = sharedIndices.IndexOf(b.x);
					v[3] = verts[b.y];
					c[3] = pb.colorsInternal[b.y];
					s[3] = sharedIndices.IndexOf(b.y);
				}
				else
				{
					v[2] = verts[b.y];
					c[2] = pb.colorsInternal[b.y];
					s[2] = sharedIndices.IndexOf(b.y);
					v[3] = verts[b.x];
					c[3] = pb.colorsInternal[b.x];
					s[3] = sharedIndices.IndexOf(b.x);
				}

				pb.AppendFace(
					v,
					c,
					new Vector2[v.Length],
					new Face( new int[6] {2, 1, 0, 2, 3, 1 }, mat, uvs, 0, -1, -1, false ),
					s);

				return true;
			}
	#endif

		/// <summary>
		/// Given an array of "donors", this method returns a merged pb_Object.
		/// </summary>
		/// <param name="pbs"></param>
		/// <param name="combined"></param>
		/// <returns></returns>
		public static bool CombineObjects(ProBuilderMesh[] pbs, out ProBuilderMesh combined)
		 {
			combined = null;

			if(pbs.Length < 1) return false;

			List<Vector3> v = new List<Vector3>();
			List<Vector2> u = new List<Vector2>();
			List<Color> c = new List<Color>();
			List<Face> f = new List<Face>();
			List<IntArray> s = new List<IntArray>();
			List<IntArray> suv = new List<IntArray>();

			foreach(ProBuilderMesh pb in pbs)
			{
				int vertexCount = v.Count;

				// Vertices
				v.AddRange(pb.VerticesInWorldSpace());

				// UVs
				u.AddRange(pb.texturesInternal);

				// Colors
				c.AddRange(pb.colorsInternal);

				// Faces
				Face[] faces = new Face[pb.facesInternal.Length];
				for(int i = 0; i < faces.Length; i++)
				{
					faces[i] = new Face(pb.facesInternal[i]);
					faces[i].manualUV = true;
					faces[i].ShiftIndices(vertexCount);
					faces[i].RebuildCaches();
				}
				f.AddRange(faces);

				// Shared Indices
				IntArray[] si = pb.GetSharedIndices();
				for(int i = 0; i < si.Length; i++)
				{
					for(int n = 0; n < si[i].length; n++)
						si[i][n] += vertexCount;
				}
				s.AddRange(si);

				// Shared Indices UV
				{
					IntArray[] si_uv = pb.GetSharedIndicesUV();
					for(int i = 0; i < si_uv.Length; i++)
					{
						for(int n = 0; n < si_uv[i].length; n++)
							si_uv[i][n] += vertexCount;
					}

					suv.AddRange(si_uv);
				}
			}

			GameObject go = Object.Instantiate(pbs[0].gameObject);
			go.transform.position = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;

			// Destroy the children
			foreach(Transform t in go.transform)
				Object.DestroyImmediate(t.gameObject);

			if(go.GetComponent<ProBuilderMesh>()) Object.DestroyImmediate(go.GetComponent<ProBuilderMesh>());
			if(go.GetComponent<Entity>()) Object.DestroyImmediate(go.GetComponent<Entity>());

			combined = go.AddComponent<ProBuilderMesh>();

			combined.SetPositions(v.ToArray());
			combined.SetUVs(u.ToArray());
			combined.SetColors(c.ToArray());
			combined.SetFaces(f.ToArray());

			combined.sharedIndicesInternal = s.ToArray();
			combined.SetSharedIndicesUV(suv.ToArray());
			combined.ToMesh();
			combined.CenterPivot( pbs[0].transform.position );
			combined.Refresh();

			// refresh donors since deleting the children of the instantiated object could cause them to lose references
			foreach(ProBuilderMesh pb in pbs)
				pb.Verify();

			return true;
		 }

		/// <summary>
		/// "ProBuilder-ize" function
		/// </summary>
		/// <param name="t"></param>
		/// <param name="preserveFaces"></param>
		/// <returns></returns>
		public static ProBuilderMesh CreatePbObjectWithTransform(Transform t, bool preserveFaces)
		{
			Mesh m = t.GetComponent<MeshFilter>().sharedMesh;

			Vector3[] m_vertices 	= MeshUtility.GetMeshAttribute<Vector3[]>(t.gameObject, x => x.vertices);
			Color[] m_colors 		= MeshUtility.GetMeshAttribute<Color[]>(t.gameObject, x => x.colors);
			Vector2[] m_uvs 		= MeshUtility.GetMeshAttribute<Vector2[]>(t.gameObject, x => x.uv);

			List<Vector3> verts = preserveFaces ? new List<Vector3>(m.vertices) : new List<Vector3>();
			List<Color> cols = preserveFaces ? new List<Color>(m.colors) : new List<Color>();
			List<Vector2> uvs = preserveFaces ? new List<Vector2>(m.uv) : new List<Vector2>();
			List<Face> faces = new List<Face>();

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

							cols.Add(m_colors != null ? m_colors[tris[i+0]] : Color.white);
							cols.Add(m_colors != null ? m_colors[tris[i+1]] : Color.white);
							cols.Add(m_colors != null ? m_colors[tris[i+2]] : Color.white);

							uvs.Add(m_uvs[tris[i+0]]);
							uvs.Add(m_uvs[tris[i+1]]);
							uvs.Add(m_uvs[tris[i+2]]);

							faceTris = new int[3] { i+0, i+1, i+2 };
						}

						faces.Add(
							new Face(
								faceTris,
								t.GetComponent<MeshRenderer>().sharedMaterials[n],
								new AutoUnwrapSettings(),
								0,		// smoothing group
								-1,		// texture group
								-1,		// element group
								true 	// manualUV
							));
					}
				}
			}

			GameObject go = (GameObject) Object.Instantiate(t.gameObject);
			go.GetComponent<MeshFilter>().sharedMesh = null;

			ProBuilderMesh pb = go.AddComponent<ProBuilderMesh>();
			pb.GeometryWithVerticesFaces(verts.ToArray(), faces.ToArray());

			pb.colorsInternal = cols.ToArray();
			pb.SetUVs(uvs.ToArray());

			pb.gameObject.name = t.name;

			go.transform.position = t.position;
			go.transform.localRotation = t.localRotation;
			go.transform.localScale = t.localScale;

			pb.CenterPivot(null);

			return pb;
		}

		/// <summary>
		/// ProBuilderize in-place function. You must call ToMesh() and Refresh() after
		/// returning from this function, as this only creates the pb_Object and sets its
		/// fields. This allows you to record the mesh and gameObject for Undo operations.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="preserveFaces"></param>
		/// <returns></returns>
		public static bool ResetPbObjectWithMeshFilter(ProBuilderMesh pb, bool preserveFaces)
		{
			MeshFilter mf = pb.gameObject.GetComponent<MeshFilter>();

			if(mf == null || mf.sharedMesh == null)
			{
				Log.Error(pb.name + " does not have a mesh or Mesh Filter component.");
				return false;
			}

			Mesh m = mf.sharedMesh;

			int vertexCount 		= m.vertexCount;
			Vector3[] m_vertices 	= MeshUtility.GetMeshAttribute<Vector3[]>(pb.gameObject, x => x.vertices);
			Color[] m_colors 		= MeshUtility.GetMeshAttribute<Color[]>(pb.gameObject, x => x.colors);
			Vector2[] m_uvs 		= MeshUtility.GetMeshAttribute<Vector2[]>(pb.gameObject, x => x.uv);

			List<Vector3> verts 	= preserveFaces ? new List<Vector3>(m.vertices) : new List<Vector3>();
			List<Color> cols 		= preserveFaces ? new List<Color>(m.colors) : new List<Color>();
			List<Vector2> uvs 		= preserveFaces ? new List<Vector2>(m.uv) : new List<Vector2>();
			List<Face> faces 	= new List<Face>();

			MeshRenderer mr = pb.gameObject.GetComponent<MeshRenderer>();
			if(mr == null) mr = pb.gameObject.AddComponent<MeshRenderer>();

			Material[] sharedMaterials = mr.sharedMaterials;
			int mat_length = sharedMaterials.Length;

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

							cols.Add(m_colors != null && m_colors.Length == vertexCount ? m_colors[tris[i+0]] : Color.white);
							cols.Add(m_colors != null && m_colors.Length == vertexCount ? m_colors[tris[i+1]] : Color.white);
							cols.Add(m_colors != null && m_colors.Length == vertexCount ? m_colors[tris[i+2]] : Color.white);

							uvs.Add(m_uvs[tris[i+0]]);
							uvs.Add(m_uvs[tris[i+1]]);
							uvs.Add(m_uvs[tris[i+2]]);

							faceTris = new int[3] { i+0, i+1, i+2 };
						}

						faces.Add(
							new Face(
								faceTris,
								sharedMaterials[n >= mat_length ? mat_length - 1 : n],
								new AutoUnwrapSettings(),
								0,		// smoothing group
								-1,		// texture group
								-1,		// element group
								true 	// manualUV
							));
					}
				}
			}

			pb.positionsInternal = verts.ToArray();
			pb.texturesInternal = uvs.ToArray();
			pb.facesInternal = faces.ToArray();
			pb.sharedIndicesInternal = IntArrayUtility.ExtractSharedIndices(verts.ToArray());
			pb.colorsInternal = cols.ToArray();

			return true;
		}
	}
}
