#if UNITY_EDITOR
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


        // Put in an editor class - SettingsManager
        // Only use in DrawShapeTool
        internal void SetToLastParams()
        {
            var name = "ShapeBuilder." + GetType().Name;
            JsonUtility.FromJsonOverwrite(EditorPrefs.GetString(name), this);
        }

        internal void SaveParams()
        {
            var name = "ShapeBuilder." + GetType().Name;
            var data = JsonUtility.ToJson(this);
            EditorPrefs.SetString(name, data);
        }
#endif
    }
}
