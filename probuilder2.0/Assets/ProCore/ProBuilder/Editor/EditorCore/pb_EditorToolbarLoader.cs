using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Actions;
using System.Linq;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarLoader
	{
		public delegate pb_MenuAction OnLoadActionsDelegate();
		public static OnLoadActionsDelegate onLoadMenu;

		public static void RegisterMenuItem(OnLoadActionsDelegate getMenuAction)
		{
			if(onLoadMenu != null)
			{
				onLoadMenu -= getMenuAction;
				onLoadMenu += getMenuAction;
			}
			else
			{
				onLoadMenu = getMenuAction;
			}
		}

		public static void UnRegisterMenuItem(OnLoadActionsDelegate getMenuAction)
		{
			onLoadMenu -= getMenuAction;
		}

		public static List<pb_MenuAction> GetActions()
		{
			List<pb_MenuAction> defaults = new List<pb_MenuAction>()
			{
				// tools
				new OpenShapeEditor(),
				new OpenMaterialEditor(),
				new OpenUVEditor(),
				new OpenVertexColorEditor(),
				new OpenSmoothingEditor(),
				new OpenMirrorObjectsEditor(),

				new ToggleSelectBackFaces(),
				new ToggleHandleAlignment(),

				new GrowSelection(),
				new ShrinkSelection(),
				new InvertSelection(),
				new SelectEdgeLoop(),
				new SelectEdgeRing(),

				// object
				new MergeObjects(),
				new FlipObjectNormals(),
				new SubdivideObject(),
				new FreezeTransform(),
				new ConformObjectNormals(),
				new TriangulateObject(),

				// All
				new SetPivotToSelection(),

				// Faces (All)
				new DeleteFaces(),
				new DetachFaces(),
				new ExtrudeFaces(),

				// Face
				new ConformFaceNormals(),
				new FlipFaceEdge(),
				new FlipFaceNormals(),
				new MergeFaces(),
				new SubdivideFaces(),
				
				// Edge
				new BridgeEdges(),
				new ConnectEdges(),
				new ExtrudeEdges(),
				new InsertEdgeLoop(),
				new SubdivideEdges(),
				new BevelEdges(),

				// Vertex
				new CollapseVertices(),
				new WeldVertices(),
				new ConnectVertices(),
				new SplitVertices(),

				// Entity
				new EntityType_Detail(),
			};

			if(onLoadMenu != null)
			{
				foreach(OnLoadActionsDelegate del in onLoadMenu.GetInvocationList())
					defaults.Add(del());
			}

			return defaults;
		}
	}
}
