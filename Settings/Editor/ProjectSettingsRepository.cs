using System;
using System.IO;
using UnityEngine;

namespace UnityEditor.SettingsManagement
{
    /// <summary>
    /// A settings repository that stores data to a JSON file.
    /// </summary>
    [Serializable]
    public class ProjectSettingsRepository : ISettingsRepository
    {
        const bool k_PrettyPrintJson = true;
        string m_Path;
        bool m_Initialized;

        [SerializeField]
        SettingsDictionary m_Dictionary = new SettingsDictionary();

        /// <summary>
        /// Constructor sets the serialized data path.
        /// </summary>
        /// <param name="path">The path to which settings will be saved in JSON format.</param>
        public ProjectSettingsRepository(string path)
        {
            m_Path = path;
            m_Initialized = false;
        }

        void Init()
        {
            if (m_Initialized)
                return;

            m_Initialized = true;

            if (File.Exists(path))
            {
                m_Dictionary = null;
                var json = File.ReadAllText(path);
                EditorJsonUtility.FromJsonOverwrite(json, this);
            }
        }

        /// <value>
        /// This repository implementation is relevant to the Project scope.
        /// </value>
        /// <inheritdoc cref="ISettingsRepository.scope"/>
        public SettingsScope scope
        {
            get { return SettingsScope.Project; }
        }

        /// <value>
        /// File path to the serialized settings data.
        /// </value>
        /// <inheritdoc cref="ISettingsRepository.path"/>
        public string path
        {
            get { return m_Path; }
        }

        /// <summary>
        /// Save all settings to their serialized state.
        /// </summary>
        /// <inheritdoc cref="ISettingsRepository.Save"/>
        public void Save()
        {
            Init();
            File.WriteAllText(path, EditorJsonUtility.ToJson(this, k_PrettyPrintJson));
        }

        /// <summary>
        /// Set a value for key of type T.
        /// </summary>
        /// <param name="key">The settings key.</param>
        /// <param name="value">The value to set. Must be serializable.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <inheritdoc cref="ISettingsRepository.Set{T}"/>
        public void Set<T>(string key, T value)
        {
            Init();
            m_Dictionary.Set<T>(key, value);
        }

        /// <summary>
        /// Get a value with key of type T, or return the fallback value if no matching key is found.
        /// </summary>
        /// <param name="key">The settings key.</param>
        /// <param name="fallback">If no key with a value of type T is found, this value is returned.</param>
        /// <typeparam name="T">Type of value to search for.</typeparam>
        /// <inheritdoc cref="ISettingsRepository.Get{T}"/>
        public T Get<T>(string key, T fallback = default(T))
        {
            Init();
            return m_Dictionary.Get<T>(key, fallback);
        }

        /// <summary>
        /// Does the repository contain a setting with key and type.
        /// </summary>
        /// <param name="key">The settings key.</param>
        /// <typeparam name="T">The type of value to search for.</typeparam>
        /// <returns>True if a setting matching both key and type is found, false if no entry is found.</returns>
        /// <inheritdoc cref="ISettingsRepository.ContainsKey{T}"/>
        public bool ContainsKey<T>(string key)
        {
            Init();
            return m_Dictionary.ContainsKey<T>(key);
        }

        /// <summary>
        /// Remove a key value pair from the settings repository.
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <inheritdoc cref="ISettingsRepository.Remove{T}"/>
        public void Remove<T>(string key)
        {
            Init();
            m_Dictionary.Remove<T>(key);
        }
    }
}
