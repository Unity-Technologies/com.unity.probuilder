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
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Faces with Material %m", true, pb_Constant.MENU_TOOLS)]
		public static bool VerifySelectFaces()
		{
			return pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Faces with Material %m", false, pb_Constant.MENU_TOOLS)]
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

			// aaand also set to face seelction mode
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
	}
}

#endif
