// #define DEBUG

using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using ProBuilder2.Common;	// Can't assign components to namespaces 

[AddComponentMenu("")]	// Don't let the user add this to any object.
/**
 *	\brief Determines how this #pb_Object should behave in game.
 */
public class pb_Entity : MonoBehaviour
{
	pb_Object pb;

	public EntityType entityType { get { return _entityType; } }

	[SerializeField]
	[HideInInspector]
	private EntityType _entityType;

	/**
	 *	\brief Performs Entity specific initialization tasks (turn off renderer for nodraw faces, hide colliders, etc)
	 */
	public void Awake()
	{
		pb = GetComponent<pb_Object>();
		if(pb == null) 
		{
			return;
		}

		switch(entityType)
		{
			case EntityType.Occluder:
				// Destroy(gameObject);
			break;

			case EntityType.Detail:
			break;

			case EntityType.Trigger:
				#if !DEBUG
				GetComponent<MeshRenderer>().enabled = false;
				#endif
			break;

			case EntityType.Collider:
				#if !DEBUG
				GetComponent<MeshRenderer>().enabled = false;
				#endif
			break;
		}
	}

	public void SetEntity(EntityType t)
	{
		_entityType = t;
	}
}