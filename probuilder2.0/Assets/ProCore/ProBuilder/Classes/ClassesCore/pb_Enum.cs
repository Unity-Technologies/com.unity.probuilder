using UnityEngine;
using System.Collections;

/**
 * Contains runtime enumerators.
 */
namespace ProBuilder2.Common
{
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

}