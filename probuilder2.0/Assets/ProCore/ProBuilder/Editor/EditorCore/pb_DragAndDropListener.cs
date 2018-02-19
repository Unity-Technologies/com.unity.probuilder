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

		static pb_DragAndDropListener()
		{
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}

		public static bool IsDragging()
		{
			return s_IsSceneViewDragAndDrop;
		}

		static Material GetMaterialFromDragReferences(UObject[] references)
		{
			Material mat = references.FirstOrDefault(x => x is Material) as Material;

			if (mat != null)
				return mat;

			Texture2D tex = references.FirstOrDefault(x => x is Texture2D) as Texture2D;
			string texPath = AssetDatabase.GetAssetPath(mat.mainTexture);

			if (tex != null && !string.IsNullOrEmpty(texPath))
			{
				mat = pb_Reflection.Invoke(null, "GetDefaultMaterial") as Material;

				if (mat == null)
				{
					pb_Log.Debug("material is still null, bailing");
					pb_Log.Debug("material is still null, bailing");
					mat = new Material(Shader.Find("Standard"));
				}

				if (mat.shader == null)
				{
					pb_Log.Debug("material is still null, bailing");
					UObject.DestroyImmediate(mat);
					return null;
				}

				mat.mainTexture = tex;

				int lastDot = texPath.LastIndexOf(".", StringComparison.InvariantCulture);
				texPath = texPath.Substring(0, texPath.Length - (texPath.Length - lastDot));
				texPath = AssetDatabase.GenerateUniqueAssetPath(texPath + ".mat");
				AssetDatabase.CreateAsset(mat, texPath);
				AssetDatabase.Refresh();
			}

			return null;
		}

		static void OnSceneGUI(SceneView sceneView)
		{
			var evt = Event.current;

			if (evt.type == EventType.DragUpdated)
			{
				s_IsSceneViewDragAndDrop = true;
			}
			else if (evt.type == EventType.DragExited)
			{
				s_IsSceneViewDragAndDrop = false;
			}
			else if (evt.type == EventType.DragPerform)
			{
				s_IsSceneViewDragAndDrop = false;

				int materialIndex;
				GameObject go = HandleUtility.PickGameObject(evt.mousePosition, out materialIndex);
				pb_Object pb = go.GetComponent<pb_Object>();

				if (pb != null)
				{
					Material draggedMaterial = GetMaterialFromDragReferences(DragAndDrop.objectReferences);

					if (draggedMaterial != null)
					{
						var mr = pb.GetComponent<MeshRenderer>();
						Material hoveredMaterial = mr == null ? null : mr.sharedMaterials[materialIndex];

						bool isFaceMode = pb_Editor.instance != null &&
						                  pb_Editor.instance.editLevel == EditLevel.Geometry &&
						                  pb_Editor.instance.selectionMode == SelectMode.Face;

						pb_Undo.RecordObject(pb, "Set Face Material");

						foreach (var face in pb.faces)
						{
							if (hoveredMaterial == null || face.material == hoveredMaterial)
							{
								face.material = draggedMaterial;
							}
						}

						pb.ToMesh();
						pb.Refresh();
						pb.Optimize();
					}

					evt.Use();
				}
			}
		}
	}
}