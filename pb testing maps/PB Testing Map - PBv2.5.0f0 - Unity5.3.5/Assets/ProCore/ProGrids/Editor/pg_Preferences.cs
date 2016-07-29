using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProGrids
{
	public class pg_Preferences {

		static Color _gridColorX;
		static Color _gridColorY;
		static Color _gridColorZ;
		static float _alphaBump;
		static bool _scaleSnapEnabled;
		static int _snapMethod;
		static float _BracketIncreaseValue;
		static SnapUnit _GridUnits;

		/** Defaults **/
		public static Color GRID_COLOR_X = new Color(.9f, .46f, .46f, .08f);
		public static Color GRID_COLOR_Y = new Color(.46f, .9f, .46f, .08f);
		public static Color GRID_COLOR_Z = new Color(.46f, .46f, .9f, .08f);
		public static float ALPHA_BUMP = .1f;
		public static bool USE_AXIS_CONSTRAINTS = false;
		public static bool SHOW_GRID = true;

		static string[] SnapMethod = new string[]
		{
			"Snap on Selected Axis",
			"Snap on All Axes"
		};

		static int[] SnapVals = new int[] { 1, 0 };

		static bool prefsLoaded = false;

		/** GUI ITEMS **/
		static Rect resetRect = new Rect(0f, 0f, 0f, 0f);

		[PreferenceItem ("ProGrids")]
		public static void PreferencesGUI ()
		{
			if(!prefsLoaded)
			{
				prefsLoaded = LoadPreferences();
				OnWindowResize();
			}

			// EditorGUILayout.HelpBox("Changes will take effect on the next ProGrids open.", MessageType.Info);

			GUILayout.Label("Grid Colors per Axis", EditorStyles.boldLabel);
			_gridColorX = EditorGUILayout.ColorField("X Axis", _gridColorX);
			_gridColorY = EditorGUILayout.ColorField("Y Axis", _gridColorY);
			_gridColorZ = EditorGUILayout.ColorField("Z Axis", _gridColorZ);

			_alphaBump = EditorGUILayout.Slider(new GUIContent("Tenth Line Alpha", "Every 10th line will have it's alpha value bumped by this amount."), _alphaBump, 0f, 1f);

			// not used
			// _BracketIncreaseValue = EditorGUILayout.FloatField(new GUIContent("Grid Increment Value", "Affects the amount by which the bracket keys will increment or decrement that snap value."), _BracketIncreaseValue);

			_GridUnits = (SnapUnit)EditorGUILayout.EnumPopup("Grid Units", _GridUnits);

			_scaleSnapEnabled = EditorGUILayout.Toggle("Snap On Scale", _scaleSnapEnabled);

			// GUILayout.BeginHorizontal();
			// EditorGUILayout.PrefixLabel(new GUIContent("Axis Constraints", "If toggled, objects will be automatically grid aligned on all axes when moving."));

			_snapMethod = EditorGUILayout.IntPopup("Snap Method", _snapMethod, SnapMethod, SnapVals);

			// GUILayout.EndHorizontal();
			
			if(GUI.Button(resetRect, "Reset"))
			{
				if(EditorUtility.DisplayDialog("Delete ProGrids editor preferences?", "Are you sure you want to delete these?, this action cannot be undone.", "Yes", "No"))
					ResetPrefs();
			}

			if(GUI.changed)
				SetPreferences();
		}

		public static bool LoadPreferences()
		{
			_scaleSnapEnabled = EditorPrefs.HasKey("scaleSnapEnabled") ? EditorPrefs.GetBool("scaleSnapEnabled") : false;
			_gridColorX = (EditorPrefs.HasKey("gridColorX")) ? pg_Util.ColorWithString(EditorPrefs.GetString("gridColorX")) : GRID_COLOR_X;
			_gridColorY = (EditorPrefs.HasKey("gridColorY")) ? pg_Util.ColorWithString(EditorPrefs.GetString("gridColorY")) : GRID_COLOR_Y;
			_gridColorZ = (EditorPrefs.HasKey("gridColorZ")) ? pg_Util.ColorWithString(EditorPrefs.GetString("gridColorZ")) : GRID_COLOR_Z;
			_alphaBump = (EditorPrefs.HasKey("pg_alphaBump")) ? EditorPrefs.GetFloat("pg_alphaBump") : ALPHA_BUMP;
			_snapMethod = System.Convert.ToInt32( 
				(EditorPrefs.HasKey(pg_Constant.UseAxisConstraints)) ? EditorPrefs.GetBool(pg_Constant.UseAxisConstraints) : USE_AXIS_CONSTRAINTS
				);
			_BracketIncreaseValue = EditorPrefs.HasKey(pg_Constant.BracketIncreaseValue) ? EditorPrefs.GetFloat(pg_Constant.BracketIncreaseValue) : .25f;
			_GridUnits = (SnapUnit)(EditorPrefs.HasKey(pg_Constant.GridUnit) ? EditorPrefs.GetInt(pg_Constant.GridUnit) : 0);

			return true;
		}

		public static void SetPreferences()
		{
			EditorPrefs.SetBool("scaleSnapEnabled", _scaleSnapEnabled);
			EditorPrefs.SetString("gridColorX", _gridColorX.ToString("f3"));
			EditorPrefs.SetString("gridColorY", _gridColorY.ToString("f3"));
			EditorPrefs.SetString("gridColorZ", _gridColorZ.ToString("f3"));
			EditorPrefs.SetFloat("pg_alphaBump", _alphaBump);
			EditorPrefs.SetBool(pg_Constant.UseAxisConstraints, System.Convert.ToBoolean(_snapMethod));
			EditorPrefs.SetFloat(pg_Constant.BracketIncreaseValue, _BracketIncreaseValue);
			EditorPrefs.SetInt(pg_Constant.GridUnit, (int)_GridUnits);

			if(pg_Editor.instance != null)
			{
				pg_Editor.instance.LoadPreferences();
			}
		}

		public static void ResetPrefs()
		{
			EditorPrefs.DeleteKey("scaleSnapEnabled");
			EditorPrefs.DeleteKey("gridColorX");
			EditorPrefs.DeleteKey("gridColorY");
			EditorPrefs.DeleteKey("gridColorZ");
			EditorPrefs.DeleteKey("pg_alphaBump");
			EditorPrefs.DeleteKey(pg_Constant.UseAxisConstraints);
			EditorPrefs.DeleteKey(pg_Constant.BracketIncreaseValue);
			EditorPrefs.DeleteKey(pg_Constant.GridUnit);
			EditorPrefs.DeleteKey("showgrid");

			LoadPreferences();
		}

		public static void OnWindowResize()
		{
			int pad = 10, buttonWidth = 100, buttonHeight = 20;
			resetRect = new Rect(Screen.width-pad-buttonWidth, Screen.height-pad-buttonHeight, buttonWidth, buttonHeight);
		}
	}
}
