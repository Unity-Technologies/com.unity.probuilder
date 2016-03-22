using UnityEngine;
using System.Collections;
using ProBuilder2.EditorCommon;
using System.Reflection;

namespace ProBuilder2.UpgradeKit
{
	public static class pb_EditorUpgradeKitUtils
	{
#region OBJECT

		/**
		 * Initialize a pb_Object component with the data from a pb_SerializableObject.
		 */
		public static void InitObjectWithSerializedObject(pb_Object pb, pb_SerializableObject serialized)
		{
			/**
			 * On older probuilder versions, the Set() methods also applied to mesh.
			 * To avoid errors, initialize the mesh and renderer components.
			 */
			pb.msh = pb.msh ?? new Mesh();

			if(!pb.gameObject.GetComponent<MeshRenderer>())
				pb.gameObject.AddComponent<MeshRenderer>();

			pb.SetVertices( serialized.GetVertices() );

			pb.msh.vertices = pb.vertices;

			if(!pb_UpgradeKitUtils.InvokeFunction(pb, "SetUV", new object[] { (object)serialized.GetUVs() } ))
				pb.msh.uv = serialized.GetUVs();

			if(!pb_UpgradeKitUtils.InvokeFunction(pb, "SetColors", new object[] { (object)serialized.GetColors() } ))
				pb.msh.colors = serialized.GetColors();

			pb_UpgradeKitUtils.InvokeFunction(pb, "SetSharedIndices", new object[] { (object)serialized.GetSharedIndices().ToPbIntArray() } );

			pb_UpgradeKitUtils.InvokeFunction(pb, "SetSharedIndicesUV", new object[] { (object)serialized.GetSharedIndicesUV().ToPbIntArray() } );

			pb.SetFaces( serialized.GetFaces() );

			pb_UpgradeKitUtils.RebuildMesh(pb);
		}

		/**
		 * Initialize a pb_Entity component with pb_SerializableEntity object.
		 */
		public static void InitEntityWithSerializedObject(pb_Entity entity, pb_SerializableEntity serialized)
		{
			// SetEntityType is an extension method (editor-only) that also sets the static flags to 
			// match the entity use.

			// do this song and dance because ya can't implicitly convert int to enum, and EntityType has changed from a ProBuilder.EntityType
			// enum to namespace ProBuilder2.Common, and I can't figure a better way to make this work
			pb_UpgradeKitUtils.InvokeFunction(entity, "SetEntity", new object[] { System.Enum.ToObject(entity.entityType.GetType(), serialized.entityType) });
		
			// Leave commented because if the user made modifications to the colliders or flags this will reset them.
		//	pb_Editor_Utility.SetEntityType( entity.entityType, entity.gameObject );
		}
	}
#endregion
}