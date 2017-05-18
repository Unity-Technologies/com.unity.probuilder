using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	A serializable object that stores an array of materials. Used by pb_Material_Editor.
	 */
	public class pb_MaterialPalette : ScriptableObject, pb_IHasDefault
	{
		public Material[] array;

		public static implicit operator Material[](pb_MaterialPalette materialArray)
		{
			return materialArray.array;
		}

		public Material this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public void SetDefaultValues()
		{
			array = new Material[10] {
				pb_Constant.DefaultMaterial,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			 };
		}
	}
}
