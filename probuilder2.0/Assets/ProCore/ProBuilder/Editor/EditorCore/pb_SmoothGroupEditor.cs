#define PB_ENABLE_SMOOTH_GROUP_PREVIEW

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProBuilder2.Common;
using ProBuilder2.Interface;

namespace ProBuilder2.EditorCommon
{
	public class pb_SmoothGroupEditor : EditorWindow
	{
		private static bool m_ShowPreview = true;
#if PB_ENABLE_SMOOTH_GROUP_PREVIEW
		private Dictionary<pb_Object, Mesh> m_PreviewMeshes = new Dictionary<pb_Object, Mesh>();
		private static Material m_FaceMaterial = null;
		private static Material faceMaterial
		{
			get
			{
				if (m_FaceMaterial == null)
				{
					m_FaceMaterial = new Material(Shader.Find("Hidden/ProBuilder/SmoothingPreview"));
					m_FaceMaterial.hideFlags = HideFlags.HideAndDontSave;
				}

				return m_FaceMaterial;
			}
		}
#endif
		private static Dictionary<pb_Object, Dictionary<int, List<pb_Face>>> m_SmoothGroups =
			new Dictionary<pb_Object, Dictionary<int, List<pb_Face>>>();
		private static Dictionary<pb_Object, HashSet<int>> m_SelectedGroups = new Dictionary<pb_Object, HashSet<int>>();
		private static Dictionary<pb_Object, bool> m_IsVisible = new Dictionary<pb_Object, bool>();
		private static GUIStyle m_GroupButtonStyle = null;
		private static GUIStyle groupButtonStyle
		{
			get
			{
				if (m_GroupButtonStyle == null)
				{
					m_GroupButtonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
					m_GroupButtonStyle.fixedWidth = 24;
					m_GroupButtonStyle.fixedHeight = 24;
				}
				return m_GroupButtonStyle;
			}
		}
		private GUIContent m_groupKeyContent = new GUIContent("21", "Smoothing Group");
		private Vector2 m_Scroll = Vector2.zero;
		private GUIContent m_HelpIcon = null;

		private static readonly Color SelectStateMixed = Color.yellow;
		private static readonly Color SelectStateNormal = Color.green;
		private static readonly Color SelectStateInUse = new Color(.2f, .8f, .2f, .5f);

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editors/Smoothing Groups")]
		public static void MenuOpenSmoothGroupEditor()
		{
			GetWindow<pb_SmoothGroupEditor>(true, "Smooth Group Editor", true);
		}

		private void OnEnable()
		{
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			Selection.selectionChanged += OnSelectionChanged;
			pb_Object.onElementSelectionChanged += OnElementSelectionChanged;
			this.autoRepaintOnSceneChange = true;

			// can't load icons from constructor
			m_HelpIcon = new GUIContent(pb_IconUtility.GetIcon("Toolbar/Help"), "Open Documentation");
			// load prefs
			m_ShowPreview = pb_PreferencesInternal.GetBool("pb_SmoothingGroupEditor::m_ShowPreview", false);
			OnSelectionChanged();
		}

		private void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			Selection.selectionChanged -= OnSelectionChanged;
			pb_Object.onElementSelectionChanged -= OnElementSelectionChanged;

#if PB_ENABLE_SMOOTH_GROUP_PREVIEW
			ClearPreviewMeshes();
#endif
		}

		private void OnSelectionChanged()
		{
			m_SmoothGroups.Clear();
			m_SelectedGroups.Clear();

			foreach (pb_Object pb in pb_Selection.Top())
			{
				OnElementSelectionChanged(pb);
				CollectSmoothingGroups(pb);
			}

#if PB_ENABLE_SMOOTH_GROUP_PREVIEW
			RebuildPreviewMeshes();
#endif

			this.Repaint();
		}

		private void OnElementSelectionChanged(pb_Object pb)
		{
			HashSet<int> selected;

			if(!m_SelectedGroups.TryGetValue(pb, out selected))
				m_SelectedGroups.Add(pb, selected = new HashSet<int>());
			else
				selected.Clear();

			foreach(pb_Face face in pb.SelectedFaces)
				selected.Add(face.smoothingGroup);
		}

		private void CollectSmoothingGroups(pb_Object pb)
		{
			Dictionary<int, List<pb_Face>> smoothDictionary;

			if(!m_SmoothGroups.TryGetValue(pb, out smoothDictionary))
				m_SmoothGroups.Add(pb, smoothDictionary = new Dictionary<int, List<pb_Face>>());
			else
				smoothDictionary.Clear();

			foreach (pb_Face face in pb.faces)
			{
				List<pb_Face> affected;

				if (!smoothDictionary.TryGetValue(face.smoothingGroup, out affected))
					smoothDictionary.Add(face.smoothingGroup, new List<pb_Face>() {face});
				else
					affected.Add(face);
			}
		}

#if PB_ENABLE_SMOOTH_GROUP_PREVIEW
		private void ClearPreviewMeshes()
		{
			foreach (var kvp in m_PreviewMeshes)
				Object.DestroyImmediate(kvp.Value);

			m_PreviewMeshes.Clear();
		}

		private void RebuildPreviewMeshes()
		{
			ClearPreviewMeshes();

			foreach (var kvp in m_SmoothGroups)
			{
				Mesh m = new Mesh()
				{
					hideFlags = HideFlags.HideAndDontSave,
					name = kvp.Key.name + "_SmoothingPreview"
				};
				m_PreviewMeshes.Add(kvp.Key, m);

				RebuildPreviewMesh(kvp.Key);
			}
			SceneView.RepaintAll();
		}

		private void RebuildPreviewMesh(pb_Object pb)
		{
			List<int> indices = new List<int>();
			Color32[] colors = new Color32[pb.vertexCount];
			int colorIndex = 0;

			Dictionary<int, List<pb_Face>> groups;

			// how?
			if(!m_SmoothGroups.TryGetValue(pb, out groups))
				return;

			foreach (KeyValuePair<int, List<pb_Face>> smoothGroup in groups)
			{
				if (smoothGroup.Key > pb_Smoothing.SMOOTHING_GROUP_NONE)
				{
					Color32 color = GetDistinctColor(colorIndex++);
					var groupIndices = smoothGroup.Value.SelectMany(y => y.indices);
					indices.AddRange(groupIndices);
					foreach (int i in groupIndices)
						colors[i] = color;
				}
			}

			Mesh m;
			if(!m_PreviewMeshes.TryGetValue(pb, out m))
				return;
			m.vertices = pb.vertices;
			m.colors32 = colors;
			m.triangles = indices.ToArray();
		}
#endif

		private void OnGUI()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (GUILayout.Button("Scene Preview",
				m_ShowPreview ? pb_EditorGUIUtility.GetOnStyle(EditorStyles.toolbarButton) : EditorStyles.toolbarButton))
			{
				m_ShowPreview = !m_ShowPreview;
				pb_PreferencesInternal.SetBool("pb_SmoothingGroupEditor::m_ShowPreview", m_ShowPreview);
			}

			GUILayout.FlexibleSpace();
			if(GUILayout.Button(m_HelpIcon, pb_EditorGUIUtility.toolbarHelpIcon))
				Application.OpenURL("http://procore3d.github.io/probuilder2/toolbar/tool-panels/#smoothing-groups");
			GUILayout.EndHorizontal();

			// border style is 4 margin, 4 pad, 1px content. inner is accounted for by btn size + btn margin.
			float area = (position.width - 10);
			float margin = Mathf.Max(groupButtonStyle.margin.left, groupButtonStyle.margin.right);
			int columns = (int)(area / (groupButtonStyle.CalcSize(m_groupKeyContent).x + margin)) - 1;

			if (m_SmoothGroups.Count < 1)
			{
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Select a ProBuilder Mesh", pb_EditorGUIUtility.CenteredGreyMiniLabel);
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				return;
			}

			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			foreach (var mesh in m_SmoothGroups)
			{
				pb_Object pb = mesh.Key;

				bool isVisible;

				if(!m_IsVisible.TryGetValue(pb, out isVisible))
					m_IsVisible.Add(pb, isVisible = true);

				GUILayout.BeginVertical(pb_EditorGUIUtility.SettingsGroupStyle);

				if(GUILayout.Button(pb.name, EditorStyles.boldLabel))
					m_IsVisible[pb] = (isVisible = !isVisible);

				Color stateColor = m_SelectedGroups[pb].Contains(0) ? SelectStateMixed : SelectStateNormal;

				if (isVisible)
				{
					int column = 0;

					GUILayout.BeginHorizontal();

					for (int i = 1; i < pb_Smoothing.SMOOTH_RANGE_MAX; i++)
					{
						if(m_SelectedGroups[pb].Contains(i))
							GUI.backgroundColor = stateColor;
						// if this group is used (but is not currently selected) show it as a muted green
						else if(m_SmoothGroups[pb].ContainsKey(i))
							GUI.backgroundColor = SelectStateInUse;

						if (GUILayout.Button(i.ToString(), groupButtonStyle))
							SetGroup(pb, i);
						GUI.backgroundColor = Color.white;

						if (++column > columns)
						{
							column = 0;
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
						}
					}

					GUILayout.EndHorizontal();
				}

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Break Smoothing"))
					SetGroup(pb, pb_Smoothing.SMOOTHING_GROUP_NONE);

				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Expand Selection"))
				{
					pbUndo.RecordSelection(pb, "Select with Smoothing Group");
					var all = mesh.Value.Where(x => m_SelectedGroups[pb].Contains(x.Key)).SelectMany(y => y.Value);
					pb.SetSelectedFaces(all);
					pb_Editor.Refresh();
				}
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			EditorGUILayout.EndScrollView();
		}

		private void OnSceneGUI(SceneView view)
		{
			if (m_SmoothGroups.Count > 1)
			{
				Handles.BeginGUI();
				foreach(var kvp in m_SmoothGroups)
					Handles.Label(kvp.Key.transform.position, kvp.Key.name, EditorStyles.boldLabel);
				Handles.EndGUI();
			}

#if PB_ENABLE_SMOOTH_GROUP_PREVIEW

			Event evt = Event.current;

			if (m_ShowPreview && evt.type == EventType.Repaint)
			{
				int index = 0;

				foreach (var kvp in m_PreviewMeshes)
				{
					faceMaterial.SetColor("_Color", GetDistinctColor(index++));
					faceMaterial.SetPass(0);
					Graphics.DrawMeshNow(kvp.Value, kvp.Key.transform.localToWorldMatrix);
				}
			}
#endif
		}

		private void SetGroup(pb_Object pb, int index)
		{
			foreach (pb_Face face in pb.SelectedFaces)
				face.smoothingGroup = index;

			CollectSmoothingGroups(pb);
			OnElementSelectionChanged(pb);
			RebuildPreviewMesh(pb);

			// todo pb.Rebuild
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		private static Color32 GetDistinctColor(int index)
		{
			return m_KellysMaxContrastSet[index % m_KellysMaxContrastSet.Length];
		}

		/**
		 * https://stackoverflow.com/questions/470690/how-to-automatically-generate-n-distinct-colors
		 */
		private static readonly Color32[] m_KellysMaxContrastSet = new Color32[]
		{
			UIntToColor(0xFFFFB300), //Vivid Yellow
			UIntToColor(0xFF803E75), //Strong Purple
			UIntToColor(0xFFFF6800), //Vivid Orange
//			UIntToColor(0xFFA6BDD7), //Very Light Blue
			UIntToColor(0xFFC10020), //Vivid Red
			UIntToColor(0xFFCEA262), //Grayish Yellow
			UIntToColor(0xFF817066), //Medium Gray

			//The following will not be good for people with defective color vision
			UIntToColor(0xFF007D34), //Vivid Green
			UIntToColor(0xFFF6768E), //Strong Purplish Pink
			UIntToColor(0xFF00538A), //Strong Blue
			UIntToColor(0xFFFF7A5C), //Strong Yellowish Pink
			UIntToColor(0xFF53377A), //Strong Violet
			UIntToColor(0xFFFF8E00), //Vivid Orange Yellow
			UIntToColor(0xFFB32851), //Strong Purplish Red
			UIntToColor(0xFFF4C800), //Vivid Greenish Yellow
			UIntToColor(0xFF7F180D), //Strong Reddish Brown
			UIntToColor(0xFF93AA00), //Vivid Yellowish Green
			UIntToColor(0xFF593315), //Deep Yellowish Brown
			UIntToColor(0xFFF13A13), //Vivid Reddish Orange
			UIntToColor(0xFF232C16), //Dark Olive Green
		};

		private static Color32 UIntToColor(uint color)
		{
			byte r = (byte) (color >> 16);
			byte g = (byte) (color >> 8);
			byte b = (byte) (color >> 0);
			return new Color32(r, g, b, 255);
		}
	}
}