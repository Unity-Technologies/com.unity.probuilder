using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	[Serializable]
	public class pb_PreferencesDictionaryEntry : pb_DictionaryObject<string, pb_PrefValue> {}

	[Serializable]
	public class pb_PreferencesDictionary : pb_DictionaryObject<Type, pb_PreferencesDictionaryEntry> {}

	public class pb_PreferencesObject : ScriptableObject,
		pb_IHasDefault,
		ISerializationCallbackReceiver,
		IEnumerable,
		System.Collections.Generic.IEnumerable<KeyValuePair<Type, pb_PreferencesDictionaryEntry>>
	{
		public void SetDefaultValues() {}

		[SerializeField] pb_PreferencesDictionary m_PreferencesByType = new pb_PreferencesDictionary();

		public void OnBeforeSerialize()
		{
			Debug.Log("serialize");
		}

		public void OnAfterDeserialize()
		{
			Debug.Log("de-serialize");
		}

		public void Set<T>(string key, T value)
		{
			pb_PreferencesDictionaryEntry prefs;

			if(!m_PreferencesByType.dictionary.TryGetValue(typeof(T), out prefs))
			{
				prefs = new pb_PreferencesDictionaryEntry();
				m_PreferencesByType.dictionary.Add(typeof(T), prefs);
			}

			prefs.Set(key, new pb_PrefValue(value));

			// Box value then cast. Not ideal, but the alternative is separate
			// functions for each type dictionary.  This maintains that type
			// safety while avoiding lots of duplicate code.
			// object boxValue = value;

			// if(typeof(T) == typeof(float))
			// 	floatValues.Set(key, (float) boxValue);
			// else if(typeof(T) == typeof(int))
			// 	intValues.Set(key, (int) boxValue);
			// else

			// Debug.LogWarning(string.Format("type of \"{0}\" does not exist in preferences.", typeof(T).ToString()));
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<KeyValuePair<Type, pb_PreferencesDictionaryEntry>> GetEnumerator()
		{
			return m_PreferencesByType.dictionary.GetEnumerator();
		}

	}
}
