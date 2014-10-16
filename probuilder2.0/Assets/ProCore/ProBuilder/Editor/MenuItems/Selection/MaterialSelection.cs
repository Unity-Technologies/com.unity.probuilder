using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

public class MaterialSelection : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select All Faces With Material", true, pb_Constant.MENU_SELECTION + 2)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Faces with Vertex Colors", true, pb_Constant.MENU_SELECTION + 2)]
	public static bool VerifySelectAction()
	{
		return pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select All Faces With Material", false, pb_Constant.MENU_SELECTION + 2)]
	public static void MenuSelectFacesWithMaterial()
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			List<Material> mat = new List<Material>();
			foreach(pb_Face f in pb.SelectedFaces)
			{
				mat.Add(f.material);
			}

			pb_Face[] faces = System.Array.FindAll(pb.faces, x => mat.Contains(x.material));

			pb.SetSelectedFaces(faces);
			if(pb_Editor.instance)
				pb_Editor.instance.UpdateSelection();
			
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	}

	/**
	 * Checks the current face selection, then selects the face selection to all faces with matching vertex colors.
	 */
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Faces with Vertex Colors", false, pb_Constant.MENU_SELECTION + 2)]
	public static void MenuSelectFacesWithColor()
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			HashSet<Color32> cols = new HashSet<Color32>();
			
			foreach(pb_Face f in pb.SelectedFaces)
				cols.Add(f.color);

			pb_Face[] faces = System.Array.FindAll(pb.faces, x => cols.Contains(x.color));

			pb.SetSelectedFaces(faces);
			if(pb_Editor.instance)
				pb_Editor.instance.UpdateSelection();
			
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	}
}
