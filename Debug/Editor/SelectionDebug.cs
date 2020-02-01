using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class SelectionDebug : ConfigurableWindow
	{
		static class Styles
		{
			static bool s_RowToggle;
			static readonly Color RowOddColor = new Color(.45f, .45f, .45f, .2f);
			static readonly Color RowEvenColor = new Color(.30f, .30f, .30f, .2f);

			public static void BeginRow(int index = -1)
			{
				if (index > -1)
					s_RowToggle = index % 2 == 0;

				var bg = GUI.backgroundColor;
				GUI.backgroundColor = s_RowToggle ? RowEvenColor : RowOddColor;
				GUILayout.BeginHorizontal(UI.EditorStyles.rowStyle);
				s_RowToggle = !s_RowToggle;
				GUI.backgroundColor = bg;
			}

			public static void EndRow()
			{
				GUILayout.EndHorizontal();
			}
		}

		[MenuItem("Tools/ProBuilder/Debug/Selection Editor")]
		static void Init()
		{
			GetWindow<SelectionDebug>();
		}

		static bool s_DisplayElementGroups = true;
        static bool s_DisplaySelection = true;

		void OnEnable()
		{
			MeshSelection.objectSelectionChanged += Repaint;
			ProBuilderMesh.elementSelectionChanged += Repaint;
		}

		void OnDisable()
		{
			MeshSelection.objectSelectionChanged -= Repaint;
			ProBuilderMesh.elementSelectionChanged -= Repaint;
		}

		void Repaint(ProBuilderMesh mesh)
		{
			Repaint();
		}

		void OnGUI()
		{
			s_DisplayElementGroups = EditorGUILayout.Foldout(s_DisplayElementGroups, "Element Groups");

			if (s_DisplayElementGroups)
			{
				foreach (var group in MeshSelection.elementSelection)
				{
					GUILayout.Label(group.mesh.name);

					foreach (var element in group.elementGroups)
					{
						Styles.BeginRow();
						GUILayout.Label(element.indices.ToString(","));
						Styles.EndRow();
					}
				}
			}

            s_DisplaySelection = EditorGUILayout.Foldout(s_DisplaySelection, "Selection");

            if (s_DisplaySelection)
            {
                foreach (var kvp in SelectionManager.instance.selection)
                {
                    GUILayout.Label(kvp.Key.name);

                    var collection = kvp.Value;
                    Styles.BeginRow();
                    GUILayout.Label($"Faces: {collection.selectedFaceCount}");
                    Styles.EndRow();
                    Styles.BeginRow();
                    GUILayout.Label($"Edges: {collection.selectedEdgeCount}");
                    Styles.EndRow();
                    Styles.BeginRow();
                    GUILayout.Label($"Verts: {collection.selectedVertexCount}");
                    Styles.EndRow();
                }
            }

		}
	}
}
