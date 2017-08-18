using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.Interface;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Shape creation panel implementation.
	 */
	public class pb_ShapeEditor : EditorWindow
	{
		public static void MenuOpenShapeEditor()
		{
			EditorWindow.GetWindow<pb_ShapeEditor>(
				pb_PreferencesInternal.GetBool(pb_Constant.pbShapeWindowFloating),
				"Shape Editor",
				true).Show();
		}

		static Color PREVIEW_COLOR = new Color(.5f, .9f, 1f, .56f);
		private Material _prevMat;
		public Material previewMat
		{
			get
			{
				if(_prevMat == null)
				{
					_prevMat = new Material(Shader.Find("Diffuse"));
					_prevMat.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");
					_prevMat.SetColor("_Color", PREVIEW_COLOR);
				}
				return _prevMat;
			}
		}

		// toogle for closing the window after shape creation from the prefrences window
		private bool prefClose { get {
			  	return pb_PreferencesInternal.HasKey(pb_Constant.pbCloseShapeWindow) ? pb_PreferencesInternal.GetBool(pb_Constant.pbCloseShapeWindow) : false;
			}
		}

		// Material userMaterial = null;

		void OnEnable()
		{
			// userMaterial = pb_Preferences_Internal.GetMaterial(pb_Constant.pbDefaultMaterial);
		}

		void OnDestroy()
		{
		}

		void OpenContextMenu()
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem (
				new GUIContent("Window/Open as Floating Window", ""),
				pb_PreferencesInternal.GetBool(pb_Constant.pbShapeWindowFloating),
				() => { SetFloating(true); } );
			menu.AddItem (
				new GUIContent("Window/Open as Dockable Window", ""),
				!pb_PreferencesInternal.GetBool(pb_Constant.pbShapeWindowFloating),
				() => { SetFloating(false); } );

			menu.ShowAsContext ();
		}

		void SetFloating(bool floating)
		{
			pb_PreferencesInternal.SetBool(pb_Constant.pbShapeWindowFloating, floating);
			this.Close();
			MenuOpenShapeEditor();
		}

		[MenuItem("GameObject/3D Object/" + pb_Constant.PRODUCT_NAME + " Cube _%k")]
		public static void MenuCreateCube()
		{
			pb_Object pb = pb_ShapeGenerator.CubeGenerator(Vector3.one);
			pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

			Material mat = pb_PreferencesInternal.GetMaterial(pb_Constant.pbDefaultMaterial);

			if(mat != null)
			{
				pb.SetFaceMaterial(pb.faces, mat);
				pb.ToMesh();
				pb.Refresh();
			}
			else
			{
				Debug.Log("mat is null");
			}

			pb_EditorUtility.InitObject(pb);
			pb_EditorUtility.SetPivotAndSnapWithPref(pb, null);
		}

		void OnGUI()
		{
			if(Event.current.type == EventType.ContextClick)
				OpenContextMenu();

		}
	}
}
