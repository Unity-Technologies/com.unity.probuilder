# ProBuilder Mesh Operations

This document covers the algorithms and techniques used in ProBuilder for mesh manipulation, including geometric operations, triangulation, and topology modifications.

## Overview

The `Runtime/MeshOperations/` directory contains the core algorithms that transform and manipulate ProBuilder meshes. These operations form the backbone of ProBuilder's modeling capabilities.

## Triangulation System

**Location**: `Runtime/MeshOperations/Triangulation.cs`

ProBuilder uses the **Poly2Tri** library for robust polygon triangulation.

### Algorithm: Delaunay Triangulation with Sweep Line

**External Library**: [Poly2Tri](https://github.com/jhasse/poly2tri) (located in `External/Poly2Tri/`)

**Algorithm Reference**: 
- [Delaunay Triangulation](https://en.wikipedia.org/wiki/Delaunay_triangulation)
- [Sweep Line Algorithm](https://en.wikipedia.org/wiki/Fortune%27s_algorithm)

### Key Methods:
```csharp
public static bool Triangulate(IList<Vector2> points, out List<int> indexes)
public static bool TriangulateVertices(Vector3[] vertices, out List<int> triangles)
public static bool SortAndTriangulate(IList<Vector2> points, out List<int> indexes)
```

### Process:
1. **Project to 2D**: Find best-fit plane for 3D polygon
2. **Sort Points**: Order vertices counter-clockwise
3. **Triangulate**: Apply Delaunay triangulation
4. **Map Back**: Convert 2D indices back to 3D vertex indices

### Differences from Standard Implementation:
- **Unity Integration**: Modified to work with Unity's coordinate system
- **Error Handling**: Enhanced fallbacks for degenerate cases
- **Performance**: Cached triangulation context for repeated operations

## Extrusion Operations

**Location**: `Runtime/MeshOperations/ExtrudeElements.cs`

Implements face and edge extrusion with multiple strategies.

### Face Extrusion Methods:

#### 1. Individual Faces (`ExtrudeMethod.IndividualFaces`)
Each face is extruded along its own normal:
```
Original Face → Duplicate Face → Connect Edges
```

#### 2. Face Normal (`ExtrudeMethod.FaceNormal`)  
Faces are grouped and extruded along averaged normal:
```
Face Group → Calculate Average Normal → Extrude as Group
```

#### 3. Vertex Normal (`ExtrudeMethod.VertexNormal`)
Extrusion follows vertex normals for smooth results:
```
Per-Vertex → Use Vertex Normal → Smooth Extrusion
```

### Edge Extrusion:
```csharp
public static Edge[] Extrude(ProBuilderMesh mesh, IEnumerable<Edge> edges, 
                           float distance, bool extrudeAsGroup, bool enableManifoldExtrude)
```

### Algorithm Steps:
1. **Manifold Check**: Verify edges can be extruded safely
2. **Direction Calculation**: Determine extrusion direction per edge
3. **Vertex Duplication**: Create new vertices for extruded elements
4. **Face Creation**: Generate connecting faces between original and extruded elements
5. **Topology Update**: Update shared vertex information

## Subdivision Algorithms

**Location**: `Runtime/MeshOperations/Subdivision.cs`

### Connect Elements Algorithm
ProBuilder's subdivision uses a "connect elements" approach:

```csharp
public static Face[] Subdivide(ProBuilderMesh pb, IList<Face> faces)
{
    return ConnectElements.Connect(pb, faces);
}
```

### Process:
1. **Center Point Insertion**: Add vertex at center of each face
2. **Edge Midpoints**: Add vertices at edge midpoints
3. **Face Subdivision**: Split each face into smaller faces
4. **Connectivity**: Maintain proper topology relationships

### Subdivision Pattern:
```
Original Quad:     Subdivided:
+-------+          +---+---+
|       |          | \ | / |
|   +   |    →     +---+---+
|       |          | / | \ |
+-------+          +---+---+
```

## Boolean Operations (CSG)

**Location**: `External/CSG/`

ProBuilder implements **Constructive Solid Geometry** using a modified CSG algorithm.

### Algorithm Reference:
- [CSG Wikipedia](https://en.wikipedia.org/wiki/Constructive_solid_geometry)
- Based on: "Merging BSP trees yields polyhedral Boolean operations" by Bruce Naylor (1990)

### Operations Supported:
1. **Union**: Combine two meshes
2. **Subtraction**: Remove one mesh from another  
3. **Intersection**: Keep only overlapping regions

### CSG Process:
1. **BSP Tree Construction**: Build Binary Space Partitioning trees for each mesh
2. **Tree Operations**: Perform boolean operations on BSP trees
3. **Polygon Classification**: Classify polygons as inside/outside/on boundary
4. **Mesh Reconstruction**: Convert result back to ProBuilder mesh

### Implementation Details:
```csharp
// Located in External/CSG/CSG.cs
public static Model Union(Model a, Model b)
public static Model Subtract(Model a, Model b)
public static Model Intersect(Model a, Model b)
```

### Differences from Standard CSG:
- **Robust Handling**: Enhanced error handling for edge cases
- **Unity Integration**: Works with ProBuilder's mesh format
- **Performance**: Optimized for interactive use

## Surface Topology Operations

**Location**: `Runtime/MeshOperations/SurfaceTopology.cs`

Handles complex topology modifications and mesh validation.

### Key Operations:
- **Manifold Detection**: Check if mesh is manifold (every edge shared by exactly 2 faces)
- **Edge Loop Detection**: Find continuous chains of connected edges
- **Hole Filling**: Close gaps in mesh topology
- **Vertex Welding**: Merge nearby vertices

### Manifold Validation:
```csharp
public static bool IsManifold(ProBuilderMesh mesh)
{
    // Check each edge is shared by exactly 2 faces
    // Verify no T-junctions or non-manifold vertices
}
```

## Element Selection Algorithms

**Location**: `Runtime/MeshOperations/ElementSelection.cs`

Implements intelligent selection algorithms for mesh elements.

### Selection Methods:

#### 1. Edge Loops
Selects continuous chains of connected edges:
```
Start Edge → Find Connected Edges → Continue Until Loop Closes
```

#### 2. Face Loops  
Selects rings of faces around topology:
```
Start Face → Find Adjacent Faces → Follow Ring Pattern
```

#### 3. Connected Elements
Selects all elements connected to seed selection:
```
Seed Selection → Flood Fill → Stop at Boundaries
```

### Algorithm: Flood Fill Selection
1. **Initialize Queue**: Add seed elements to queue
2. **Process Queue**: For each element, find neighbors
3. **Filter Neighbors**: Apply selection criteria
4. **Add to Result**: Include valid neighbors in selection
5. **Repeat**: Until queue is empty

## UV Operations

**Location**: `Runtime/MeshOperations/UV/`

### Automatic UV Unwrapping

#### Planar Projection:
Projects UV coordinates along a plane normal:
```csharp
Vector2 uv = new Vector2(
    Vector3.Dot(worldPos, uAxis),
    Vector3.Dot(worldPos, vAxis)
);
```

#### Box Projection:
Six-sided projection for complex shapes:
1. **Determine Primary Axis**: Find face normal's dominant direction
2. **Select Projection Plane**: Choose appropriate face of box
3. **Project Coordinates**: Apply planar projection on selected plane

#### Cylindrical Projection:
```csharp
float angle = Mathf.Atan2(localPos.z, localPos.x);
float height = localPos.y;
Vector2 uv = new Vector2(angle / (2f * Mathf.PI), height);
```

### UV Stitching
**Location**: `Runtime/MeshOperations/UV/TextureStitching.cs`

Algorithms for connecting UV islands:
1. **Edge Detection**: Find UV seam edges
2. **Island Identification**: Group connected UV vertices
3. **Stitching Strategy**: Determine how to connect islands
4. **UV Adjustment**: Modify coordinates to eliminate seams

## Mesh Validation and Repair

**Location**: `Runtime/MeshOperations/MeshValidation.cs`

### Validation Checks:
1. **Degenerate Triangles**: Triangles with zero area
2. **Duplicate Vertices**: Vertices at same position
3. **Invalid Indices**: Face indices out of bounds
4. **Topology Consistency**: Proper edge/face relationships

### Repair Operations:
```csharp
public static ActionResult EnsureMeshIsValid(ProBuilderMesh mesh)
{
    // Remove degenerate faces
    // Merge duplicate vertices  
    // Fix invalid topology
    // Recalculate normals
}
```

## Performance Optimizations

### Spatial Data Structures

#### KdTree (External Library)
**Location**: `External/KdTree/`

Used for spatial queries and nearest neighbor searches:
- **Vertex Welding**: Find nearby vertices to merge
- **Collision Detection**: Fast intersection tests
- **Selection Queries**: Find elements in 3D regions

**Algorithm Reference**: [k-d tree](https://en.wikipedia.org/wiki/K-d_tree)

### Batch Operations
Many operations are optimized to process multiple elements:
```csharp
// Instead of processing one face at a time
foreach(Face face in selection)
    ExtrudeFace(face);

// Batch process for better performance  
ExtrudeFaces(selection);
```

### Memory Management
- **Object Pooling**: Reuse temporary collections
- **Lazy Calculation**: Compute expensive data only when needed
- **Dirty Tracking**: Update only what changed

## Algorithm Complexity

### Common Operations:
- **Triangulation**: O(n log n) where n = vertices
- **CSG Boolean**: O(n²) worst case, O(n log n) average
- **Subdivision**: O(n) where n = faces
- **Extrusion**: O(n) where n = selected elements
- **Selection**: O(n) for flood fill, O(1) for direct selection

## Error Handling Patterns

### Graceful Degradation:
1. **Validate Input**: Check for valid mesh state
2. **Detect Problems**: Identify potential issues early
3. **Apply Fixes**: Use repair algorithms when possible
4. **Fallback**: Use simpler operations if complex ones fail
5. **User Feedback**: Provide clear error messages

### Common Edge Cases:
- **Non-manifold geometry**: Handle T-junctions and boundary edges
- **Degenerate polygons**: Skip zero-area faces
- **Floating point precision**: Use epsilon comparisons
- **Memory constraints**: Limit operation complexity

## Integration Points

### Unity Mesh System:
Operations work with Unity's mesh format through `ProBuilderMesh.ToMesh()`:
```csharp
// ProBuilder Operation
mesh.ExtrudeFaces(selectedFaces);

// Convert to Unity Mesh
mesh.ToMesh();
mesh.Refresh();
```

### Undo System:
All operations are designed to work with Unity's Undo system:
```csharp
Undo.RecordObject(mesh, "Extrude Faces");
// Perform operation
```

This modular design allows for complex mesh operations while maintaining performance and reliability.