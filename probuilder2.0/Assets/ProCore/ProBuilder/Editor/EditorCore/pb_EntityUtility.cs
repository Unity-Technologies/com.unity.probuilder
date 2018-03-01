using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	static class pb_EntityUtility
	{
		const StaticEditorFlags StaticEditorFlags_All =
			StaticEditorFlags.LightmapStatic |
			StaticEditorFlags.OccluderStatic |
			StaticEditorFlags.BatchingStatic |
			StaticEditorFlags.OccludeeStatic |
			StaticEditorFlags.NavigationStatic |
			StaticEditorFlags.OffMeshLinkGeneration |
			StaticEditorFlags.ReflectionProbeStatic;

		/// <summary>
		/// Sets the EntityType for the passed gameObject.
		/// </summary>
		/// <param name="newEntityType">The type to set.</param>
		/// <param name="target">The gameObject to apply the EntityType to.  Must contains pb_Object and pb_Entity components.  Method does contain null checks.</param>
		[Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
		public static void SetEntityType(EntityType newEntityType, GameObject target)
		{
			pb_Entity ent = target.GetComponent<pb_Entity>();

			if (ent == null)
				ent = target.AddComponent<pb_Entity>();

			pb_Object pb = target.GetComponent<pb_Object>();

			if (!ent || !pb)
				return;

			SetEditorFlags(StaticEditorFlags_All, target);

			switch (newEntityType)
			{
				case EntityType.Detail:
				case EntityType.Occluder:
					SetBrush(target);
					break;

				case EntityType.Trigger:
					SetTrigger(target);
					break;

				case EntityType.Collider:
					SetCollider(target);
					break;

				case EntityType.Mover:
					SetDynamic(target);
					break;
			}

			ent.SetEntity(newEntityType);
		}

		[Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
		static void SetBrush(GameObject target)
		{
			EntityType et = target.GetComponent<pb_Entity>().entityType;

			if (et == EntityType.Trigger ||
			    et == EntityType.Collider)
			{
				pb_Object pb = target.GetComponent<pb_Object>();
				pb.SetFaceMaterial(pb.faces, pb_Material.DefaultMaterial);
				pb.ToMesh();
				pb.Refresh();
			}
		}

		[Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
		static void SetDynamic(GameObject target)
		{
			EntityType et = target.GetComponent<pb_Entity>().entityType;

			SetEditorFlags((StaticEditorFlags) 0, target);

			if (et == EntityType.Trigger ||
			    et == EntityType.Collider)
			{
				pb_Object pb = target.GetComponent<pb_Object>();
				pb.SetFaceMaterial(pb.faces, pb_Material.DefaultMaterial);

				pb.ToMesh();
				pb.Refresh();
			}
		}

		[Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
		static void SetTrigger(GameObject target)
		{
			pb_Object pb = target.GetComponent<pb_Object>();
			pb.SetFaceMaterial(pb.faces, pb_Material.TriggerMaterial);
			SetIsTrigger(true, target);
			SetEditorFlags((StaticEditorFlags) 0, target);

			pb.ToMesh();
			pb.Refresh();
		}

		[Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
		static void SetCollider(GameObject target)
		{
			pb_Object pb = target.GetComponent<pb_Object>();
			pb.SetFaceMaterial(pb.faces, pb_Material.ColliderMaterial);
			pb.ToMesh();
			pb.Refresh();

			SetEditorFlags((StaticEditorFlags) (StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration),
				target);
		}

		[Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
		static void SetEditorFlags(StaticEditorFlags editorFlags, GameObject target)
		{
			GameObjectUtility.SetStaticEditorFlags(target, editorFlags);
		}

		[Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
		static void SetIsTrigger(bool val, GameObject target)
		{
			Collider[] colliders = pb_Util.GetComponents<Collider>(target);
			foreach (Collider col in colliders)
			{
				if (val && col is MeshCollider)
					((MeshCollider) col).convex = true;
				col.isTrigger = val;
			}
		}
	}
}
