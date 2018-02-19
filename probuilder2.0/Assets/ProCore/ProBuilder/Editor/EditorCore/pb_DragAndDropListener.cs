using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder.Core;
using ProBuilder.Interface;
using ProBuilder.EditorCore;
using UObject = UnityEngine.Object;
using System.Linq;

namespace ProBuilder.EditorCore
{
	[InitializeOnLoad]
	static class pb_DragAndDropListener
	{
		static bool s_IsSceneViewDragAndDrop;
		static Mesh s_PreviewMesh;
		static Material s_PreviewMaterial;
		static int s_PreviewSubmesh;
		static pb_Object s_CurrentPreview;
		static bool s_IsFaceDragAndDropOverrideEnabled;

		static pb_DragAndDropListener()
		{
			SceneView.onSceneGUIDelegate += OnSceneGUI;
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

		public static bool IsDragging()
		{
			return s_IsSceneViewDragAndDrop;
		}

		static bool isFaceMode
		{
			get
			{
				return pb_Editor.instance != null &&
				       pb_Editor.instance.editLevel == EditLevel.Geometry &&
				       pb_Editor.instance.selectionMode == SelectMode.Face;
			}
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
				var defaultMaterial = pb_Reflection.Invoke(null, typeof(Material), "GetDefaultMaterial") as Material;

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

		static void SetMeshPreview(pb_Object mesh)
		{
			if (s_CurrentPreview != mesh)
			{
				s_PreviewMesh.Clear();
				s_CurrentPreview = mesh;

				if (s_CurrentPreview != null)
				{
					s_PreviewMaterial = GetMaterialFromDragReferences(DragAndDrop.objectReferences, false);
					s_IsFaceDragAndDropOverrideEnabled = isFaceMode && s_PreviewMaterial != null && mesh.SelectedFaceCount > 0;

					if (s_IsFaceDragAndDropOverrideEnabled)
					{
						s_PreviewMesh.vertices = mesh.vertices;
						Vector2[] uvs = mesh.uv;
						if(uvs != null && uvs.Length == mesh.vertexCount)
							s_PreviewMesh.uv = uvs;
						s_PreviewMesh.triangles = pb_Face.AllTriangles(mesh.SelectedFaces);
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
				if(!s_IsSceneViewDragAndDrop)
					s_IsSceneViewDragAndDrop = true;

				GameObject go = HandleUtility.PickGameObject(evt.mousePosition, out s_PreviewSubmesh);
				SetMeshPreview(go != null ? go.GetComponent<pb_Object>() : null);

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
				SetMeshPreview(go != null ? go.GetComponent<pb_Object>() : null);

				if (s_CurrentPreview != null)
				{
					pb_Undo.RecordObject(s_CurrentPreview, "Set Face Material");

					if (s_IsFaceDragAndDropOverrideEnabled)
					{
						foreach (var face in s_CurrentPreview.SelectedFaces)
							face.material = s_PreviewMaterial;
					}
					else if(s_PreviewSubmesh > -1)
					{
						Material draggedMaterial = GetMaterialFromDragReferences(DragAndDrop.objectReferences, true);

						if (draggedMaterial != null)
						{
							var mr = s_CurrentPreview.GetComponent<MeshRenderer>();
							Material hoveredMaterial = mr == null ? null : mr.sharedMaterials[s_PreviewSubmesh];

							foreach (var face in s_CurrentPreview.faces)
							{
								if (hoveredMaterial == null || face.material == hoveredMaterial)
									face.material = draggedMaterial;
							}
						}
					}

					s_CurrentPreview.ToMesh();
					s_CurrentPreview.Refresh();
					s_CurrentPreview.Optimize();

					evt.Use();
				}

				SetMeshPreview(null);
			}
			else if (evt.type == EventType.Repaint)
			{
				if (s_IsFaceDragAndDropOverrideEnabled && s_PreviewMaterial.SetPass(0))
					Graphics.DrawMeshNow(s_PreviewMesh, s_CurrentPreview.transform.localToWorldMatrix, 0);
			}
		}
	}
}