using UnityEngine;
using System.Collections;

namespace ProBuilder2.Serialization
{
	/**
	 * This component stores serialized data.
	 */
	public class pb_SerializedComponent : MonoBehaviour
	{
		[SerializeField] string _object;	///< JSON serialized pb_SerializableObject	
		[SerializeField] string _entity;	///< JSON serialized pb_Entity

		public string GetObjectData()
		{
			return _object;
		}

		public string GetEntityData()
		{
			return _entity;
		}

		public void SetObjectData(string objectData)
		{
			_object = objectData;
		}

		public void SetEntityData(string entityData)
		{
			_entity = entityData;
		}
	}
}