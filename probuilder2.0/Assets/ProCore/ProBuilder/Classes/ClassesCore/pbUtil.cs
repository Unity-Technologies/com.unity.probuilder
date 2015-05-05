using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#if PB_DEBUG
using Parabox.Debug;
#endif

/**
 *	\brief A series of handy extensions, ranging from Array utilities
 *	to 3d math.
 */
namespace ProBuilder2.Common
{
	public static class pbUtil 
	{

#region COMPONENT WRANGLING

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
	public static T[] GetComponents<T>(this GameObject[] _gameObjects) where T : Component
	{
		List<T> c = new List<T>();
		for(int i = 0; i < _gameObjects.Length; i++)
		{
			c.AddRange(_gameObjects[i].transform.GetComponentsInChildren<T>());
		}
		return c.ToArray() as T[];
	}

	/**
	 *	\brief Returns all components present in a given GameObject.  Conducts a deep search.
	 *	@param _gameObject GameObject to search for specified components.
	 *	\returns Array of T.
	 */
	public static T[] GetComponents<T>(GameObject go) where T : Component
	{
		return GetComponents<T>(new Transform[]{go.transform});
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
	public static T[] GetComponents<T>(this Transform[] _transforms) where T : Component
	{
		List<T> c = new List<T>();
		foreach(Transform t in _transforms)
		{
			c.AddRange(t.GetComponentsInChildren<T>());
		}
		return c.ToArray() as T[];
	}
#endregion

#region TRANSFORM

	/**
	 * Extension to transform.TransformPoint(Vector3 v) for arrays of Vector3[].
	 */
	public static Vector3[] ToWorldSpace(this Transform t, Vector3[] v)
	{
		Vector3[] w = new Vector3[v.Length];
		for(int i = 0; i < w.Length; i++)
			w[i] = t.TransformPoint(v[i]);
		return w;
	}
#endregion

#region ARRAY / LIST UTILITY
	
	public static T[] ValuesWithIndices<T>(T[] arr, int[] indices)
	{
		T[] vals = new T[indices.Length];
		for(int i = 0; i < indices.Length; i++)
			vals[i] = arr[indices[i]];
		return vals;
	}

	public static int[] AllIndexesOf<T>(T[] arr, T instance)
	{
		List<int> indices = new List<int>();
		for(int i = 0; i < arr.Length; i++)
		{
			if(arr[i].Equals(instance))
				indices.Add(i);
		}
		return indices.ToArray();
	}

	/**
	 * Equivalent to Linq EqualsAll()
	 */
	public static bool IsEqual<T>(this T[] a, T[] b)
	{
		if(a.Length != b.Length)
		{
			return false;
		}
		else
		{
			for(int i = 0; i < a.Length; i++)
				if(!a[i].Equals(b[i]))
					return false;
	
			return true;
		}
	}

	public static T[] Add<T>(this T[] arr, T val)
	{	
		T[] v = new T[arr.Length+1];
		System.Array.ConstrainedCopy(arr, 0, v, 0, arr.Length);
		v[arr.Length] = val;
		return v;
	}

	public static T[] AddRange<T>(this T[] arr, T[] val)
	{
		T[] ret = new T[arr.Length + val.Length];
		System.Array.ConstrainedCopy(arr, 0, ret, 0, arr.Length);
		System.Array.ConstrainedCopy(val, 0, ret, arr.Length, val.Length);
		return ret;
	}

	/**
	 * Remove @val from @arr.
	 */
	public static T[] Remove<T>(this T[] arr, T val)
	{
		List<T> list = new List<T>(arr);
		list.Remove(val);
		return list.ToArray();
	}

	public static T[] Remove<T>(this T[] arr, IEnumerable<T> val)
	{
		return arr.Except(val).ToArray();
	}

	public static T[] RemoveAt<T>(this T[] arr, int index)
	{
		T[] newArray = new T[arr.Length-1];
		int n = 0;
		for(int i = 0; i < arr.Length; i++)
		{
			if(i != index) {
				newArray[n] = arr[i];
				n++;
			}
		}
		return newArray;
	}
	
	/**
	 * Remove elements at @indices from an array.
	 */
	public static T[] RemoveAt<T>(this IList<T> list, IList<int> indices)
	{
		List<int> indices_sorted = new List<int>(indices);
		indices_sorted.Sort();

		return SortedRemoveAt(list, indices_sorted);
	}

	/**
	 * Remove elements at @indices from an array, but accepts a pre-sorted indices list.
	 */
	public static T[] SortedRemoveAt<T>(this IList<T> list, IList<int> sorted_indices)
	{
		int indices_length = sorted_indices.Count;
		int len = list.Count;

		T[] newArray = new T[len - indices_length];
		int n = 0;

		for(int i = 0; i < len; i++)
		{
			if(n < indices_length && sorted_indices[n] == i)
			{
				// handle duplicate indices
				while(n < indices_length && sorted_indices[n] == i)	
					n++;

				continue;
			}

			newArray[i-n] = list[i];
		}

		return newArray;
	}

	/**
	 * Holds a start and end index for a binary search.
	 */
	private struct SearchRange
	{
		public int begin, end;

		public SearchRange(int begin, int end)
		{
			this.begin = begin;
			this.end = end;
		}

		public bool Valid() { return end - begin > 1; }
		public int Center() { return begin + (end-begin)/2; }

		public override string ToString()
		{
			return "{" + begin + ", " + end + "} : " + Center();
		}
	}

	/**
	 * Given a sorted list and value, returns the index of the greatest value in sorted_list that is 
	 * less than value.  Ex: List( { 0, 1, 4, 6, 7 } ) Value(5) returns 2, which is the index of value 
	 * 4.
	 * If value is less than sorted[0], -1 is returned.  If value is greater than sorted[end], sorted.Count-1 
	 * is returned.  If an exact match is found, the index prior to that match is returned.
	 */
	public static int NearestIndexPriorToValue<T>(IList<T> sorted_list, T value) where T : System.IComparable<T>
	{
		int count = sorted_list.Count;
		if(count < 1) return -1;

		SearchRange range = new SearchRange(0, count-1);

		if(value.CompareTo(sorted_list[0]) < 0)
			return -1;

		if(value.CompareTo(sorted_list[count-1]) > 0)
			return count-1;

		while( range.Valid() )
		{
			if( sorted_list[range.Center()].CompareTo(value) > 0)
			{
				// sb.AppendLine(sorted_list[range.Center()] + " > " + value + " [" + range.begin + ", " + range.end +"] -> [" + range.begin + ", " + range.Center() + "]");
				range.end = range.Center();
			}
			else
			{
				// sb.AppendLine(sorted_list[range.Center()] + " < " + value + " [" + range.begin + ", " + range.end +"] -> [" + range.Center() + ", " + range.end + "]");
				range.begin = range.Center();

				if( sorted_list[range.begin+1].CompareTo(value) >= 0 )
				{
					return range.begin;
				}
			}
		}
	
		return 0;
	}

	public static T[] FilledArray<T>(T val, int length)
	{
		T[] arr = new T[length];
		for(int i = 0; i < length; i++) {
			arr[i] = val;
		}
		return arr;
	}
	
	/**
	 * True if any value is present in both arrays.
	 */
	public static bool ContainsMatch<T>(this T[] a, T[] b)
	{
		int ind = -1;
		for(int i = 0; i < a.Length; i++)
		{
			ind = System.Array.IndexOf(b, a[i]);
			if(ind > -1) return true;//ind;
		}
		return false;// ind;
	}

	/**
	 * True if any value is present in both arrays.
	 */
	public static bool ContainsMatch<T>(this T[] a, T[] b, out int index_a, out int index_b)
	{
		index_b = -1;
		for(index_a = 0; index_a < a.Length; index_a++)
		{
			index_b = System.Array.IndexOf(b, a[index_a]);
			if(index_b > -1) return true;//ind;
		}
		return false;// ind;
	}

	/**
	 * Concatenate two arrays.
	 */
	public static T[] Concat<T>(this T[] x, T[] y)
	{
		if (x == null) throw new ArgumentNullException("x");
		if (y == null) throw new ArgumentNullException("y");
		int oldLen = x.Length;
		Array.Resize<T>(ref x, x.Length + y.Length);
		Array.Copy(y, 0, x, oldLen, y.Length);
		return x;
	}

	/**
	 * Returns the index of the array that contains this value.  -1 if not found.
	 */
	public static int IndexOf<T>(this List<List<T>> InList, T InValue)
	{
		for(int i = 0; i < InList.Count; i++)
		{
			for(int x = 0; x < InList[i].Count; x++)
				if( InList[i][x].Equals(InValue) )
					return i;
		}

		return -1;
	}
#endregion

#region SNAP

	public static Vector3 SnapValue(Vector3 vertex, float snpVal)
	{
		// snapValue is a global setting that comes from ProGrids
		return new Vector3(
			snpVal * Mathf.Round(vertex.x / snpVal),
			snpVal * Mathf.Round(vertex.y / snpVal),
			snpVal * Mathf.Round(vertex.z / snpVal));
	}

	public static float SnapValue(float val, float snpVal)
	{
		return snpVal * Mathf.Round(val / snpVal);
	}

	/**
	 *	An override that accepts a vector3 to use as a mask for which values to snap.  Ex;
	 *	Snap((.3f, 3f, 41f), (0f, 1f, .4f)) only snaps Y and Z values.
	 */
	public static Vector3 SnapValue(Vector3 vertex, Vector3 snap)
	{
		float _x = vertex.x, _y = vertex.y, _z = vertex.z;
		Vector3 v = new Vector3(
			( Mathf.Abs(snap.x) < 0.0001f ? _x : snap.x * Mathf.Round(_x / snap.x) ),
			( Mathf.Abs(snap.y) < 0.0001f ? _y : snap.y * Mathf.Round(_y / snap.y) ),
			( Mathf.Abs(snap.z) < 0.0001f ? _z : snap.z * Mathf.Round(_z / snap.z) )
			);
		return v;
	}
#endregion

#region ENUM

	// http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
	public static T ParseEnum<T>(string value, T defaultValue) where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
		if (string.IsNullOrEmpty(value)) return defaultValue;

		foreach (T item in Enum.GetValues(typeof(T)))
		{
			if (item.ToString().ToLower().Equals(value.Trim().ToLower())) return item;
		}
		return defaultValue;
	}
#endregion

#region Mesh
	
	/**
	 *	\brief Performs a deep copy of a mesh and returns a new mesh object.
	 *	@param _mesh The mesh to copy.
	 *	\returns Copied mesh object.
	 */
	public static Mesh DeepCopyMesh(Mesh _mesh)
	{
		Vector3[] v = new Vector3[_mesh.vertices.Length];
		int[][]   t = new int[_mesh.subMeshCount][];
		Vector2[] u = new Vector2[_mesh.uv.Length];
		Vector2[] u2 = new Vector2[_mesh.uv2.Length];
		Vector4[] tan = new Vector4[_mesh.tangents.Length];
		Vector3[] n = new Vector3[_mesh.normals.Length];
		Color32[] c = new Color32[_mesh.colors32.Length];

		System.Array.Copy(_mesh.vertices, v, v.Length);

		for(int i = 0; i < t.Length; i++)
			t[i] = _mesh.GetTriangles(i);

		System.Array.Copy(_mesh.uv, u, u.Length);
		System.Array.Copy(_mesh.uv2, u2, u2.Length);
		System.Array.Copy(_mesh.normals, n, n.Length);
		System.Array.Copy(_mesh.tangents, tan, tan.Length);
		System.Array.Copy(_mesh.colors32, c, c.Length);

		Mesh m = new Mesh();

		m.Clear();
		m.name = _mesh.name;

		m.vertices = v;
		
		m.subMeshCount = t.Length;
		for(int i = 0; i < t.Length; i++)
			m.SetTriangles(t[i], i);

		m.uv = u;
		m.uv2 = u2; 
		m.tangents = tan;
		m.normals = n;
		m.colors32 = c;

		return m;
	}
#endregion

#region STRING UTILITY

	/**
	 *	\brief Returns a string formatted by the passed seperator parameter.
	 *	\code{cs}
	 *	int[] myArray = new int[3]{0, 1, 2};
	 *
	 *	// Prints "0, 1, 2"
	 *	Debug.Log(myArray.ToFormattedString(", "));
	 *	@param _delimiter Inserts this string between entries.
	 *	\returns Formatted string.
	 */
	public static string ToFormattedString<T>(this T[] t, string _delimiter)
	{
		return t.ToFormattedString(_delimiter, 0, -1);
	}

	public static string ToFormattedString<T>(this T[] t, string _delimiter, int entriesPerLine, int maxEntries)
	{
		int len = maxEntries > 0 ? (int)Mathf.Min(t.Length, maxEntries) : t.Length;
		if(t == null || len < 1)
			return "Empty Array.";

		StringBuilder str = new StringBuilder();

		// str.Append(_delimiter.Replace("\n", "") + (t[0] == null ? "null" : t[0].ToString()) + _delimiter );

		for(int i = 0; i < len-1; i++)
		{
			if(entriesPerLine > 0 && (i+1) % entriesPerLine == 0)
				str.AppendLine( ((t[i] == null) ? "null" : t[i].ToString()) + _delimiter );
			else
				str.Append( ((t[i] == null) ? "null" : t[i].ToString()) + _delimiter);
		}
		
		str.Append( (t[len-1] == null) ? "null" : t[len-1].ToString() );

		return str.ToString();		
	}

	public static string ToFormattedString(this pb_UV[] t, string _delimiter)
	{
		int len = t.Length;
		if(t == null || len < 1)
			return "Empty Array.";

		StringBuilder str = new StringBuilder();

		for(int i = 0; i < len-1; i++)
		{
			str.AppendLine( ((t[i] == null) ? "null" : t[i].ToString(_delimiter)) + "\n" );
		}
		
		str.Append( (t[len-1] == null) ? "null" : t[len-1].ToString(_delimiter) );

		return str.ToString();		
	}

	/**
	 *	\brief Returns a string formatted by the passed seperator parameter.
	 *	\code{cs}
	 *	List<int> myList = new List<int>(){0, 1, 2};
	 *
	 *	// Prints "0, 1, 2"
	 *	Debug.Log(myList.ToFormattedString(", "));
	 *	@param _delimiter Inserts this string between entries.
	 *	\returns Formatted string.
	 */
	public static string ToFormattedString<T>(this List<T> t, string _delimiter)
	{
		return t.ToArray().ToFormattedString(_delimiter);
	}

	public static string ToFormattedString<T>(this HashSet<T> t, string _delimiter)
	{
		return t.ToArray().ToFormattedString(_delimiter);
	}

	public static string StringWithDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dict)
	{
		string str = "";
		foreach(KeyValuePair<TKey, TValue> kvp in dict) {
			str += "Key:" + kvp.Key + "  Val: " + kvp.Value + "\n";
		}
		return str;
	}

	public static bool ColorWithString(string value, out Color col)
	{
		string valid = "01234567890.,";
        value = new string(value.Where(c => valid.Contains(c)).ToArray());
        string[] rgba = value.Split(',');
        
        if(rgba.Length < 4)
        {
        	col = Color.white;
        	return false;
        }
        	// return new Color(0f, .86f, 1f, .275f);

		col = new Color(
			float.Parse(rgba[0]),
			float.Parse(rgba[1]),
			float.Parse(rgba[2]),
			float.Parse(rgba[3]));
		
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
#endregion

#region OVERRIDE

	/**
	 * Component-wise division.
	 */
	public static Vector2 DivideBy(this Vector2 v, Vector2 o)
	{
		return new Vector2(v.x/o.x, v.y/o.y);
	}

	/**
	 * Component-wise division.
	 */
	public static Vector3 DivideBy(this Vector3 v, Vector3 o)
	{
		return new Vector3(v.x/o.x, v.y/o.y, v.z/o.z);
	}
#endregion
}
}