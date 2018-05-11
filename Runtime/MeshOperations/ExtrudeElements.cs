using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// Face and edge extrusion.
	/// </summary>
	public static class ExtrudeElements
	{
		/// <summary>
		/// Extrude a collection of faces.
		/// </summary>
		/// <param name="mesh">The source mesh.</param>
		/// <param name="faces">The faces to extrude.</param>
		/// <param name="method">Describes how faces are extruded.</param>
		/// <param name="distance">The distance to extrude faces.</param>
		/// <returns>True on success, false if the action failed.</returns>
		public static bool Extrude(this ProBuilderMesh mesh, IEnumerable<Face> faces, ExtrudeMethod method, float distance)
		{
			switch(method)
			{
				case ExtrudeMethod.IndividualFaces:
					return ExtrudePerFace(mesh, faces, distance);

				default:
					return ExtrudeAsGroups(mesh, faces, method == ExtrudeMethod.FaceNormal, distance);
			}
		}

		/// <summary>
		/// Extrude a collection of edges.
		/// </summary>
		/// <param name="mesh">The source mesh.</param>
		/// <param name="edges">The edges to extrude.</param>
		/// <param name="distance">The distance to extrude.</param>
		/// <param name="extrudeAsGroup">If true adjacent edges will be extruded retaining a shared vertex, if false the shared vertex will be split.</param>
		/// <param name="enableManifoldExtrude">Pass true to allow this function to extrude manifold edges, false to disallow.</param>
		/// <returns>The extruded edges.</returns>
		public static Edge[] Extrude(this ProBuilderMesh mesh, IEnumerable<Edge> edges, float distance, bool extrudeAsGroup, bool enableManifoldExtrude)
		{
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            if (edges == null)
                throw new System.ArgumentNullException("edges");

            IntArray[] sharedIndices = mesh.sharedIndicesInternal;
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();

			List<Edge> validEdges = new List<Edge>();
			List<Face> edgeFaces = new List<Face>();

			foreach(Edge e in edges)
			{
				int faceCount = 0;
				Face fa = null;

				foreach(Face f in mesh.facesInternal)
				{
					if(f.edgesInternal.IndexOf(e, lookup) > -1)
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
				return null;

			Vector3[] localVerts = mesh.positionsInternal;
			Vector3[] oNormals = mesh.mesh.normals;

			int[] allEdgeIndices = new int[validEdges.Count * 2];
			int c = 0;
			for(int i = 0; i < validEdges.Count; i++)
			{
				allEdgeIndices[c++] = validEdges[i].x;
				allEdgeIndices[c++] = validEdges[i].y;
			}

			List<Edge> extrudedIndices = new List<Edge>();
			// used to set the editor selection to the newly created edges
			List<Edge> newEdges = new List<Edge>();

			// build out new faces around validEdges

			for(int i = 0; i < validEdges.Count; i++)
			{
				Edge edge = validEdges[i];
				Face face = edgeFaces[i];

				// Averages the normals using only vertices that are on the edge
				Vector3 xnorm = extrudeAsGroup ? InternalMeshUtility.AverageNormalWithIndices( sharedIndices[lookup[edge.x]], allEdgeIndices, oNormals ) : Math.Normal(mesh, face);
				Vector3 ynorm = extrudeAsGroup ? InternalMeshUtility.AverageNormalWithIndices( sharedIndices[lookup[edge.y]], allEdgeIndices, oNormals ) : Math.Normal(mesh, face);

				int x_sharedIndex = lookup[edge.x];
				int y_sharedIndex = lookup[edge.y];

				Face newFace = mesh.AppendFace(
					new Vector3[4]
					{
						localVerts [ edge.x ],
						localVerts [ edge.y ],
						localVerts [ edge.x ] + xnorm.normalized * distance,
						localVerts [ edge.y ] + ynorm.normalized * distance
					},
					new Color[4]
					{
						mesh.colorsInternal[ edge.x ],
						mesh.colorsInternal[ edge.y ],
						mesh.colorsInternal[ edge.x ],
						mesh.colorsInternal[ edge.y ]
					},
					new Vector2[4],
					new Face( new int[6] {2, 1, 0, 2, 3, 1 }, face.material, new AutoUnwrapSettings(), 0, -1, -1, false ),
					new int[4] { x_sharedIndex, y_sharedIndex, -1, -1 });

				newEdges.Add(new Edge(newFace.indices[3], newFace.indices[4]));

				extrudedIndices.Add(new Edge(x_sharedIndex, newFace.indices[3]));
				extrudedIndices.Add(new Edge(y_sharedIndex, newFace.indices[4]));
			}

			sharedIndices = mesh.sharedIndicesInternal;

			// merge extruded vertex indices with each other
			if(extrudeAsGroup)
			{
				for(int i = 0; i < extrudedIndices.Count; i++)
				{
					int val = extrudedIndices[i].x;
					for(int n = 0; n < extrudedIndices.Count; n++)
					{
						if(n == i)
							continue;

						if(extrudedIndices[n].x == val)
						{
							IntArrayUtility.MergeSharedIndices(ref sharedIndices, extrudedIndices[n].y, extrudedIndices[i].y);
							break;
						}
					}
				}
			}

			mesh.sharedIndicesInternal = sharedIndices;

			// todo Should only need to invalidate caches on affected faces
			foreach(Face f in mesh.facesInternal)
				f.InvalidateCache();

			return newEdges.ToArray();
		}

		/// <summary>
		/// Split any shared vertices so that this face may be moved independently of the main object.
		/// </summary>
		/// <param name="mesh">The source mesh.</param>
		/// <param name="faces">The faces to split from the mesh.</param>
		/// <returns>The faces created forming the detached face group.</returns>
		public static List<Face> DetachFaces(this ProBuilderMesh mesh, IEnumerable<Face> faces)
		{
			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(mesh));
			int sharedIndicesOffset = mesh.sharedIndicesInternal.Length;
			Dictionary<int, int> lookup = mesh.sharedIndicesInternal.ToDictionary();

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

				data.face.indices = indices.ToArray();
				detached.Add(data);
			}

			FaceRebuildData.Apply(detached, mesh, vertices, null, lookup);
			mesh.DeleteFaces(faces);

			mesh.ToMesh();

			return detached.Select(x => x.face).ToList();
		}

		/// <summary>
		/// Extrude each face in faces individually along it's normal by distance.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		static bool ExtrudePerFace(ProBuilderMesh pb, IEnumerable<Face> faces, float distance)
		{
			if(faces == null || !faces.Any())
				return false;

			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(pb));
			int sharedIndexMax = pb.sharedIndicesInternal.Length;
			int sharedIndexOffset = 0;
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();
			Dictionary<int, int> lookupUV = pb.sharedIndicesUVInternal.ToDictionary();

			List<Face> newFaces = new List<Face>(pb.facesInternal);
			Dictionary<int, int> used = new Dictionary<int, int>();

			foreach(Face face in faces)
			{
				face.smoothingGroup = Smoothing.smoothingGroupNone;
				face.textureGroup = -1;

				Vector3 delta = Math.Normal(pb, face) * distance;
				Edge[] edges = face.edgesInternal;

				used.Clear();

				for(int i = 0; i < edges.Length; i++)
				{
					int vc = vertices.Count;
					int x = edges[i].x, y = edges[i].y;

					if( !used.ContainsKey(x) )
					{
						used.Add(x, lookup[x]);
						lookup[x] = sharedIndexMax + (sharedIndexOffset++);
					}

					if( !used.ContainsKey(y) )
					{
						used.Add(y, lookup[y]);
						lookup[y] = sharedIndexMax + (sharedIndexOffset++);
					}

					lookup.Add(vc + 0, used[x]);
					lookup.Add(vc + 1, used[y]);
					lookup.Add(vc + 2, lookup[x]);
					lookup.Add(vc + 3, lookup[y]);

					Vertex xx = new Vertex(vertices[x]), yy = new Vertex(vertices[y]);
					xx.position += delta;
					yy.position += delta;

					vertices.Add( new Vertex(vertices[x]) );
					vertices.Add( new Vertex(vertices[y]) );

					vertices.Add( xx );
					vertices.Add( yy );

					Face bridge = new Face(
						new int[6] { vc + 0, vc + 1, vc + 2, vc + 1, vc + 3, vc + 2 }, // indices
						face.material, // material
						new AutoUnwrapSettings(face.uv), // UV material
						face.smoothingGroup, // smoothing group
						-1, // texture group
						-1, // uv element group
						false // manualUV flag
					);

					newFaces.Add(bridge);
				}

				for(int i = 0; i < face.distinctIndices.Length; i++)
				{
					vertices[face.distinctIndices[i]].position += delta;

					// Break any UV shared connections
					if( lookupUV != null && lookupUV.ContainsKey(face.distinctIndices[i]) )
						lookupUV.Remove(face.distinctIndices[i]);
				}
			}

			pb.SetVertices(vertices);
			pb.SetFaces(newFaces.ToArray());
			pb.SetSharedIndexes(lookup);
			pb.SetSharedIndexesUV(lookupUV);

			return true;
		}

		/// <summary>
		/// Extrude faces as groups.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <param name="compensateAngleVertexDistance"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		static bool ExtrudeAsGroups(ProBuilderMesh pb, IEnumerable<Face> faces, bool compensateAngleVertexDistance, float distance)
		{
			if(faces == null || !faces.Any())
				return false;

			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(pb));
			int sharedIndexMax = pb.sharedIndicesInternal.Length;
			int sharedIndexOffset = 0;
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();
			Dictionary<int, int> lookupUV = pb.sharedIndicesUVInternal.ToDictionary();

			List<Face> newFaces = new List<Face>(pb.facesInternal);
			// old triangle index -> old shared index
			Dictionary<int, int> oldSharedMap = new Dictionary<int, int>();
			// old shared index -> new shared index
			Dictionary<int, int> newSharedMap = new Dictionary<int, int>();
			// bridge face extruded edges, maps vertex index to new extruded vertex position
			Dictionary<int, int> delayPosition = new Dictionary<int, int>();
			// used to average the direction of vertices shared by perimeter edges
			// key[shared index], value[normal count, normal sum]
			Dictionary<int, SimpleTuple<Vector3, Vector3, List<int>>> extrudeMap = new Dictionary<int, SimpleTuple<Vector3, Vector3,List<int>>>();

			List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb, faces, true, lookup);
			List<HashSet<Face>> groups = GetFaceGroups(wings);

			foreach(HashSet<Face> group in groups)
			{
				Dictionary<EdgeLookup, Face> perimeter = GetPerimeterEdges(group, lookup);

				newSharedMap.Clear();
				oldSharedMap.Clear();

				foreach(var edgeAndFace in perimeter)
				{
					EdgeLookup edge = edgeAndFace.Key;
					Face face = edgeAndFace.Value;

					int vc = vertices.Count;
					int x = edge.local.x, y = edge.local.y;

					if( !oldSharedMap.ContainsKey(x) )
					{
						oldSharedMap.Add(x, lookup[x]);
						int newSharedIndex = -1;

						if(newSharedMap.TryGetValue(lookup[x], out newSharedIndex))
						{
							lookup[x] = newSharedIndex;
						}
						else
						{
							newSharedIndex = sharedIndexMax + (sharedIndexOffset++);
							newSharedMap.Add(lookup[x], newSharedIndex);
							lookup[x] = newSharedIndex;
						}
					}

					if( !oldSharedMap.ContainsKey(y) )
					{
						oldSharedMap.Add(y, lookup[y]);
						int newSharedIndex = -1;

						if(newSharedMap.TryGetValue(lookup[y], out newSharedIndex))
						{
							lookup[y] = newSharedIndex;
						}
						else
						{
							newSharedIndex = sharedIndexMax + (sharedIndexOffset++);
							newSharedMap.Add(lookup[y], newSharedIndex);
							lookup[y] = newSharedIndex;
						}
					}

					lookup.Add(vc + 0, oldSharedMap[x]);
					lookup.Add(vc + 1, oldSharedMap[y]);
					lookup.Add(vc + 2, lookup[x]);
					lookup.Add(vc + 3, lookup[y]);

					delayPosition.Add(vc + 2, x);
					delayPosition.Add(vc + 3, y);

					vertices.Add( new Vertex(vertices[x]) );
					vertices.Add( new Vertex(vertices[y]) );

					// extruded edge will be positioned later
					vertices.Add( null );
					vertices.Add( null );

					Face bridge = new Face(
						new int[6] { vc + 0, vc + 1, vc + 2, vc + 1, vc + 3, vc + 2 }, // indices
						face.material, // material
						new AutoUnwrapSettings(face.uv), // UV material
						face.smoothingGroup, // smoothing group
						-1, // texture group
						-1, // uv element group
						false // manualUV flag
					);

					newFaces.Add(bridge);
				}

				foreach(Face face in group)
				{
					// @todo keep together if possible
					face.textureGroup = -1;

					Vector3 normal = Math.Normal(pb, face);

					for(int i = 0; i < face.distinctIndices.Length; i++)
					{
						int idx = face.distinctIndices[i];

						// If this vertex is on the perimeter but not part of a perimeter edge
						// move the sharedIndex to match it's new value.
						if(!oldSharedMap.ContainsKey(idx) && newSharedMap.ContainsKey(lookup[idx]))
							lookup[idx] = newSharedMap[lookup[idx]];

						int com = lookup[idx];

						// Break any UV shared connections
						if( lookupUV != null && lookupUV.ContainsKey(face.distinctIndices[i]) )
							lookupUV.Remove(face.distinctIndices[i]);

						// add the normal to the list of normals for this shared vertex
						SimpleTuple<Vector3, Vector3, List<int>> dir = null;

						if(extrudeMap.TryGetValue(com, out dir))
						{
							dir.item1 += normal;
							dir.item3.Add(idx);
						}
						else
						{
							extrudeMap.Add(com, new SimpleTuple<Vector3, Vector3,List<int>>(normal, normal, new List<int>() { idx }));
						}
					}
				}
			}

			foreach(var kvp in extrudeMap)
			{
				Vector3 direction = (kvp.Value.item1 / kvp.Value.item3.Count);
				direction.Normalize();

				// If extruding by face normal extend vertices on seams by the hypotenuse
				float modifier = compensateAngleVertexDistance ? Math.Secant(Vector3.Angle(direction, kvp.Value.item2) * Mathf.Deg2Rad) : 1f;

				direction.x *= distance * modifier;
				direction.y *= distance * modifier;
				direction.z *= distance * modifier;

				foreach(int i in kvp.Value.item3)
				{
					vertices[i].position += direction;
				}
			}

			foreach(var kvp in delayPosition)
				vertices[kvp.Key] = new Vertex(vertices[kvp.Value]);

			pb.SetVertices(vertices);
			pb.SetFaces(newFaces.ToArray());
			pb.SetSharedIndexes(lookup);
			pb.SetSharedIndexesUV(lookupUV);

			return true;
		}

		static List<HashSet<Face>> GetFaceGroups(List<WingedEdge> wings)
		{
			HashSet<Face> used = new HashSet<Face>();
			List<HashSet<Face>> groups = new List<HashSet<Face>>();

			foreach(WingedEdge wing in wings)
			{
				if(used.Add(wing.face))
				{
					HashSet<Face> group = new HashSet<Face>() { wing.face };

					ElementSelection.Flood(wing, group);

					foreach(Face f in group)
						used.Add(f);

					groups.Add(group);
				}
			}

			return groups;
		}

		static Dictionary<EdgeLookup, Face> GetPerimeterEdges(HashSet<Face> faces, Dictionary<int, int> lookup)
		{
			Dictionary<EdgeLookup, Face> perimeter = new Dictionary<EdgeLookup, Face>();
			HashSet<EdgeLookup> used = new HashSet<EdgeLookup>();

			foreach(Face face in faces)
			{
				foreach(Edge edge in face.edgesInternal)
				{
					EdgeLookup e = new EdgeLookup(lookup[edge.x], lookup[edge.y], edge.x, edge.y);

					if(!used.Add(e))
					{
						if(perimeter.ContainsKey(e))
							perimeter.Remove(e);
					}
					else
					{
						perimeter.Add(e, face);
					}
				}
			}

			return perimeter;
		}
	}
}
