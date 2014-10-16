
using System.Collections;

namespace ProBuilder2.EditorCommon
{
	public enum SelectMode
	{
		Vertex,
		Edge,
		Face
	}
	
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