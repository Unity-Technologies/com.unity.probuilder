using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Serialized dictionary object.
	 */
	public class pb_DictionaryObject<T, K> : ISerializationCallbackReceiver,
		IEnumerable,
		IEnumerable<KeyValuePair<T, K>>
	{
		public Dictionary<T, K> dictionary = new Dictionary<T, K>();

		[SerializeField] private T[] m_Keys;
		[SerializeField] private K[] m_Values;

		public void OnBeforeSerialize()
		{
			m_Keys = dictionary.Keys.ToArray();
			m_Values = dictionary.Values.ToArray();
		}

		public void OnAfterDeserialize()
		{
			dictionary = new Dictionary<T, K>();

			for(int i = 0; i < System.Math.Min(m_Keys.Length, m_Values.Length); i++)
				dictionary.Add(m_Keys[i], m_Values[i]);

			m_Keys = null;
			m_Values = null;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<KeyValuePair<T, K>> GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		public void Set(T key, K value)
		{
			if(dictionary.ContainsKey(key))
				dictionary[key] = value;
			else
				dictionary.Add(key, value);
		}
	}
}
