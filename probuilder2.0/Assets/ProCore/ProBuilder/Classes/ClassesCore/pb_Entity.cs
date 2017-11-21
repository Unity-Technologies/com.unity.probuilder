using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// Provides some additional functionality to GameObjects, like managing visiblity and colliders.
	/// </summary>
	/// <remarks>For backwards compatibility reasons this class remains outside of the ProBuilder2.Common namespace.</remarks>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class pb_Entity : MonoBehaviour
	{
		public EntityType entityType { get { return _entityType; } }

		[SerializeField]
		[HideInInspector]
		EntityType _entityType;

		/// <summary>
		/// Performs Entity specific initialization tasks (turn off renderer for nodraw faces, hide colliders, etc)
		/// </summary>
		public void Awake()
		{
			MeshRenderer mr = GetComponent<MeshRenderer>();

			if(!mr) return;

			switch(entityType)
			{
				case EntityType.Occluder:
				break;

				case EntityType.Detail:
				break;

				case EntityType.Trigger:
					mr.enabled = false;
				break;

				case EntityType.Collider:
					mr.enabled = false;
				break;
			}
		}

		/// <summary>
		/// Set the entity type.
		/// </summary>
		/// <param name="t"></param>
		public void SetEntity(EntityType t)
		{
			_entityType = t;
		}
	}
}