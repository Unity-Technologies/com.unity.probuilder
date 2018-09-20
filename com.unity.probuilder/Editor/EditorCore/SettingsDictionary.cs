using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace UnityEngine.ProBuilder
{
	[Serializable]
	sealed class ValueWrapper<T>
	{
#if PRETTY_PRINT_JSON
		const bool k_PrettyPrintJson = true;
#else
		const bool k_PrettyPrintJson = false;
#endif

		[SerializeField]
		T m_Value;

		public static string Serialize(T value)
		{
			var obj = new ValueWrapper<T>() { m_Value = value };
			return EditorJsonUtility.ToJson(obj, k_PrettyPrintJson);
		}

		public static T Deserialize(string json)
		{
			var value = (object)Activator.CreateInstance<ValueWrapper<T>>();
			EditorJsonUtility.FromJsonOverwrite(json, value);
			return ((ValueWrapper<T>)value).m_Value;
		}
	}

	[Serializable]
	sealed class SettingsDictionary : ISerializationCallbackReceiver
	{
		[Serializable]
		struct SettingsKeyValuePair
		{
			public string type;
			public string key;
			public string value;
		}

#pragma warning disable 0649
		[SerializeField]
		List<SettingsKeyValuePair> m_DictionaryValues = new List<SettingsKeyValuePair>();
#pragma warning restore 0649

		internal Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();

		public bool ContainsKey<T>(string key)
		{
			var type = typeof(T).AssemblyQualifiedName;
			return dictionary.ContainsKey(type) && dictionary[type].ContainsKey(key);
		}

		public void Set<T>(string key, T value)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			var type = typeof(T).AssemblyQualifiedName;

			SetJson(type, key, ValueWrapper<T>.Serialize(value));
		}

		internal void SetJson(string type, string key, string value)
		{
			Dictionary<string, string> entries;

			if (!dictionary.TryGetValue(type, out entries))
				dictionary.Add(type, entries = new Dictionary<string, string>());

			if (entries.ContainsKey(key))
				entries[key] = value;
			else
				entries.Add(key, value);
		}

		public T Get<T>(string key, T fallback = default(T))
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			var type = typeof(T).AssemblyQualifiedName;
			Dictionary<string, string> entries;

			if (dictionary.TryGetValue(type, out entries) && entries.ContainsKey(key))
			{
				try
				{
					return ValueWrapper<T>.Deserialize(entries[key]);
				}
				catch
				{
					return fallback;
				}
			}

			return fallback;
		}

		public void OnBeforeSerialize()
		{
			if (m_DictionaryValues == null)
				return;

			m_DictionaryValues.Clear();

			foreach (var type in dictionary)
			{
				foreach (var entry in type.Value)
				{
					m_DictionaryValues.Add(new SettingsKeyValuePair()
					{
						type = type.Key,
						key = entry.Key,
						value = entry.Value
					});
				}
			}
		}

		public void OnAfterDeserialize()
		{
			dictionary.Clear();

			foreach (var entry in m_DictionaryValues)
			{
				Dictionary<string, string> entries;

				if (dictionary.TryGetValue(entry.type, out entries))
					entries.Add(entry.key, entry.value);
				else
					dictionary.Add(entry.type, new Dictionary<string, string>() { { entry.key, entry.value } });
			}
		}
	}
}