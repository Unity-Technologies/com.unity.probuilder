using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A series of handy extensions.
	/// todo Clean up and move to appropriate classes.
	/// </summary>
	static class InternalUtility
	{
		/**
		 *	\brief Returns all components present in an array of GameObjects.  Deep search.
		 *	@param _gameObjects GameObject array to search for specified components.
		 *	Generic method.  Usage ex.
		 *	\code{cs}
		 *	GameObject[] gos = new GameObject[10];
		 *	for(int i = 0; i < gos.Length; i++)
		 *	{
		 *		// Create 10 new pb_Objects
		 *		gos[i] = ProBuilder.CreatePrimitive(ProBuilder.Shape.Cube).gameObject;
		 *
		 *		// Line them up nicely
		 *		gos[i].transform.position = new Vector3((i * 2) - (gos.Length/2), 0f, 0f);
		 *	}
		 *
		 *	// Get all pb_Objects from GameObject array
		 *	pb_Object[] pb = gos.GetComponents<pb_Object>();
		 *
		 *	// Now do move their vertices up or down some random amount
		 *	for(int i = 0; i < pb.Length; i++)
		 *	{
		 *		pb.TranslateVertices(new Vector3(0f, Randome.Range(-3f, 3f), 0f));
		 *	}
		 *	\endcode
		 *	\sa GetComponents<Transform[]>()
		 *	\returns Array of T.
		 */
		public static T[] GetComponents<T>(this IEnumerable<GameObject> gameObjects) where T : Component
		{
			List<T> c = new List<T>();
			foreach(GameObject go in gameObjects)
				c.AddRange(go.transform.GetComponentsInChildren<T>());
			return c.ToArray();
		}

		/**
		 *	\brief Returns all components present in a given GameObject.  Conducts a deep search.
		 *	@param _gameObject GameObject to search for specified components.
		 *	\returns Array of T.
		 */
		public static T[] GetComponents<T>(GameObject go) where T : Component
		{
			return go.transform.GetComponentsInChildren<T>();
		}

		/**
		 *	\brief Returns all components present in a Transform array.  Deep search.
		 *	@param _transforms Transform array to search for specified components.
		 *	Generic method.  Usage ex.
		 *	\code{cs}
		 *	Transform[] t_arr = new Transform[10];
		 *	for(int i = 0; i < gos.Length; i++)
		 *	{
		 *		// Create 10 new pb_Objects
		 *		t_arr[i] = ProBuilder.CreatePrimitive(ProBuilder.Shape.Cube).transform;
		 *
		 *
		 *		// Line them up nicely
		 *		t_arr[i].position = new Vector3((i * 2) - (gos.Length/2), 0f, 0f);
		 *	}
		 *
		 *	// Get all pb_Objects from GameObject array
		 *	pb_Object[] pb = t_arr.GetComponents<pb_Object>();
		 *
		 *	// Now do move their vertices up or down some random amount
		 *	for(int i = 0; i < pb.Length; i++)
		 *	{
		 *		pb.TranslateVertices(new Vector3(0f, Randome.Range(-3f, 3f), 0f));
		 *	}
		 *
		 *	\endcode
		 *	\returns Array of T.
		 */
		public static T[] GetComponents<T>(this IEnumerable<Transform> transforms) where T : Component
		{
			List<T> c = new List<T>();
			foreach(Transform t in transforms)
				c.AddRange(t.GetComponentsInChildren<T>());
			return c.ToArray() as T[];
		}

		public static GameObject EmptyGameObjectWithTransform(Transform t)
		{
			GameObject go 					= new GameObject();
			go.transform.position 			= t.position;
			go.transform.localRotation 		= t.localRotation;
			go.transform.localScale 		= t.localScale;
			return go;
		}

		public static T NextEnumValue<T>(this T current) where T : IConvertible
		{
			Assert.IsTrue(current is Enum);

			var values = Enum.GetValues(current.GetType());

			for(int i = 0, c = values.Length; i < c; i++)
				if(current.Equals(values.GetValue((i))))
					return (T) values.GetValue((i+1)%c);

			return current;
		}

		public static string ControlKeyString(char character)
		{
			if( character == PreferenceKeys.CMD_SUPER )
				return "Control";
			else if( character == PreferenceKeys.CMD_SHIFT )
				return "Shift";
			else if( character == PreferenceKeys.CMD_OPTION )
				return "Alt";
			else if( character == PreferenceKeys.CMD_ALT )
				return "Alt";
			else if( character == PreferenceKeys.CMD_DELETE )
				return "Delete";
			else
				return character.ToString();
		}

		/**
		 *	Attempt to parse a color from string input.
		 */
		public static bool TryParseColor(string value, ref Color col)
		{
			string valid = "01234567890.,";
			value = new string(value.Where(c => valid.Contains(c)).ToArray());
			string[] rgba = value.Split(',');

			if(rgba.Length < 4)
				return false;

			try
			{
				float r = float.Parse(rgba[0]);
				float g = float.Parse(rgba[1]);
				float b = float.Parse(rgba[2]);
				float a = float.Parse(rgba[3]);

				col.r = r;
				col.g = g;
				col.b = b;
				col.a = a;
			}
			catch
			{
				return false;
			}

			return true;
		}

		/**
		 *	\brief Convert a string to a Vector3 array.
		 ()
		 *	@param str A string formatted like so: (x, y, z)\n(x2, y2, z2).
		 *	\sa #StringWithArray
		 *	\returns A Vector3[] array.
		 */
		public static Vector3[] StringToVector3Array(string str)
		{
			List<Vector3> v = new List<Vector3>();

			str = str.Replace(" ", "");				// Remove white space
			string[] lines = str.Split('\n');		// split into vector lines

			foreach(string vec in lines)
			{
				if(vec.Contains("//"))
					continue;

				string[] values = vec.Split(',');

				if(values.Length < 3)
					continue;

				float v0, v1, v2;
				if( !float.TryParse(values[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v0) )
					continue;
				if( !float.TryParse(values[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v1) )
					continue;
				if( !float.TryParse(values[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v2) )
					continue;
				v.Add(new Vector3(v0, v1, v2));
			}
			return v.ToArray();
		}
	}
}
