//#define PB_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [Flags]
    enum SettingVisibility
    {
        None = 0 << 0,
        /// <value>
        /// Matches any static field implementing IPref and tagged with [UserSettingAttribute(visibleInSettingsProvider = true)].
        /// </value>
        /// <summary>
        /// These fields are automatically scraped by the SettingsProvider and displayed.
        /// </summary>
        Visible = 1 << 0,

        /// <value>
        /// Matches any static field implementing IPref and tagged with [UserSettingAttribute(visibleInSettingsProvider = false)].
        /// </value>
        /// <summary>
        /// These fields will be reset by the "Reset All" menu in SettingsProvider, but are not shown in the interface.
        /// Typically these fields require some conditional formatting or data handling, and are shown in the
        /// SettingsProvider UI with a [UserSettingBlockAttribute].
        /// </summary>
        Hidden = 1 << 1,

        /// <value>
        /// A static or instance field tagged with [SettingsKeyAttribute].
        /// </value>
        /// <summary>
        /// Unlisted settings are not shown in the SettingsProvider, but are reset to default values by the "Reset All"
        /// context menu.
        /// </summary>
        Unlisted = 1 << 2,

        /// <value>
        /// A static field implementing IPref that is not marked with any setting attribute.
        /// </value>
        /// <summary>
        /// Unregistered IPref fields are not affected by the SettingsProvider.
        /// </summary>
        Unregistered = 1 << 3
    }

    [AttributeUsage(AttributeTargets.Field)]
    sealed class UserSettingAttribute : Attribute
    {
        string m_Category;
        GUIContent m_Title;
        bool m_VisibleInSettingsProvider;

        public string category
        {
            get { return m_Category; }
        }

        public GUIContent title
        {
            get { return m_Title; }
        }

        public bool visibleInSettingsProvider
        {
            get { return m_VisibleInSettingsProvider; }
        }

        public UserSettingAttribute()
        {
            m_VisibleInSettingsProvider = false;
        }

        public UserSettingAttribute(string category, string title, string tooltip = null)
        {
            m_Category = category;
            m_Title = new GUIContent(title, tooltip);
            m_VisibleInSettingsProvider = true;
        }
    }

    /// <summary>
    /// Register a field with Settings, but do not automatically create a property field in the SettingsProvider.
    /// Unlike UserSettingAttribute, this attribute is valid for instance properties as well as static.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    sealed class SettingsKeyAttribute : Attribute
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

        public SettingsKeyAttribute(string key, Settings.Scope scope = Settings.Scope.Project)
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
        object GetDefaultValue();
        void SetValue(object value, bool saveProjectSettingsImmediately = false);

        /// <summary>
        /// Set the current value back to the default.
        /// </summary>
        /// <param name="saveProjectSettingsImmediately">True to immediately re-serialize project settings.</param>
        void Reset(bool saveProjectSettingsImmediately = false);

        /// <summary>
        /// Delete the saved setting. Does not clear the current value.
        /// </summary>
        /// <see cref="Reset"/>
        /// <param name="saveProjectSettingsImmediately">True to immediately re-serialize project settings.</param>
        void Delete(bool saveProjectSettingsImmediately = false);
    }

    sealed class Pref<T> : IPref
    {
        bool m_Initialized;
        string m_Key;
        T m_Value;
        T m_DefaultValue;
        Settings.Scope m_Scope;

        Pref() { }

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

        public object GetDefaultValue()
        {
            return ValueWrapper<T>.DeepCopy(m_DefaultValue);
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
                throw new ArgumentException("Value must be of type " + typeof(T) + "\n" + key + " expecting value of type " + type +", received " + value.GetType());
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

        public void Reset(bool saveProjectSettingsImmediately = false)
        {
            value = defaultValue;

            if(saveProjectSettingsImmediately)
                Settings.Save();
        }

        public T defaultValue
        {
            get { return ValueWrapper<T>.DeepCopy(m_DefaultValue); }
        }

        public T value
        {
            get
            {
                if (!m_Initialized)
                {
                    m_Initialized = true;

                    // DeepCopy uses EditorJsonUtility which is not permitted during construction
                    m_DefaultValue = ValueWrapper<T>.DeepCopy(m_Value);

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
        public static IEnumerable<IPref> FindUserSettings(SettingVisibility visibility, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            var loadedTypes = typeof(UserSettings).Assembly.GetTypes();
            var loadedFields = loadedTypes.SelectMany(x => x.GetFields(flags));
            var settings = new List<IPref>();

            if ((visibility & (SettingVisibility.Visible | SettingVisibility.Unlisted)) > 0)
            {
                var attributes = loadedFields.Where(prop => Attribute.IsDefined(prop, typeof(UserSettingAttribute)));

                foreach (var field in attributes)
                {
                    var userSetting = (UserSettingAttribute) Attribute.GetCustomAttribute(field, typeof(UserSettingAttribute));

                    if (!field.IsStatic || !typeof(IPref).IsAssignableFrom(field.FieldType))
                    {
                        Log.Error("[UserSetting] is only valid on static fields of a type implementing `interface IPref`. \"" + field.Name + "\" (" + field.FieldType + ")\n" + field.DeclaringType);
                        continue;
                    }

                    bool visible = userSetting.visibleInSettingsProvider;

                    if (visible && (visibility & SettingVisibility.Visible) == SettingVisibility.Visible)
                        settings.Add((IPref)field.GetValue(null));
                    else if (!visible && (visibility & SettingVisibility.Hidden) == SettingVisibility.Hidden)
                        settings.Add((IPref)field.GetValue(null));
                }
            }

            if ((visibility & SettingVisibility.Unlisted) == SettingVisibility.Unlisted)
            {
                var settingsKeys = loadedFields.Where(y => Attribute.IsDefined(y, typeof(SettingsKeyAttribute)));

                foreach (var field in settingsKeys)
                {
                    if (field.IsStatic)
                    {
                        settings.Add((IPref)field.GetValue(null));
                    }
                    else
                    {
                        var settingAttribute = (SettingsKeyAttribute) Attribute.GetCustomAttribute(field, typeof(SettingsKeyAttribute));
                        var pref = CreateGenericPref(settingAttribute.key, settingAttribute.scope, field);
                        if (pref != null)
                            settings.Add(pref);
                        else
                            Log.Warning("Failed adding [SettingsKey] " + field.FieldType + "\"" + settingAttribute.key + "\" in " +  field.DeclaringType);
                    }
                }
            }

            if ((visibility & SettingVisibility.Unregistered) == SettingVisibility.Unregistered)
            {
                var unregisterd = loadedFields.Where(y => typeof(IPref).IsAssignableFrom(y.FieldType)
                    && !Attribute.IsDefined(y, typeof(SettingsKeyAttribute))
                    && !Attribute.IsDefined(y, typeof(UserSettingAttribute)) );

                foreach (var field in unregisterd)
                {
                    if (field.IsStatic)
                    {
                        settings.Add((IPref)field.GetValue(null));
                    }
                    else
                    {
#if PB_DEBUG
                        Log.Warning("Found unregistered instance field: "
                            + field.FieldType
                            + " "
                            + field.Name
                            + " in " + field.DeclaringType);
#endif
                    }
                }
            }

            return settings;
        }

        static IPref CreateGenericPref(string key, Settings.Scope scope, FieldInfo field)
        {
            try
            {
                var type = field.FieldType;
                if (typeof(IPref).IsAssignableFrom(type) && type.IsGenericType)
                    type = type.GetGenericArguments().FirstOrDefault();
                var genericPrefClass = typeof(Pref<>).MakeGenericType(type);
                var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
                return (IPref) Activator.CreateInstance(genericPrefClass, new object[] { key, defaultValue, scope });
            }
            catch
            {
                return null;
            }
        }
    }
}