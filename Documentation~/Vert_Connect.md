# ![Connect Vertices Icon](images/icons/Vert_Connect.png) Connect Vertices

The __Connect Vertices__ action creates a new edge that connects the selected vertices.

![Insert a new edge between two vertices on a face](images/ConnectVerts_Example.png)



> **Tip:** You can also use this action with the **Alt/Opt+E** shortcut, or from the ProBuilder menu (**Tools** > **ProBuilder** > **Selection** > **Smart Connect**).

If you select more than two vertices, ProBuilder creates as many new edges as required, and adds extra vertices where necessary in order to keep the geometry valid. For example, if you connect three vertices around a quad, ProBuilder creates a new vertex in the middle to support the three new edges.

You can connect across several faces as long as they share a selected vertex.
