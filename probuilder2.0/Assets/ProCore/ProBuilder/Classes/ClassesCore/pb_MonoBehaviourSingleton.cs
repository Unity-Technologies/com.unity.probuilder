using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static MonoBehaviour _instance;

		public static T instance
		{
			get
			{
				if(_instance == null)
				{
					T[] danglers = Resources.FindObjectsOfTypeAll<T>();

					if(danglers == null || danglers.Length < 1)
					{
						GameObject go = new GameObject();
						go.name = typeof(T).ToString();
						_instance = go.AddComponent<T>();
					}
					else
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

		public virtual void Awake()
		{
			if(_instance == null)
				_instance = this;
			else
				GameObject.Destroy(this);
		}

		public virtual void OnEnable()
		{
			_instance = this;
		}
	}
}