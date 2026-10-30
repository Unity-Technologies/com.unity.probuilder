# ProBuilder Developer Documentation

This folder contains comprehensive technical documentation for developers working on the ProBuilder codebase. The documentation covers architecture, algorithms, and practical development guidance.

## Quick Start

If you're new to ProBuilder development:

1. **Start with [Overview.md](Overview.md)** - Get a high-level understanding of the system architecture
2. **Read [Getting-Started.md](Getting-Started.md)** - Learn practical development workflows
3. **Dive deeper** with the specific component documentation as needed

## Documentation Structure

### [Overview.md](Overview.md)
High-level architecture and system overview including:
- Core system layers (Runtime, Editor, External)
- Data flow and design principles
- Package structure and organization
- Key concepts and terminology

### [Core-Components.md](Core-Components.md)
Deep dive into fundamental classes and data structures:
- **ProBuilderMesh** - Central mesh component
- **Face, Edge, Vertex** - Geometric primitives  
- **Shared Vertex System** - Topology management
- **WingedEdge** - Advanced topology navigation
- **Material and UV systems**

### [Mesh-Operations.md](Mesh-Operations.md)
Algorithms and techniques for mesh manipulation:
- **Triangulation** - Poly2Tri Delaunay triangulation
- **Extrusion** - Face and edge extrusion algorithms
- **Subdivision** - Connect elements algorithm
- **Boolean Operations** - CSG using BSP trees
- **UV Operations** - Automatic unwrapping and stitching

### [Editor-Tools.md](Editor-Tools.md)
Editor architecture and tool system:
- **ProBuilderEditor** - Central controller
- **Selection System** - Element selection and management
- **Tool System** - Unity EditorTools integration
- **Scene View Integration** - 3D interaction and picking
- **UI Components** - Windows, overlays, and gizmos

### [Getting-Started.md](Getting-Started.md)
Practical development guide covering:
- **Development scenarios** - Bug fixes, new features, optimization
- **Debugging techniques** - Visual debugging, logging, validation
- **Best practices** - Code organization, error handling, performance
- **Testing strategies** - Unit tests, integration tests, performance tests

## Key External Libraries

ProBuilder integrates several external libraries for specialized algorithms:

### Poly2Tri (`External/Poly2Tri/`)
- **Purpose**: Robust polygon triangulation
- **Algorithm**: Delaunay triangulation using sweep line
- **Reference**: [Poly2Tri GitHub](https://github.com/jhasse/poly2tri)
- **Usage**: Converting polygonal faces to triangles for rendering

### CSG (`External/CSG/`)
- **Purpose**: Constructive Solid Geometry operations
- **Algorithm**: BSP tree-based boolean operations
- **Reference**: [CSG Wikipedia](https://en.wikipedia.org/wiki/Constructive_solid_geometry)
- **Usage**: Union, subtraction, and intersection of meshes

### KdTree (`External/KdTree/`)
- **Purpose**: Spatial data structure for performance
- **Algorithm**: k-dimensional tree for nearest neighbor queries
- **Reference**: [k-d tree Wikipedia](https://en.wikipedia.org/wiki/K-d_tree)
- **Usage**: Vertex welding, collision detection, spatial queries

## Algorithm References

The documentation includes links to external resources explaining the algorithms used:

- **[Winged Edge Data Structure](https://en.wikipedia.org/wiki/Winged_edge)** - Topology navigation
- **[Delaunay Triangulation](https://en.wikipedia.org/wiki/Delaunay_triangulation)** - Robust polygon tessellation
- **[Fortune's Algorithm](https://en.wikipedia.org/wiki/Fortune%27s_algorithm)** - Sweep line triangulation
- **[BSP Trees](https://en.wikipedia.org/wiki/Binary_space_partitioning)** - Spatial partitioning for CSG

## Development Workflow

### For Bug Fixes:
1. Identify the affected component using [Getting-Started.md](Getting-Started.md)
2. Study the relevant algorithm documentation
3. Create a minimal reproduction case
4. Debug using the techniques described
5. Validate the fix with existing tests

### For New Features:
1. Understand the architecture from [Overview.md](Overview.md)
2. Identify the appropriate layer for your feature
3. Study similar existing features for patterns
4. Implement following the established conventions
5. Add comprehensive tests and documentation

### For Performance Issues:
1. Profile to identify bottlenecks
2. Check [Mesh-Operations.md](Mesh-Operations.md) for algorithm complexity
3. Apply optimization techniques from [Getting-Started.md](Getting-Started.md)
4. Validate performance improvements with benchmarks

## Contributing Guidelines

When working on ProBuilder:

- **Follow Established Patterns** - Study existing code for conventions
- **Maintain Compatibility** - Consider runtime vs editor boundaries
- **Add Tests** - Especially for new algorithms and edge cases
- **Document Algorithms** - Include references for complex algorithms
- **Consider Performance** - Profile operations on large meshes
- **Validate Thoroughly** - Mesh operations must maintain topology integrity

## Validation and Testing

ProBuilder includes extensive validation systems:

- **Mesh Validation** - Check topology integrity after operations
- **Unit Tests** - Algorithm correctness and edge case handling  
- **Integration Tests** - Complete workflow validation
- **Performance Tests** - Ensure operations scale appropriately

See [Getting-Started.md](Getting-Started.md) for detailed testing strategies.

## Additional Resources

### Unity Documentation:
- [Unity Editor Scripting](https://docs.unity3d.com/Manual/editor-EditorWindows.html)
- [Unity Package Development](https://docs.unity3d.com/Manual/CustomPackages.html)
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

### Computational Geometry:
- [Real-Time Rendering Book](http://www.realtimerendering.com/)
- [Computational Geometry Algorithms](https://en.wikipedia.org/wiki/Computational_geometry)
- [Mesh Processing Library](http://www.cs.cmu.edu/~kmcrane/Projects/ModelRepository/)

This documentation is designed to help developers understand, maintain, and extend ProBuilder's capabilities while preserving its robustness and performance characteristics.