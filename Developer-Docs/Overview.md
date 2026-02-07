# ProBuilder Developer Documentation - Overview

## Introduction

ProBuilder is Unity's 3D modeling and level design tool that allows users to build, edit, and texture custom geometry directly within the Unity Editor. This documentation provides developers with a comprehensive understanding of ProBuilder's architecture, core systems, and codebase organization.

## High-Level Architecture

ProBuilder follows a modular architecture organized into several key layers:

### 1. Runtime Core (`Runtime/Core/`)
The foundational layer containing core data structures and utilities:
- **ProBuilderMesh**: The central component that manages all mesh data
- **Face, Edge, Vertex**: Fundamental geometric primitives
- **Shared Vertex System**: Manages vertex sharing and topology
- **Material and UV Management**: Handles texturing and material assignment

### 2. Mesh Operations (`Runtime/MeshOperations/`)
High-level mesh manipulation algorithms:
- **Extrusion**: Face and edge extrusion operations
- **Subdivision**: Mesh refinement and tessellation
- **Boolean Operations**: CSG operations for combining meshes
- **Triangulation**: Polygon tessellation using external algorithms
- **Element Selection**: Tools for selecting mesh components

### 3. Shape Generation (`Runtime/Shapes/`)
Procedural shape creation system:
- **ShapeGenerator**: Factory for creating primitive shapes
- **Parametric Shapes**: Configurable geometric primitives
- **Custom Shape Support**: Framework for user-defined shapes

### 4. Editor Tools (`Editor/EditorCore/`)
Editor-specific functionality and UI:
- **ProBuilderEditor**: Main editor controller and tool management
- **Selection System**: Mesh element selection and manipulation
- **Gizmos and Handles**: 3D manipulation widgets
- **Tool Windows**: UV editor, material palette, etc.

### 5. External Libraries (`External/`)
Third-party algorithms and utilities:
- **Poly2Tri**: Delaunay triangulation library
- **CSG**: Constructive Solid Geometry operations
- **KdTree**: Spatial data structure for performance

## Core Data Flow

```
User Input → Editor Tools → ProBuilderMesh → Mesh Operations → Unity Mesh
    ↑                                                              ↓
    └─────────── Scene View Rendering ←──────────────────────────┘
```

1. **User Input**: Mouse/keyboard interactions in the Scene View
2. **Editor Tools**: Translate input into mesh operations
3. **ProBuilderMesh**: Central data structure holding all mesh information
4. **Mesh Operations**: Algorithms that modify the ProBuilderMesh
5. **Unity Mesh**: Final compiled mesh for rendering

## Key Design Principles

### 1. Separation of Concerns
- **Runtime**: Core data structures and algorithms (game-ready)
- **Editor**: Tools and UI components (editor-only)
- **External**: Third-party libraries (isolated dependencies)

### 2. Non-Destructive Editing
ProBuilder maintains its own mesh representation (`ProBuilderMesh`) separate from Unity's `Mesh` component, allowing for:
- Undo/Redo operations
- Preservation of topology information
- Complex mesh operations without data loss

### 3. Element-Based Selection
The system operates on three levels of mesh elements:
- **Vertices**: Individual points in 3D space
- **Edges**: Connections between two vertices
- **Faces**: Polygonal surfaces defined by vertex loops

### 4. Shared Vertex Management
ProBuilder uses a sophisticated shared vertex system where:
- Multiple mesh vertices can reference the same geometric position
- Allows for hard/soft edge control
- Enables complex topology operations

## Package Structure

```
com.unity.probuilder/
├── Runtime/
│   ├── Core/              # Core data structures
│   ├── MeshOperations/    # Mesh manipulation algorithms
│   └── Shapes/            # Shape generation
├── Editor/
│   ├── EditorCore/        # Core editor functionality
│   ├── MenuActions/       # Menu system and actions
│   └── Overlays/          # UI overlays and windows
├── External/              # Third-party libraries
├── Debug/                 # Debug tools and utilities
└── Developer-Docs/        # This documentation
```

## Getting Started

For developers looking to:

- **Fix bugs**: Start with the relevant module (Core, MeshOperations, or Editor)
- **Add features**: Understand the data flow and identify the appropriate layer
- **Extend functionality**: Review the MenuActions and tool architecture
- **Optimize performance**: Focus on MeshOperations and External algorithms

Continue reading the specific documentation sections for detailed information about each component.

## Documentation Index

1. [Core Components](Core-Components.md) - Deep dive into fundamental classes
2. [Mesh Operations](Mesh-Operations.md) - Algorithms and mesh manipulation
3. [Editor Tools](Editor-Tools.md) - Editor architecture and tools
4. [Getting Started](Getting-Started.md) - Practical development guide