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
		private class SmoothGroupData
		{
			public bool isVisible;
			public Dictionary<int, List<pb_Face>> groups;
			public Dictionary<int, Color> groupColors;
			public HashSet<int> selected;
			public Mesh previewMesh;

			public SmoothGroupData(pb_Object pb)
			{
				groups = new Dictionary<int, List<pb_Face>>();
				selected = new HashSet<int>();
				groupColors = new Dictionary<int, Color>();
				isVisible = true;

				previewMesh = new Mesh()
				{
					hideFlags = HideFlags.HideAndDontSave,
					name = pb.name + "_SmoothingPreview"
				};

				Rebuild(pb);
			}

			~SmoothGroupData()
			{
				EditorApplication.delayCall += () => { Object.DestroyImmediate(previewMesh); };
			}

			public void Rebuild(pb_Object pb)
			{
				CacheGroups(pb);
				CacheSelected(pb);
				RebuildPreviewMesh(pb);
			}

			public void CacheGroups(pb_Object pb)
			{
				groups.Clear();

				foreach (pb_Face face in pb.faces)
				{
					List<pb_Face> affected;

					if (!groups.TryGetValue(face.smoothingGroup, out affected))
						groups.Add(face.smoothingGroup, new List<pb_Face>() {face});
					else
						affected.Add(face);
				}
			}

			public void CacheSelected(pb_Object pb)
			{
				selected.Clear();

				foreach(pb_Face face in pb.SelectedFaces)
					selected.Add(face.smoothingGroup);
			}

			private void RebuildPreviewMesh(pb_Object pb)
			{
				List<int> indices = new List<int>();
				Color32[] colors = new Color32[pb.vertexCount];
				int colorIndex = 0;
				groupColors.Clear();

				foreach (KeyValuePair<int, List<pb_Face>> smoothGroup in groups)
				{
					if (smoothGroup.Key > pb_Smoothing.SMOOTHING_GROUP_NONE)
					{
						Color32 color = GetDistinctColor(colorIndex++);
						groupColors.Add(smoothGroup.Key, color);
						var groupIndices = smoothGroup.Value.SelectMany(y => y.indices);
						indices.AddRange(groupIndices);
						foreach (int i in groupIndices)
							colors[i] = color;
					}
				}

				previewMesh.Clear();
				previewMesh.vertices = pb.vertices;
				previewMesh.colors32 = colors;
				previewMesh.triangles = indices.ToArray();
			}
		}

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

		private static GUIStyle m_GroupButtonStyle = null;
		private static GUIStyle groupButtonStyle
		{
			get
			{
				if (m_GroupButtonStyle == null)
				{
					m_GroupButtonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
					m_GroupButtonStyle.normal.background = pb_IconUtility.GetIcon("Toolbar/Background/RoundedRect_Normal");
					m_GroupButtonStyle.hover.background = pb_IconUtility.GetIcon("Toolbar/Background/RoundedRect_Hover");
					m_GroupButtonStyle.active.background = pb_IconUtility.GetIcon("Toolbar/Background/RoundedRect_Pressed");
					m_GroupButtonStyle.border = new RectOffset(3, 3, 3, 3);
					m_GroupButtonStyle.margin = new RectOffset(4, 4, 4, 6);
					m_GroupButtonStyle.alignment = TextAnchor.MiddleCenter;
					m_GroupButtonStyle.fixedWidth = IconWidth;
					m_GroupButtonStyle.fixedHeight = IconHeight;
				}
				return m_GroupButtonStyle;
			}
		}

		private static GUIStyle m_ColorKeyStyle = null;
		private static GUIStyle colorKeyStyle
		{
			get
			{
				if (m_ColorKeyStyle == null)
				{
					m_ColorKeyStyle = new GUIStyle(groupButtonStyle);
					m_ColorKeyStyle.fixedWidth = IconWidth;
					m_ColorKeyStyle.fixedHeight = 3;
					m_ColorKeyStyle.padding = new RectOffset(4, 4, 0, 4);
					m_ColorKeyStyle.normal.background = EditorGUIUtility.whiteTexture;
				}
				return m_ColorKeyStyle;
			}
		}

		private static GUIStyle m_WordWrappedRichText = null;

		private static GUIStyle wordWrappedRichText
		{
			get
			{
				if (m_WordWrappedRichText == null)
				{
					m_WordWrappedRichText = new GUIStyle(EditorStyles.wordWrappedLabel);
					m_WordWrappedRichText.richText = true;
					m_WordWrappedRichText.alignment = TextAnchor.LowerLeft;
				}
				return m_WordWrappedRichText;
			}
		}

		private const int IconWidth = 24;
		private const int IconHeight = 24;
		private GUIContent m_groupKeyContent = new GUIContent("21", "Smoothing Group");
		private Vector2 m_Scroll = Vector2.zero;
		private GUIContent m_HelpIcon = null;
		private GUIContent m_BreakSmoothingContent = null;
		private Dictionary<pb_Object, SmoothGroupData> m_SmoothGroups = new Dictionary<pb_Object, SmoothGroupData>();
		private static bool m_ShowPreview = true;
		private static bool m_IsMovingVertices = false;
		private static bool m_ShowHelp = false;
		private static readonly Color SelectStateMixed = Color.yellow;
		private static readonly Color SelectStateNormal = Color.green;
		private static readonly Color SelectStateInUse = new Color(.2f, .8f, .2f, .5f);

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editors/Smoothing Groups")]
		public static void MenuOpenSmoothGroupEditor()
		{
			bool isUtility = pb_PreferencesInternal.GetBool("pb_SmoothGroupEditor::m_IsWindowUtility", true);
			GetWindow<pb_SmoothGroupEditor>(isUtility, "Smooth Group Editor", true);
		}

		private void OnEnable()
		{
			if(pb_Editor.instance)
				pb_Editor.instance.SetSelectionMode(SelectMode.Face);

			SceneView.onSceneGUIDelegate += OnSceneGUI;
			Selection.selectionChanged += OnSelectionChanged;
			Undo.undoRedoPerformed += OnSelectionChanged;
			pb_Object.onElementSelectionChanged += OnElementSelectionChanged;
			pb_Editor.OnVertexMovementBegin += OnBeginVertexMovement;
			pb_Editor.OnVertexMovementFinish += OnFinishVertexMovement;
			this.autoRepaintOnSceneChange = true;
			m_HelpIcon = new GUIContent(pb_IconUtility.GetIcon("Toolbar/Help"), "Open Documentation");
			m_ShowPreview = pb_PreferencesInternal.GetBool("pb_SmoothingGroupEditor::m_ShowPreview", false);
			m_BreakSmoothingContent = new GUIContent(pb_IconUtility.GetIcon("Toolbar/Face_BreakSmoothing"),
				"Clear the selected faces of their smoothing groups");
			OnSelectionChanged();
		}

		private void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			Selection.selectionChanged -= OnSelectionChanged;
			Undo.undoRedoPerformed -= OnSelectionChanged;
			pb_Object.onElementSelectionChanged -= OnElementSelectionChanged;
			m_SmoothGroups.Clear();
		}

		private void OnDestroy()
		{
			m_SmoothGroups.Clear();
		}

		private void OnBeginVertexMovement(pb_Object[] selection)
		{
			m_IsMovingVertices = true;
		}

		private void OnFinishVertexMovement(pb_Object[] selection)
		{
			m_IsMovingVertices = false;
			OnSelectionChanged();
		}

		private void OnSelectionChanged()
		{
			m_SmoothGroups.Clear();

			foreach (pb_Object pb in pb_Selection.Top())
				m_SmoothGroups.Add(pb, new SmoothGroupData(pb));

			this.Repaint();
		}

		private void OnElementSelectionChanged(pb_Object pb)
		{
			SmoothGroupData data;

			if(!m_SmoothGroups.TryGetValue(pb, out data))
				m_SmoothGroups.Add(pb, data = new SmoothGroupData(pb));
			else
				data.CacheSelected(pb);
		}

		private static void SetWindowIsUtility(bool isUtility)
		{
			pb_PreferencesInternal.SetBool("pb_SmoothGroupEditor::m_IsWindowUtility", isUtility);
			GetWindow<pb_SmoothGroupEditor>().Close();
			MenuOpenSmoothGroupEditor();
		}

		private void OnGUI()
		{
			Event evt = Event.current;

			if (evt.type == EventType.ContextClick)
			{
				bool isUtility = pb_PreferencesInternal.GetBool("pb_SmoothGroupEditor::m_IsWindowUtility", true);

				GenericMenu menu = new GenericMenu();
				menu.AddItem (new GUIContent("Open As Floating Window", ""), isUtility, () => SetWindowIsUtility(true));
				menu.AddItem (new GUIContent("Open As Dockable Window", ""), !isUtility, () => SetWindowIsUtility(false));
				menu.ShowAsContext ();
			}

			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (GUILayout.Button("Scene Preview",
				m_ShowPreview ? pb_EditorGUIUtility.GetOnStyle(EditorStyles.toolbarButton) : EditorStyles.toolbarButton))
			{
				m_ShowPreview = !m_ShowPreview;
				pb_PreferencesInternal.SetBool("pb_SmoothingGroupEditor::m_ShowPreview", m_ShowPreview);
			}

			GUILayout.FlexibleSpace();
			if(GUILayout.Button(m_HelpIcon, pb_EditorGUIUtility.toolbarHelpIcon))
				m_ShowHelp = !m_ShowHelp;
			GUILayout.EndHorizontal();

			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			if (m_ShowHelp)
			{
				GUILayout.BeginVertical(pb_EditorGUIUtility.SettingsGroupStyle);

				GUILayout.Label("Create and Clear Smoothing Groups", EditorStyles.boldLabel);

				GUILayout.Label("Adjacent faces with the same smoothing group will appear to have a soft adjoining edge.", wordWrappedRichText);
				GUILayout.Space(2);
				GUILayout.Label("<b>To smooth</b> a selected group of faces, click one of the Smooth Group buttons.", wordWrappedRichText);
				GUILayout.Label("<b>To clear</b> selected faces of their smooth group, click the [Break] icon.", wordWrappedRichText);
				GUILayout.Label("<b>To select</b> all faces in a group, Right+Click or Alt+Click a smooth group button.", wordWrappedRichText);
				GUILayout.Space(2);

				pb_EditorGUILayout.BeginRow();
				GUI.backgroundColor = Color.white;
				GUILayout.Button("1", groupButtonStyle);
				GUILayout.Label("An unused smooth group", wordWrappedRichText);
				pb_EditorGUILayout.EndRow();

				pb_EditorGUILayout.BeginRow();
				GUI.backgroundColor = SelectStateInUse;
				GUILayout.Button("1", groupButtonStyle);
				GUILayout.Label("A smooth group that is in use, but not in the current selection", wordWrappedRichText);
				pb_EditorGUILayout.EndRow();

				pb_EditorGUILayout.BeginRow();
				GUI.backgroundColor = SelectStateNormal;
				GUILayout.Button("1", groupButtonStyle);
				GUILayout.Label("A smooth group that is currently selected", wordWrappedRichText);
				pb_EditorGUILayout.EndRow();

				pb_EditorGUILayout.BeginRow();
				GUI.backgroundColor = SelectStateMixed;
				GUILayout.Button("1", groupButtonStyle);
				GUI.backgroundColor = Color.white;
				GUILayout.Label("A smooth group is selected, but the selection also contains non-grouped faces", wordWrappedRichText);
				pb_EditorGUILayout.EndRow();

				if(GUILayout.Button("Open Documentation"))
					Application.OpenURL("http://procore3d.github.io/probuilder2/toolbar/tool-panels/#smoothing-groups");

				GUILayout.EndVertical();
			}

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

				// End inspector scroll if exiting early
				EditorGUILayout.EndScrollView();

				return;
			}

			foreach (var mesh in m_SmoothGroups)
			{
				pb_Object pb = mesh.Key;
				SmoothGroupData data = mesh.Value;

				GUILayout.BeginVertical(pb_EditorGUIUtility.SettingsGroupStyle);

				if(GUILayout.Button(pb.name, EditorStyles.boldLabel))
					data.isVisible = !data.isVisible;

				Color stateColor = data.selected.Contains(0) ? SelectStateMixed : SelectStateNormal;

				if (data.isVisible)
				{
					int column = 0;

					GUILayout.BeginHorizontal();

					for (int i = 1; i < pb_Smoothing.SMOOTH_RANGE_MAX; i++)
					{
						if(data.selected.Contains(i))
							GUI.backgroundColor = stateColor;
						// if this group is used (but is not currently selected) show it as a muted green
						else if(data.groups.ContainsKey(i))
							GUI.backgroundColor = SelectStateInUse;

						if (m_ShowPreview)
							GUILayout.BeginVertical(GUILayout.MaxWidth(IconWidth));

						if (GUILayout.Button(i.ToString(), groupButtonStyle))
						{
							// if right click or alt click select the faces instead of setting a group
							if((Event.current.modifiers & EventModifiers.Alt) == EventModifiers.Alt ||
								Event.current.button != 0)
								SelectGroups(pb, new HashSet<int>() { i });
							else
								SetGroup(pb, i);
						}

						if (m_ShowPreview)
						{
							GUI.backgroundColor = data.groupColors.ContainsKey(i) ? data.groupColors[i] : Color.clear;
							GUILayout.Label("", colorKeyStyle);
							GUILayout.EndVertical();
						}
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
				GUILayout.FlexibleSpace();

//				if (GUILayout.Button("Expand Selection"))
//					SelectGroups(pb, data.selected);

				if (GUILayout.Button(m_BreakSmoothingContent,
					pb_ToolbarGroupUtility.GetStyle(pb_ToolbarGroup.Geometry, true)))
					SetGroup(pb, pb_Smoothing.SMOOTHING_GROUP_NONE);

				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			EditorGUILayout.EndScrollView();

			// This isn't great, but we need hover previews to work
			if(mouseOverWindow == this)
				Repaint();
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

			Event evt = Event.current;

			if (m_ShowPreview && !m_IsMovingVertices && evt.type == EventType.Repaint)
			{
				int index = 0;

				foreach (var kvp in m_SmoothGroups)
				{
					Mesh m = kvp.Value.previewMesh;
					if(m == null)
						continue;
					faceMaterial.SetColor("_Color", GetDistinctColor(index++));
					faceMaterial.SetPass(0);
					Graphics.DrawMeshNow(m, kvp.Key.transform.localToWorldMatrix);
				}
			}
		}

		private void SelectGroups(pb_Object pb, HashSet<int> groups)
		{
			pbUndo.RecordSelection(pb, "Select with Smoothing Group");

			if( (Event.current.modifiers & EventModifiers.Shift) == EventModifiers.Shift ||
				(Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control )
				pb.SetSelectedFaces(pb.faces.Where(x => groups.Contains(x.smoothingGroup) || pb.SelectedFaces.Contains(x)));
			else
				pb.SetSelectedFaces(pb.faces.Where(x => groups.Contains(x.smoothingGroup)));
			pb_Editor.Refresh();
		}

		private void SetGroup(pb_Object pb, int index)
		{
			foreach (pb_Face face in pb.SelectedFaces)
				face.smoothingGroup = index;

			SmoothGroupData data;
			if(!m_SmoothGroups.TryGetValue(pb, out data))
				m_SmoothGroups.Add(pb, new SmoothGroupData(pb));
			else
				data.Rebuild(pb);

			// todo pb.Rebuild
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();

			pb_Editor.Refresh();
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