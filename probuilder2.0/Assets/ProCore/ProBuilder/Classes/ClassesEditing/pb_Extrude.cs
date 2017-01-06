using UnityEngine;
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
			return ExtrudePerFace(pb, faces);
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
	}
}
