using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Helper functions for working with Reflection.
	/// </summary>
	static class pb_Reflection
	{
		public static bool enableWarnings = true;
		const BindingFlags k_AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		static void Warning(string text)
		{
			if(enableWarnings)
				Debug.LogWarning(text);
		}

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

		/// <summary>
		/// Call a method with args.
		/// </summary>
		/// <param name="target">Target object</param>
		/// <param name="method">The name of the method to invoke</param>
		/// <param name="flags">Optional BindingFlags. If none are passed, defaults to Public|NonPublic|Static|Instance</param>
		/// <param name="args">Any additional arguments to pass to the invoked function</param>
		/// <returns></returns>
		public static object Invoke(
			object target,
			string method,
			BindingFlags flags = k_AllFlags,
			params object[] args)
		{
			if (target == null)
			{
				Warning("Invoke failed, target is null and no type was provided.");
				return null;
			}

			return Invoke(target, target.GetType(), method, null, flags, args);
		}

		public static object Invoke(
			object target,
			string type,
			string method,
			BindingFlags flags = k_AllFlags,
			string assembly = null,
			params object[] args)
		{
			Type t = GetType(type, assembly);

			if (t == null && target != null)
				t = target.GetType();

			if (t != null)
				return Invoke(target, t, method, null, flags, args);
			else
				Warning("Invoke failed, type is null: " + type);

			return null;
		}

		/// <summary>
		/// Invoke a method with arguments.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <param name="method"></param>
		/// <param name="methodParams"></param>
		/// <param name="flags"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static object Invoke(
			object target,
			Type type,
			string method,
			Type[] methodParams = null,
			BindingFlags flags = k_AllFlags,
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

			if(t == null)
			{
				Warning(string.Format("Could not find type \"{0}\"!", type));
				return null;
			}
			else
				return GetValue(target, t, member, flags);
		}

		public static object GetValue(object target, Type type, string member, BindingFlags flags = k_AllFlags)
		{
			PropertyInfo pi = type.GetProperty(member, flags);

			if(pi != null)
				return pi.GetValue(target, null);

			FieldInfo fi = type.GetField(member, flags);

			if(fi != null)
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
