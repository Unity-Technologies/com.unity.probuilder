using System;

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

        public UserSettingBlockAttribute(string category, params string[] keywords)
        {
            m_Category = category;
            m_Keywords = keywords;
        }
    }

    interface IPref
    {
        string key { get; }
        Type type { get; }

        object GetValue();
        void SetValue(object value);
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

        public void SetValue(object value)
        {
            // we do want to allow null values
            if(value != null && !(value is T))
                throw new ArgumentException("Value must be of type " + typeof(T));
            this.value = (T) value;
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

            set
            {
                if (Equals(m_Value, value))
                    return;

                m_Value = value;
                Settings.Set<T>(key, m_Value, m_Scope);
            }
        }

        public static implicit operator T(Pref<T> pref)
        {
            return pref.value;
        }
    }
}