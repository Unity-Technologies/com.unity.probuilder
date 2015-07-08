using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProBuilder2.Common
{
	/**
	 * Simple object pool implementation.
	 */
	public class pb_ObjectPool<T> where T : UnityEngine.Object, new()
	{
		public int desiredSize;

		public System.Func<T> constructor;
		public System.Action<T> destructor;

		private Queue<T> pool = new Queue<T>();

		public pb_ObjectPool(int initialSize, int desiredSize, System.Func<T> constructor, System.Action<T> destructor)
		{
			this.constructor = constructor;
			this.destructor = destructor == null ? DestroyObject : destructor;
			this.desiredSize = desiredSize;

			for(int i = 0; i < initialSize && i < desiredSize; i++)
				this.pool.Enqueue( constructor() );
		}
 
		public T Get()
		{
			return (T) (pool.Count > 0 ? (T)pool.Dequeue() : constructor != null ? constructor() : new T());
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
			int count = pool.Count;

			for(int i = 0; i < count; i++)
				if(destructor != null)
					destructor( (T) pool.Dequeue() );
				else
					DestroyObject( (T) pool.Dequeue() );
		}

		static void DestroyObject(T obj)
		{
			GameObject.DestroyImmediate( obj );
		}

		void OnDestroy()
		{
			Empty();
		}
	}
}