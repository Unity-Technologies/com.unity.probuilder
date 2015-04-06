using UnityEngine;
using System.Collections;

namespace ProBuilder2.Common
{
	/**
	 * Simple object pool implementation.
	 */
	[System.Serializable]
	public class pb_ObjectPool
	{
		public int desiredSize;
		public System.Func<Object> constructor;

		[SerializeField] private Queue pool = new Queue();

		public pb_ObjectPool(int initialSize, int desiredSize, System.Func<Object> constructor)
		{
			this.constructor = constructor;
			this.desiredSize = desiredSize;

			for(int i = 0; i < initialSize && i < desiredSize; i++)
				pool.Enqueue( constructor() );
		}
 
		public Object Get()
		{
			return pool.Count > 0 ? (Object)pool.Dequeue() : constructor();
		}

		public void Put(Object obj)
		{
			if(pool.Count < desiredSize)
				pool.Enqueue(obj);
			else
				GameObject.DestroyImmediate(obj);
		}

		public void Empty()
		{
			for(int i = 0; i < pool.Count; i++)
			{
				GameObject.DestroyImmediate( (UnityEngine.Object)pool.Dequeue() );
			}
		}
	}
}