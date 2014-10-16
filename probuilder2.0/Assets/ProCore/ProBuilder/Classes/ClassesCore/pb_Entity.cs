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

	public bool isTrigger = false;
	public PhysicMaterial physicMaterial;
	public bool forceConvex = false;
	public bool smoothSphereCollisions = false;
	public Vector3 center = Vector3.zero;
	public Vector3 size = Vector3.one;
	public bool userSetDimensions = false;

	[HideInInspector]
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
			// Debug.LogError("pb is null: " + gameObject.name);
			return;
		}

		if(pb.containsNodraw)
			pb.HideNodraw();

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
				// Destroy(GetComponent<MeshRenderer>());
				// Destroy(this);
				#endif
			break;

			case EntityType.Collider:
				// Destroy(GetComponent<pb_Object>());
				#if !DEBUG
				GetComponent<MeshRenderer>().enabled = false;
				// Destroy(GetComponent<MeshRenderer>());
				// Destroy(this);
				#endif
			break;
		}
	}

	public void SetEntity(EntityType t)
	{
		_entityType = t;
	}
}