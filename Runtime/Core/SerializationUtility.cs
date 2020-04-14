#if ENABLE_DRIVEN_PROPERTIES
using System.Reflection;
using System;
using System.Diagnostics;

namespace UnityEngine.ProBuilder
{
    static class SerializationUtility
    {
        static MethodInfo m_RegisterProperty;
        static MethodInfo m_UnregisterProperty;

        // used by tests
        internal static MethodInfo registerProperty
        {
            get
            {
                if (m_RegisterProperty == null)
                {
                    Type drivenPropertyManager = typeof(Transform).Assembly.GetType("UnityEngine.DrivenPropertyManager");
                    m_RegisterProperty = drivenPropertyManager.GetMethod("RegisterProperty", BindingFlags.Public | BindingFlags.Static);
                }
                return m_RegisterProperty;
            }
        }

        internal static MethodInfo unregisterProperty
        {
            get
            {
                if (m_UnregisterProperty == null)
                {
                    Type drivenPropertyManager = typeof(Transform).Assembly.GetType("UnityEngine.DrivenPropertyManager");
                    m_UnregisterProperty = drivenPropertyManager.GetMethod("UnregisterProperty", BindingFlags.Public | BindingFlags.Static);
                }
                return m_UnregisterProperty;
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void RegisterDrivenProperty(Object driver, Object target, string property)
        {
#if UNITY_2020_1_OR_NEWER && RUNTIME_VISIBLE_TO_PROBUILDER_LANDED
            DrivenPropertyManager.RegisterProperty(driver, target, property);
#else
            registerProperty.Invoke(null, new object[] { driver, target, property });
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void UnregisterDrivenProperty(Object driver, Object target, string property)
        {
#if UNITY_2020_1_OR_NEWER && RUNTIME_VISIBLE_TO_PROBUILDER_LANDED
            DrivenPropertyManager.UnregisterProperty(driver, target, property);
#else
            unregisterProperty.Invoke(null, new object[] { driver, target, property });
#endif
        }
    }
}
#endif
