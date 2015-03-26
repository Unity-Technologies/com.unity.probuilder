using UnityEngine;
using System.Runtime.Serialization;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.Serialization
{
	/**
	 * Serializable class for pb_Entity.
	 */
	public class pb_SerializableEntity :  ISerializable
	{
		public EntityType entityType;

		public pb_SerializableEntity(pb_Entity entity)
		{
			entityType = entity.entityType;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("entityType", entityType, typeof(EntityType));
		}

		public pb_SerializableEntity(SerializationInfo info, StreamingContext context)
		{
			this.entityType = (EntityType)info.GetValue( "entityType", typeof(EntityType) );
		}
	}
}