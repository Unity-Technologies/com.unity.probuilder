#pragma warning disable 0414

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Custom editor for pb_UV type.
	/// </summary>
	static class AutoUVEditor
	{
#region MEMBERS

		static ProBuilderEditor editor
		{
			get { return ProBuilderEditor.instance; }
		}

		static AutoUnwrapSettings m_AutoUVSettings = new AutoUnwrapSettings();
		static int textureGroup = -1;
		static List<AutoUnwrapSettings> m_AutoUVSettingsInSelection = new List<AutoUnwrapSettings>();
		static Dictionary<string, bool> m_AutoUVSettingsDiff = new Dictionary<string, bool>()
		{
			{ "projectionAxis", false },
			{ "useWorldSpace", false },
			{ "flipU", false },
			{ "flipV", false },
			{ "swapUV", false },
			{ "fill", false },
			{ "scalex", false },
			{ "scaley", false },
			{ "offsetx", false },
			{ "offsety", false },
			{ "rotation", false },
			{ "anchor", false },
			{ "manualUV", false },
			{ "textureGroup", false }
		};

		public enum Axis2D
		{
			XY,
			X,
			Y
		}

		static Vector2 m_ScrollPosition;

#endregion

#region ONGUI

		/// <summary>
		///
		/// </summary>
		/// <param name="selection"></param>
		/// <param name="maxWidth"></param>
		/// <returns>Returns true on GUI change detected.</returns>
		public static bool OnGUI(ProBuilderMesh[] selection, float width)
		{
			UpdateDiffDictionary(selection);

			m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

			int tempInt = -1;
			float tempFloat = 0f;
			Vector2 tempVec2 = Vector2.zero;
			bool tempBool = false;

			EditorGUI.BeginChangeCheck();

			/**
			 * Set Tile mode
			 */
			GUILayout.Label("Tiling & Alignment", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
				tempInt = (int)m_AutoUVSettings.fill;
				EditorGUI.showMixedValue = m_AutoUVSettingsDiff["fill"];
				GUILayout.Label("Fill Mode", GUILayout.MaxWidth(80), GUILayout.MinWidth(80));
				m_AutoUVSettings.fill = (AutoUnwrapSettings.Fill)EditorGUILayout.EnumPopup(m_AutoUVSettings.fill);
				if(tempInt != (int)m_AutoUVSettings.fill) SetFill(m_AutoUVSettings.fill, selection);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				bool enabled = GUI.enabled;
				GUI.enabled = !m_AutoUVSettings.useWorldSpace;
				tempInt = (int) m_AutoUVSettings.anchor;
				EditorGUI.showMixedValue = m_AutoUVSettingsDiff["anchor"];
				GUILayout.Label("Anchor", GUILayout.MaxWidth(80), GUILayout.MinWidth(80));
				m_AutoUVSettings.anchor = (AutoUnwrapSettings.Anchor) EditorGUILayout.EnumPopup(m_AutoUVSettings.anchor);
				if(tempInt != (int)m_AutoUVSettings.anchor) SetAnchor(m_AutoUVSettings.anchor, selection);
				GUI.enabled = enabled;
			GUILayout.EndHorizontal();

			UnityEngine.GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
			UI.EditorGUIUtility.DrawSeparator(1);
			UnityEngine.GUI.backgroundColor = Color.white;

			GUILayout.Label("Transform", EditorStyles.boldLabel);

			/**
			 * Offset
			 */
			EditorGUI.showMixedValue = m_AutoUVSettingsDiff["offsetx"] || m_AutoUVSettingsDiff["offsety"];
			tempVec2 = m_AutoUVSettings.offset;
			UnityEngine.GUI.SetNextControlName("offset");
			m_AutoUVSettings.offset = EditorGUILayout.Vector2Field("Offset", m_AutoUVSettings.offset, GUILayout.MaxWidth(width));
			if(tempVec2.x != m_AutoUVSettings.offset.x) { SetOffset(m_AutoUVSettings.offset, Axis2D.X, selection); }
			if(tempVec2.y != m_AutoUVSettings.offset.y) { SetOffset(m_AutoUVSettings.offset, Axis2D.Y, selection); }

			/**
			 * Rotation
			 */
			tempFloat = m_AutoUVSettings.rotation;
			EditorGUI.showMixedValue = m_AutoUVSettingsDiff["rotation"];
			GUILayout.Label(new GUIContent("Rotation", "Rotation around the center of face UV bounds."), GUILayout.MaxWidth(width-64));
			UnityEngine.GUI.SetNextControlName("rotation");
			EditorGUI.BeginChangeCheck();
			tempFloat = EditorGUILayout.Slider(tempFloat, 0f, 360f, GUILayout.MaxWidth(width));
			if(EditorGUI.EndChangeCheck())
				SetRotation(tempFloat, selection);

			/**
			 * Scale
			 */
			EditorGUI.showMixedValue = m_AutoUVSettingsDiff["scalex"] || m_AutoUVSettingsDiff["scaley"];
			tempVec2 = m_AutoUVSettings.scale;
			UnityEngine.GUI.SetNextControlName("scale");
			EditorGUI.BeginChangeCheck();
			m_AutoUVSettings.scale = EditorGUILayout.Vector2Field("Tiling", m_AutoUVSettings.scale, GUILayout.MaxWidth(width));

			if(EditorGUI.EndChangeCheck())
			{
				if(tempVec2.x != m_AutoUVSettings.scale.x) { SetScale(m_AutoUVSettings.scale, Axis2D.X, selection); }
				if(tempVec2.y != m_AutoUVSettings.scale.y) { SetScale(m_AutoUVSettings.scale, Axis2D.Y, selection); }
			}

			// Draw tiling shortcuts
			GUILayout.BeginHorizontal();
			if( GUILayout.Button(".5", EditorStyles.miniButtonLeft) )	SetScale(Vector2.one * 2f, Axis2D.XY, selection);
			if( GUILayout.Button("1", EditorStyles.miniButtonMid) )		SetScale(Vector2.one, Axis2D.XY, selection);
			if( GUILayout.Button("2", EditorStyles.miniButtonMid) )		SetScale(Vector2.one * .5f, Axis2D.XY, selection);
			if( GUILayout.Button("4", EditorStyles.miniButtonMid) )		SetScale(Vector2.one * .25f, Axis2D.XY, selection);
			if( GUILayout.Button("8", EditorStyles.miniButtonMid) )		SetScale(Vector2.one * .125f, Axis2D.XY, selection);
			if( GUILayout.Button("16", EditorStyles.miniButtonRight) ) 	SetScale(Vector2.one * .0625f, Axis2D.XY, selection);
			GUILayout.EndHorizontal();

			GUILayout.Space(4);

			UnityEngine.GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
			UI.EditorGUIUtility.DrawSeparator(1);
			UnityEngine.GUI.backgroundColor = Color.white;

			/**
			 * Special
			 */
			GUILayout.Label("Special", EditorStyles.boldLabel);

			tempBool = m_AutoUVSettings.useWorldSpace;
			EditorGUI.showMixedValue = m_AutoUVSettingsDiff["useWorldSpace"];
			m_AutoUVSettings.useWorldSpace = EditorGUILayout.Toggle("World Space", m_AutoUVSettings.useWorldSpace);
			if(m_AutoUVSettings.useWorldSpace != tempBool) SetUseWorldSpace(m_AutoUVSettings.useWorldSpace, selection);

			UnityEngine.GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
			UI.EditorGUIUtility.DrawSeparator(1);
			UnityEngine.GUI.backgroundColor = Color.white;


			// Flip U
			tempBool = m_AutoUVSettings.flipU;
			EditorGUI.showMixedValue = m_AutoUVSettingsDiff["flipU"];
			m_AutoUVSettings.flipU = EditorGUILayout.Toggle("Flip U", m_AutoUVSettings.flipU);
			if(tempBool != m_AutoUVSettings.flipU) SetFlipU(m_AutoUVSettings.flipU, selection);

			// Flip V
			tempBool = m_AutoUVSettings.flipV;
			EditorGUI.showMixedValue = m_AutoUVSettingsDiff["flipV"];
			m_AutoUVSettings.flipV = EditorGUILayout.Toggle("Flip V", m_AutoUVSettings.flipV);
			if(tempBool != m_AutoUVSettings.flipV) SetFlipV(m_AutoUVSettings.flipV, selection);

			tempBool = m_AutoUVSettings.swapUV;
			EditorGUI.showMixedValue = m_AutoUVSettingsDiff["swapUV"];
			m_AutoUVSettings.swapUV = EditorGUILayout.Toggle("Swap U/V", m_AutoUVSettings.swapUV);
			if(tempBool != m_AutoUVSettings.swapUV) SetSwapUV(m_AutoUVSettings.swapUV, selection);

			/**
			 * Texture Groups
			 */
			GUILayout.Label("Texture Groups", EditorStyles.boldLabel);

			tempInt = textureGroup;
			EditorGUI.showMixedValue = m_AutoUVSettingsDiff["textureGroup"];

			UnityEngine.GUI.SetNextControlName("textureGroup");
			textureGroup = UI.EditorGUIUtility.IntFieldConstrained(new GUIContent("Texture Group", "Faces in a texture group will be UV mapped as a group, just as though you had selected these faces and used the \"Planar Project\" action"), textureGroup, (int) width);

			if(tempInt != textureGroup)
			{
				SetTextureGroup(selection, textureGroup);

				foreach(var kvp in editor.selectedFacesInEditZone)
					kvp.Key.RefreshUV(kvp.Value);

				SceneView.RepaintAll();

				m_AutoUVSettingsDiff["textureGroup"] = false;
			}

			if(GUILayout.Button(new GUIContent("Group Selected Faces", "This sets all selected faces to share a texture group.  What that means is that the UVs on these faces will all be projected as though they are a single plane.  Ideal candidates for texture groups are floors with multiple faces, walls with edge loops, flat surfaces, etc.")))
			{
				for(int i = 0; i < selection.Length; i++)
					TextureGroupSelectedFaces(selection[i]);

				ProBuilderEditor.Refresh();
			}

			if(GUILayout.Button(new GUIContent("Break Selected Groups", "This resets all the selected face Texture Groups.")))
			{
				SetTextureGroup(selection, -1);

				foreach(var kvp in editor.selectedFacesInEditZone)
				{
					kvp.Key.ToMesh();
					kvp.Key.Refresh();
					kvp.Key.Optimize();
				}

				SceneView.RepaintAll();

				m_AutoUVSettingsDiff["textureGroup"] = false;

				ProBuilderEditor.Refresh();
			}

			/* Select all in current texture group */
			if(GUILayout.Button(new GUIContent("Select Texture Group", "Selects all faces contained in this texture group.")))
			{
				for(int i = 0; i < selection.Length; i++)
					selection[i].SetSelectedFaces( System.Array.FindAll(selection[i].facesInternal, x => x.textureGroup == textureGroup) );

				ProBuilderEditor.Refresh();
			}

			if(GUILayout.Button(new GUIContent("Reset UVs", "Reset UV projection parameters.")))
			{
				UndoUtility.RecordSelection(selection, "Reset UVs");

				for(int i = 0; i < selection.Length; i++)
				{
					foreach(Face face in selection[i].GetSelectedFaces())
						face.uv = new AutoUnwrapSettings();
				}

				ProBuilderEditor.Refresh();
			}


			UnityEngine.GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
			UI.EditorGUIUtility.DrawSeparator(1);
			UnityEngine.GUI.backgroundColor = Color.white;

			/**
			 * Clean up
			 */
			GUILayout.EndScrollView();
			EditorGUI.showMixedValue = false;

			return EditorGUI.EndChangeCheck();
		}

		static void UpdateDiffDictionary(ProBuilderMesh[] selection)
		{
			m_AutoUVSettingsInSelection.Clear();

			if(selection == null || selection.Length < 1)
				return;

			m_AutoUVSettingsInSelection = selection.SelectMany(x => x.GetSelectedFaces()).Where(x => !x.manualUV).Select(x => x.uv).ToList();

			// Clear values for each iteration
			foreach(string key in m_AutoUVSettingsDiff.Keys.ToList())
				m_AutoUVSettingsDiff[key] = false;

			if(m_AutoUVSettingsInSelection.Count < 1) return;

			m_AutoUVSettings = new AutoUnwrapSettings(m_AutoUVSettingsInSelection[0]);

			foreach(AutoUnwrapSettings u in m_AutoUVSettingsInSelection)
			{
				// if(u.projectionAxis != m_AutoUVSettings.projectionAxis)
				// 	m_AutoUVSettingsDiff["projectionAxis"] = true;
				if(u.useWorldSpace != m_AutoUVSettings.useWorldSpace)
					m_AutoUVSettingsDiff["useWorldSpace"] = true;
				if(u.flipU != m_AutoUVSettings.flipU)
					m_AutoUVSettingsDiff["flipU"] = true;
				if(u.flipV != m_AutoUVSettings.flipV)
					m_AutoUVSettingsDiff["flipV"] = true;
				if(u.swapUV != m_AutoUVSettings.swapUV)
					m_AutoUVSettingsDiff["swapUV"] = true;
				if(u.fill != m_AutoUVSettings.fill)
					m_AutoUVSettingsDiff["fill"] = true;
				if(!Math.Approx(u.scale.x, m_AutoUVSettings.scale.x))
					m_AutoUVSettingsDiff["scalex"] = true;
				if(!Math.Approx(u.scale.y, m_AutoUVSettings.scale.y))
					m_AutoUVSettingsDiff["scaley"] = true;
				if(!Math.Approx(u.offset.x, m_AutoUVSettings.offset.x))
					m_AutoUVSettingsDiff["offsetx"] = true;
				if(!Math.Approx(u.offset.y, m_AutoUVSettings.offset.y))
					m_AutoUVSettingsDiff["offsety"] = true;
				if(!Math.Approx(u.rotation, m_AutoUVSettings.rotation))
					m_AutoUVSettingsDiff["rotation"] = true;
				if(u.anchor != m_AutoUVSettings.anchor)
					m_AutoUVSettingsDiff["anchor"] = true;
			}

			foreach(ProBuilderMesh pb in selection)
			{
				if(m_AutoUVSettingsDiff["manualUV"] && m_AutoUVSettingsDiff["textureGroup"])
					break;

				Face[] selFaces = pb.GetSelectedFaces();

				if(!m_AutoUVSettingsDiff["manualUV"])
					m_AutoUVSettingsDiff["manualUV"] = System.Array.Exists(selFaces, x => x.manualUV);

				List<int> texGroups = selFaces.Select(x => x.textureGroup).Distinct().ToList();
				textureGroup = texGroups.FirstOrDefault(x => x > -1);

				if(!m_AutoUVSettingsDiff["textureGroup"])
					m_AutoUVSettingsDiff["textureGroup"] = texGroups.Count() > 1;
			}
		}
#endregion

#region MODIFY SINGLE PROPERTIES

		private static void SetFlipU(bool flipU, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Flip U");
			for(int i = 0; i < sel.Length; i++)
			{
				foreach(Face q in sel[i].GetSelectedFaces()) {
					q.uv.flipU = flipU;
				}
			}
		}

		private static void SetFlipV(bool flipV, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Flip V");
			for(int i = 0; i < sel.Length; i++) {
				foreach(Face q in sel[i].GetSelectedFaces()) {
					q.uv.flipV = flipV;
				}
			}
		}

		private static void SetSwapUV(bool swapUV, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Swap U, V");
			for(int i = 0; i < sel.Length; i++) {
				foreach(Face q in sel[i].GetSelectedFaces()) {
					q.uv.swapUV = swapUV;
				}
			}
		}

		private static void SetUseWorldSpace(bool useWorldSpace, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Use World Space UVs");
			for(int i = 0; i < sel.Length; i++) {
				foreach(Face q in sel[i].GetSelectedFaces()) {
					q.uv.useWorldSpace = useWorldSpace;
				}
			}
		}

		private static void SetFill(AutoUnwrapSettings.Fill fill, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Fill UVs");
			for(int i = 0; i < sel.Length; i++)
			{
				foreach(Face q in sel[i].GetSelectedFaces()) {
					q.uv.fill = fill;
				}
			}
		}

		private static void SetAnchor(AutoUnwrapSettings.Anchor anchor, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Set UV Anchor");

			for(int i = 0; i < sel.Length; i++)
			{
				foreach(Face q in sel[i].GetSelectedFaces())
					q.uv.anchor = anchor;
			}
		}

		private static void SetOffset(Vector2 offset, Axis2D axis, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Offset UVs");

			for(int i = 0; i < sel.Length; i++)
			{
				foreach(Face q in sel[i].GetSelectedFaces()) {
					switch(axis)
					{
						case Axis2D.XY:
							q.uv.offset = offset;
							break;
						case Axis2D.X:
							q.uv.offset = new Vector2(offset.x, q.uv.offset.y);
							break;
						case Axis2D.Y:
							q.uv.offset = new Vector2(q.uv.offset.x, offset.y);
							break;
					}
				}
			}
		}

		private static void SetRotation(float rot, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Rotate UVs");

			for(int i = 0; i < sel.Length; i++)
			{
				foreach(Face q in sel[i].GetSelectedFaces()) {
					q.uv.rotation = rot;
				}
			}
		}

		private static void SetScale(Vector2 scale, Axis2D axis, ProBuilderMesh[] sel)
		{
			UndoUtility.RecordSelection(sel, "Scale UVs");

			for(int i = 0; i < sel.Length; i++)
			{
				foreach(Face q in sel[i].GetSelectedFaces()) {
					switch(axis)
					{
						case Axis2D.XY:
							q.uv.scale = scale;
							break;
						case Axis2D.X:
							q.uv.scale = new Vector2(scale.x, q.uv.scale.y);
							break;
						case Axis2D.Y:
							q.uv.scale = new Vector2(q.uv.scale.x, scale.y);
							break;
					}
				}
			}
		}
#endregion

#region TEXTURE GROUPS

		private static void SetTextureGroup(ProBuilderMesh[] selection, int tex)
		{
			UndoUtility.RecordSelection(selection, "Set Texture Group " + textureGroup);

			foreach(ProBuilderMesh pb in selection)
			{
				if(pb.selectedFaceCount < 1)
					continue;

				Face[] faces = pb.GetSelectedFaces();
				AutoUnwrapSettings cuv = faces[0].uv;

				foreach(Face f in faces)
				{
					f.textureGroup = tex;
					f.uv = new AutoUnwrapSettings(cuv);
				}
			}

		}

		private static void TextureGroupSelectedFaces(ProBuilderMesh pb)//, pb_Face face)
		{
			if(pb.selectedFaceCount < 1) return;

			Face[] faces = pb.GetSelectedFaces();

			AutoUnwrapSettings cont_uv = faces[0].uv;

			int texGroup = pb.GetUnusedTextureGroup();

			UndoUtility.RecordSelection(pb, "Create Texture Group" + textureGroup);

			foreach(Face f in faces)
			{
				f.uv = new AutoUnwrapSettings(cont_uv);
				f.textureGroup = texGroup;
			}
		}
#endregion
	}
}
