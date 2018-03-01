using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder.Core
{
	[DisallowMultipleComponent]
	class pb_TriggerBehaviour : pb_EntityBehaviour
	{
		public override void Initialize()
		{
			var collision = gameObject.GetComponent<Collider>();

			if (!collision)
				collision = gameObject.AddComponent<MeshCollider>();

			var meshCollider = collision as MeshCollider;

			if (meshCollider)
				meshCollider.convex = true;

			collision.isTrigger = true;

			SetMaterial(pb_Material.TriggerMaterial);
		}

		public override void OnEnterPlayMode()
		{
			var r = GetComponent<Renderer>();

			if (r != null)
				r.enabled = false;
		}
	}
}