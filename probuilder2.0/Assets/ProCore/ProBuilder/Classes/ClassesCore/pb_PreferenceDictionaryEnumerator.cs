using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	class pb_PreferenceDictionaryEnumerator : IEnumerator
	{
		// Enumerators are positioned before the first element
		// until the first MoveNext() call.
		private int m_Position = -1;

		private pb_PreferenceDictionary m_Preferences;

		public pb_PreferenceDictionaryEnumerator(pb_PreferenceDictionary dictionary)
		{
			m_Preferences = dictionary;
		}

		public bool MoveNext()
		{
			m_Position++;

			return (m_Position < m_Preferences.Length);
		}

		public void Reset()
		{
			m_Position = -1;
		}

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		public IEnumerable Current
		{
			get
			{
				if(m_Position == 0)
					return m_Preferences.GetBoolDictionary();
				else if(m_Position == 1)
					return m_Preferences.GetIntDictionary();
				else if(m_Position == 2)
					return m_Preferences.GetFloatDictionary();
				else if(m_Position == 3)
					return m_Preferences.GetStringDictionary();
				else if(m_Position == 4)
					return m_Preferences.GetColorDictionary();
				else if(m_Position == 5)
					return m_Preferences.GetMaterialDictionary();
				else
					throw new System.InvalidOperationException();
			}
		}
	}
}
