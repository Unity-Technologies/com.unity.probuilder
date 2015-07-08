using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProBuilder2.Common
{
	/**
	 * Simple object pool implementation.
	 * In the future it would be awesome to make this generic for type safety, but 
	 * for now we need it to serialize so that's a no-go.
	 */
	[System.Serializable]
	public class pb_ObjectPool : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField] private UnityEngine.Object[] _queue;
		[SerializeField] private pb_Type constructorTarget;
		[SerializeField] private string constructorMethod;
		[SerializeField] private pb_Type destructorTarget;
		[SerializeField] private string destructorMethod;

		public void OnBeforeSerialize()
		{
			Debug.Log("OnBeforeSerialize(I)");
			_queue = pool.ToArray();

			MethodInfo mi = constructor.Method;
			constructorTarget = new pb_Type(mi.ReflectedType);
			constructorMethod = mi.Name;

			mi = destructor.Method;
			destructorTarget = new pb_Type(mi.ReflectedType);
			destructorMethod = mi.Name;
		}

		public void OnAfterDeserialize()
		{
			foreach(UnityEngine.Object obj in _queue)
				pool.Enqueue(obj);
			
			constructor = (System.Func<UnityEngine.Object>) System.Delegate.CreateDelegate(typeof(System.Func<UnityEngine.Object>), constructorTarget, constructorMethod);
			destructor = (System.Action<UnityEngine.Object>) System.Delegate.CreateDelegate(typeof(System.Action<UnityEngine.Object>), destructorTarget, destructorMethod);

			Debug.Log("OnAfterSerialize(I)");
			// destructor( constructor() ); 
		}

		public int desiredSize;
		public System.Func<UnityEngine.Object> constructor;
		public System.Action<UnityEngine.Object> destructor;

		private Queue<UnityEngine.Object> pool = new Queue<UnityEngine.Object>();

		public static pb_ObjectPool CreateInstance(int initialSize, int desiredSize, System.Func<UnityEngine.Object> constructor, System.Action<UnityEngine.Object> destructor)
		{
			pb_ObjectPool instance = ScriptableObject.CreateInstance<pb_ObjectPool>();

			instance.constructor = constructor;
			instance.destructor = destructor == null ? DestroyObject : destructor;
			instance.desiredSize = desiredSize;

			for(int i = 0; i < initialSize && i < desiredSize; i++)
				instance.pool.Enqueue( constructor() );

			return instance;
		}
 
		public T Get<T>() where T : UnityEngine.Object, new()
		{
			return (T) (pool.Count > 0 ? (T)pool.Dequeue() : constructor != null ? constructor() : new T());
		}

		public void Put(UnityEngine.Object obj)
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
					destructor( (UnityEngine.Object) pool.Dequeue() );
				else
					DestroyObject( (UnityEngine.Object) pool.Dequeue() );
		}

		static void DestroyObject(UnityEngine.Object obj)
		{
			GameObject.DestroyImmediate( obj );
		}

		void OnDestroy()
		{
			Empty();
		}
	}
}