using UnityEngine;
using System.Collections;

namespace ProBuilder2.Common
{
	/**
	 * Simple object pool implementation.
	 */
	[System.Serializable]
	public class pb_ObjectPool<T> where T : UnityEngine.Object
	{
		public int desiredSize;
		public System.Func<T> constructor;
		public System.Action<T> destructor;

		[SerializeField] private Queue pool = new Queue();

		public pb_ObjectPool(int initialSize, int desiredSize, System.Func<T> constructor, System.Action<T> destructor)
		{
			this.constructor = constructor;
			this.destructor = destructor == null ? DestroyObject<T> : destructor;
			this.desiredSize = desiredSize;

			for(int i = 0; i < initialSize && i < desiredSize; i++)
				pool.Enqueue( constructor() );
		}
 
		public T Get()
		{
			return pool.Count > 0 ? (T)pool.Dequeue() : constructor();
		}

		public void Put(T obj)
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
				destructor( (T)pool.Dequeue() );
			}
		}

		static void DestroyObject<O>(O obj) where O : UnityEngine.Object
		{
			GameObject.DestroyImmediate( (O)obj );
		}
	}
}