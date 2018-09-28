using System;
using UnityEngine;

namespace UnityEditor.SettingsManagement
{
	[AttributeUsage(AttributeTargets.Field)]
    public sealed class UserSettingAttribute : Attribute
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
    public sealed class SettingsKeyAttribute : Attribute
    {
        string m_Key;
        SettingScope m_Scope;

        public string key
        {
            get { return m_Key; }
        }

        public SettingScope scope
        {
            get { return m_Scope; }
        }

        public SettingsKeyAttribute(string key, SettingScope scope = SettingScope.Project)
        {
            m_Key = key;
            m_Scope = scope;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UserSettingBlockAttribute : Attribute
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
}