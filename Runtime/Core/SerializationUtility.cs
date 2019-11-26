using System.Reflection;
using System;

namespace UnityEngine.ProBuilder
{
    static class SerializationUtility
    {
        // todo Cache reflection
        public static void RegisterDrivenProperty(Object driver, Object target, string property)
        {
#if UNITY_2020_1_OR_NEWER && RUNTIME_VISIBLE_TO_PROBUILDER_LANDED
            DrivenPropertyManager.RegisterProperty(driver, target, property);
#else
            Type drivenPropertyManager = typeof(Transform).Assembly.GetType("UnityEngine.DrivenPropertyManager");
            MethodInfo registerProperty = drivenPropertyManager.GetMethod("RegisterProperty", BindingFlags.Public | BindingFlags.Static);
            registerProperty.Invoke(null, new object[] { driver, target, property });
#endif
        }

        public static void UnregisterDrivenProperty(Object driver, Object target, string property)
        {
#if UNITY_2020_1_OR_NEWER && RUNTIME_VISIBLE_TO_PROBUILDER_LANDED
            DrivenPropertyManager.UnregisterProperty(driver, target, property);
#else
            Type drivenPropertyManager = typeof(Transform).Assembly.GetType("UnityEngine.DrivenPropertyManager");
            MethodInfo unregisterProperty = drivenPropertyManager.GetMethod("UnregisterProperty", BindingFlags.Public | BindingFlags.Static);
            unregisterProperty.Invoke(null, new object[] { driver, target, property });
#endif
        }
    }
}
