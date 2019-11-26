using System;
using UnityEngine;
using System.Collections;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Defines what objects are selectable for the scene tool.
    /// </summary>
    [System.Flags]
    public enum SelectMode
    {
        /// <summary>
        /// No selection mode defined.
        /// </summary>
        None = 0 << 0,

        /// <summary>
        /// Objects are selectable.
        /// </summary>
        Object = 1 << 0,

        /// <summary>
        /// Vertices are selectable.
        /// </summary>
        Vertex = 1 << 1,

        /// <summary>
        /// Edges are selectable.
        /// </summary>
        Edge = 1 << 2,

        /// <summary>
        /// Faces are selectable.
        /// </summary>
        Face = 1 << 3,

        /// <summary>
        /// Texture coordinates are selectable.
        /// </summary>
        TextureFace = 1 << 4,

        /// <summary>
        /// Texture coordinates are selectable.
        /// </summary>
        TextureEdge = 1 << 5,

        /// <summary>
        /// Texture coordinates are selectable.
        /// </summary>
        TextureVertex = 1 << 6,

        /// <summary>
        /// Other input tool (Poly Shape editor, Bezier editor, etc)
        /// </summary>
        InputTool = 1 << 7,

        /// <summary>
        /// Match any value.
        /// </summary>
        Any = 0xFFFF
    }

    /// <summary>
    /// Element selection mode.
    /// </summary>
    enum ComponentMode
    {
        /// <summary>
        /// Vertices are selectable.
        /// </summary>
        Vertex = 0x0,
        /// <summary>
        /// Edges are selectable.
        /// </summary>
        Edge = 0x1,
        /// <summary>
        /// Faces are selectable.
        /// </summary>
        Face = 0x2
    }

    /// <summary>
    /// Defines what the current tool interacts with in the scene view.
    /// </summary>'
    internal enum EditLevel
    {
        /// <summary>
        /// The transform tools interact with GameObjects.
        /// </summary>
        Top = 0,
        /// <summary>
        /// The current tool interacts with mesh geometry (faces, edges, vertices).
        /// </summary>
        Geometry = 1,
        /// <summary>
        /// Tools are affecting mesh UVs. This corresponds to UVEditor in-scene editing.
        /// </summary>
        Texture = 2,
        /// <summary>
        /// A custom ProBuilder tool mode is engaged.
        /// </summary>
        Plugin = 3
    }

    /// <summary>
    /// Determines what GameObject flags this object will have.
    /// </summary>
    enum EntityType
    {
        Detail,
        Occluder,
        Trigger,
        Collider,
        Mover
    }

    enum ColliderType
    {
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

    enum HandleAxis
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        Free = 1 << 3
    }

    /// <summary>
    /// pb_ShapeEditor enum.
    /// </summary>
    [System.Obsolete("See pb_ShapeType")]
    enum Shape
    {
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
    public enum Axis
    {
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
    public enum WindingOrder
    {
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
    /// Describes methods of sorting points in 2d space.
    /// </summary>
    public enum SortMethod
    {
        /// <summary>
        /// Order the vertices clockwise.
        /// </summary>
        Clockwise,
        /// <summary>
        /// Order the vertices counter-clockwise.
        /// </summary>
        CounterClockwise
    };

    /// <summary>
    /// A flag which sets the triangle culling mode.
    /// </summary>
    [System.Flags]
    public enum CullingMode
    {
        /// <summary>
        /// Both front and back faces are rendered.
        /// </summary>
        None = 0 << 0,
        /// <summary>
        /// Back faces are culled.
        /// </summary>
        Back = 1 << 0,
        /// <summary>
        /// Front faces are culled.
        /// </summary>
        Front = 1 << 1,
        /// <summary>
        /// Both front and back faces are culled.
        /// </summary>
        FrontBack = Front | Back,
    }

    /// <summary>
    /// Defines the behavior of drag selection in the scene view for mesh elements.
    /// </summary>
    public enum RectSelectMode
    {
        /// <summary>
        /// Any mesh element touching the drag rectangle is selected.
        /// </summary>
        Partial,
        /// <summary>
        /// Mesh elements must be completely enveloped by the drag rect to be selected.
        /// </summary>
        Complete
    }

    /// <summary>
    /// Describes why a @"UnityEngine.ProBuilder.ProBuilderMesh" is considered to be out of sync with it's UnityEngine.MeshFilter component.
    /// </summary>
    public enum MeshSyncState
    {
        /// <summary>
        /// The MeshFilter mesh is null.
        /// </summary>
        Null,
        /// <summary>
        /// The MeshFilter mesh is not owned by the ProBuilderMesh component. Use @"UnityEngine.ProBuilder.ProBuilderMesh.MakeUnique" to remedy.
        /// </summary>
        /// <remarks>This is only used in editor.</remarks>
        InstanceIDMismatch,
        /// <summary>
        /// The mesh is valid, but does not have a UV2 channel.
        /// </summary>
        /// <remarks>This is only used in editor.</remarks>
        Lightmap,
        /// <summary>
        /// The mesh is in sync.
        /// </summary>
        InSync
    }

    /// <summary>
    /// Mesh attributes bitmask.
    /// </summary>
    [System.Flags]
    public enum MeshArrays
    {
        /// <summary>
        /// Vertex positions.
        /// </summary>
        Position = 0x1,
        /// <summary>
        /// First UV channel.
        /// </summary>
        Texture0 = 0x2,
        /// <summary>
        /// Second UV channel. Commonly called UV2 or Lightmap UVs in Unity terms.
        /// </summary>
        Texture1 = 0x4,
        /// <summary>
        /// Second UV channel. Commonly called UV2 or Lightmap UVs in Unity terms.
        /// </summary>
        Lightmap = 0x4,
        /// <summary>
        /// Third UV channel.
        /// </summary>
        Texture2 = 0x8,
        /// <summary>
        /// Vertex UV4.
        /// </summary>
        Texture3 = 0x10,
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
    /// <seealso cref="ProBuilderMesh.Refresh"/>
    [System.Flags]
    public enum RefreshMask
    {
        /// <summary>
        /// Textures channel will be rebuilt.
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
        /// Re-assign the MeshCollider sharedMesh.
        /// </summary>
        Collisions = 0x10,
        /// <summary>
        /// Refresh all optional mesh attributes.
        /// </summary>
        All = UV | Colors | Normals | Tangents | Collisions
    };

    /// <summary>
    /// Describes the different methods of face extrusion.
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
