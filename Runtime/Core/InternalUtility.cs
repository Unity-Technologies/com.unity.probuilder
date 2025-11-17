using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.ProBuilder
{
    static class InternalUtility
    {
        // @todo
        public static T[] GetComponents<T>(GameObject go) where T : Component
        {
            return go.transform.GetComponentsInChildren<T>();
        }

        // @todo
        public static T[] GetComponents<T>(this IEnumerable<Transform> transforms) where T : Component
        {
            List<T> c = new List<T>();
            foreach (Transform t in transforms)
                c.AddRange(t.GetComponentsInChildren<T>());
            return c.ToArray() as T[];
        }

        public static GameObject EmptyGameObjectWithTransform(Transform t)
        {
            GameObject go                   = new GameObject();
            go.transform.localPosition      = t.localPosition;
            go.transform.localRotation      = t.localRotation;
            go.transform.localScale         = t.localScale;

            #if UNITY_EDITOR
            StageUtility.PlaceGameObjectInCurrentStage(go);
            #endif

            return go;
        }

        public static GameObject MeshGameObjectWithTransform(string name, Transform t, Mesh mesh, Material mat, bool inheritParent)
        {
            GameObject go = InternalUtility.EmptyGameObjectWithTransform(t);
            go.name = name;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            go.hideFlags = HideFlags.HideAndDontSave;

            if (inheritParent)
                go.transform.SetParent(t.parent, false);

            return go;
        }

        public static T NextEnumValue<T>(this T current) where T : IConvertible
        {
            Assert.IsTrue(current is Enum);

            var values = Enum.GetValues(current.GetType());

            for (int i = 0, c = values.Length; i < c; i++)
                if (current.Equals(values.GetValue((i))))
                    return (T)values.GetValue((i + 1) % c);

            return current;
        }

        public static string ControlKeyString(char character)
        {
            if (character == PreferenceKeys.CMD_SUPER)
                return "Control";
            else if (character == PreferenceKeys.CMD_SHIFT)
                return "Shift";
            else if (character == PreferenceKeys.CMD_OPTION)
                return "Alt";
            else if (character == PreferenceKeys.CMD_ALT)
                return "Alt";
            else if (character == PreferenceKeys.CMD_DELETE)
#if UNITY_EDITOR_WIN
                return "Backspace";
#else
                return "Delete";
#endif
            else
                return character.ToString();
        }

        /**
         *  Attempt to parse a color from string input.
         */
        public static bool TryParseColor(string value, ref Color col)
        {
            string valid = "01234567890.,";
            value = new string(value.Where(c => valid.Contains(c)).ToArray());
            string[] rgba = value.Split(',');

            if (rgba.Length < 4)
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

        /// <summary>
        /// Get a reference to an existing component, or add a new component if one does not already exist.
        /// </summary>
        public static T DemandComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.DemandComponent<T>();
        }

        public static T DemandComponent<T>(this GameObject gameObject) where T : Component
        {
            T component;
            if (!gameObject.TryGetComponent<T>(out component))
                component = gameObject.AddComponent<T>();
            return component;
        }

#if UNITY_6000_4_OR_NEWER
        /// <summary>
        /// Returns the <see cref="EntityId"/> identifier for the given <see cref="UnityEngine.Object"/>.
        /// This method abstracts the Unity version differences between <c>GetEntityId()</c>
        /// (available in Unity 6000.4 or newer) and <c>GetInstanceID()</c> in older Unity versions.
        ///
        /// Use this method to obtain an object identifier that is compatible with APIs expecting
        /// <see cref="EntityId"/> in newer Unity versions, such as
        /// <see cref="UnityEditor.AssetDatabase.GetAssetPath(EntityId)"/>.
        /// </summary>
        /// <param name="o">The Unity object to get an identifier for.</param>
        /// <returns>The object's unique <see cref="EntityId"/>.</returns>
        internal static EntityId GetObjectId(this Object o)
        {
            return o.GetEntityId();
        }
#else
        /// <summary>
        /// Returns the integer identifier for the given <see cref="UnityEngine.Object"/>.
        /// This method abstracts the Unity version differences between <c>GetEntityId()</c>
        /// (available in Unity 6000.4 or newer) and <c>GetInstanceID()</c> in older Unity versions.
        ///
        /// Use this method to obtain an object identifier that is compatible with APIs expecting
        /// an integer in older Unity versions, such as
        /// <see cref="UnityEditor.AssetDatabase.GetAssetPath(int)"/>.
        /// </summary>
        /// <param name="o">The Unity object to get an identifier for.</param>
        /// <returns>The object's unique integer identifier.</returns>
        internal static int GetObjectId(this Object o)
        {
            return o.GetInstanceID();
        }
#endif
    }
}
