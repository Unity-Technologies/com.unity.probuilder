# Glossary

## -C-

<a name="coincident"></a>

### Coincident

_Coincident_ vertices share the same coordinate space, but are [incident](#incident) to two separate edges.

## -D-

<a name="degenerate"></a>

### Degenerate triangles

A degenerate triangle does not have any area.

## -I-

<a name="incident"></a>

### Incident

When an edge connects two vertices, those vertices are both _incident_ to the edge.

## -M-

<a name="manifold"></a>

### Manifold and non-manifold geometry

Manifold geometry is well-formed and could exist in the real world. Non-manifold geometry can cause errors and could de-stabilize your project. The following are examples of non-manifold geometry:

* Inner faces (faces enclosed inside another set of faces)
* Overlapping faces (occupying the same space)
* Faces that bisect another mesh
* Self-intersecting faces

## -Q-

<a name="quad"></a>

### Quad (quadrangle)

Most polygons are "triangulated", or divided into faces with three edges. However, ProBuilder supports four-sided faces, or "quadrangles" in many cases. "Quad" is the abbreviation for "quadrangle".

## -W-

<a name="winding"></a>

### Winding order

Winding order determines how a polygon's vertices are rendered (clockwise vs. counter-clockwise). A clockwise winding order is sometimes called "right-handed" and a counter-clockwise winding order is sometimes called "left-handed". In left-handed coordinate systems like Unity, where the y-axis is "up", counter-clockwise winding is front-facing. For right-handed systems, the rendering order is opposite . 

<a name="winged-edge"></a>

### Winged edge

When three or more faces meet at a boundary, the vertex where they meet is _coincident_ with multiple faces and edges. The [WingedEdge](xref:UnityEngine.ProBuilder.WingedEdge) is a structure that holds information about a face, an edge, and a vertex at that boundary. It also provides a list of coincident edges so you can use the [EdgeLookup](xref:UnityEngine.ProBuilder.EdgeLookup) to access information about them through scripting.
