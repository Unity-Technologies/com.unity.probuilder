using UnityEngine;
using System.Collections;
using System.Reflection;

namespace ProBuilder2.UpgradeKit
{
	public static class pb_UpgradeKitUtils
	{
#region REFLECTION

		internal static bool TryGetProperty<T>(object target, string propertyName, System.Type propertyType, ref T value)
		{
			return TryGetProperty(target, propertyName, propertyType, BindingFlags.Instance | BindingFlags.Public, ref value);
		}

		internal static bool TryGetProperty<T>(object target, string propertyName, System.Type propertyType, BindingFlags flags, ref T value)
		{
			PropertyInfo property = target.GetType().GetProperty(propertyName, flags);

			if(property != null)
			{
				var val = property.GetValue(target, null);

				if(val != null && val is T)
				{
					value = (T)val;
					return true;
				}
			}

			return false;
		}

		internal static bool TrySetProperty<T>(object target, string propertyName, T value)
		{
			return TrySetProperty(target, propertyName, BindingFlags.Instance | BindingFlags.Public, value);
		}

		internal static bool TrySetProperty<T>(object target, string propertyName, BindingFlags flags, T value)
		{
			PropertyInfo property = target.GetType().GetProperty(propertyName, flags);

			if(property != null)
			{
				property.SetValue(target, value, null);
				return true;
			}

			return false;
		}

		/**
		 * If ToMesh() is present, this invokes pb.ToMesh() then pb.Refresh().  If not, pb.GenerateSubmeshes(false) is used.
		 * The false parameter tells pb not to hide nodraw faces.
		 */
		public static void RebuildMesh(pb_Object pb)
		{
			if( InvokeFunction(pb, "ToMesh", null) )
			{
				InvokeFunction(pb, "Refresh", null);
			}
			else
			{
				// GenerateSubmeshes also calls 'Refresh()'
				InvokeFunction(pb, "GenerateSubmeshes", new System.Type[] { typeof(bool) }, new object[] { false });
			}
		}

		public static bool InvokeFunction(object target, string methodName, object[] parameters)
		{
			return InvokeFunction(target, methodName, null, parameters);
		}
		
		public static bool InvokeFunction(object target, string methodName, System.Type[] argumentTypes, object[] parameters)
		{
			MethodInfo mi;
			if(argumentTypes != null)
				mi = target.GetType().GetMethod(methodName, argumentTypes);
			else
				mi = target.GetType().GetMethod(methodName);

			if(mi != null)
			{
				try {
					mi.Invoke(target, parameters);
					return true;
				} catch (System.Exception e) {
					Debug.Log(e);
					return false;				
				}
			}
			
			return false;				
		}
#endregion

#region GENERAL

		public static void SetColors(this pb_Object pb, Color[] colors) { }

		public static pb_IntArray[] ToPbIntArray(this int[][] arr)
		{
			pb_IntArray[] pbint = new pb_IntArray[arr.Length];
			for(int i = 0; i < arr.Length; i++)
				pbint[i] = new pb_IntArray(arr[i]);
			return pbint;
		}

		internal static Material GetDefaultMaterial()
		{
			return (Material)Resources.Load("Materials/Default_Prototype", typeof(Material)) ?? UnityDefaultDiffuse; 
		}
		
		internal static Material _UnityDefaultDiffuse = null;
		internal static Material UnityDefaultDiffuse
		{
			get
			{
				if( _UnityDefaultDiffuse == null )
				{
					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
					_UnityDefaultDiffuse = go.GetComponent<MeshRenderer>().sharedMaterial;
					GameObject.DestroyImmediate(go);

					Debug.Log("Create and Destroy PrimitiveType");
				}
				
				return _UnityDefaultDiffuse;
			}
		}

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
	}
#endregion
}
