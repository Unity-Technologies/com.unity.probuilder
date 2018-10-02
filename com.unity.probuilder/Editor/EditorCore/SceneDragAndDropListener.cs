using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using UObject = UnityEngine.Object;
using System.Linq;
using Math = UnityEngine.ProBuilder.Math;
using ArrUtil = UnityEngine.ProBuilder.ArrayUtility;

namespace UnityEditor.ProBuilder
{
	[InitializeOnLoad]
	static class SceneDragAndDropListener
	{
		static bool s_IsSceneViewDragAndDrop;
		static Mesh s_PreviewMesh;
		static Material s_PreviewMaterial;
		static int s_PreviewSubmesh;
		static ProBuilderMesh s_CurrentPreview;
		static bool s_IsFaceDragAndDropOverrideEnabled;
		static Matrix4x4 s_Matrix;

		static SceneDragAndDropListener()
		{
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

			s_PreviewMesh = new Mesh()
			{
				name = "pb_DragAndDropListener::PreviewMesh",
				hideFlags = HideFlags.HideAndDontSave
			};
		}

		static void OnBeforeAssemblyReload()
		{
			UObject.DestroyImmediate(s_PreviewMesh);
		}

		public static bool isDragging
		{
			get { return s_IsSceneViewDragAndDrop; }
		}

		static bool isFaceMode
		{
			get { return ProBuilderEditor.selectMode == SelectMode.Face; }
		}

		static Material GetMaterialFromDragReferences(UObject[] references, bool createMaterialForTexture)
		{
			Material mat = references.FirstOrDefault(x => x is Material) as Material;

			if (!createMaterialForTexture || mat != null)
				return mat;

			Texture2D tex = references.FirstOrDefault(x => x is Texture2D) as Texture2D;
			string texPath = tex != null ? AssetDatabase.GetAssetPath(tex) : null;

			if (!string.IsNullOrEmpty(texPath))
			{
				var defaultMaterial = ReflectionUtility.Invoke(null, typeof(Material), "GetDefaultMaterial") as Material;

				if (defaultMaterial == null)
					mat = new Material(Shader.Find("Standard"));
				else
					mat = new Material(defaultMaterial.shader);

				if (mat.shader == null)
				{
					UObject.DestroyImmediate(mat);
					return null;
				}

				mat.mainTexture = tex;

				int lastDot = texPath.LastIndexOf(".", StringComparison.InvariantCulture);
				texPath = texPath.Substring(0, texPath.Length - (texPath.Length - lastDot));
				texPath = AssetDatabase.GenerateUniqueAssetPath(texPath + ".mat");
				AssetDatabase.CreateAsset(mat, texPath);
				AssetDatabase.Refresh();

				return mat;
			}

			return null;
		}

		static void SetMeshPreview(ProBuilderMesh mesh)
		{
			if (s_CurrentPreview != mesh)
			{
				s_PreviewMesh.Clear();
				s_CurrentPreview = mesh;

				if (s_CurrentPreview != null)
				{
					s_PreviewMaterial = GetMaterialFromDragReferences(DragAndDrop.objectReferences, false);
					s_IsFaceDragAndDropOverrideEnabled = isFaceMode && s_PreviewMaterial != null && mesh.selectedFaceCount > 0;

					if (s_IsFaceDragAndDropOverrideEnabled)
					{
						s_PreviewMesh.vertices = mesh.positionsInternal;

						if(mesh.HasArrays(MeshArrays.Color))
							s_PreviewMesh.colors = mesh.colorsInternal;
						if(mesh.HasArrays(MeshArrays.Normal))
							s_PreviewMesh.normals = mesh.normalsInternal;
						if(mesh.HasArrays(MeshArrays.Texture0))
							s_PreviewMesh.uv = mesh.texturesInternal;

						s_Matrix = mesh.transform.localToWorldMatrix;
						s_PreviewMesh.triangles = mesh.selectedFacesInternal.SelectMany(x => x.indexes).ToArray();
					}
				}
				else
				{
					s_IsFaceDragAndDropOverrideEnabled = false;
				}
			}
		}

		static void OnSceneGUI(SceneView sceneView)
		{
			var evt = Event.current;

			if (evt.type == EventType.DragUpdated)
			{
				if (!s_IsSceneViewDragAndDrop)
					s_IsSceneViewDragAndDrop = true;

				GameObject go = HandleUtility.PickGameObject(evt.mousePosition, out s_PreviewSubmesh);

				SetMeshPreview(go != null ? go.GetComponent<ProBuilderMesh>() : null);

				if (s_IsFaceDragAndDropOverrideEnabled)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					evt.Use();
				}
			}
			else if (evt.type == EventType.DragExited)
			{
				if (s_IsFaceDragAndDropOverrideEnabled)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					evt.Use();
					SetMeshPreview(null);
				}

				s_IsSceneViewDragAndDrop = false;
			}
			else if (evt.type == EventType.DragPerform)
			{
				s_IsSceneViewDragAndDrop = false;
				GameObject go = HandleUtility.PickGameObject(evt.mousePosition, out s_PreviewSubmesh);
				SetMeshPreview(go != null ? go.GetComponent<ProBuilderMesh>() : null);

				if (s_CurrentPreview != null && s_IsFaceDragAndDropOverrideEnabled)
				{
					var renderer = go.GetComponent<Renderer>();
					var materials = renderer.sharedMaterials;
					var submeshCount = materials.Length;
					var index = -1;

					for (int i = 0; i < submeshCount && index < 0; i++)
					{
						if (materials[i] == s_PreviewMaterial)
							index = i;
					}

					if (index < 0)
					{
						// Material doesn't exist in MeshRenderer.sharedMaterials, now check if there is an unused
						// submeshIndex that we can replace with this value instead of creating a new entry.
						var submeshIndexes = new bool[submeshCount];

						foreach (var face in s_CurrentPreview.facesInternal)
							submeshIndexes[Math.Clamp(face.submeshIndex, 0, submeshCount - 1)] = true;

						index = Array.IndexOf(submeshIndexes, false);

						if (index > -1)
						{
							materials[index] = s_PreviewMaterial;
							renderer.sharedMaterials = materials;
						}
						else
						{
							index = materials.Length;
							var copy = new Material[index + 1];
							Array.Copy(materials, copy, index);
							copy[index] = s_PreviewMaterial;
							renderer.sharedMaterials = copy;
						}
					}

					UndoUtility.RecordObject(s_CurrentPreview, "Set Face Material");

					foreach (var face in s_CurrentPreview.selectedFacesInternal)
						face.submeshIndex = index;

					FilterUnusedSubmeshIndexes(s_CurrentPreview, materials);

					s_CurrentPreview.ToMesh();
					s_CurrentPreview.Refresh();
					s_CurrentPreview.Optimize();

					evt.Use();
				}

				SetMeshPreview(null);
			}
			else if (evt.type == EventType.Repaint && s_IsFaceDragAndDropOverrideEnabled)
			{
				if (s_PreviewMaterial.SetPass(0))
					Graphics.DrawMeshNow(s_PreviewMesh, s_Matrix, 0);
			}
		}

		static void FilterUnusedSubmeshIndexes(ProBuilderMesh mesh, Material[] materials)
		{
			var submeshCount = materials.Length;
			var used = new bool[submeshCount];

			foreach (var face in mesh.facesInternal)
				used[Math.Clamp(face.submeshIndex, 0, submeshCount - 1)] = true;

			var unused = ArrUtil.AllIndexesOf(used, x => !x);

			if (unused.Any())
			{
				foreach (var face in mesh.facesInternal)
				{
					var original = face.submeshIndex;
					foreach(var index in unused)
						if (original > index)
							face.submeshIndex--;
				}

				mesh.renderer.sharedMaterials = ArrUtil.RemoveAt(materials, unused);
			}

		}
	}
}