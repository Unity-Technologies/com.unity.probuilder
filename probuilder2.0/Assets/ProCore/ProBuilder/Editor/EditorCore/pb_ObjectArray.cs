using UnityEngine;
using System.Collections;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/**
	 *	Used to store arrays of materials. Made obsolete by pb_MaterialArray.
	 *
	 *	Notes:
	 *	- In the Editor folder because WebGL doesn't like ProceduralMaterial types.
	 *	- Named pb_ObjectArray because this was initially intended to be a base type for
	 *	  other save-able object types. It is only used for materials, but changing the name
	 *	  to something more suitable would mean breaking existing material palettes for
	 *	  lots of people.
	 */
	[System.Obsolete(
		"pb_ObjectArray is deprecated. ProBuilder Material Editor now saves material palettes as pb_MaterialArray. You may safely delete this asset.")]
	[System.Serializable]
	public class pb_ObjectArray : ScriptableObject, pb_IHasDefault
	{
		// Stored as object for backwards compatibility.
		[SerializeField] public Object[] array;

		public T[] GetArray<T>()
		{
			T[] arr = new T[array.Length];

			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is ProceduralMaterial)
				{
					arr[i] = (T) System.Convert.ChangeType(array[i], typeof(ProceduralMaterial));
				}
				else
				{
					if (array[i] is T)
					{
						arr[i] = (T) System.Convert.ChangeType(array[i], typeof(T));
					}
					else
					{
						arr[i] = default(T);
					}
				}
			}

			return (T[]) arr;
		}

		public void SetDefaultValues()
		{
			array = new Material[10]
			{
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
