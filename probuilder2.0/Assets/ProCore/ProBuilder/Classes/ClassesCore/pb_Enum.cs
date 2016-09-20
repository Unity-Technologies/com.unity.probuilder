
using UnityEngine;
using System.Collections;

/**
 * Contains runtime enumerators.
 */
namespace ProBuilder2.Common
{
	/**
	 * Element selection mode.
	 */
	[System.Flags]
	public enum SelectMode
	{
		Vertex = 0,
		Edge = 1,
		Face = 2
	}

	/**
	 * The editor level - top (no pb action), geo, texture, plugin.
	 */
	[System.Flags]
	public enum EditLevel {
		Top = 0,
		Geometry = 1,
		Texture = 2,
		Plugin = 4
	}

	public enum HandleAlignment {
		World = 0,
		Local = 1,
		Plane = 2
	}

	/**
	 * When drag selecting elements, does the shift key
	 *	- Always add to the selection (Add)
	 *	- Always subtract from the selection (Remove)
	 *	- Invert the selected faces (Difference)
	 */
	public enum DragSelectMode
	{
		Add,
		Subtract,
		Difference
	}

	/**
	 * Determines what GameObject flags this object will have.
	 */
	public enum EntityType {
		Detail,
		Occluder,
		Trigger,
		Collider,
		Mover
	}

	/**
	 * Deprecated.
	 */
	public enum ColliderType {
		None,
		BoxCollider,
		MeshCollider
	}

	public enum ProjectionAxis
	{
		X,			// projects on x axis
		Y,			// projects on y axis
		Z,			// projects on z axis
		X_Negative,
		Y_Negative,
		Z_Negative
	}

	/**
	 * Used to generate geo.
	 */
	public enum Shape {
		Cube,
		Stair,
		Prism,
		Cylinder,
		Plane,
		Door,
		Pipe,
		Cone,
		Sprite,
		Arch,
		Icosahedron,
		Torus,
		Custom
	}

	// !-- Todo: Replace the various other Axis enums with this
	public enum Axis {
		Right,
		Left,
		Up,
		Down,
		Forward,
		Backward
	}

	/**
	 * Unused.
	 */
	public enum UV2Method {
		Unity,
		BinPack
	}

	/**
	 * Describes the winding order of mesh triangles.
	 */
	public enum WindingOrder {
		Unknown,
		Clockwise,
		CounterClockwise
	}

	/**
	 *	Describes methods of sorting 2d vertices.
	 */
	public enum SortMethod {
		Clockwise,
		CounterClockwise
	};

	/**
	 * Describes different culling options.
	 */
	public enum Culling
	{
		Back = 0x0,
		Front = 0x1,
		FrontBack = 0x2
	}

	/**
	 * If Verify() rebuilds the pb_Object mesh, this describes the reasoning.
	 */
	public enum MeshRebuildReason
	{
		Null,
		InstanceIDMismatch,
		Lightmap,
		None
	}

	public enum AttibuteType
	{
		Position,
		UV0,
		UV1,
		UV2,
		UV3,
		Color,
		Normal,
		Tangent
	};

	public enum IndexFormat
	{
		Local = 0x0,
		Common = 0x1,
		Both = 0x2
	};
}
