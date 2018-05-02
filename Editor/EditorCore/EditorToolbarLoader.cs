using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using Actions = UnityEditor.ProBuilder.Actions;
using System.Linq;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Responsible for loading menu actions in to the pb_Toolbar.
	/// </summary>
	public static class EditorToolbarLoader
	{
		internal static System.Func<MenuAction> onLoadMenu;
		static List<MenuAction> s_Defaults;

		public static void RegisterMenuItem(System.Func<MenuAction> getMenuAction)
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

		public static void UnRegisterMenuItem(System.Func<MenuAction> getMenuAction)
		{
			onLoadMenu -= getMenuAction;
		}

		public static T GetInstance<T>() where T : MenuAction, new()
		{
			T instance = (T) GetActions().FirstOrDefault(x => x is T);

			if(instance == null)
			{
				instance = new T();
				if(s_Defaults != null)
					s_Defaults.Add(instance);
				else
					s_Defaults = new List<MenuAction>() { instance };
			}

			return instance;
		}

		internal static List<MenuAction> GetActions(bool forceReload = false)
		{
			if(s_Defaults != null && !forceReload)
				return s_Defaults;

			s_Defaults = new List<MenuAction>()
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
				foreach(System.Func<MenuAction> del in onLoadMenu.GetInvocationList())
					s_Defaults.Add(del());
			}

			s_Defaults.Sort(MenuAction.CompareActionsByGroupAndPriority);

			return s_Defaults;
		}
	}
}
