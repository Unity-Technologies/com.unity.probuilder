using UnityEngine;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/**
	 *	Helper functions for working with transforms.
	 */
	public static class pbTransformUtil
	{
		private static Dictionary<Transform, Transform[]> _childrenStack = new Dictionary<Transform, Transform[]>(); 

		/**
		 *	Unparent all children from a transform, saving them for later re-parenting (see ReparentChildren).
		 */
		public static void UnparentChildren(Transform t)
		{
			Transform[] children = new Transform[t.childCount];

			for(int i = 0; i < t.childCount; i++)
			{
				Transform child = t.GetChild(i);
				children[i] = child;
				child.SetParent(null, true);
			}

			_childrenStack.Add(t, children);
		}

		/**
		 *	Re-parent all children to a transform.  Must have called UnparentChildren prior.
		 */
		public static void ReparentChildren(Transform t)
		{
			Transform[] children;

			if(_childrenStack.TryGetValue(t, out children))
			{
				foreach(Transform c in children)
					c.SetParent(t, true);

				_childrenStack.Remove(t);
			}
		}
	}
}
