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
        [Obsolete]
        public static T[] GetComponents<T>(this IEnumerable<GameObject> gameObjects) where T : Component
        {
            List<T> c = new List<T>();
            foreach (GameObject go in gameObjects)
                c.AddRange(go.transform.GetComponentsInChildren<T>());
            return c.ToArray();
        }

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
            go.transform.position           = t.position;
            go.transform.localRotation      = t.localRotation;
            go.transform.localScale         = t.localScale;

            #if UNITY_EDITOR
            StageUtility.PlaceGameObjectInCurrentStage(go);
            #endif

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

        /**
         *  \brief Convert a string to a Vector3 array.
         ()
         *  @param str A string formatted like so: (x, y, z)\n(x2, y2, z2).
         *  \sa #StringWithArray
         *  \returns A Vector3[] array.
         */
        public static Vector3[] StringToVector3Array(string str)
        {
            List<Vector3> v = new List<Vector3>();

            str = str.Replace(" ", "");             // Remove white space
            string[] lines = str.Split('\n');       // split into vector lines

            foreach (string vec in lines)
            {
                if (vec.Contains("//"))
                    continue;

                string[] values = vec.Split(',');

                if (values.Length < 3)
                    continue;

                float v0, v1, v2;
                if (!float.TryParse(values[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v0))
                    continue;
                if (!float.TryParse(values[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v1))
                    continue;
                if (!float.TryParse(values[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v2))
                    continue;
                v.Add(new Vector3(v0, v1, v2));
            }
            return v.ToArray();
        }

#if !UNITY_2019_2_OR_NEWER
        public static bool TryGetComponent<T>(this Component source, out T component) where T : Component
        {
            return (component = source.GetComponent<T>()) != null;
        }

        public static bool TryGetComponent<T>(this GameObject source, out T component) where T : Component
        {
            return (component = source.GetComponent<T>()) != null;
        }
#endif

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
    }
}
