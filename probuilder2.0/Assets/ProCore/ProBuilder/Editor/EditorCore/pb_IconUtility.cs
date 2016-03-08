using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public static class pb_IconUtility
	{
		private static Dictionary<string, Texture2D> m_icons = new Dictionary<string, Texture2D>();

		public static Texture2D GetIcon(string iconName)
		{
			Texture2D icon = null;

			if(!m_icons.TryGetValue(iconName, out icon))
			{
				icon = Resources.Load<Texture2D>("Icons/" + iconName);
				
				if(icon == null)
				{
					Debug.LogWarning("failed to find icon: " + iconName);
					icon = Resources.Load<Texture2D>("Icons/null");
				}

				m_icons.Add(iconName, icon);
			}

			return icon;
		}

	}
}
