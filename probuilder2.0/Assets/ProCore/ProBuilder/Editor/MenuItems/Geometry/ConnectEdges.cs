#if !PROTOTYPE
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.Actions
{
	public class SplitOperations : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Subdivide Face", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
		public static bool MenuVerifySplitOperationGeometryFace()
		{
			pb_Editor editor = pb_Editor.instance;
			return editor && editor.editLevel == EditLevel.Geometry && editor.selectionMode == SelectMode.Face && editor.selectedFaceCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Connect Vertices", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
		public static bool MenuVerifySplitOperationVertex()
		{
			pb_Editor editor = pb_Editor.instance;
			return editor && editor.editLevel == EditLevel.Geometry && editor.selectionMode == SelectMode.Vertex && editor.selectedVertexCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Insert Edge Loop &u", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Connect Edges", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
		public static bool MenuVerifySplitOperationEdge()
		{
			pb_Editor editor = pb_Editor.instance;
			return editor && editor.editLevel == EditLevel.Geometry && editor.selectionMode == SelectMode.Edge && editor.selectedEdgeCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Subdivide Object", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
		public static bool MenuVerifySplitOperationObject()
		{
			pb_Editor editor = pb_Editor.instance;
			return editor && editor.selection.Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Smart Connect _&e", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
		public static bool MenuVerifySmartConnect()
		{
			pb_Editor editor = pb_Editor.instance;
			return editor && editor.selectedVertexCount > 1;
		}
		/**
		 * "Smart Connect" exists because even if shortcuts are mutually exclusive via Verify, they can't share.
		 */
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Connect Vertices", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_VERTEX)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Connect Edges", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Subdivide Face", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Smart Connect _&e", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_USEINFERRED)]
		public static void MenuConnectInferUse()
		{
			switch(pb_Editor.instance.selectionMode)
			{
				case SelectMode.Vertex:
					pb_Menu_Commands.MenuConnectVertices(pbUtil.GetComponents<pb_Object>(Selection.transforms));
					break;

				case SelectMode.Face:
					pb_Menu_Commands.MenuSubdivideFace(pbUtil.GetComponents<pb_Object>(Selection.transforms));
					break;

				case SelectMode.Edge:
					pb_Menu_Commands.MenuConnectEdges(pbUtil.GetComponents<pb_Object>(Selection.transforms));
					break;
			}

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Insert Edge Loop &u", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
		public static void MenuInsertEdgeLoop()
		{
			pb_Menu_Commands.MenuInsertEdgeLoop(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
		
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Subdivide Object", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
		public static void MenuSubdivideObject()
		{
			pb_Menu_Commands.MenuSubdivide(pbUtil.GetComponents<pb_Object>(Selection.transforms));		
		}
	}
}
#endif