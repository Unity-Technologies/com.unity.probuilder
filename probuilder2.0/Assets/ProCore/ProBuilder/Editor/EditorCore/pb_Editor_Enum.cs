
using System.Collections;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Element selection mode.
	 */
	public enum SelectMode
	{
		Vertex,
		Edge,
		Face
	}
	
	/**
	 * The editor level - top (no pb action), geo, texture, plugin.
	 */
	public enum EditLevel {
		Top,
		Geometry,
		Texture,
		Plugin
	}

	public enum HandleAlignment {
		World,
		Plane,
		Local
	}
}