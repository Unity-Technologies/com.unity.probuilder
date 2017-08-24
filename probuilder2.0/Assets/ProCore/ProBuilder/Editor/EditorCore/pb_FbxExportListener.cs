using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using FbxExporters.Editor; // @todo reflect this to avoid dependency
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	/*
	 * Register a delegate with the ModelExporter class so that ProBuilder can modify the mesh
	 * prior to conversion to an FBX node.
	 */
	[InitializeOnLoad]
	static class pb_FbxExportListener
	{
		static pb_FbxExportListener()
		{
			ModelExporter.onWillConvertGameObjectToNode += OnWillConvertGameObjectToNode;
			ModelExporter.onDidConvertGameObjectToNode += OnDidConvertGameObjectToNode;
			ModelExporter.onGetMeshInfo += OnGetMeshInfo;
		}

		private static bool OnGetMeshInfo(
			GameObject go,
			out Vector3[] positions,
			out Vector3[] normals,
			out Color32[] colors,
			out Vector2[] textures,
			out Vector4[] tangents,
			out int[][][] indices)
		{
			pb_Object mesh = go.GetComponent<pb_Object>();

			if(mesh == null)
			{
				positions = null;
				normals = null;
				colors = null;
				textures = null;
				tangents = null;
				indices = null;
				return false;
			}

			mesh.ToMesh();
			mesh.Refresh();

			MeshRenderer mr = mesh.gameObject.GetComponent<MeshRenderer>();
			Material[] sharedMaterials = mr != null ? mr.sharedMaterials : new Material[0] {};
			int subMeshCount = sharedMaterials.Length;

			// After ToMesh() the UnityEngine mesh matches pb_Object. Optimize() will change that.
			UnityEngine.Mesh umesh = mesh.msh;

			positions = umesh.vertices;
			normals = umesh.normals;
			textures = umesh.uv;
			tangents = umesh.tangents;
			colors = umesh.colors32;
			indices = new int[subMeshCount][][];

			for(int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
			{
				pb_Face[] faces = mesh.faces.Where(x => x.material == sharedMaterials[subMeshIndex]).ToArray();

				List<int[]> addl = new List<int[]>(faces.Length + faces.Length / 2);

				for(int ff = 0; ff < faces.Length; ff++)
				{
					int[] quad = faces[ff].ToQuad();

					if(quad != null)
					{
						addl.Add(quad);
					}
					else
					{
						// @todo instead of just splitting the face into triangles we could instead get
						// a polygon edge ring and export an ngon. Might make a good option?
						for(int ii = 0; ii < faces[ff].indices.Length; ii += 3)
							addl.Add(new int[3] { faces[ff][ii], faces[ff][ii+1], faces[ff][ii+2] });
					}
				}

				indices[subMeshIndex] = addl.ToArray();
			}

			// catch null-material faces
			pb_Face[] facesWithNoMaterial = mesh.faces.Where(x => x.material == null).ToArray();

			if(facesWithNoMaterial != null && facesWithNoMaterial.Length > 0)
			{
				int[][] addl = new int[facesWithNoMaterial.Length][];

				for(int ff = 0; ff < facesWithNoMaterial.Length; ff++)
					facesWithNoMaterial[ff].ToQuadOrTriangles(out addl[ff]);

				pbUtil.Add<int[][]>(indices, addl);
			}

			mesh.Optimize();

			return true;
		}

		private static void OnWillConvertGameObjectToNode(GameObject go)
		{
			pb_Log.Debug("OnWillConvertGameObjectToNode: " + go.name);

			pb_Object pb = go != null ? go.GetComponent<pb_Object>() : null;

			if(pb != null)
			{
				pb.ToMesh();
				pb.Refresh();
			}
		}

		private static void OnDidConvertGameObjectToNode(GameObject go)
		{
			pb_Log.Debug("OnDidConvertGameObjectToNode: " + go.name);

			pb_Object pb = go != null ? go.GetComponent<pb_Object>() : null;

			if(pb != null)
				pb.Optimize();
		}
	}
}