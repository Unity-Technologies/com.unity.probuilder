using UnityEditor;
using UnityEngine;
using ProBuilder2.Interface;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Popup window in UV editor with the "Render UV Template" options.
	 */
	public class pb_UV_Render_Options : EditorWindow
	{
		const string PREF_IMAGESIZE = "pb_UVTemplate_imageSize";
		const string PREF_LINECOLOR = "pb_UVTemplate_lineColor";
		const string PREF_BACKGROUNDCOLOR = "pb_UVTemplate_backgroundColor";
		const string PREF_TRANSPARENTBACKGROUND = "pb_UVTemplate_transparentBackground";
		const string PREF_HIDEGRID = "pb_UVTemplate_hideGrid";

		enum ImageSize
		{
			_256 = 256,
			_512 = 512,
			_1024 = 1024,
			_2048 = 2048,
			_4096 = 4096,
		};

		ImageSize imageSize = ImageSize._1024;
		Color lineColor = Color.green;
		Color backgroundColor = Color.black;
		bool transparentBackground = true;
		bool hideGrid = true;

		void OnEnable()
		{
			if( EditorPrefs.HasKey(PREF_IMAGESIZE) )
				imageSize = (ImageSize)EditorPrefs.GetInt(PREF_IMAGESIZE);

			if( EditorPrefs.HasKey(PREF_LINECOLOR) )
				lineColor = pb_Preferences_Internal.GetColor(PREF_LINECOLOR);

			if( EditorPrefs.HasKey(PREF_BACKGROUNDCOLOR) )
				backgroundColor = pb_Preferences_Internal.GetColor(PREF_BACKGROUNDCOLOR);

			if( EditorPrefs.HasKey(PREF_TRANSPARENTBACKGROUND) )
				transparentBackground = EditorPrefs.GetBool(PREF_TRANSPARENTBACKGROUND);

			if( EditorPrefs.HasKey(PREF_HIDEGRID) )
				hideGrid = EditorPrefs.GetBool(PREF_HIDEGRID);
		}
		
		public delegate void ScreenshotFunc(int ImageSize, bool HideGrid, Color LineColor, bool TransparentBackground, Color BackgroundColor);
		public ScreenshotFunc screenFunc;

		void OnGUI()
		{
			GUILayout.Label("Render UVs", EditorStyles.boldLabel);

			pb_GUI_Utility.DrawSeparator(2, pb_Constant.ProBuilderDarkGray);
			GUILayout.Space(2);

			imageSize = (ImageSize)EditorGUILayout.EnumPopup(new GUIContent("Image Size", "The pixel size of the image to be rendered."), imageSize);

			hideGrid = EditorGUILayout.Toggle(new GUIContent("Hide Grid", "Hide or show the grid lines."), hideGrid);

			lineColor = EditorGUILayout.ColorField(new GUIContent("Line Color", "The color of the template lines."), lineColor);

			transparentBackground = EditorGUILayout.Toggle(new GUIContent("Transparent Background", "If true, only the template lines will be rendered, leaving the background fully transparent."), transparentBackground);

			GUI.enabled = !transparentBackground;
			backgroundColor = EditorGUILayout.ColorField(new GUIContent("Background Color", "If `TransparentBackground` is off, this will be the fill color of the image."), backgroundColor);
			GUI.enabled = true;

			if(GUILayout.Button("Save UV Template"))
			{
				EditorPrefs.SetInt(PREF_IMAGESIZE, (int)imageSize);
				EditorPrefs.SetString(PREF_LINECOLOR, lineColor.ToString());
				EditorPrefs.SetString(PREF_BACKGROUNDCOLOR, backgroundColor.ToString());
				EditorPrefs.SetBool(PREF_TRANSPARENTBACKGROUND, transparentBackground);
				EditorPrefs.SetBool(PREF_HIDEGRID, hideGrid);

				if(pb_Editor.instance == null || pb_Editor.instance.selection.Length < 1)	
				{
					Debug.LogWarning("Abandoning UV render because no ProBuilder objects are selected.");
					this.Close();
					return;
				}

				screenFunc((int)imageSize, hideGrid, lineColor, transparentBackground, backgroundColor);
				this.Close();
			}
		}
	}
}