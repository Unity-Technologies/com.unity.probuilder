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
		public static bool Extrude(this pb_Object pb, pb_Face[] faces)
		{
			// return ExtrudePerFace(pb, faces);
			return ExtrudeAsGroups(pb, faces);
		}

		/**
		 * Extrude each face in faces individually along it's normal by distance.
		 */
		private static bool ExtrudePerFace(pb_Object pb, pb_Face[] faces, float distance = .25f)
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
		private static bool ExtrudeAsGroups(pb_Object pb, pb_Face[] faces, float distance = .25f)
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
			Dictionary<int, int> used = new Dictionary<int, int>();
			// old shared index -> new shared index
			Dictionary<int, int> sharedMap = new Dictionary<int, int>();
			// bridge face extruded edges, maps vertex index to new extruded vertex position
			Dictionary<int, int> delayPosition = new Dictionary<int, int>();

			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, faces, true, lookup);
			List<HashSet<pb_Face>> groups = GetFaceGroups(wings);

			foreach(HashSet<pb_Face> group in groups)
			{
				foreach(pb_Face face in group)
				{
					face.smoothingGroup = -1;
					face.textureGroup = -1;
				}

				Vector3 delta = pb_Math.Normal(pb, group.First()) * distance;

				sharedMap.Clear();
				used.Clear();

				foreach(pb_Edge edge in pbMeshUtils.GetPerimeterEdges(lookup, group))
				{
					int vc = vertices.Count;
					int x = edge.x, y = edge.y;

					if( !used.ContainsKey(x) )
					{
						used.Add(x, lookup[x]);
						int newSharedIndex = -1;

						if(sharedMap.TryGetValue(lookup[x], out newSharedIndex))
						{
							lookup[x] = newSharedIndex;
						}
						else
						{
							newSharedIndex = sharedIndexMax + (sharedIndexOffset++);
							sharedMap.Add(lookup[x], newSharedIndex);
							lookup[x] = newSharedIndex;
						}
					}

					if( !used.ContainsKey(y) )
					{
						used.Add(y, lookup[y]);
						int newSharedIndex = -1;

						if(sharedMap.TryGetValue(lookup[y], out newSharedIndex))
						{
							lookup[y] = newSharedIndex;
						}
						else
						{
							newSharedIndex = sharedIndexMax + (sharedIndexOffset++);
							sharedMap.Add(lookup[y], newSharedIndex);
							lookup[y] = newSharedIndex;
						}
					}

					lookup.Add(vc + 0, used[x]);
					lookup.Add(vc + 1, used[y]);
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
						new int[6] { vc + 0, vc + 1, vc + 2, vc + 1, vc + 3, vc + 2}, 	// indices
						group.First().material,											// material
						new pb_UV(group.First().uv),									// UV material
						group.First().smoothingGroup,									// smoothing group
						-1,																// texture group
						-1,																// uv element group
						false															// manualUV flag
						);

					newFaces.Add(bridge);
				}

				foreach(pb_Face face in group)
				{
					for(int i = 0; i < face.distinctIndices.Length; i++)
					{
						int idx = face.distinctIndices[i];

						// If this vertex is on the perimeter but not part of a perimeter edge
						// move the sharedIndex to match it's new value.
						if(!used.ContainsKey(idx) && sharedMap.ContainsKey(lookup[idx]))
							lookup[idx] = sharedMap[lookup[idx]];

						vertices[idx].position.x += delta.x;
						vertices[idx].position.y += delta.y;
						vertices[idx].position.z += delta.z;

						// Break any UV shared connections
						if( lookupUV != null && lookupUV.ContainsKey(face.distinctIndices[i]) )
							lookupUV.Remove(face.distinctIndices[i]);
					}
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
	}
}
