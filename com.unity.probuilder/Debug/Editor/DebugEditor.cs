using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	sealed class DebugEditor : ConfigurableWindow
	{
		static Dictionary<string, bool> s_Expanded = new Dictionary<string, bool>();
		Vector2 m_Scroll = Vector2.zero;

		[MenuItem("Tools/ProBuilder/Debug/Debug Window")]
		static void Init()
		{
			GetWindow<DebugEditor>(IsUtilityWindow<DebugEditor>(), "ProBuilder Debug", true);
		}

		void OnEnable()
		{
			MeshSelection.objectSelectionChanged += OnSelectionChanged;
		}

		void OnDisable()
		{
			MeshSelection.objectSelectionChanged -= OnSelectionChanged;
		}

		void OnSelectionChanged()
		{
			Repaint();
		}

		void OnGUI()
		{
			DoContextMenu();

			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			foreach (var mesh in MeshSelection.top)
			{
				DoMeshInfo(mesh);
			}

			EditorGUILayout.EndScrollView();
		}

		void DoMeshInfo(ProBuilderMesh mesh)
		{
			DoSharedVerticesInfo(mesh);
			DoSharedTexturesInfo(mesh);
		}

		static void BeginSectionHeader(ProBuilderMesh mesh, string field)
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			var fieldInfo = typeof(ProBuilderMesh).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
			var id = GetPropertyId(mesh, fieldInfo.Name);
			if (!s_Expanded.ContainsKey(id))
				s_Expanded.Add(id, true);
			s_Expanded[id] = EditorGUILayout.Foldout(s_Expanded[id], fieldInfo.MemberType + " " + fieldInfo.Name);
			GUILayout.FlexibleSpace();
		}

		static void EndSectionHeader()
		{
			GUILayout.EndHorizontal();
		}

		static string GetPropertyId(ProBuilderMesh mesh, string property)
		{
			return string.Format("{0}.{1}", mesh.GetInstanceID(), property);
		}

		void DoSharedVerticesInfo(ProBuilderMesh mesh)
		{
			BeginSectionHeader(mesh, "m_SharedVertices");
			if(GUILayout.Button("Invalidate Cache", EditorStyles.toolbarButton))
				mesh.InvalidateSharedVertexLookup();
			GUILayout.EndHorizontal();

			var sharedVertices = mesh.sharedVerticesInternal;

			for (int i = 0; i < sharedVertices.Length; i++)
				GUILayout.Label(sharedVertices[i].ToString(", "));
		}

		void DoSharedTexturesInfo(ProBuilderMesh mesh)
		{
			BeginSectionHeader(mesh, "m_SharedTextures");
			if(GUILayout.Button("Invalidate Cache", EditorStyles.toolbarButton))
				mesh.InvalidateSharedTextureLookup();
			GUILayout.EndHorizontal();

			var sharedVertices = mesh.sharedTextures;

			for (int i = 0; sharedVertices != null && i < sharedVertices.Length; i++)
				GUILayout.Label(sharedVertices[i].ToString(", "));
		}
	}
}
