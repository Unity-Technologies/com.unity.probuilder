#pragma warning disable 0414

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.Interface;
using ProBuilder2.MeshOperations;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.EditorCommon
{
	public class pb_AutoUV_Editor
	{

	#if !PROTOTYPE

	#region MEMBERS

		public const string ROTATION_CONTROL_NAME = "rotation";
		static pb_Editor editor { get { return pb_Editor.instance; } }

		static pb_UV uv_gui = new pb_UV();		// store GUI changes here, so we may selectively apply them later
		static int textureGroup = -1;

		static List<pb_UV> uv_selection = new List<pb_UV>();
		static Dictionary<string, bool> uv_diff = new Dictionary<string, bool>() {
			{"projectionAxis", false},
			{"useWorldSpace", false},
			{"flipU", false},
			{"flipV", false},
			{"swapUV", false},
			{"fill", false},
			{"scalex", false},
			{"scaley", false},
			{"offsetx", false},
			{"offsety", false},
			{"rotation", false},
			{"justify", false},
			{"manualUV", false},
			{"textureGroup", false}
		};

		public enum pb_Axis2d {
			XY,
			X,
			Y
		}
	#endregion

	#region ONGUI

		static Vector2 scrollPos;

		/**
		 * Returns true on GUI change detected.
		 */
		public static bool OnGUI(pb_Object[] selection, int maxWidth)
		{	
			int width = maxWidth - 36;	// scrollbar is 36px

			UpdateDiffDictionary(selection);

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			int tempInt = -1;
			float tempFloat = 0f;
			Vector2 tempVec2 = Vector2.zero;
			bool tempBool = false;

			EditorGUI.BeginChangeCheck();

			/**
			 * Set Tile mode
			 */
			GUILayout.Label("Tiling", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();

					tempInt = (int)uv_gui.fill;
					EditorGUI.showMixedValue = uv_diff["fill"];
					GUILayout.Label("Fill Mode", GUILayout.MaxWidth(80), GUILayout.MinWidth(80));
					uv_gui.fill = (pb_UV.Fill)EditorGUILayout.EnumPopup(uv_gui.fill);
					if(tempInt != (int)uv_gui.fill) SetFill(uv_gui.fill, selection);

			GUILayout.EndHorizontal();	

			UnityEngine.GUI.backgroundColor = pb_Constant.ProBuilderLightGray;
			pb_GUI_Utility.DrawSeparator(1);
			UnityEngine.GUI.backgroundColor = Color.white;

			GUILayout.Label("Transform", EditorStyles.boldLabel);

			/**
			 * Offset
			 */
			EditorGUI.showMixedValue = uv_diff["offsetx"] || uv_diff["offsety"];
			tempVec2 = uv_gui.offset;
			UnityEngine.GUI.SetNextControlName("offset");
			uv_gui.offset = EditorGUILayout.Vector2Field("Offset", uv_gui.offset, GUILayout.MaxWidth(width));
			if(tempVec2.x != uv_gui.offset.x) { SetOffset(uv_gui.offset, pb_Axis2d.X, selection); }
			if(tempVec2.y != uv_gui.offset.y) { SetOffset(uv_gui.offset, pb_Axis2d.Y, selection); }

			/**
			 * Rotation
			 */
			tempFloat = uv_gui.rotation;
			EditorGUI.showMixedValue = uv_diff["rotation"];
			GUILayout.Label(new GUIContent("Rotation", "Rotation around the center of face UV bounds."), GUILayout.MaxWidth(width-64));

			UnityEngine.GUI.SetNextControlName(ROTATION_CONTROL_NAME);
			uv_gui.rotation = EditorGUILayout.Slider(uv_gui.rotation, 0f, 360f, GUILayout.MaxWidth(width));
			
			/**
			 * Scale
			 */
			EditorGUI.showMixedValue = uv_diff["scalex"] || uv_diff["scaley"];
			tempVec2 = uv_gui.scale;
			UnityEngine.GUI.SetNextControlName("scale");
			
			uv_gui.scale = EditorGUILayout.Vector2Field("Scale", uv_gui.scale, GUILayout.MaxWidth(width));
			
			// Draw tiling shortcuts
			GUILayout.BeginHorizontal();

			if( GUILayout.Button(".5", EditorStyles.miniButtonLeft) )	uv_gui.scale = Vector2.one * 2f;
			if( GUILayout.Button("1", EditorStyles.miniButtonMid) )		uv_gui.scale = Vector2.one;
			if( GUILayout.Button("2", EditorStyles.miniButtonMid) )		uv_gui.scale = Vector2.one * .5f;
			if( GUILayout.Button("4", EditorStyles.miniButtonMid) )		uv_gui.scale = Vector2.one * .25f;
			if( GUILayout.Button("8", EditorStyles.miniButtonMid) )		uv_gui.scale = Vector2.one * .125f;
			if( GUILayout.Button("16", EditorStyles.miniButtonRight) ) 	uv_gui.scale = Vector2.one * .0625f;

			GUILayout.EndHorizontal();

			if(tempVec2.x != uv_gui.scale.x) { SetScale(uv_gui.scale, pb_Axis2d.X, selection); }
			if(tempVec2.y != uv_gui.scale.y) { SetScale(uv_gui.scale, pb_Axis2d.Y, selection); }

			if(tempFloat != uv_gui.rotation) { SetRotation(uv_gui.rotation, selection); }

			GUILayout.Space(4);

			UnityEngine.GUI.backgroundColor = pb_Constant.ProBuilderLightGray;
			pb_GUI_Utility.DrawSeparator(1);
			UnityEngine.GUI.backgroundColor = Color.white;

			/**
			 * Special
			 */
			GUILayout.Label("Special", EditorStyles.boldLabel);

			// Flip U
			tempBool = uv_gui.flipU;
			EditorGUI.showMixedValue = uv_diff["flipU"];
			uv_gui.flipU = EditorGUILayout.Toggle("Flip U", uv_gui.flipU);
			if(tempBool != uv_gui.flipU) SetFlipU(uv_gui.flipU, selection); 

			// Flip V
			tempBool = uv_gui.flipV;
			EditorGUI.showMixedValue = uv_diff["flipV"];
			uv_gui.flipV = EditorGUILayout.Toggle("Flip V", uv_gui.flipV);
			if(tempBool != uv_gui.flipV) SetFlipV(uv_gui.flipV, selection);  
				
			tempBool = uv_gui.swapUV;
			EditorGUI.showMixedValue = uv_diff["swapUV"];
			uv_gui.swapUV = EditorGUILayout.Toggle("Swap U/V", uv_gui.swapUV);
			if(tempBool != uv_gui.swapUV) SetSwapUV(uv_gui.swapUV, selection);  

			UnityEngine.GUI.backgroundColor = pb_Constant.ProBuilderLightGray;
			pb_GUI_Utility.DrawSeparator(1);
			UnityEngine.GUI.backgroundColor = Color.white;

			tempBool = uv_gui.useWorldSpace;
			EditorGUI.showMixedValue = uv_diff["useWorldSpace"];
			uv_gui.useWorldSpace = EditorGUILayout.Toggle("World Space", uv_gui.useWorldSpace);
			if(uv_gui.useWorldSpace != tempBool) SetUseWorldSpace(uv_gui.useWorldSpace, selection);  


			/**
			 * Texture Groups
			 */
			GUILayout.Label("Texture Groups", EditorStyles.boldLabel);

			tempInt = textureGroup;
			EditorGUI.showMixedValue = uv_diff["textureGroup"];

			UnityEngine.GUI.SetNextControlName("textureGroup");
			textureGroup = pb_GUI_Utility.IntFieldConstrained( new GUIContent("Texture Group", "Faces in a texture group will be UV mapped as a group, just as though you had selected these faces and used the \"Planar Project\" action"), textureGroup, width);

			if(tempInt != textureGroup)
			{
				SetTextureGroup(selection, textureGroup);
				
				for(int i = 0; i < editor.SelectedFacesInEditZone.Length; i++)
					selection[i].RefreshUV(editor.SelectedFacesInEditZone[i]);
				
				SceneView.RepaintAll();

				uv_diff["textureGroup"] = false;
			}

			if(GUILayout.Button(new GUIContent("Group Selected Faces", "This sets all selected faces to share a texture group.  What that means is that the UVs on these faces will all be projected as though they are a single plane.  Ideal candidates for texture groups are floors with multiple faces, walls with edge loops, flat surfaces, etc."), GUILayout.MaxWidth(width)))
			{
				for(int i = 0; i < selection.Length; i++)
				{
					TextureGroupSelectedFaces(selection[i]);
				}

				pb_Editor.instance.UpdateSelection();
			}

			/* Select all in current texture group */
			if(GUILayout.Button(new GUIContent("Select Texture Group", "Selects all faces contained in this texture group."), GUILayout.MaxWidth(width)))
			{
				for(int i = 0; i < selection.Length; i++)
					selection[i].SetSelectedFaces( System.Array.FindAll(selection[i].faces, x => x.textureGroup == textureGroup) );

				pb_Editor.instance.UpdateSelection();
			}

			UnityEngine.GUI.backgroundColor = pb_Constant.ProBuilderLightGray;
			pb_GUI_Utility.DrawSeparator(1);
			UnityEngine.GUI.backgroundColor = Color.white;

			/**
			 * Clean up
			 */
			GUILayout.EndScrollView();
			EditorGUI.showMixedValue = false;

			return EditorGUI.EndChangeCheck();
		}

		/**
		 * Sets the pb_UV list and diff tables.
		 */
		static void UpdateDiffDictionary(pb_Object[] selection)
		{
			uv_selection.Clear();
			
			if(selection == null || selection.Length < 1)
				return;

			uv_selection = selection.SelectMany(x => x.SelectedFaces).Where(x => !x.manualUV).Select(x => x.uv).ToList();

			// Clear values for each iteration
			foreach(string key in uv_diff.Keys.ToList())
				uv_diff[key] = false;

			if(uv_selection.Count < 1) return;
			
			uv_gui = new pb_UV(uv_selection[0]);

			foreach(pb_UV u in uv_selection)
			{
				// if(u.projectionAxis != uv_gui.projectionAxis)
				// 	uv_diff["projectionAxis"] = true;
				if(u.useWorldSpace != uv_gui.useWorldSpace)
					uv_diff["useWorldSpace"] = true;
				if(u.flipU != uv_gui.flipU)
					uv_diff["flipU"] = true;
				if(u.flipV != uv_gui.flipV)
					uv_diff["flipV"] = true;
				if(u.swapUV != uv_gui.swapUV)
					uv_diff["swapUV"] = true;
				if(u.fill != uv_gui.fill)
					uv_diff["fill"] = true;
				if(u.scale.x != uv_gui.scale.x)
					uv_diff["scalex"] = true;
				if(u.scale.y != uv_gui.scale.y)
					uv_diff["scaley"] = true;
				if(u.offset.x != uv_gui.offset.x)
					uv_diff["offsetx"] = true;
				if(u.offset.y != uv_gui.offset.y)
					uv_diff["offsety"] = true;
				if(u.rotation != uv_gui.rotation)
					uv_diff["rotation"] = true;
				if(u.justify != uv_gui.justify)
					uv_diff["justify"] = true;
			}

			foreach(pb_Object pb in selection)
			{
				if(uv_diff["manualUV"] && uv_diff["textureGroup"])
					break;
					
				pb_Face[] selFaces = pb.SelectedFaces;

				if(!uv_diff["manualUV"])
					uv_diff["manualUV"] = System.Array.Exists(selFaces, x => x.manualUV);

				List<int> texGroups = selFaces.Select(x => x.textureGroup).Distinct().ToList();
				textureGroup = texGroups.FirstOrDefault(x => x > -1);

				if(!uv_diff["textureGroup"])
					uv_diff["textureGroup"] = texGroups.Count() > 1;
			}
		}
	#endregion

	#region MODIFY SINGLE PROPERTIES

		private static void SetFlipU(bool flipU, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Flip U");
			for(int i = 0; i < sel.Length; i++)
			{
				foreach(pb_Face q in sel[i].SelectedFaces) {
					q.uv.flipU = flipU;
				}
			}
		}

		private static void SetFlipV(bool flipV, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Flip V");
			for(int i = 0; i < sel.Length; i++) {
				foreach(pb_Face q in sel[i].SelectedFaces) {
					q.uv.flipV = flipV;
				}
			}
		}

		private static void SetSwapUV(bool swapUV, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Swap U, V");
			for(int i = 0; i < sel.Length; i++) {
				foreach(pb_Face q in sel[i].SelectedFaces) {
					q.uv.swapUV = swapUV;
				}
			}
		}

		private static void SetUseWorldSpace(bool useWorldSpace, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Use World Space UVs");
			for(int i = 0; i < sel.Length; i++) {
				foreach(pb_Face q in sel[i].SelectedFaces) {
					q.uv.useWorldSpace = useWorldSpace;
				}
			}
		}

		private static void SetFill(pb_UV.Fill fill, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Fill UVs");
			for(int i = 0; i < sel.Length; i++)
			{
				foreach(pb_Face q in sel[i].SelectedFaces) {
					q.uv.fill = fill;
				}
			}
		}

		private static void SetJustify(pb_UV.Justify justify, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Justify UVs");
			for(int i = 0; i < sel.Length; i++)
			{
				foreach(pb_Face q in sel[i].SelectedFaces) {
					q.uv.justify = justify;
				}
			}
		}

		private static void SetOffset(Vector2 offset, pb_Axis2d axis, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Offset UVs");

			for(int i = 0; i < sel.Length; i++)
			{
				foreach(pb_Face q in sel[i].SelectedFaces) {
					switch(axis)
					{
						case pb_Axis2d.XY:
							q.uv.offset = offset;
							break;
						case pb_Axis2d.X:
							q.uv.offset.x = offset.x;
							break;
						case pb_Axis2d.Y:
							q.uv.offset.y = offset.y;
							break;
					}
				}
			}		
		}

		private static void SetRotation(float rot, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Rotate UVs");
			for(int i = 0; i < sel.Length; i++)
			{
				foreach(pb_Face q in sel[i].SelectedFaces) {
					q.uv.rotation = rot;
				}
			}		
		}	

		private static void SetScale(Vector2 scale, pb_Axis2d axis, pb_Object[] sel)
		{
			pbUndo.RecordObjects(sel, "Scale UVs");
			for(int i = 0; i < sel.Length; i++)
			{
				foreach(pb_Face q in sel[i].SelectedFaces) {
					switch(axis)
					{
						case pb_Axis2d.XY:
							q.uv.scale = scale;
							break;
						case pb_Axis2d.X:
							q.uv.scale.x = scale.x;
							break;
						case pb_Axis2d.Y:
							q.uv.scale.y = scale.y;
							break;
					}
				}
			}		
		}
	#endregion

	#region TEXTURE GROUPS
			
		private static void SetTextureGroup(pb_Object[] selection, int tex)
		{
			pbUndo.RecordObjects(selection, "Set Texture Group " + textureGroup);

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedFaceIndices.Length < 1) continue;

				pb_Face[] faces = pb.SelectedFaces;
				pb_UV cuv = faces[0].uv;

				foreach(pb_Face f in faces)
				{
					f.textureGroup = tex;
					f.SetUV( new pb_UV(cuv) );
				}
			}

		}

		private static void TextureGroupSelectedFaces(pb_Object pb)//, pb_Face face)
		{
			if(pb.SelectedFaceIndices.Length < 1) return;

			pb_Face[] faces = pb.SelectedFaces;

			pb_UV cont_uv = faces[0].uv;

			int texGroup = pb.UnusedTextureGroup();

			pbUndo.RecordObject(pb, "Create Texture Group" + textureGroup);

			foreach(pb_Face f in faces)
			{
				f.SetUV( new pb_UV(cont_uv) );
				f.textureGroup = texGroup;
			}
		}
	#endregion
	#endif
	}
}