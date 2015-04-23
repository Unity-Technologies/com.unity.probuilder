using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for the ProBuilderize functions.
	 */
	public class pb_ProBuilderize : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection", true, pb_Constant.MENU_ACTIONS + 1)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection (Preserve Faces)", true, pb_Constant.MENU_ACTIONS + 2)]
		public static bool VerifyProBuilderize()
		{
			return Selection.transforms.Length - pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}	

		// [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection", false, pb_Constant.MENU_ACTIONS + 1)]
		// public static void MenuProBuilderizeTris()
		// {
		// 	ProBuilderizeObjects(false, false);
		// }

		// [MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection (Preserve Faces)", false, pb_Constant.MENU_ACTIONS + 2)]
		// public static void MenuProBuilderizeQuads()
		// {
		// 	ProBuilderizeObjects(true, false);
		// }

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection (Preserve Faces)", false, pb_Constant.MENU_ACTIONS + 4)]
		public static void MenuProBuilderizeQuads2()
		{
			ProBuilderizeObjects(true, true);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection", false, pb_Constant.MENU_ACTIONS + 3)]
		public static void MenuProBuilderizeTris2()
		{
			ProBuilderizeObjects(false, true);
		}

		public static void ProBuilderizeObjects(bool preserveFaces, bool inPlace)
		{
			foreach(Transform t in Selection.transforms)
			{
				if(t.GetComponent<MeshFilter>())
				{
					if(inPlace)
						ProBuilderizeInPlace(t, preserveFaces);
					else
						ProBuilderize(t, preserveFaces);
				}
			}

			if(pb_Editor.instance != null)
				pb_Editor.instance.UpdateSelection();
		}

		/**
		 * Duplicates the object and makes it editable.
		 */
		public static pb_Object ProBuilderize(Transform t, bool preserveFaces)
		{

			pb_Object pb = pbMeshOps.CreatePbObjectWithTransform(t, preserveFaces);

			pb_Editor_Utility.SetEntityType(EntityType.Detail, pb.gameObject);

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
			
			t.gameObject.SetActive(false);

			return pb;
		}

		/**
		 * Adds pb_Object and pb_Entity to object without duplicating the objcet.  Is undo-able.
		 */
		public static void ProBuilderizeInPlace(Transform t, bool preserveFaces)
		{
			Undo.RegisterFullObjectHierarchyUndo(t.gameObject, "ProBuilderize");

			MeshRenderer mr = t.GetComponent<MeshRenderer>();

			pb_Object pb = pbMeshOps.AddPbObjectToObject(t, preserveFaces);

			EntityType entityType = EntityType.Detail;

			if(mr != null && mr.sharedMaterials != null && mr.sharedMaterials.Any(x => x.name.Contains("Collider")))
				entityType = EntityType.Collider;
			else
			if(mr != null && mr.sharedMaterials != null && mr.sharedMaterials.Any(x => x.name.Contains("Trigger")))
				entityType = EntityType.Trigger;

			// if this was previously a pb_Object, or similarly any other instance asset, destroy it.
			// if it is backed by saved asset, leave the mesh asset alone but assign a new mesh to the 
			// renderer so that we don't modify the asset.
			if( AssetDatabase.GetAssetPath(t.GetComponent<MeshFilter>().sharedMesh) == "" )
				Undo.DestroyObjectImmediate(t.GetComponent<MeshFilter>().sharedMesh);
			else
				t.GetComponent<MeshFilter>().sharedMesh = new Mesh();

			pb.ToMesh();
			pb.Refresh(); 
			pb.Optimize();

			// Don't call the editor version of SetEntityType because that will
			// reset convexity and trigger settings, which we can assume are user
			// set already.
			pb.gameObject.GetComponent<pb_Entity>().SetEntity(entityType);
			// pb_Editor_Utility.SetEntityType(entityType, t.gameObject);
		}
	}
}