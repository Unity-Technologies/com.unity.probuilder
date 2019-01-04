using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Helper functions for working with Reflection.
    /// </summary>
    static class ReflectionUtility
    {
        const BindingFlags k_AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Get a component with type name.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentTypeName"></param>
        /// <returns></returns>
        public static object GetComponent(this GameObject gameObject, string componentTypeName)
        {
            return gameObject.GetComponent(componentTypeName);
        }

        /// <summary>
        /// Fetch a type with name and optional assembly name. `type` should include namespace.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Type GetType(string type, string assembly = null)
        {
            Type t = Type.GetType(type);

            if (t == null)
            {
                IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();

                if (assembly != null)
                    assemblies = assemblies.Where(x => x.FullName.Contains(assembly));

                foreach (Assembly ass in assemblies)
                {
                    t = ass.GetType(type);

                    if (t != null)
                        return t;
                }
            }

            return t;
        }

        /// <summary>
        /// Fetch a value using GetProperty or GetField.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="member"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static object GetValue(object target, string type, string member, BindingFlags flags = k_AllFlags)
        {
            Type t = GetType(type);

            if (t == null)
            {
                Log.Warning("Could not find type \"{0}\"!", type);
                return null;
            }
            else
                return GetValue(target, t, member, flags);
        }

        public static object GetValue(object target, Type type, string member, BindingFlags flags = k_AllFlags)
        {
            PropertyInfo pi = type.GetProperty(member, flags);

            if (pi != null)
                return pi.GetValue(target, null);

            FieldInfo fi = type.GetField(member, flags);

            if (fi != null)
                return fi.GetValue(target);

            return null;
        }

        /// <summary>
        /// Set a propery or field.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool SetValue(object target, string member, object value, BindingFlags flags = k_AllFlags)
        {
            if (target == null)
                return false;

            PropertyInfo pi = target.GetType().GetProperty(member, flags);

            if (pi != null)
                pi.SetValue(target, value, flags, null, null, null);

            FieldInfo fi = target.GetType().GetField(member, flags);

            if (fi != null)
                fi.SetValue(target, value);

            return pi != null || fi != null;
        }

        static public Delegate GetOpenDelegate<T>(Type type, string methodName)
        {
            MethodInfo methodInfo = type.GetMethod(methodName);
            if (methodInfo != null)
                return Delegate.CreateDelegate(typeof(T), methodInfo);
            else
            {
                Log.Warning("Couldn't get method '{0}' from type {1}", methodName, type.ToString());
            }
            return null;
        }

        static public Delegate GetOpenDelegate<T>(Type type, string methodName, BindingFlags bindings)
        {
            MethodInfo methodInfo = type.GetMethod(methodName, bindings);
            if (methodInfo != null)
                return Delegate.CreateDelegate(typeof(T), methodInfo);
            else
            {
                Log.Warning("Couldn't get method '{0}' from type {1}", methodName, type.ToString());
            }

            return null;
        }

        static public Delegate GetOpenDelegateOnProperty<T>(Type type, string propertyName)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo != null)
                return Delegate.CreateDelegate(typeof(T), propertyInfo.GetGetMethod());
            else
            {
                Log.Warning("Couldn't get property '{0}' from type {1}", propertyName, type.ToString());
            }
            return null;
        }

        static public Delegate GetOpenDelegateOnProperty<T>(Type type, string propertyName, BindingFlags bindings)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, bindings);
            MethodInfo methodInfo = propertyInfo.GetGetMethod(true);

            if (methodInfo != null)
                return Delegate.CreateDelegate(typeof(T), methodInfo);//propertyInfo.GetGetMethod());
            else
            {
                Log.Warning("Couldn't get property '{0}' from type {1}", propertyName, type.ToString());
            }
            return null;
        }

        static public Delegate GetClosedDelegateOnProperty<T>(Type type, object target, string propertyName)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo != null)
                return Delegate.CreateDelegate(typeof(T), target, propertyInfo.GetGetMethod());
            else
            {
                Log.Warning("Couldn't get property '{0}' from type {1}", propertyName, type.ToString());
            }
            return null;
        }

        static public Delegate GetClosedDelegateOnProperty<T>(Type type, object target, string propertyName, BindingFlags bindings)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, bindings);
            MethodInfo methodInfo = propertyInfo.GetGetMethod(true);

            if (methodInfo != null)
                return Delegate.CreateDelegate(typeof(T), target, methodInfo);
            else
            {
                Log.Warning("Couldn't get property '{0}' from type {1}", propertyName, type.ToString());
            }
            return null;
        }

        static public FieldInfo GetFieldInfo(Type type, string fieldName, BindingFlags bindings)
        {
            FieldInfo fieldInfo = type.GetField(fieldName, bindings);

            if (fieldInfo != null)
                return fieldInfo;
            else
            {
                Log.Warning("Couldn't get field '{0}' from type {1}", fieldName, type.ToString());
            }
            return null;
        }
    }
}
