using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.ProBuilder
{
	sealed class SettingsDictionary
	{
		[Serializable]
		struct SerializableWrapper<T>
		{
			[SerializeField]
			public T value;

			public SerializableWrapper(T value)
			{
				this.value = value;
			}
		}

		Dictionary<string, Dictionary<string, string>> m_Dictionary = new Dictionary<string, Dictionary<string, string>>();

		public bool ContainsKey<T>(string key)
		{
			var type = typeof(T).AssemblyQualifiedName;
			return m_Dictionary.ContainsKey(type) && m_Dictionary[type].ContainsKey(key);
		}

		public void Set<T>(string key, T value)
		{
			if(string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			var type = typeof(T).AssemblyQualifiedName;

			Dictionary<string, string> entries;

			if (!m_Dictionary.TryGetValue(type, out entries))
				m_Dictionary.Add(type, entries = new Dictionary<string, string>());
			if (entries.ContainsKey(key))
				entries[key] = EditorJsonUtility.ToJson(new SerializableWrapper<T>(value));
			else
				entries.Add(key, EditorJsonUtility.ToJson(new SerializableWrapper<T>(value)));
		}

		public T Get<T>(string key, T fallback = default)
		{
			if(string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			var type = typeof(T).AssemblyQualifiedName;
			Dictionary<string, string> entries;

			if (m_Dictionary.TryGetValue(type, out entries) && entries.ContainsKey(key))
			{
				try
				{
					// type must be boxed for FromJsonOverwrite to correctly populate object
					var value = (object) new SerializableWrapper<T>(Activator.CreateInstance<T>());
					EditorJsonUtility.FromJsonOverwrite(entries[key], value);
					return ((SerializableWrapper<T>)value).value;
				}
				catch
				{
					return fallback;
				}
			}

			return fallback;
		}
	}

	public class Settings
	{

	}
}
