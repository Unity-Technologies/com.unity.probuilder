using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public abstract class pb_MenuOption : EditorWindow
	{
		public static pb_MenuOption Show(System.Type type)
		{
			if( !type.IsSubclassOf(typeof(pb_MenuOption)) )
			{
				Debug.Log(type.ToString() + " is not of type pb_MenuOption");
				return null;
			}

			foreach(EditorWindow win in Resources.FindObjectsOfTypeAll(type))
				win.Close();

			return EditorWindow.GetWindow(type, true, "Option", true) as pb_MenuOption;
		}

		public virtual bool IsEnabled(EditLevel editLevel, SelectMode selectionMode, pb_Object[] selection)
		{
			return true;
		}
	}
}
