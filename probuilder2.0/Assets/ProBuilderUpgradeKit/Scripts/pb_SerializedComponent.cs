using UnityEngine;
using System.Collections;

namespace ProBuilder2.UpgradeKit
{
	/**
	 * This component stores serialized data.
	 */
	public class pb_SerializedComponent : MonoBehaviour
	{
		[SerializeField] string _object;			///< JSON serialized pb_SerializableObject	
		[SerializeField] string _entity;			///< JSON serialized pb_Entity

		/**
		 *	True if serialization script broke a prefab instance, false if not.  Catches case where
		 *	user has manually disconnected a prefab.  Breaking the prefab instance is necessary
		 *	because otherwise 2 instances of pb_SerializedComponent will be applied per-prefab instance.
		 */
		[HideInInspector] public bool isPrefabInstance;		

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