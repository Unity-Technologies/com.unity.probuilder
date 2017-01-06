using UnityEngine;
using System.Linq;
using ProBuilder2.Common;
using System.Collections.Generic;

namespace ProBuilder2.MeshOperations
{
	/**
	 * pb_Object extension methods for face and edge extrusion.
	 */
	public static class pb_Extrude
	{
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

		/**
		 * Extrude each face in faces individually along it's normal by distance.
		 */
		private static bool ExtrudePerFace(pb_Object pb, pb_Face[] faces, float distance)
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
				face.smoothingGroup = -1;
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

		/**
		 * Extrude faces as groups.
		 */
		private static bool ExtrudeAsGroups(pb_Object pb, pb_Face[] faces, bool compensateAngleVertexDistance, float distance)
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
					face.smoothingGroup = -1;
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

		private static List<HashSet<pb_Face>> GetFaceGroups(List<pb_WingedEdge> wings)
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

		/**
		 * returns perimeter edges by key<edge>, value<face>
		 */
		private static Dictionary<pb_EdgeLookup, pb_Face> GetPerimeterEdges(HashSet<pb_Face> faces, Dictionary<int, int> lookup)
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
	}
}
