using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace ProBuilder2.Common
{
	/*
	 * Options when exporting FBX files.
	 */
	public class pb_FbxOptions
	{
		public bool quads;
		public bool ngons;
	}

	/*
	 * Register a delegate with the ModelExporter class so that ProBuilder can modify the mesh
	 * prior to conversion to an FBX node.
	 */
	[InitializeOnLoad]
	static class pb_FbxExportListener
	{
		private static pb_FbxOptions m_Options = new pb_FbxOptions()
		{
			quads = true,
			ngons = false
		};

		private static bool m_FbxExportDelegateIsLoaded = false;

		public static bool FbxExportEnabled { get { return m_FbxExportDelegateIsLoaded; } }

		static pb_FbxExportListener()
		{
			Type modelExporterType = pb_Reflection.GetType("FbxExporters.Editor.ModelExporter");
			EventInfo onGetMeshInfoEvent = modelExporterType != null ? modelExporterType.GetEvent("onGetMeshInfo") : null;
			m_FbxExportDelegateIsLoaded = false;

			if(onGetMeshInfoEvent != null)
			{
				try
				{
					Type delegateType = onGetMeshInfoEvent.EventHandlerType;
					MethodInfo add = onGetMeshInfoEvent.GetAddMethod();
					MethodInfo ogmiMethod = typeof(pb_FbxExportListener).GetMethod("OnGetMeshInfo", BindingFlags.Static | BindingFlags.NonPublic);
					Delegate d = Delegate.CreateDelegate(delegateType, ogmiMethod);
					add.Invoke(null, new object[] { d });
					m_FbxExportDelegateIsLoaded = true;
				}
				catch
				{
					pb_Log.Warning("Failed loading FbxExporter delegates. Fbx export will still work correctly, but ProBuilder will not be able export quads or ngons.");
				}

				ReloadOptions();
			}
		}

		public static void ReloadOptions()
		{
			m_Options.quads = pb_PreferencesInternal.GetBool("Export::m_FbxQuads", true);
			m_Options.ngons = pb_PreferencesInternal.GetBool("Export::m_FbxNgons", false);
		}

		/*
		 * When FbxExporter wants a MeshInfo object from a GameObject this function tries to
		 * override that with the pb_Object data. If it's not a pb_Object this returns false.
		 */
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
					// If quad export is enabled attempt to write polygon as quad
					int[] quad = m_Options.quads ? faces[ff].ToQuad() : null;

					if(quad != null)
					{
						addl.Add(quad);
					}
					else
					{
						if(m_Options.ngons)
						{
							List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(faces[ff]);
							int[] ring = new int[perimeter.Count];
							for(int ii = 0; ii < perimeter.Count; ii++)
								ring[ii] = perimeter[ii].x;
							addl.Add(ring);
						}
						else
						{
							for(int ii = 0; ii < faces[ff].indices.Length; ii += 3)
								addl.Add(new int[3] { faces[ff][ii], faces[ff][ii+1], faces[ff][ii+2] });
						}
					}
				}

				indices[subMeshIndex] = addl.ToArray();
			}

			// catch null-material faces
			// @todo what does FbxExporter do for submeshes > sharedMaterial count?
			pb_Face[] facesWithNoMaterial = mesh.faces.Where(x => x.material == null).ToArray();

			if(facesWithNoMaterial != null && facesWithNoMaterial.Length > 0)
			{
				int[][] addl = new int[facesWithNoMaterial.Length][];

				for(int ff = 0; ff < facesWithNoMaterial.Length; ff++)
					facesWithNoMaterial[ff].ToQuadOrTriangles(out addl[ff]);

				pbUtil.Add<int[][]>(indices, addl);
			}

			// reset the mesh back to optimized state
			mesh.Optimize();

			return true;
		}
	}
}