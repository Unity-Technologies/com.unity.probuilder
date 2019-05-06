using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.SettingsManagement
{
    /// <summary>
    /// The Settings class is provides an interface for getting and setting preference values. It manages the available
    /// `ISettingsRepository` instances.
    /// </summary>
    public sealed class Settings
    {
        ISettingsRepository[] m_SettingsRepositories;

        /// <value>
        /// An event that is raised prior to an `ISettingsRepository` serializing it's current state.
        /// </value>
        public event Action beforeSettingsSaved;

        /// <value>
        /// An event that is raised after an `ISettingsRepository` has serialized it's current state.
        /// </value>
        public event Action afterSettingsSaved;

        Settings()
        {
        }

        /// <summary>
        /// Create a new Settings instance with a collection of settings repositories.
        /// </summary>
        /// <param name="repositories"></param>
        public Settings(IEnumerable<ISettingsRepository> repositories)
        {
            m_SettingsRepositories = repositories.ToArray();
        }

        /// <summary>
        /// Find a settings repository that matches the requested scope.
        /// </summary>
        /// <param name="scope">The scope of the settings repository to match.</param>
        /// <returns>
        /// An ISettingsRepository instance that is implementing the requested scope. May return null if no
        /// matching repository is found.
        /// </returns>
        public ISettingsRepository GetRepository(SettingsScope scope)
        {
            foreach (var repo in m_SettingsRepositories)
                if (repo.scope == scope)
                    return repo;
            return null;
        }

        /// <summary>
        /// Serialize the state of all settings repositories.
        /// </summary>
        public void Save()
        {
            if (beforeSettingsSaved != null)
                beforeSettingsSaved();

            foreach (var repo in m_SettingsRepositories)
                repo.Save();

            if (afterSettingsSaved != null)
                afterSettingsSaved();
        }

        /// <summary>
        /// Set a value for key of type T.
        /// </summary>
        /// <param name="key">The settings key.</param>
        /// <param name="value">The value to set. Must be serializable.</param>
        /// <param name="scope">Which scope this settings should be saved in.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        public void Set<T>(string key, T value, SettingsScope scope = SettingsScope.Project)
        {
            bool foundScopeRepository = false;

            foreach (var repo in m_SettingsRepositories)
            {
                if (repo.scope == scope)
                {
                    repo.Set<T>(key, value);
                    foundScopeRepository = true;
                }
            }

            if (!foundScopeRepository)
                Debug.LogWarning("No repository for scope " + scope + " found!");
        }

        /// <summary>
        /// Get a value with key of type T, or return the fallback value if no matching key is found.
        /// </summary>
        /// <param name="key">The settings key.</param>
        /// <param name="scope">Which scope this settings should be retrieved from.</param>
        /// <param name="fallback">If no key with a value of type T is found, this value is returned.</param>
        /// <typeparam name="T">Type of value to search for.</typeparam>
        public T Get<T>(string key, SettingsScope scope = SettingsScope.Project, T fallback = default(T))
        {
            foreach (var repo in m_SettingsRepositories)
            {
                if (repo.scope == scope)
                    return repo.Get<T>(key, fallback);
            }

            Debug.LogWarning("No repository for scope " + scope + " found!");
            return fallback;
        }

        /// <summary>
        /// Does the repository contain a setting with key and type.
        /// </summary>
        /// <param name="key">The settings key.</param>
        /// <typeparam name="T">The type of value to search for.</typeparam>
        /// <param name="scope">Which scope should be searched for matching key.</param>
        /// <returns>True if a setting matching both key and type is found, false if no entry is found.</returns>
        public bool ContainsKey<T>(string key, SettingsScope scope = SettingsScope.Project)
        {
            foreach (var repo in m_SettingsRepositories)
            {
                if (repo.scope == scope)
                    return repo.ContainsKey<T>(key);
            }

            Debug.LogWarning("No repository for scope " + scope + " found!");
            return false;
        }

        /// <summary>
        /// Remove a key value pair from a settings repository.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="scope">Which scope should be searched for matching key.</param>
        /// <typeparam name="T">The type that this key is pointing to.</typeparam>
        public void DeleteKey<T>(string key, SettingsScope scope = SettingsScope.Project)
        {
            bool foundScopeRepository = false;

            foreach (var repo in m_SettingsRepositories)
            {
                if (repo.scope == scope)
                {
                    foundScopeRepository = true;
                    repo.Remove<T>(key);
                }
            }

            if (!foundScopeRepository)
                Debug.LogWarning("No repository for scope " + scope + " found!");
        }
    }
}
