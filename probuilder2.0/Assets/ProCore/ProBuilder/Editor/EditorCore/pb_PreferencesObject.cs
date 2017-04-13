using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public class pb_PreferencesObject : ScriptableObject, pb_IHasDefault
	{
		public void SetDefaultValues() {}

		public pb_DictionaryObject<string, float> floatValues = new pb_DictionaryObject<string, float>();

		public pb_DictionaryObject<string, int> intValues = new pb_DictionaryObject<string, int>();

		public void Set<T>(string key, T value)
		{
			// Box value then cast. Not ideal, but the alternative is separate
			// functions for each type dictionary.  This maintains that type
			// safety while avoiding lots of duplicate code.
			object boxValue = value;

			if(typeof(T) == typeof(float))
				floatValues.Set(key, (float) boxValue);
			else if(typeof(T) == typeof(int))
				intValues.Set(key, (int) boxValue);
			else
				Debug.LogWarning(string.Format("type of \"{0}\" does not exist in preferences.", typeof(T).ToString()));
		}
	}
}
