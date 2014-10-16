using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{

	public class NoDrawFix : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Fix GameObject Flags", false, pb_Constant.MENU_REPAIR)]
		public static void FixNoDraw()
		{
			pb_Object[] all = FindObjectsOfType(typeof(pb_Object)) as pb_Object[];

			for(int i = 0; i < all.Length; i++)
			{
				EditorUtility.DisplayProgressBar(
					"Fixing NoDraw Flags",
					"Working over " + all[i].id + ".",
					((float)i / all.Length));
			
				StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags( all[i].gameObject );
				
				// if nodraw found
				if(all[i].containsNodraw)
				{
					if( (flags & StaticEditorFlags.BatchingStatic) == StaticEditorFlags.BatchingStatic )
					{
						flags ^= StaticEditorFlags.BatchingStatic;
						GameObjectUtility.SetStaticEditorFlags(all[i].gameObject, flags);
					}
				}
				else
				{
					pb_Entity ent = all[i].GetComponent<pb_Entity>();

					// if nodraw not found, and entity type should be batching static
					if(ent == null || ent.entityType != EntityType.Mover)
					{
						flags = flags | StaticEditorFlags.BatchingStatic;
						GameObjectUtility.SetStaticEditorFlags(all[i].gameObject, flags);
					}
				}
			}

			EditorUtility.ClearProgressBar();
			if(all.Length > 0)
				EditorUtility.DisplayDialog("Fix GameObject Flags", "Successfully repaired StaticEditorFlags.", "Okay");

		} 
	}
}