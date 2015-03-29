using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.UpgradeKit
{
	public static class pb_UpgradeKitUtils
	{
#region REFLECTION
		/**
		 * These would be marked `internal`, but since classes compiled in the editor pass need to access them, they 
		 * have to be public.  So just don't use these, k?
		 */

		 /**
		  * Attempt to set a field by name with value.  Returns true if successful.
		  */
		public static bool TryGetField<T>(object target, string fieldName, ref T value)
		{
			return TryGetField(target, fieldName, BindingFlags.Instance, ref value);
		}

		public static bool TryGetField<T>(object target, string fieldName, BindingFlags flags, ref T value)
		{
			FieldInfo field = target.GetType().GetField(fieldName);

			if(field != null)
			{
				var val = field.GetValue(target);

				if(val != null && val is T)
				{
					value = (T)val;
					return true;
				}
			}

			return false;
		}

		public static bool TrySetField<T>(object target, string fieldName, T value)
		{
			return TrySetField(target, fieldName, BindingFlags.Instance | BindingFlags.Public, value);
		}

		public static bool TrySetField<T>(object target, string fieldName, BindingFlags flags, T value)
		{
			FieldInfo field = target.GetType().GetField(fieldName);

			if(field != null)
			{
				field.SetValue(target, value);
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
			if( InvokeFunction(pb, "ToMesh", new System.Type[0] {}, null) )
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

		/**
		 * Compare vertices, uvs, and normals of a mesh.
		 */
		public static bool AreEqual(pb_Object lhs, pb_Object rhs)
		{
			if(lhs == null || rhs == null)
				return false;

			Color[] lhs_colors = lhs.msh != null ? lhs.msh.colors : new Color[lhs.vertexCount];
			Color[] rhs_colors = rhs.msh != null ? rhs.msh.colors : new Color[rhs.vertexCount];

			return 	lhs.vertices.SequenceEqual(rhs.vertices) && 
					lhs.uv.SequenceEqual(rhs.uv) &&
					lhs_colors.SequenceEqual(rhs_colors) &&
					FacesAreEqual(lhs.faces, rhs.faces);
		}

		/**
		 * Compare values of faces.
		 */
		public static bool FacesAreEqual(pb_Face[] lhs, pb_Face[] rhs)
		{
			if(lhs.Length != rhs.Length)
				return false;

			for(int i = 0; i < lhs.Length; i++)
			{
				if( !lhs[i].indices.SequenceEqual(rhs[i].indices) || 
					lhs[i].smoothingGroup != rhs[i].smoothingGroup ||
					lhs[i].textureGroup != rhs[i].textureGroup ||
					lhs[i].elementGroup != rhs[i].elementGroup ||
					lhs[i].manualUV != rhs[i].manualUV ||
					lhs[i].material != rhs[i].material)
					return false;
			}

			return true;
		}
#endregion
	}
}
