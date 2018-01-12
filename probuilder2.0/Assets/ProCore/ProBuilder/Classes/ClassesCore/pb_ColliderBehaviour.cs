using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder.Core
{
	[DisallowMultipleComponent]
	class pb_ColliderBehaviour : pb_EntityBehaviour
	{
		public override void Initialize()
		{
			var collision = gameObject.GetComponent<Collider>();
			if (!collision)
				collision = gameObject.AddComponent<MeshCollider>();
			collision.isTrigger = false;
			SetMaterial(pb_Constant.ColliderMaterial);
		}

		public override void OnEnterPlayMode()
		{
			var r = GetComponent<Renderer>();

			if (r != null)
				r.enabled = false;
		}
	}
}
