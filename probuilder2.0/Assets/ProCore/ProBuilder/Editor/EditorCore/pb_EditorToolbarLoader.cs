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

		private static List<pb_MenuAction> _defaults;

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

		public static T GetInstance<T>() where T : pb_MenuAction, new()
		{
			T instance = (T) GetActions().FirstOrDefault(x => x is T);

			if(instance == null)
			{
				instance = new T();
				if(_defaults != null)
					_defaults.Add(instance);
				else
					_defaults = new List<pb_MenuAction>() { instance };
			}

			return instance;
		}

		public static List<pb_MenuAction> GetActions(bool forceReload = false)
		{
			if(_defaults != null && !forceReload)
				return _defaults;

			_defaults = new List<pb_MenuAction>()
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
				new CenterPivot(),
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

				// Vertex
				new CollapseVertices(),
				new WeldVertices(),
				new ConnectVertices(),
				new SplitVertices(),

				// Entity
				new SetEntityType_Detail(),
				// new SetEntityType_Occluder(),
				new SetEntityType_Mover(),
				new SetEntityType_Collider(),
				new SetEntityType_Trigger(),
			};

			if(onLoadMenu != null)
			{
				foreach(OnLoadActionsDelegate del in onLoadMenu.GetInvocationList())
					_defaults.Add(del());
			}

			return _defaults;
		}
	}
}
