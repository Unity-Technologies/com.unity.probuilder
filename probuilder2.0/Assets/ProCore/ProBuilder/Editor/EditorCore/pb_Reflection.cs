using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/// <summary>
	/// Helper functions for working with Reflection.
	/// </summary>
	static class pb_Reflection
	{
		public static bool enableWarnings = true;
		const BindingFlags ALL_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static void Warning(string text)
		{
			if(enableWarnings)
				Debug.LogWarning(text);
		}

		/**
		 *	Get a component with type name.
		 */
		public static object GetComponent(this GameObject gameObject, string componentTypeName)
		{
			return gameObject.GetComponent(componentTypeName);
		}

		/**
		 *	Fetch a type with name and optional assembly name.  `type` should include namespace.
		 */
		public static Type GetType(string type, string assembly = null)
		{
			Type t = Type.GetType(type);

			if(t == null)
			{
				IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();

				if(assembly != null)
					assemblies = assemblies.Where(x => x.FullName.Contains(assembly));

				foreach(Assembly ass in assemblies)
				{
					t = ass.GetType(type);

					if(t != null)
						return t;
				}
			}

			return t;
		}

		/**
		 *	Call a method with args.
		 */
		public static object Invoke(object target,
									string method,
									BindingFlags flags = ALL_FLAGS,
									params object[] args)
		{
			if(target == null)
			{
				Warning("Invoke failed, target is null and no type was provided.");
				return null;
			}

			return Invoke(target, target.GetType(), method, null, flags, args);
		}

		public static object Invoke(object target,
									string type,
									string method,
									BindingFlags flags = ALL_FLAGS,
									string assembly = null,
									params object[] args)
		{
			Type t = GetType(type, assembly);

			if(t == null && target != null)
				t = target.GetType();

			if(t != null)
				return Invoke(target, t, method, null, flags, args);
			else
				Warning("Invoke failed, type is null: " + type);

			return null;
		}

		public static object Invoke(object target,
									Type type,
									string method,
									Type[] methodParams = null,
									BindingFlags flags = ALL_FLAGS,
									params object[] args)
		{
			MethodInfo mi = null;

			if(methodParams == null)
				mi = type.GetMethod(method, flags);
			else
				mi = type.GetMethod(method, flags, null, methodParams, null);

			if(mi == null)
			{
				Warning("Failed to find method " + method + " in type " + type);
				return null;
			}

			return mi.Invoke(target, args);
		}

		/**
		 *	Fetch a value using GetProperty or GetField.
		 */
		public static object GetValue(object target, string type, string member, BindingFlags flags = ALL_FLAGS)
		{
			Type t = GetType(type);

			if(t == null)
			{
				Warning(string.Format("Could not find type \"{0}\"!", type));
				return null;
			}
			else
				return GetValue(target, t, member, flags);
		}

		public static object GetValue(object target, Type type, string member, BindingFlags flags = ALL_FLAGS)
		{
			PropertyInfo pi = type.GetProperty(member, flags);

			if(pi != null)
				return pi.GetValue(target, null);

			FieldInfo fi = type.GetField(member, flags);

			if(fi != null)
				return fi.GetValue(target);

			return null;
		}

		/**
		 *	Set a propery or field.
		 */
		public static bool SetValue(object target, string member, object value, BindingFlags flags = ALL_FLAGS)
		{
			if(target == null)
				return false;

			PropertyInfo pi = target.GetType().GetProperty(member, flags);

			if(pi != null)
				pi.SetValue(target, value, flags, null, null, null);

			FieldInfo fi = target.GetType().GetField(member, flags);

			if(fi != null)
				fi.SetValue(target, value);

			return pi != null || fi != null;
		}
	}
}
