using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Simple object pool implementation.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	sealed class ObjectPool<T> where T : Object, new()
	{
		public int desiredSize;

		public System.Func<T> constructor;
		public System.Action<T> destructor;

		Queue pool = new Queue();	// VS compiler doesn't recognize Queue<T> as existing?

		public ObjectPool(int initialSize, int desiredSize, System.Func<T> constructor, System.Action<T> destructor)
		{
			this.constructor = constructor;
			this.destructor = destructor == null ? DestroyObject : destructor;
			this.desiredSize = desiredSize;

			for(int i = 0; i < initialSize && i < desiredSize; i++)
				this.pool.Enqueue( constructor != null ? constructor() : new T() );
		}

		public T Get()
		{
			T obj = pool.Count > 0 ? (T)pool.Dequeue() : null;
			if(obj == null)
				obj = constructor == null ? new T() : constructor();
			return obj;
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
