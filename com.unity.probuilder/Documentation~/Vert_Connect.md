# ![Connect Vertices Icon](images/icons/Vert_Connect.png) Connect Vertices

Creates a new edge connecting the selected vertices.

![Insert a new edge between two vertices on a face](images/ConnectVerts_Example.png)



> ***Tip:*** You can also use this tool with the **Alt+E** (Windows) or **Opt+E** (Mac) hotkey.

If you select more than two vertices, ProBuilder creates as many new edges as possible, adding extra vertices where necessary in order to keep the geometry valid. For example, connecting three vertices around a quad creates a new vertex in the middle to support the three new edges.

You can connect across several faces as long as they share a selected vertex.