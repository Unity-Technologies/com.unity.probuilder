using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.EditorCommon
{

	/**
	 * Interface for lightmap channel generation paramters.
	 */
	public class pb_Lightmap_Editor : EditorWindow 
	{
		public pb_Editor editor;
		Dictionary<string, bool> diff = new Dictionary<string, bool>() {
			{"angleError", false},
			{"areaError", false},
			{"hardAngle", false},
			{"packMargin", false}
		};

		float sampleAngleError = 8f, sampleAreaError = 15f, sampleHardAngle = 88f, samplePackMargin = 20f;

		public static pb_Lightmap_Editor Init(pb_Editor _editor)
		{
			pb_Lightmap_Editor win = EditorWindow.GetWindow<pb_Lightmap_Editor>(true, "UV2 Param Gen. Settings", true);
			win.Show();
			win.editor = _editor;
			win.OnSelectionUpdate(win.editor.selection);

			return win;
		}

		public void OnEnable()
		{
			pb_Editor.OnSelectionUpdate += new pb_Editor.OnSelectionUpdateEventHandler(OnSelectionUpdate);
			this.autoRepaintOnSceneChange = true;
			SetWindowSize(new Vector2(300f, 240f));
		}

		public void SetWindowSize(Vector2 size)
		{
			this.minSize = size;
			this.maxSize = size;
		}

		public bool showAdvancedPanel = false;
		public void OnGUI()
		{
			EditorGUILayout.HelpBox("Lightmap UVs are automatically generated.  This dialog controls how the UV2 channel is constructed.  Default values will work for most geometry, though if you are having difficulties with light blemishes this editor offers fine-grained control of the UV2 generation parameters.", MessageType.Warning);

			EditorGUI.showMixedValue = diff["angleError"];
			sampleAngleError = EditorGUILayout.Slider("Angle Error", sampleAngleError, 1f, 75f);
			if(GUI.changed) { SetAngleError(sampleAngleError);  UpdateDiffDictionary(); RefreshUV2();  GUI.changed = false; }

			EditorGUI.showMixedValue = diff["areaError"];
			sampleAreaError = EditorGUILayout.Slider("Area Error", sampleAreaError, 1f, 75f);
			if(GUI.changed) { SetAreaError(sampleAreaError);  UpdateDiffDictionary(); RefreshUV2();  GUI.changed = false; }

			EditorGUI.showMixedValue = diff["hardAngle"];
			sampleHardAngle = EditorGUILayout.Slider("Hard Angle", sampleHardAngle, 0f, 180f);
			if(GUI.changed) { SetHardAngle(sampleHardAngle);  UpdateDiffDictionary(); RefreshUV2();  GUI.changed = false; }

			EditorGUI.showMixedValue = diff["packMargin"];
			samplePackMargin = EditorGUILayout.Slider("Pack Margin", samplePackMargin, 1f, 64f);
			if(GUI.changed) { SetPackMargin(samplePackMargin);  UpdateDiffDictionary(); RefreshUV2();  GUI.changed = false; }

			EditorGUI.showMixedValue = false;

			if(GUILayout.Button("Reset Values to Default")) {
				foreach(pb_Object pb in editor.selection)
					ResetObjectToDefaultValues(pb);

				RefreshUV2();
			}

			showAdvancedPanel = EditorGUILayout.Foldout(showAdvancedPanel, "Advanced");

			if(showAdvancedPanel)
			{
				SetWindowSize(new Vector2(300f, 290f));
				if(GUILayout.Button(new GUIContent("Refresh All", "UV2 is now automatically generated, but if you are upgrading from a previous version this may be necessary.  Caution: Will be slooow in a big scene.")))
				{
					pb_Object[] all = (pb_Object[])FindObjectsOfType(typeof(pb_Object));
					foreach(pb_Object pb in all)
						pb.GenerateUV2();
				}

				if(GUILayout.Button(new GUIContent("Apply Settings To All", "Caution: Will be slow in a big scene.")))
				{
					pb_Object[] all = (pb_Object[])FindObjectsOfType(typeof(pb_Object));
					foreach(pb_Object pb in all) {
						pb.angleError = sampleAngleError;
						pb.areaError = sampleAreaError;
						pb.hardAngle = sampleHardAngle;
						pb.packMargin = samplePackMargin;
						pb.GenerateUV2();
						EditorUtility.SetDirty(pb);
					}
				}

				if(GUILayout.Button(new GUIContent("Make Current Settings Default", "Sets the default ProBuilder UnwrapParams to the currently set values.")))
				{
					EditorPrefs.SetFloat("pbAngleError", sampleAngleError);
					EditorPrefs.SetFloat("pbAreaError", sampleAreaError);
					EditorPrefs.SetFloat("pbHardAngle", sampleHardAngle);
					EditorPrefs.SetFloat("pbPackMargin", samplePackMargin);
				}

				if(GUILayout.Button(new GUIContent("Reset Settings to ProBuilder Default", "Sets the default UnwrapParams to the ProBuilder standard.")))
				{
					ResetProBuilderDefaults();
				}
			} 
			else
				SetWindowSize(new Vector2(300f, 210f));

			// if(GUILayout.Button("Generate UV2 Channel"))
			// 	GenerateEditorLightmaps();
		}

		public void OnSelectionUpdate(pb_Object[] newSelection)
		{
			UpdateDiffDictionary();
			Repaint();
		}

		// @ todo - have this use serialized proeprty
		public void UpdateDiffDictionary()
		{
			// Clear values for each iteration
			foreach(string key in diff.Keys.ToList())
				diff[key] = false;

			if(editor.selection.Length < 1)
				return;

			sampleAngleError	= editor.selection[0].angleError;
			sampleAreaError		= editor.selection[0].areaError;
			sampleHardAngle		= editor.selection[0].hardAngle;
			samplePackMargin	= editor.selection[0].packMargin;

			foreach(pb_Object pb in editor.selection)
			{
				if(sampleAngleError != pb.angleError)
					diff["angleError"] = true;
				if(sampleAreaError != pb.areaError)
					diff["areaError"] = true;	
				if(sampleHardAngle != pb.hardAngle)
					diff["angleError"] = true;
				if(samplePackMargin != pb.packMargin)
					diff["packMargin"] = true;	
			}
		}

		void SetAngleError(float val)
		{
			foreach(pb_Object pb in editor.selection)
			{
				pb.angleError = val;
			}
		}

		void SetAreaError(float val)
		{
			foreach(pb_Object pb in editor.selection)
			{
				pb.areaError = val;
			}
		}
		
		void SetHardAngle(float val)
		{
			foreach(pb_Object pb in editor.selection)
			{
				pb.hardAngle = val;
			}
		}
		
		void SetPackMargin(float val)
		{
			foreach(pb_Object pb in editor.selection)
			{
				pb.packMargin = val;
			}
		}

		void RefreshUV2()
		{
			foreach(pb_Object pb in editor.selection)
				pb.GenerateUV2();
		}

		public void ResetObjectToDefaultValues(pb_Object pb)
		{
			// here's something fun that the documentation doesn't mention- these values are not actually what 
			// gets fed to UnwrapParam!  They're human readable, not actual values
			SetAngleError( GetDefaultAngleError() );
			SetAreaError ( GetDefaultAreaError() );
			SetHardAngle ( GetDefaultHardAngle() );
			SetPackMargin( GetDefaultPackMargin() );

			UpdateDiffDictionary();
		}

		public static void SetObjectUnwrapParamsToDefault(pb_Object pb)
		{
			pb.angleError = GetDefaultAngleError();
			pb.areaError  = GetDefaultAreaError();
			pb.hardAngle  = GetDefaultHardAngle();
			pb.packMargin = GetDefaultPackMargin();
		}

		public void ResetProBuilderDefaults()
		{
			EditorPrefs.SetFloat("pbAngleError", 8f);
			EditorPrefs.SetFloat("pbAreaError", 15f);
			EditorPrefs.SetFloat("pbHardAngle", 88f);
			EditorPrefs.SetFloat("pbPackMargin", 20f);// this is actual default - 3.90625f);

			foreach(pb_Object pb in editor.selection) {
				ResetObjectToDefaultValues(pb);
				pb.GenerateUV2();
			}
		}

		public static float GetDefaultAngleError()
		{
			if(EditorPrefs.HasKey("pbAngleError"))
				return EditorPrefs.GetFloat("pbAngleError");
			else
				return 8f;
		}

		public static float GetDefaultAreaError()
		{
			if(EditorPrefs.HasKey("pbAreaError"))
				return EditorPrefs.GetFloat("pbAreaError");
			else
				return 15f;
		}

		public static float GetDefaultHardAngle()
		{		if(EditorPrefs.HasKey("pbHardAngle"))
				return EditorPrefs.GetFloat("pbHardAngle");
			else
				return 88f;
		}

		public static float GetDefaultPackMargin()
		{
			if(EditorPrefs.HasKey("pbPackMargin"))
				return EditorPrefs.GetFloat("pbPackMargin");
			else
				return 20f;
		}
	}
}