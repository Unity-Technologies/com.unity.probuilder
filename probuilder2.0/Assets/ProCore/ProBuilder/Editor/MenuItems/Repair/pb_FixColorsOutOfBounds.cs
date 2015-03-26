using UnityEditor;
using UnityEngine;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for manually re-generating all ProBuilder color arrays in scene.
	 */
	public class pb_FixColorsOutOfBounds : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Fix Colors Out of Bounds", false, pb_Constant.MENU_REPAIR)]
		public static void MenuFixColors()
		{	
			int count = 0;
			foreach(pb_Object pb in Resources.FindObjectsOfTypeAll(typeof(pb_Object)).Where(x => x.hideFlags == HideFlags.None))
			{
				// if(pb.colors == null || pb.colors.Length != pb.vertexCount)
				// {
					pb.SetColors( pbUtil.FilledArray(Color.white, pb.vertexCount) );

					pb.ToMesh();
					pb.Refresh();
					pb.Finalize();

					count++;
				// }
			}


		Debug.Log("count: " + count);
			if(pb_Editor.instance != null)
				pb_Editor.instance.UpdateSelection();

			SceneView.RepaintAll();
		}
	}
}