using UnityEngine;

namespace ProBuilder2.EditorCommon
{
	public enum pb_IconGroup
	{
		Tool,
		Selection,
		Object,
		Geometry
	}

	public static class pb_IconGroupUtility
	{
		static readonly Color ToolColor 		= new Color(0.6666f, 0.4f, 0.2f, 1f);
		static readonly Color SelectionColor 	= new Color(0.1411f, 0.4941f, 0.6392f, 1f);
		static readonly Color ObjectColor 		= new Color(0.4f, 0.6f, 0.1333f, 1f);
		static readonly Color GeometryColor		= new Color(0.7333f, 0.1333f, 0.2f, 1f);

		public static Color GetColor(pb_IconGroup group)
		{
			if( group == pb_IconGroup.Tool )
				return ToolColor;
			else if( group == pb_IconGroup.Selection )
				return SelectionColor;
			else if( group == pb_IconGroup.Object )
				return ObjectColor;
			else if( group == pb_IconGroup.Geometry )
				return GeometryColor;

			return Color.white;
		}
	}
}
