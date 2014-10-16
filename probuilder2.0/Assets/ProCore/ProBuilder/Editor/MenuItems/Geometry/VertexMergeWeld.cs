#if !PROTOTYPE

#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	public class VertexMergeWeld : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Collapse Selected Vertices &c", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 0)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Weld Selected Vertices &v", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 1)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Split Selected Vertices &x", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 2)]
		public static bool VerifyVertexOps()
		{
			return pb_Editor.instance && pb_Editor.instance.selectedVertexCount > 1;
		}

		/**
		 *	Collapses all selected vertices to a single central vertex.
		 */
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Collapse Selected Vertices &c", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 0)]
		public static void CollapseVertices()
		{
			if( EditorWindow.focusedWindow != null && EditorWindow.focusedWindow is pb_UV_Editor)
				pb_UV_Editor.instance.Menu_CollapseUVs();
			else
				pb_Menu_Commands.MenuCollapseVertices(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
		
		/**
		 *	For each vertex within epsilon distance, collapse.
		 */
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Weld Selected Vertices &v", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 1)]
		public static void WeldVertices()
		{
			if( EditorWindow.focusedWindow != null && EditorWindow.focusedWindow is pb_UV_Editor)
				pb_UV_Editor.instance.Menu_SewUVs();
			else
				pb_Menu_Commands.MenuWeldVertices(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
	
		/**
		 *	Splits the selected vertices into separate vertices.  When faces are selected, they will be detached (meaning only the selected face
		 *	vertex will be removed from the shared index).
		 */
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Split Selected Vertices &x", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX + 2)]
		public static void SplitVertices()
		{
			if( EditorWindow.focusedWindow != null && EditorWindow.focusedWindow is pb_UV_Editor)
				pb_UV_Editor.instance.Menu_SplitUVs();
			else
				pb_Menu_Commands.MenuSplitVertices(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
	}
}
#endif