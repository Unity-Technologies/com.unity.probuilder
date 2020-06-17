#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.ProBuilder
{
    public abstract class Shape
    {
        public abstract void RebuildMesh(ProBuilderMesh mesh, Vector3 size);

#if UNITY_EDITOR
        public virtual string name
        {
            get { return ObjectNames.NicifyVariableName(GetType().Name); }
        }

        //public void ResetParameters()
        //{
        //    var members = GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        //    foreach (var fi in members)
        //    {
        //        if (fi.GetValue(this) is IUserSetting setting)
        //            setting.Reset();
        //        // if(fi.FieldType.BaseType?.GetGenericTypeDefinition() == typeof(UserSetting<>))
        //        // {
        //        //     var setting = fi.GetValue(this) as IUserSetting;
        //        //     Debug.Log($"Name: {fi.Name}\nType: {fi.FieldType}\nValue {fi.GetValue(this)}");
        //        // }
        //    }
        //}
#endif
    }
}
