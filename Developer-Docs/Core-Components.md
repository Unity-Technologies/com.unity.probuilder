# ProBuilder Core Components

This document details the fundamental classes and data structures that form the foundation of ProBuilder's mesh representation and manipulation system.

## ProBuilderMesh - The Central Component

**Location**: `Runtime/Core/ProBuilderMesh.cs`

`ProBuilderMesh` is the heart of ProBuilder, extending `MonoBehaviour` to provide a Unity component that manages all mesh data and operations.

### Key Responsibilities:
- **Mesh Data Storage**: Maintains vertices, faces, edges, and UV coordinates
- **Unity Integration**: Bridges between ProBuilder's format and Unity's Mesh system
- **Serialization**: Handles saving/loading of mesh data
- **Mesh Compilation**: Converts internal format to Unity Mesh for rendering

### Key Properties:
```csharp
// Core mesh elements
public Face[] faces              // Array of face definitions
public Vector3[] positions       // Vertex positions in local space
public SharedVertex[] sharedVertices  // Vertex sharing information

// Material and UV data
public Vector2[] textures0       // Primary UV coordinates
public Material[] materials      // Per-face materials
public AutoUnwrapSettings[] unwrapParameters  // UV generation settings
```

### Important Methods:
- `ToMesh()`: Compiles ProBuilder format to Unity Mesh
- `Refresh()`: Updates visual representation
- `GetVertices()`: Returns all vertex data as Vertex structs
- `SetVertices()`: Updates vertex data from Vertex array

## Face - Polygon Definition

**Location**: `Runtime/Core/Face.cs`

Represents a polygonal face composed of triangles with associated material and UV settings.

### Structure:
```csharp
public class Face
{
    int[] indexes;              // Triangle indices (always triangulated)
    int smoothingGroup;         // For normal smoothing
    AutoUnwrapSettings uv;      // UV projection settings
    Material material;          // Face material
    bool manualUV;             // UV coordinate source
}
```

### Key Concepts:
- **Triangle Storage**: All faces are internally stored as triangles, even if originally quads
- **Smoothing Groups**: Faces sharing a smoothing group have averaged normals
- **UV Modes**: Faces can use automatic UV projection or manual coordinates
- **Material Assignment**: Each face can have its own material

## Edge - Vertex Connection

**Location**: `Runtime/Core/Edge.cs`

A simple structure representing a connection between two vertices.

### Structure:
```csharp
public struct Edge
{
    public int a, b;           // Vertex indices
    
    public bool IsValid()      // Checks if edge points to valid vertices
    public static Edge Empty   // Represents invalid edge (-1, -1)
}
```

### Usage:
- **Topology Queries**: Finding adjacent faces, edge loops
- **Selection Operations**: Edge-based selection and manipulation
- **Mesh Operations**: Extrusion, loop cuts, beveling

## Vertex - Point Data

**Location**: `Runtime/Core/Vertex.cs`

Comprehensive vertex structure containing all per-vertex attributes.

### Structure:
```csharp
public struct Vertex
{
    public Vector3 position;    // 3D position
    public Color color;         // Vertex color
    public Vector3 normal;      // Surface normal
    public Vector4 tangent;     // Tangent vector (with handedness in w)
    
    // UV coordinates (up to 4 channels)
    public Vector2 uv0, uv2;
    public Vector4 uv3, uv4;
}
```

### Notes:
- **Complete Attribute Set**: Contains all data needed for rendering
- **UV Channels**: Supports multiple UV sets for advanced materials
- **Vertex Colors**: Per-vertex color information for special effects

## Shared Vertex System

**Location**: `Runtime/Core/SharedVertex.cs`

ProBuilder's sophisticated system for managing vertex sharing and topology.

### Concept:
```
Geometric Vertices: [A, B, C, D, E, F]
Shared Groups:     [[A,D], [B,E], [C,F]]
```

Multiple mesh vertices can reference the same geometric position while maintaining different attributes (normals, UVs, colors).

### SharedVertex Structure:
```csharp
public sealed class SharedVertex : IEnumerable<int>
{
    int[] m_Vertices;          // Array of vertex indices sharing position
}
```

### Benefits:
- **Hard/Soft Edges**: Control over normal smoothing
- **UV Seams**: Different UV coordinates at same position
- **Efficient Operations**: Topology queries and modifications
- **Undo/Redo**: Preserves sharing relationships

## WingedEdge - Topology Navigation

**Location**: `Runtime/Core/WingedEdge.cs`

Advanced data structure for efficient mesh topology navigation, based on the [Winged Edge](https://en.wikipedia.org/wiki/Winged_edge) data structure.

### Structure:
```csharp
public sealed class WingedEdge
{
    public EdgeLookup edge;     // The edge this wing represents
    public Face face;           // Connected face
    public WingedEdge next;     // Next edge in face
    public WingedEdge previous; // Previous edge in face
    public WingedEdge opposite; // Edge on adjacent face
}
```

### Usage:
- **Mesh Traversal**: Navigate between adjacent faces and edges
- **Topology Queries**: Find edge loops, face neighbors
- **Validation**: Check for manifold geometry
- **Complex Operations**: Boolean operations, subdivision

### Algorithm Reference:
The implementation follows the classic Winged Edge data structure described in:
- [Winged Edge Wikipedia](https://en.wikipedia.org/wiki/Winged_edge)
- Original paper: "Winged edge polyhedron representation" by Bruce Baumgart (1975)

## Material and UV Management

### AutoUnwrapSettings
**Location**: `Runtime/Core/AutoUnwrapSettings.cs`

Controls automatic UV coordinate generation for faces.

```csharp
public struct AutoUnwrapSettings
{
    public bool useWorldSpace;      // Use world vs local coordinates
    public bool flipU, flipV;       // UV flipping
    public Vector2 scale;           // UV scaling
    public Vector2 offset;          // UV offset
    public float rotation;          // UV rotation
    public Anchor anchor;           // Projection anchor point
}
```

### UV Projection Methods:
1. **Planar**: Project along face normal
2. **Box**: Six-sided projection
3. **Cylindrical**: Wrap around cylinder
4. **Spherical**: Spherical projection

### Built-in Materials
**Location**: `Runtime/Core/BuiltinMaterials.cs`

Manages default materials for different render pipelines:
- **Standard**: Built-in render pipeline
- **URP**: Universal Render Pipeline  
- **HDRP**: High Definition Render Pipeline

## Selection and Picking

### SelectionPicker
**Location**: `Runtime/Core/SelectionPicker.cs`

Handles 3D picking and selection of mesh elements using GPU-based selection.

### Approach:
1. **Render to Texture**: Draw mesh with unique colors per element
2. **Mouse Query**: Sample color at mouse position
3. **Color Decode**: Convert color back to element index

### Benefits:
- **Accurate Selection**: Pixel-perfect element picking
- **Performance**: GPU acceleration for complex meshes
- **Occlusion**: Automatically handles depth testing

## Utility Classes

### Math Utilities
**Location**: `Runtime/Core/Math.cs`

Mathematical functions specific to mesh operations:
- **Plane/Ray intersections**
- **Point-in-polygon tests**
- **Distance calculations**
- **Geometric projections**

### Mesh Utilities  
**Location**: `Runtime/Core/MeshUtility.cs`

Helper functions for mesh data manipulation:
- **Vertex welding and splitting**
- **Normal calculation**
- **Bounds computation**
- **Mesh validation**

## Performance Considerations

### Memory Management:
- **Object Pooling**: Reuse temporary collections
- **Lazy Evaluation**: Calculate expensive data on demand
- **Dirty Flags**: Track what needs recalculation

### Optimization Patterns:
- **Batch Operations**: Process multiple elements together
- **Spatial Indexing**: Use KdTree for spatial queries
- **Cache Friendly**: Minimize object allocations in hot paths

## Common Patterns

### Mesh Modification Workflow:
1. **Begin Operation**: `mesh.ToMesh()` and `mesh.Refresh()`
2. **Modify Data**: Update vertices, faces, or topology
3. **Validate**: Check mesh integrity
4. **Commit**: `mesh.ToMesh()` and refresh display

### Error Handling:
- **Validation**: Check for degenerate geometry
- **Graceful Degradation**: Fall back to simpler operations
- **User Feedback**: Provide actionable error messages

This foundation enables all higher-level operations while maintaining data integrity and performance.