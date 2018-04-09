using UnityEngine;
using System.Collections.Generic;
using ProBuilder.Core;
using Actions = ProBuilder.Actions;
using System.Linq;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Responsible for loading menu actions in to the pb_Toolbar.
	/// </summary>
	public static class pb_EditorToolbarLoader
	{
		public delegate pb_MenuAction OnLoadActionsDelegate();
		internal static OnLoadActionsDelegate onLoadMenu;
		static List<pb_MenuAction> _defaults;

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

		internal static List<pb_MenuAction> GetActions(bool forceReload = false)
		{
			if(_defaults != null && !forceReload)
				return _defaults;

			_defaults = new List<pb_MenuAction>()
			{
				// tools
				new Actions.OpenShapeEditor(),
				new Actions.NewBezierShape(),
				new Actions.NewPolyShape(),
				new Actions.OpenMaterialEditor(),
				new Actions.OpenUVEditor(),
				new Actions.OpenVertexColorEditor(),
				new Actions.OpenSmoothingEditor(),

				new Actions.ToggleSelectBackFaces(),
				new Actions.ToggleHandleAlignment(),
				new Actions.ToggleDragSelectionMode(),
				new Actions.ToggleDragRectMode(),

				// selection
				new Actions.GrowSelection(),
				new Actions.ShrinkSelection(),
				new Actions.InvertSelection(),
				new Actions.SelectEdgeLoop(),
				new Actions.SelectEdgeRing(),
				new Actions.SelectFaceLoop(),
				new Actions.SelectFaceRing(),
				new Actions.SelectHole(),
				new Actions.SelectVertexColor(),
				new Actions.SelectMaterial(),
				new Actions.SelectSmoothingGroup(),

				// object
				new Actions.MergeObjects(),
				new Actions.MirrorObjects(),
				new Actions.FlipObjectNormals(),
				new Actions.SubdivideObject(),
				new Actions.FreezeTransform(),
				new Actions.CenterPivot(),
				new Actions.ConformObjectNormals(),
				new Actions.TriangulateObject(),
				new Actions.GenerateUV2(),
				new Actions.ProBuilderize(),
				new Actions.Export(),
				// new Actions.ExportFbx(),
				new Actions.ExportObj(),
				new Actions.ExportAsset(),
				new Actions.ExportPly(),
				new Actions.ExportStlAscii(),
				new Actions.ExportStlBinary(),

				// All
				new Actions.SetPivotToSelection(),

				// Faces (All)
				new Actions.DeleteFaces(),
				new Actions.DetachFaces(),
				new Actions.ExtrudeFaces(),

				// Face
				new Actions.ConformFaceNormals(),
				new Actions.FlipFaceEdge(),
				new Actions.FlipFaceNormals(),
				new Actions.MergeFaces(),
				new Actions.SubdivideFaces(),
				new Actions.TriangulateFaces(),

				// Edge
				new Actions.BridgeEdges(),
				new Actions.BevelEdges(),
				new Actions.ConnectEdges(),
				new Actions.ExtrudeEdges(),
				new Actions.InsertEdgeLoop(),
				new Actions.SubdivideEdges(),

				// Vertex
				new Actions.CollapseVertices(),
				new Actions.WeldVertices(),
				new Actions.ConnectVertices(),
				new Actions.FillHole(),
				// new Actions.CreatePolygon(),
				new Actions.SplitVertices(),

				// Entity
#if ENABLE_ENTITY_TYPES
				new Actions.SetEntityType_Detail(),
				new Actions.SetEntityType_Mover(),
				new Actions.SetEntityType_Collider(),
				new Actions.SetEntityType_Trigger(),
#endif
				new Actions.SetTrigger(),
				new Actions.SetCollider(),

			};

			if(onLoadMenu != null)
			{
				foreach(OnLoadActionsDelegate del in onLoadMenu.GetInvocationList())
					_defaults.Add(del());
			}

			_defaults.Sort(pb_MenuAction.CompareActionsByGroupAndPriority);

			return _defaults;
		}
	}
}
