using UnityEngine;
using System.Runtime.Serialization;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.UpgradeKit
{
	/**
	 * Serializable class for pb_Entity.
	 */
	[System.Serializable()]
	public class pb_SerializableEntity :  ISerializable
	{
		public int entityType;

		public pb_SerializableEntity(pb_Entity entity)
		{
			entityType = (int)entity.entityType;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("entityType", entityType, typeof(int));
		}

		public pb_SerializableEntity(SerializationInfo info, StreamingContext context)
		{
			this.entityType = (int)info.GetValue( "entityType", typeof(int) );
		}
	}
}