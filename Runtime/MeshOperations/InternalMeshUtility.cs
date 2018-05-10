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
		/// Averages shared normals with the mask of all (indices contained in perimeter edge)
		/// </summary>
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
					faces[i].ShiftIndexes(vertexCount);
				}
				f.AddRange(faces);

				// Shared Indices
				IntArray[] si = pb.GetSharedIndexes();
				for(int i = 0; i < si.Length; i++)
				{
					for(int n = 0; n < si[i].length; n++)
						si[i][n] += vertexCount;
				}
				s.AddRange(si);

				// Shared Indices UV
				{
					IntArray[] si_uv = pb.GetSharedIndexesUV();
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
			combined.SetSharedIndexesUV(suv.ToArray());
			combined.ToMesh();
			combined.CenterPivot( pbs[0].transform.position );
			combined.Refresh();

			// refresh donors since deleting the children of the instantiated object could cause them to lose references
			 foreach (ProBuilderMesh pb in pbs)
				 pb.Rebuild();

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

			Vector3[] m_vertices = MeshUtility.GetMeshAttribute<Vector3[]>(t.gameObject, x => x.vertices);
			Color[] m_colors = MeshUtility.GetMeshAttribute<Color[]>(t.gameObject, x => x.colors);
			Vector2[] m_uvs = MeshUtility.GetMeshAttribute<Vector2[]>(t.gameObject, x => x.uv);

			List<Vector3> verts = preserveFaces ? new List<Vector3>(m.vertices) : new List<Vector3>();
			List<Color> cols = preserveFaces ? new List<Color>(m.colors) : new List<Color>();
			List<Vector2> uvs = preserveFaces ? new List<Vector2>(m.uv) : new List<Vector2>();
			List<Face> faces = new List<Face>();

			for (int n = 0; n < m.subMeshCount; n++)
			{
				int[] tris = m.GetTriangles(n);
				for (int i = 0; i < tris.Length; i += 3)
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
						faces[index].indices = arr;
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
						faces[index].indices = arr;
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
			pb.sharedIndicesInternal = IntArrayUtility.GetSharedIndexesWithPositions(verts.ToArray());
			pb.colorsInternal = cols.ToArray();

			return true;
		}
	}
}
