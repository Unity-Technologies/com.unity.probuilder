using System.Reflection;
using System;

namespace UnityEngine.ProBuilder
{
    static class SerializationUtility
    {
        public static void RegisterDrivenProperty(Object driver, Object target, string property)
        {
            Type drivenPropertyManager = typeof(Transform).Assembly.GetType("UnityEngine.DrivenPropertyManager");
            MethodInfo registerProperty = drivenPropertyManager.GetMethod("RegisterProperty", BindingFlags.Public | BindingFlags.Static);
            registerProperty.Invoke(null, new object[] { driver, target, property });
        }
    }
}
