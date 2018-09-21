using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.ProBuilder
{
    [AttributeUsage(AttributeTargets.Field)]
    sealed class UserSettingAttribute : Attribute
    {
        string m_Category;
        GUIContent m_Title;

        public string category
        {
            get { return m_Category; }
        }

        public GUIContent title
        {
            get { return m_Title; }
        }

        public UserSettingAttribute(string category, string title, string tooltip = null)
        {
            m_Category = category;
            m_Title = new GUIContent(title, tooltip);
        }
    }

    /// <summary>
    /// Register a Pref<T> with Settings, but do not automatically create a property field in the SettingsProvider.
    /// Unlike UserSettingAttribute, this attribute is valid for instance properties as well as static.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    sealed class HiddenSettingAttribute : Attribute
    {
        string m_Key;
        Settings.Scope m_Scope;

        public string key
        {
            get { return m_Key; }
        }

        public Settings.Scope scope
        {
            get { return m_Scope; }
        }

        public HiddenSettingAttribute(string key, Settings.Scope scope = Settings.Scope.Project)
        {
            m_Key = key;
            m_Scope = scope;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    sealed class UserSettingBlockAttribute : Attribute
    {
        string m_Category;
        string[] m_Keywords;

        public string category
        {
            get { return m_Category; }
        }

        public string[] keywords
        {
            get { return m_Keywords; }
        }

        public UserSettingBlockAttribute(string category, string[] keywords = null)
        {
            m_Category = category;
            m_Keywords = keywords;
        }
    }

    interface IPref
    {
        string key { get; }
        Type type { get; }
        Settings.Scope scope { get; }

        object GetValue();
        void SetValue(object value, bool saveProjectSettingsImmediately = false);
        void Delete(bool saveProjectSettingsImmediately = false);
    }

    sealed class Pref<T> : IPref
    {
        bool m_Initialized;
        string m_Key;
        T m_Value;
        Settings.Scope m_Scope;

        public Pref(string key, T value, Settings.Scope scope = Settings.Scope.Project)
        {
            m_Key = key;
            m_Value = value;
            m_Scope = scope;
        }

        public string key
        {
            get { return m_Key; }
        }

        public Type type
        {
            get { return typeof(T); }
        }

        public object GetValue()
        {
            return value;
        }

        public Settings.Scope scope
        {
            get { return m_Scope; }
        }

        public void SetValue(object value, bool saveProjectSettingsImmediately = false)
        {
            // we do want to allow null values
            if(value != null && !(value is T))
                throw new ArgumentException("Value must be of type " + typeof(T));
            SetValue((T) value, saveProjectSettingsImmediately);
        }

        public void SetValue(T value, bool saveProjectSettingsImmediately = false)
        {
            if (Equals(m_Value, value))
                return;

            m_Value = value;

            Settings.Set<T>(key, m_Value, m_Scope);

            if (m_Scope == Settings.Scope.Project && saveProjectSettingsImmediately)
                Settings.Save();
        }

        public void Delete(bool saveProjectSettingsImmediately = false)
        {
            Settings.Delete<T>(key, scope);

            if (saveProjectSettingsImmediately)
                Settings.Save();
        }

        public T value
        {
            get
            {
                if (!m_Initialized)
                {
                    m_Initialized = true;

                    if (Settings.ContainsKey<T>(m_Key, m_Scope))
                        m_Value = Settings.Get<T>(m_Key, m_Scope);
                }

                return m_Value;
            }

            set { SetValue(value); }
        }

        public static implicit operator T(Pref<T> pref)
        {
            return pref.value;
        }
    }

    static class UserSettings
    {
        /// <summary>
        /// Collect all registered UserSetting and HiddenSetting attributes.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IPref> FindUserSettings()
        {
            var attribs = typeof(UserSettingAttribute).Assembly.GetTypes()
                .SelectMany(x => x.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(prop => Attribute.IsDefined(prop, typeof(UserSettingAttribute)) || Attribute.IsDefined(prop, typeof(HiddenSettingAttribute))));

            List<IPref> preferences = new List<IPref>(attribs.Count());

            foreach (var field in attribs)
            {
                if (field.IsStatic && typeof(IPref).IsAssignableFrom(field.FieldType))
                {
                    preferences.Add((IPref)field.GetValue(null));
                    continue;
                }
            }

            return preferences;
        }
    }
}