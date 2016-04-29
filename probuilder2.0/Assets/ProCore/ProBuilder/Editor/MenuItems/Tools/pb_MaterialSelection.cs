#if !PROTOTYPE

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	public class pb_MaterialSelectionShortcut : EditorWindow
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Faces with Vertex Colors", true, pb_Constant.MENU_SELECTION)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Faces with Material %m", true, pb_Constant.MENU_SELECTION)]
		public static bool VerifySelectFaces()
		{
			return pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Faces with Material %m", false, pb_Constant.MENU_SELECTION)]
		public static void MenuSelectFaces()
		{
			IEnumerable<Material> materials = Selection.transforms.GetComponents<pb_Object>().SelectMany(x => x.SelectedFaces.Select(y => y.material)).Distinct();

			SelectFacesWithMatchingMaterial(materials);
		}

		private static void SelectFacesWithMatchingMaterial(IEnumerable<Material> mats)
		{
			pb_Editor editor = pb_Editor.instance;
			
			// If we're in Mode based editing, make sure that we're also in geo mode. 
			editor.SetEditLevel(EditLevel.Geometry);

			// aaand also set to face selection mode
			editor.SetSelectionMode(SelectMode.Face);

			pb_Object[] pbs = FindObjectsOfType(typeof(pb_Object)) as pb_Object[];
			
			foreach(pb_Object pb in pbs)
			{
				bool addToSelection = false;

				for(int i = 0; i < pb.faces.Length; i++)
				{
					if(mats.Contains(pb.faces[i].material))
					{
						addToSelection = true;
						pb.AddToFaceSelection(i);
					}
				}

				if(addToSelection)
					editor.AddToSelection(pb.gameObject);
			}
			
			editor.UpdateSelection();
		}

		/**
		 * Checks the current face selection, then selects the face selection to all faces with matching vertex colors.
		 */
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Faces with Vertex Colors", false, pb_Constant.MENU_SELECTION)]
		public static void MenuSelectFacesWithColor()
		{
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				HashSet<Color> cols = new HashSet<Color>();
				
				foreach(pb_Face f in pb.SelectedFaces)
				{
					foreach(int i in f.distinctIndices)
						cols.Add(pb.colors[i]);
				}

				pb_Face[] faces = System.Array.FindAll(pb.faces, x => cols.Intersect(pbUtil.ValuesWithIndices(pb.colors, x.distinctIndices)).Count() > 0);

				pb.SetSelectedFaces(faces);

				if(pb_Editor.instance)
					pb_Editor.instance.UpdateSelection();
				
				EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
			}
		}
	}
}

#endif
