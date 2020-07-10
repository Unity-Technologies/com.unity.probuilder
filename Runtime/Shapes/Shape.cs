#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
#endif

namespace UnityEngine.ProBuilder
{
    [System.Serializable]
    public abstract class Shape
    {
        public abstract void RebuildMesh(ProBuilderMesh mesh, Vector3 size);
#if UNITY_EDITOR
        public virtual string name
        {
            get { return ObjectNames.NicifyVariableName(GetType().Name); }
        }

        public void SetToLastParams()
        {
            var name = "ShapeBuilder." + GetType().Name;
            var data = JsonUtility.FromJson(EditorPrefs.GetString(name), GetType());
            if (data == null)
                return;
            var members = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fi in members)
            {
                var value = fi.GetValue(data);
                if (value == null)
                    continue;
                fi.SetValue(this, value);
            }
        }

        public void SaveParams()
        {
            var name = "ShapeBuilder." + GetType().Name;
            var data = JsonUtility.ToJson(this);
            EditorPrefs.SetString(name, data);
        }
#endif
    }
}
