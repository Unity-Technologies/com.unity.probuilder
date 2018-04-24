using UnityEngine;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A generic singleton implementation for MonoBehaviours.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		// Store a static reference to the instance of this object.
		static MonoBehaviour s_Instance;

		/// <summary>
		/// Returns an instance of T.  If no instance is available, a new one will be instantiated.
		/// </summary>
		public static T instance
		{
			get
			{
				if( nullableInstance == null )
				{
					GameObject go = new GameObject();
					go.name = typeof(T).ToString();
					s_Instance = go.AddComponent<T>();
				}

				return (T) s_Instance;
			}
		}

		/// <summary>
		/// Unlike `instance`, this returns null if no instance is found.
		/// </summary>
		public static T nullableInstance
		{
			get
			{
				if(s_Instance == null)
				{
					T[] danglers = Resources.FindObjectsOfTypeAll<T>();

					if(danglers != null && danglers.Length > 0)
					{
						// shouldn't ever have dangling instances, but just in case...
						s_Instance = danglers[0];
						for(int i = 1; i < danglers.Length; i++)
							GameObject.DestroyImmediate(danglers[i]);
					}
				}

				return (T) s_Instance;
			}
		}

		/// <summary>
		/// Return true if an instance exists, false otherwise.
		/// </summary>
		/// <returns></returns>
		public static bool Valid()
		{
			return nullableInstance != null;
		}

		/// <summary>
		/// Classes overriding Awake() should be sure to call base.Awake().
		/// </summary>
		public virtual void Awake()
		{
			if(s_Instance == null)
				s_Instance = this;
			else
				GameObject.Destroy(this);
		}

		/// <summary>
		/// Classes overriding OnEnable() should be sure to call base.OnEnable().
		/// </summary>
		public virtual void OnEnable()
		{
			s_Instance = this;
		}
	}
}
