using UnityEngine;
using System.Collections;

[System.Serializable]
public class pb_ObjectArray : ScriptableObject
{
	[SerializeField] public Object[] array;

	public T[] GetArray<T>()
	{
		T[] arr = new T[array.Length];

		for(int i = 0; i < array.Length; i++)
		{
			if(array[i] is ProceduralMaterial)
			{
				arr[i] = (T)System.Convert.ChangeType(array[i], typeof(ProceduralMaterial));
			}
			else
			{
				if(array[i] is T)
				{
					arr[i] = (T)System.Convert.ChangeType(array[i], typeof(T));
				}
				else
				{
					arr[i] = default(T);
				}
			}
		}

		return (T[])arr;
	}
}
