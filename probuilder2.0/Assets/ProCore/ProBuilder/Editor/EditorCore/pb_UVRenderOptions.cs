using ProBuilder.Core;
using ProBuilder.Interface;
using UnityEditor;
using UnityEngine;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Popup window in UV editor with the "Render UV Template" options.
	/// </summary>
	class pb_UVRenderOptions : EditorWindow
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
			if( pb_PreferencesInternal.HasKey(PREF_IMAGESIZE) )
				imageSize = (ImageSize)pb_PreferencesInternal.GetInt(PREF_IMAGESIZE);

			if( pb_PreferencesInternal.HasKey(PREF_LINECOLOR) )
				lineColor = pb_PreferencesInternal.GetColor(PREF_LINECOLOR);

			if( pb_PreferencesInternal.HasKey(PREF_BACKGROUNDCOLOR) )
				backgroundColor = pb_PreferencesInternal.GetColor(PREF_BACKGROUNDCOLOR);

			if( pb_PreferencesInternal.HasKey(PREF_TRANSPARENTBACKGROUND) )
				transparentBackground = pb_PreferencesInternal.GetBool(PREF_TRANSPARENTBACKGROUND);

			if( pb_PreferencesInternal.HasKey(PREF_HIDEGRID) )
				hideGrid = pb_PreferencesInternal.GetBool(PREF_HIDEGRID);
		}

		public delegate void ScreenshotFunc(int ImageSize, bool HideGrid, Color LineColor, bool TransparentBackground, Color BackgroundColor);
		public ScreenshotFunc screenFunc;

		void OnGUI()
		{
			GUILayout.Label("Render UVs", EditorStyles.boldLabel);

			pb_EditorGUIUtility.DrawSeparator(2, pb_Constant.ProBuilderDarkGray);
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
				pb_PreferencesInternal.SetInt(PREF_IMAGESIZE, (int)imageSize);
				pb_PreferencesInternal.SetString(PREF_LINECOLOR, lineColor.ToString());
				pb_PreferencesInternal.SetString(PREF_BACKGROUNDCOLOR, backgroundColor.ToString());
				pb_PreferencesInternal.SetBool(PREF_TRANSPARENTBACKGROUND, transparentBackground);
				pb_PreferencesInternal.SetBool(PREF_HIDEGRID, hideGrid);

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
