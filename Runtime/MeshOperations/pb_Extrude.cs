using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// pb_Object extension methods for face and edge extrusion.
	/// </summary>
	public static class pb_Extrude
	{
		/// <summary>
		/// Extrude faces using method and distance.
		/// </summary>
		/// <param name="pb">Target pb_Object.</param>
		/// <param name="faces">The faces to extrude.</param>
		/// <param name="method">How faces are extruded.</param>
		/// <param name="distance">The distance in Unity units to extrude faces.</param>
		/// <returns>True on success, false if the action failed.</returns>
		public static bool Extrude(this pb_Object pb, pb_Face[] faces, ExtrudeMethod method, float distance)
		{
			switch(method)
			{
				case ExtrudeMethod.IndividualFaces:
					return ExtrudePerFace(pb, faces, distance);

				default:
					return ExtrudeAsGroups(pb, faces, method == ExtrudeMethod.FaceNormal, distance);
			}
		}

		/// <summary>
		/// Extrude each face in faces individually along it's normal by distance.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		static bool ExtrudePerFace(pb_Object pb, pb_Face[] faces, float distance)
		{
			if(faces == null || faces.Length < 1)
				return false;

			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			int sharedIndexMax = pb.sharedIndices.Length;
			int sharedIndexOffset = 0;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> lookupUV = pb.sharedIndicesUV.ToDictionary();

			List<pb_Face> newFaces = new List<pb_Face>(pb.faces);
			Dictionary<int, int> used = new Dictionary<int, int>();

			foreach(pb_Face face in faces)
			{
				face.smoothingGroup = pb_Smoothing.SMOOTHING_GROUP_NONE;
				face.textureGroup = -1;

				Vector3 delta = pb_Math.Normal(pb, face) * distance;
				pb_Edge[] edges = face.edges;

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

					pb_Vertex xx = new pb_Vertex(vertices[x]), yy = new pb_Vertex(vertices[y]);
					xx.position += delta;
					yy.position += delta;

					vertices.Add( new pb_Vertex(vertices[x]) );
					vertices.Add( new pb_Vertex(vertices[y]) );

					vertices.Add( xx );
					vertices.Add( yy );

					pb_Face bridge = new pb_Face(
						new int[6] { vc + 0, vc + 1, vc + 2, vc + 1, vc + 3, vc + 2}, // indices
						face.material,							// material
						new pb_UV(face.uv),						// UV material
						face.smoothingGroup,					// smoothing group
						-1,										// texture group
						-1,										// uv element group
						false									// manualUV flag
						);

					newFaces.Add(bridge);
				}

				for(int i = 0; i < face.distinctIndices.Length; i++)
				{
					vertices[face.distinctIndices[i]].position.x += delta.x;
					vertices[face.distinctIndices[i]].position.y += delta.y;
					vertices[face.distinctIndices[i]].position.z += delta.z;

					// Break any UV shared connections
					if( lookupUV != null && lookupUV.ContainsKey(face.distinctIndices[i]) )
						lookupUV.Remove(face.distinctIndices[i]);
				}
			}

			pb.SetVertices(vertices);
			pb.SetFaces(newFaces.ToArray());
			pb.SetSharedIndices(lookup);
			pb.SetSharedIndicesUV(lookupUV);

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
		static bool ExtrudeAsGroups(pb_Object pb, pb_Face[] faces, bool compensateAngleVertexDistance, float distance)
		{
			if(faces == null || faces.Length < 1)
				return false;

			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			int sharedIndexMax = pb.sharedIndices.Length;
			int sharedIndexOffset = 0;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> lookupUV = pb.sharedIndicesUV.ToDictionary();

			List<pb_Face> newFaces = new List<pb_Face>(pb.faces);
			// old triangle index -> old shared index
			Dictionary<int, int> oldSharedMap = new Dictionary<int, int>();
			// old shared index -> new shared index
			Dictionary<int, int> newSharedMap = new Dictionary<int, int>();
			// bridge face extruded edges, maps vertex index to new extruded vertex position
			Dictionary<int, int> delayPosition = new Dictionary<int, int>();
			// used to average the direction of vertices shared by perimeter edges
			// key[shared index], value[normal count, normal sum]
			Dictionary<int, pb_Tuple<Vector3, Vector3, List<int>>> extrudeMap = new Dictionary<int, pb_Tuple<Vector3, Vector3,List<int>>>();

			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, faces, true, lookup);
			List<HashSet<pb_Face>> groups = GetFaceGroups(wings);

			foreach(HashSet<pb_Face> group in groups)
			{
				Dictionary<pb_EdgeLookup, pb_Face> perimeter = GetPerimeterEdges(group, lookup);

				newSharedMap.Clear();
				oldSharedMap.Clear();

				foreach(var edgeAndFace in perimeter)
				{
					pb_EdgeLookup edge = edgeAndFace.Key;
					pb_Face face = edgeAndFace.Value;

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

					vertices.Add( new pb_Vertex(vertices[x]) );
					vertices.Add( new pb_Vertex(vertices[y]) );

					// extruded edge will be positioned later
					vertices.Add( null );
					vertices.Add( null );

					pb_Face bridge = new pb_Face(
						new int[6] { vc + 0, vc + 1, vc + 2, vc + 1, vc + 3, vc + 2 }, 	// indices
						face.material,													// material
						new pb_UV(face.uv),												// UV material
						face.smoothingGroup,											// smoothing group
						-1,																// texture group
						-1,																// uv element group
						false															// manualUV flag
						);

					newFaces.Add(bridge);
				}

				foreach(pb_Face face in group)
				{
					// @todo keep together if possible
					face.textureGroup = -1;

					Vector3 normal = pb_Math.Normal(pb, face);

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
						pb_Tuple<Vector3, Vector3, List<int>> dir = null;

						if(extrudeMap.TryGetValue(com, out dir))
						{
							dir.Item1.x += normal.x;
							dir.Item1.y += normal.y;
							dir.Item1.z += normal.z;
							dir.Item3.Add(idx);
						}
						else
						{
							extrudeMap.Add(com, new pb_Tuple<Vector3, Vector3,List<int>>(normal, normal, new List<int>() { idx }));
						}
					}
				}
			}

			foreach(var kvp in extrudeMap)
			{
				Vector3 direction = (kvp.Value.Item1 / kvp.Value.Item3.Count);
				direction.Normalize();

				// If extruding by face normal extend vertices on seams by the hypotenuse
				float modifier = compensateAngleVertexDistance ? pb_Math.Secant(Vector3.Angle(direction, kvp.Value.Item2) * Mathf.Deg2Rad) : 1f;

				direction.x *= distance * modifier;
				direction.y *= distance * modifier;
				direction.z *= distance * modifier;

				foreach(int i in kvp.Value.Item3)
				{
					vertices[i].position.x += direction.x;
					vertices[i].position.y += direction.y;
					vertices[i].position.z += direction.z;
				}
			}

			foreach(var kvp in delayPosition)
				vertices[kvp.Key] = new pb_Vertex(vertices[kvp.Value]);

			pb.SetVertices(vertices);
			pb.SetFaces(newFaces.ToArray());
			pb.SetSharedIndices(lookup);
			pb.SetSharedIndicesUV(lookupUV);

			return true;
		}

		static List<HashSet<pb_Face>> GetFaceGroups(List<pb_WingedEdge> wings)
		{
			HashSet<pb_Face> used = new HashSet<pb_Face>();
			List<HashSet<pb_Face>> groups = new List<HashSet<pb_Face>>();

			foreach(pb_WingedEdge wing in wings)
			{
				if(used.Add(wing.face))
				{
					HashSet<pb_Face> group = new HashSet<pb_Face>() { wing.face };

					pb_GrowShrink.Flood(wing, group);

					foreach(pb_Face f in group)
						used.Add(f);

					groups.Add(group);
				}
			}

			return groups;
		}

		/// <summary>
		/// returns perimeter edges by key<edge>, value<face>
		/// </summary>
		static Dictionary<pb_EdgeLookup, pb_Face> GetPerimeterEdges(HashSet<pb_Face> faces, Dictionary<int, int> lookup)
		{
			Dictionary<pb_EdgeLookup, pb_Face> perimeter = new Dictionary<pb_EdgeLookup, pb_Face>();
			HashSet<pb_EdgeLookup> used = new HashSet<pb_EdgeLookup>();

			foreach(pb_Face face in faces)
			{
				foreach(pb_Edge edge in face.edges)
				{
					pb_EdgeLookup e = new pb_EdgeLookup(lookup[edge.x], lookup[edge.y], edge.x, edge.y);

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

		/// <summary>
		/// Edge extrusion override
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="edges"></param>
		/// <param name="extrudeDistance"></param>
		/// <param name="extrudeAsGroup"></param>
		/// <param name="enableManifoldExtrude"></param>
		/// <param name="extrudedEdges"></param>
		/// <returns></returns>
		public static bool Extrude(this pb_Object pb, pb_Edge[] edges, float extrudeDistance, bool extrudeAsGroup, bool enableManifoldExtrude, out pb_Edge[] extrudedEdges)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();

			List<pb_Edge> validEdges = new List<pb_Edge>();
			List<pb_Face> edgeFaces = new List<pb_Face>();

			foreach(pb_Edge e in edges)
			{
				int faceCount = 0;
				pb_Face fa = null;

				foreach(pb_Face f in pb.faces)
				{
					if(f.edges.IndexOf(e, lookup) > -1)
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
			int c = 0;
			for(int i = 0; i < validEdges.Count; i++)
			{
				allEdgeIndices[c++] = validEdges[i].x;
				allEdgeIndices[c++] = validEdges[i].y;
			}

			List<pb_Edge> extrudedIndices = new List<pb_Edge>();
			// used to set the editor selection to the newly created edges
			List<pb_Edge> newEdges = new List<pb_Edge>();

			// build out new faces around validEdges

			for(int i = 0; i < validEdges.Count; i++)
			{
				pb_Edge edge = validEdges[i];
				pb_Face face = edgeFaces[i];

				// Averages the normals using only vertices that are on the edge
				Vector3 xnorm = extrudeAsGroup ? pb_MeshOps.AverageNormalWithIndices( sharedIndices[lookup[edge.x]], allEdgeIndices, oNormals ) : pb_Math.Normal(pb, face);
				Vector3 ynorm = extrudeAsGroup ? pb_MeshOps.AverageNormalWithIndices( sharedIndices[lookup[edge.y]], allEdgeIndices, oNormals ) : pb_Math.Normal(pb, face);

				int x_sharedIndex = lookup[edge.x];
				int y_sharedIndex = lookup[edge.y];

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
							pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, extrudedIndices[n].y, extrudedIndices[i].y);
							break;
						}
					}
				}
			}

			pb.SetSharedIndices(sharedIndices);

			foreach(pb_Face f in pb.faces)
				f.RebuildCaches();

			extrudedEdges = newEdges.ToArray();
			return true;
		}

	}
}
