using UnityEngine;
using System.Collections;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /**
     *  Used to store arrays of materials. Made obsolete by pb_MaterialArray.
     *
     *  Notes:
     *  - In the Editor folder because WebGL doesn't like ProceduralMaterial types.
     *  - Named pb_ObjectArray because this was initially intended to be a base type for
     *    other save-able object types. It is only used for materials, but changing the name
     *    to something more suitable would mean breaking existing material palettes for
     *    lots of people.
     */
    [System.Obsolete(
         "pb_ObjectArray is deprecated. ProBuilder Material Editor now saves material palettes as pb_MaterialArray. You may safely delete this asset.")]
    [System.Serializable]
    // ReSharper disable once InconsistentNaming
    sealed class pb_ObjectArray : ScriptableObject, IHasDefault
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
                    arr[i] = (T)System.Convert.ChangeType(array[i], typeof(ProceduralMaterial));
                }
                else
                {
                    if (array[i] is T)
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

        public void SetDefaultValues()
        {
            array = new Material[10]
            {
                null,
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
