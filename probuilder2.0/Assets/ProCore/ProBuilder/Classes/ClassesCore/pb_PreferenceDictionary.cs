using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/**
	 * Store settings in a project-local manner.
	 */
	public class pb_PreferenceDictionary :
		ScriptableObject,
		IEnumerable,
		ISerializationCallbackReceiver,
		pb_IHasDefault
	{
		private Dictionary<string, bool> 		m_bool 		= new Dictionary<string, bool>();
		private Dictionary<string, int> 		m_int 		= new Dictionary<string, int>();
		private Dictionary<string, float> 		m_float 	= new Dictionary<string, float>();
		private Dictionary<string, string> 		m_string 	= new Dictionary<string, string>();
		private Dictionary<string, Color> 		m_Color 	= new Dictionary<string, Color>();
		private Dictionary<string, Material> 	m_Material 	= new Dictionary<string, Material>();

		[SerializeField] List<string> 	m_bool_keys;
		[SerializeField] List<string>	m_int_keys;
		[SerializeField] List<string>	m_float_keys;
		[SerializeField] List<string>	m_string_keys;
		[SerializeField] List<string>	m_Color_keys;
		[SerializeField] List<string>	m_Material_keys;

		[SerializeField] List<bool> 	m_bool_values;
		[SerializeField] List<int> 		m_int_values;
		[SerializeField] List<float> 	m_float_values;
		[SerializeField] List<string> 	m_string_values;
		[SerializeField] List<Color> 	m_Color_values;
		[SerializeField] List<Material> m_Material_values;

		/**
		 *	Perform the ritual "Please Serialize My Dictionary" dance.
		 */
		public void OnBeforeSerialize()
		{
			m_bool_keys 		= m_bool.Keys.ToList();
			m_int_keys 			= m_int.Keys.ToList();
			m_float_keys 		= m_float.Keys.ToList();
			m_string_keys 		= m_string.Keys.ToList();
			m_Color_keys 		= m_Color.Keys.ToList();

			m_bool_values 		= m_bool.Values.ToList();
			m_int_values 		= m_int.Values.ToList();
			m_float_values 		= m_float.Values.ToList();
			m_string_values 	= m_string.Values.ToList();
			m_Color_values 		= m_Color.Values.ToList();
		}

		/**
		 *	Reconstruct preference dictionaries from serialized lists.
		 */
		public void OnAfterDeserialize()
		{
			for(int i = 0; i < m_bool_keys.Count; i++)
				m_bool.Add(m_bool_keys[i], m_bool_values[i]);

			for(int i = 0; i < m_int_keys.Count; i++)
				m_int.Add(m_int_keys[i], m_int_values[i]);

			for(int i = 0; i < m_float_keys.Count; i++)
				m_float.Add(m_float_keys[i], m_float_values[i]);

			for(int i = 0; i < m_string_keys.Count; i++)
				m_string.Add(m_string_keys[i], m_string_values[i]);

			for(int i = 0; i < m_Color_keys.Count; i++)
				m_Color.Add(m_Color_keys[i], m_Color_values[i]);
		}

		public int Length { get { return 6; } }

		// Implementation for the GetEnumerator method.
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator) GetEnumerator();
		}

		public pb_PreferenceDictionaryEnumerator GetEnumerator()
		{
			return new pb_PreferenceDictionaryEnumerator(this);
		}

		/**
		 *	Clear dictionary values.
		 */
		public void SetDefaultValues()
		{
			m_bool.Clear();
			m_int.Clear();
			m_float.Clear();
			m_string.Clear();
			m_Color.Clear();
		}

		/**
		 *	Check if a key is contained within any type dictionary.
		 */
		public bool HasKey(string key)
		{
			return 	m_bool.ContainsKey(key) ||
					m_int.ContainsKey(key) ||
					m_float.ContainsKey(key) ||
					m_string.ContainsKey(key) ||
					m_Color.ContainsKey(key);
		}

		/**
		 * Fetch a value from the stored preferences.  If key is not found, a default value is returned.
		 */
		public bool GetBool(string key, bool fallback = default(bool))
		{
			bool value;
			if(m_bool.TryGetValue(key, out value))
				return value;
			return fallback;
		}

		/**
		 * Fetch a value from the stored preferences.  If key is not found, a default value is returned.
		 */
		public int GetInt(string key, int fallback = default(int))
		{
			int value;
			if(m_int.TryGetValue(key, out value))
				return value;
			return fallback;
		}

		/**
		 * Fetch a value from the stored preferences.  If key is not found, a default value is returned.
		 */
		public float GetFloat(string key, float fallback = default(float))
		{
			float value;
			if(m_float.TryGetValue(key, out value))
				return value;
			return fallback;
		}

		/**
		 * Fetch a value from the stored preferences.  If key is not found, a default value is returned.
		 */
		public string GetString(string key, string fallback = default(string))
		{
			string value;
			if(m_string.TryGetValue(key, out value))
				return value;
			return fallback;
		}

		/**
		 * Fetch a value from the stored preferences.  If key is not found, a default value is returned.
		 */
		public Color GetColor(string key, Color fallback = default(Color))
		{
			Color value;
			if(m_Color.TryGetValue(key, out value))
				return value;
			return fallback;
		}

		/**
		 * Fetch a value from the stored preferences.  If key is not found, a default value is returned.
		 */
		public Material GetMaterial(string key, Material fallback = default(Material))
		{
			Material value;
			if(m_Material.TryGetValue(key, out value))
				return value;
			return fallback;
		}

		/**
		 *	Generic set value.  Only accepts:
		 *	int,
		 *	float,
		 *	bool,
		 *	string,
		 *	Color,
		 *	Material
		 */
		public void Set<T>(string key, T value)
		{
			object o = (object) value;

			if(value is int)
				SetInt(key, (int) o);
			else if(value is float)
				SetFloat(key, (float) o);
			else if(value is bool)
				SetBool(key, (bool) o);
			else if(value is string)
				SetString(key, (string) o);
			else if(value is Color)
				SetColor(key, (Color) o);
			else if(value is Material)
				SetMaterial(key, (Material) o);
			else
				Debug.LogWarning(string.Format("Set<{0}>({1}, {2}) not valid preference type.",
					typeof(T).ToString(),
					key,
					value.ToString()));
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		public void SetBool(string key, bool value)
		{
			if(m_bool.ContainsKey(key))
				m_bool[key] = value;
			else
				m_bool.Add(key, value);
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		public void SetInt(string key, int value)
		{
			if(m_int.ContainsKey(key))
				m_int[key] = value;
			else
				m_int.Add(key, value);
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		public void SetFloat(string key, float value)
		{
			if(m_float.ContainsKey(key))
				m_float[key] = value;
			else
				m_float.Add(key, value);
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		public void SetString(string key, string value)
		{
			if(m_string.ContainsKey(key))
				m_string[key] = value;
			else
				m_string.Add(key, value);
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		public void SetColor(string key, Color value)
		{
			if(m_Color.ContainsKey(key))
				m_Color[key] = value;
			else
				m_Color.Add(key, value);
		}

		/**
		 * Set a value for key in the saved prefs.
		 */
		public void SetMaterial(string key, Material value)
		{
			if(m_Material.ContainsKey(key))
				m_Material[key] = value;
			else
				m_Material.Add(key, value);
		}

		/**
		 *	Get the internal <key, bool> dictionary.
		 */
		public Dictionary<string, bool> GetBoolDictionary() { return m_bool; }

		/**
		 *	Get the internal <key, int> dictionary.
		 */
		public Dictionary<string, int> GetIntDictionary() { return m_int; }

		/**
		 *	Get the internal <key, float> dictionary.
		 */
		public Dictionary<string, float> GetFloatDictionary() { return m_float; }

		/**
		 *	Get the internal <key, string> dictionary.
		 */
		public Dictionary<string, string> GetStringDictionary() { return m_string; }

		/**
		 *	Get the internal <key, Color> dictionary.
		 */
		public Dictionary<string, Color> GetColorDictionary() { return m_Color; }

		/**
		 *	Get the internal <key, Material> dictionary.
		 */
		public Dictionary<string, Material> GetMaterialDictionary() { return m_Material; }

		/**
		 *	Clear all stored preference key value pairs.
		 */
		public void Clear()
		{
			m_bool.Clear();
			m_int.Clear();
			m_float.Clear();
			m_string.Clear();
			m_Color.Clear();
		}
	}
}
