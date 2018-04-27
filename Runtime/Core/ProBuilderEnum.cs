using UnityEngine;
using System.Collections;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Element selection mode.
	/// </summary>
	/// <remarks>Editor only, but necessary for pb_ElementGraphics.</remarks>
	public enum SelectMode
	{
		Vertex = 0x0,
		Edge = 0x1,
		Face = 0x2
	}

	/// <summary>
	/// The ProBuilder edit level.
	/// </summary>'
	/// <remarks>Editor only, but necessary for pb_ElementGraphics.</remarks>
	public enum EditLevel
	{
		/// <summary>
		/// Unity tools are in control.
		/// </summary>
		Top = 0,
		/// <summary>
		/// Geometry editing (faces, edges, vertices)
		/// </summary>
		Geometry = 1,
		/// <summary>
		/// UV editing.
		/// </summary>
		Texture = 2,
		/// <summary>
		/// Some other ProBuilder tool has control (vertex painter)
		/// </summary>
		Plugin = 3
	}

	/// <summary>
	/// Determines what GameObject flags this object will have.
	/// </summary>
	public enum EntityType {
		Detail,
		Occluder,
		Trigger,
		Collider,
		Mover
	}

	enum ColliderType {
		None,
		BoxCollider,
		MeshCollider
	}

	/// <summary>
	/// Axis used in projecting UVs.
	/// </summary>
	public enum ProjectionAxis
	{
		/// <summary>
		/// Projects on x axis.
		/// </summary>
		X,
		/// <summary>
		/// Projects on y axis.
		/// </summary>
		Y,
		/// <summary>
		/// Projects on z axis.
		/// </summary>
		Z,
		/// <summary>
		/// Projects on -x axis.
		/// </summary>
		XNegative,
		/// <summary>
		/// Projects on -y axis.
		/// </summary>
		YNegative,
		/// <summary>
		/// Projects on -z axis.
		/// </summary>
		ZNegative
	}

	/// <summary>
	/// pb_ShapeEditor enum.
	/// </summary>
	[System.Obsolete("See pb_ShapeType")]
	enum Shape {
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

	/// <summary>
	/// Human readable axis enum.
	/// </summary>
	public enum Axis {
		/// <summary>
		/// X axis.
		/// </summary>
		Right,
		/// <summary>
		/// -X axis.
		/// </summary>
		Left,
		/// <summary>
		/// Y axis.
		/// </summary>
		Up,
		/// <summary>
		/// -Y axis.
		/// </summary>
		Down,
		/// <summary>
		/// Z axis.
		/// </summary>
		Forward,
		/// <summary>
		/// -Z axis.
		/// </summary>
		Backward
	}

	/// <summary>
	/// Describes the winding order of mesh triangles.
	/// </summary>
	public enum WindingOrder {
		/// <summary>
		/// Winding order could not be determined.
		/// </summary>
		Unknown,
		/// <summary>
		/// Winding is clockwise (right handed).
		/// </summary>
		Clockwise,
		/// <summary>
		/// Winding is counter-clockwise (left handed).
		/// </summary>
		CounterClockwise
	}

	/// <summary>
	/// Describes methods of sorting 2d vertices.
	/// </summary>
	public enum SortMethod {
		/// <summary>
		/// Order the vertices clockwise.
		/// </summary>
		Clockwise,
		/// <summary>
		/// Order the vertices counter-clockwise.
		/// </summary>
		CounterClockwise
	};

	[System.Flags]
	public enum CullingMode
	{
		None = 0 << 0,
		Back = 1 << 0,
		Front = 1 << 1,
		FrontBack = Front | Back,
	}

	public enum RectSelectMode
	{
		Partial,
		Complete
	}

	/// <summary>
	/// If pb_Object.Verify() rebuilds the mesh this describes the reasoning.
	/// </summary>
	public enum MeshRebuildReason
	{
		/// <summary>
		/// The UnityEngine mesh was null.
		/// </summary>
		Null,
		/// <summary>
		/// The UnityEngine mesh id did not match the stored pb_Object id.
		/// </summary>
		InstanceIDMismatch,
		/// <summary>
		/// The mesh was not rebuilt, but is missing the UV2 channel.
		/// </summary>
		Lightmap,
		/// <summary>
		/// The mesh was not rebuilt.
		/// </summary>
		None
	}

    /// <summary>
    /// Mesh attributes bitmask.
    /// </summary>
    [System.Flags]
    public enum Attributes
    {
        /// <summary>
        /// Vertex positions.
        /// </summary>
        Position = 0x1,
        /// <summary>
        /// Vertex UV.
        /// </summary>
        UV0 = 0x2,
        /// <summary>
        /// Vertex UV2.
        /// </summary>
        UV1 = 0x4,
        /// <summary>
        /// Vertex UV3.
        /// </summary>
        UV2 = 0x8,
        /// <summary>
        /// Vertex UV4.
        /// </summary>
        UV3 = 0x10,
        /// <summary>
        /// Vertex colors.
        /// </summary>
        Color = 0x20,
        /// <summary>
        /// Vertex normals.
        /// </summary>
        Normal = 0x40,
        /// <summary>
        /// Vertex tangents.
        /// </summary>
        Tangent = 0x80,
        /// <summary>
        /// All ProBuilder stored mesh attributes.
        /// </summary>
        All = 0xFF
    };

    enum IndexFormat
	{
		Local = 0x0,
		Common = 0x1,
		Both = 0x2
	};

	/// <summary>
	/// Selectively rebuild and apply mesh attributes to the UnityEngine.Mesh asset.
	/// </summary>
	[System.Flags]
	public enum RefreshMask
	{
        /// <summary>
        /// UV1 channel will be rebuilt.
        /// </summary>
        UV = 0x1,
        /// <summary>
        /// Colors will be rebuilt.
        /// </summary>
        Colors = 0x2,
        /// <summary>
        /// Normals will be recalculated and applied.
        /// </summary>
        Normals = 0x4,
        /// <summary>
        /// Tangents will be recalculated and applied.
        /// </summary>
        Tangents = 0x8,
        /// <summary>
        /// If userCollisions is not true, any primitive colliders will be resized to best fit the mesh bounds.
        /// </summary>
        Collisions = 0x10,
        /// <summary>
        /// Refresh all optional mesh attributes.
        /// </summary>
        All = UV | Colors | Normals | Tangents | Collisions
    };

	/// <summary>
	/// Different methods of face extrusion.
	/// </summary>
	public enum ExtrudeMethod
	{
        /// <summary>
        /// Each face is extruded separately.
        /// </summary>
        IndividualFaces = 0,
        /// <summary>
        /// Adjacent faces are merged as a group along the averaged normals.
        /// </summary>
        VertexNormal = 1,
        /// <summary>
        /// Adjacent faces are merged as a group, but faces are extruded from each face normal.
        /// </summary>
        FaceNormal = 2
    }
}
