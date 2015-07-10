using UnityEngine;

namespace ProBuilder2.Common
{
	/**
	 * A generic singleton implementation for MonoBehaviours.
	 */
	public class pb_MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		/// Store a static reference to the instance of this object.
		private static MonoBehaviour _instance;

		/**
		 * Returns an instance of T.  If no instance is available, a new one will be instantiated.
		 */
		public static T instance
		{
			get
			{
				if( nullableInstance == null )
				{
					GameObject go = new GameObject();
					go.name = typeof(T).ToString();
					_instance = go.AddComponent<T>();
				}

				return (T) _instance;
			}
		}

		/**
		 * Unlike `instance`, this returns null if no instance is found.
		 */
		public static T nullableInstance
		{
			get
			{
				if(_instance == null)
				{
					T[] danglers = Resources.FindObjectsOfTypeAll<T>();

					if(danglers != null && danglers.Length > 0)
					{
						// shouldn't ever have dangling instances, but just in case...
						_instance = danglers[0];
						for(int i = 1; i < danglers.Length; i++)
							GameObject.DestroyImmediate(danglers[i]);
					}
				}

				return (T) _instance;
			}
		}

		/**
		 * Return true if an instance exists, false otherwise.
		 */
		public static bool Valid()
		{
			return nullableInstance != null;
		}

		/**
		 * Classes overriding Awake() should be sure to call base.Awake().
		 */
		public virtual void Awake()
		{
			if(_instance == null)
				_instance = this;
			else
				GameObject.Destroy(this);
		}

		/**
		 * Classes overriding OnEnable() should be sure to call base.OnEnable().
		 */
		public virtual void OnEnable()
		{
			_instance = this;
		}
	}
}