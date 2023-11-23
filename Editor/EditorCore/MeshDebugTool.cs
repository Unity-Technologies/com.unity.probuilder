using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
	class MeshDebugTool : EditorTool
	{
		[MenuItem("Tools/ProBuilder/Repair/Mesh Debug Tool", false, PreferenceKeys.menuRepair + 100)]
		static void Init() => ToolManager.SetActiveTool<MeshDebugTool>();

		ProBuilderMesh[] m_Meshes;
		List<InvalidFaceInfo>[] m_Invalid;

		struct InvalidFaceInfo
		{
			public Vector3 position;
			public int faceIndex;
			public string problems;
		}

		void OnEnable()
		{
			Selection.selectionChanged += UpdateSelection;
			UpdateSelection();
		}

		void OnDisable()
		{
			Selection.selectionChanged -= UpdateSelection;
		}

		void UpdateSelection()
		{
			m_Meshes = Selection.GetFiltered<ProBuilderMesh>(SelectionMode.TopLevel);
			m_Invalid = new List<InvalidFaceInfo>[m_Meshes.Length];

			for (int i = 0, c = m_Meshes.Length; i < c; ++i)
			{
				m_Invalid[i] = new List<InvalidFaceInfo>();
				var faces = m_Meshes[i].facesInternal;
				for (int n = 0, f = faces.Length; n < f; n++)
					if (CheckFaceVertexAttributes(m_Meshes[i], faces[n], out var info))
						m_Invalid[i].Add(info);
			}
		}

		public override void OnToolGUI(EditorWindow window)
		{
			var evt = Event.current;

			DoSceneViewOverlay();

			for(int i = 0, c = m_Meshes.Length; i < c; ++i)
			{
				using var matrix = new Handles.DrawingScope(m_Meshes[i].transform.localToWorldMatrix);

				foreach (var info in m_Invalid[i])
				{
					Handles.DotHandleCap(0, info.position, Quaternion.identity, HandleUtility.GetHandleSize(info.position), evt.type);
					Handles.ShowSceneViewLabel(info.position, EditorGUIUtility.TempContent(info.problems));
				}
			}
		}

		void DoSceneViewOverlay()
		{
			Handles.BeginGUI();
			GUILayout.Space(4);
			GUILayout.BeginHorizontal();
			GUILayout.Space(4);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			if (m_Meshes.Length > 0)
			{
				for (int i = 0, c = m_Meshes.Length; i < c; ++i)
				{
					GUILayout.BeginHorizontal();
					if (m_Invalid[i].Count > 0 && GUILayout.Button("Fix"))
					{
						MeshValidation.EnsureMeshIsValid(m_Meshes[i], out var removedVertexCount);
						Debug.Log($"Successfully repaired {m_Meshes[i].name}. Removed {removedVertexCount} problem vertices.");
					}
					GUILayout.Label($"Mesh \"{m_Meshes[i].name}\" found {m_Invalid[i].Count} problems");
					GUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("Select a ProBuilder Mesh");
			}

			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			Handles.EndGUI();
		}

		bool CheckFaceVertexAttributes(ProBuilderMesh mesh, Face face, out InvalidFaceInfo info)
		{
			info = default;
			var positions = mesh.positionsInternal;
			var normals = mesh.normalsInternal;
			var tangents = mesh.tangentsInternal;
			var textures = mesh.texturesInternal;
			var indices = face.indexesInternal;
			var distinct = face.distinctIndexesInternal;

			for (int i = 0, c = indices.Length; i < c; i+=3)
			{
				if (Math.TriangleArea(positions[indices[i]], positions[indices[i+1]], positions[indices[i+2]]) < float.Epsilon)
					info.problems += $"Degenerate Triangle {indices[i]} {indices[i+1]} {indices[i+2]}";
			}

			for (int i = 0, c = distinct.Length; i < c; ++i)
			{
				if (normals  != null && !Math.IsNumber(normals[i]))
					info.problems += $"normals [{i}] is NaN";
				if (textures != null && !Math.IsNumber(textures[i]))
					info.problems += $"uv0 [{i}] is NaN";
				if (tangents != null && !Math.IsNumber(tangents[i]))
					info.problems += $"tangents [{i}] is NaN";
			}

			info.position = Math.Average(positions, distinct);

			return !string.IsNullOrEmpty(info.problems);
		}
	}
}
