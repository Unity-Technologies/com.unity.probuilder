# ProBuilder Editor Tools and Architecture

This document covers the editor-specific components of ProBuilder, including the tool system, selection management, UI components, and integration with Unity's editor framework.

## Editor Architecture Overview

The ProBuilder editor is built around several key systems:

1. **ProBuilderEditor**: Central controller and tool management
2. **Selection System**: Element selection and manipulation
3. **Tool System**: Unified tool interface and interaction
4. **Menu Actions**: Extensible action system
5. **UI Components**: Windows, overlays, and gizmos

## ProBuilderEditor - Central Controller

**Location**: `Editor/EditorCore/ProBuilderEditor.cs`

The main editor class that coordinates all ProBuilder functionality.

### Key Responsibilities:
- **Tool Management**: Activate/deactivate editing tools
- **Selection Coordination**: Manage mesh and element selection
- **Event Handling**: Process mouse/keyboard input
- **UI Updates**: Refresh editor windows and gizmos
- **Undo/Redo**: Integration with Unity's undo system

### Core Architecture:
```csharp
public sealed class ProBuilderEditor : IDisposable
{
    // Events for external systems
    public static event Action<IEnumerable<ProBuilderMesh>> selectionUpdated;
    public static event Action<SelectMode> selectModeChanged;
    public static event Action<IEnumerable<ProBuilderMesh>> afterMeshModification;
    
    // Current edit state
    public static SelectMode selectMode { get; set; }
    public static bool enabled { get; }
}
```

### Tool State Management:
ProBuilder maintains several tool states:
- **Object Mode**: Select entire ProBuilder objects
- **Vertex Mode**: Select and manipulate individual vertices
- **Edge Mode**: Select and manipulate edges
- **Face Mode**: Select and manipulate faces

## Selection System

**Location**: `Editor/EditorCore/MeshSelection.cs`

Manages selection of ProBuilder meshes and their elements.

### Selection Data Structure:
```csharp
public static class MeshSelection
{
    // Currently selected ProBuilder objects
    public static IEnumerable<ProBuilderMesh> topInternal { get; }
    
    // Active mesh (primary selection)
    public static ProBuilderMesh activeMesh { get; }
    
    // Element selections per mesh
    public static ReadOnlyCollection<MeshAndElementSelection> elementSelection { get; }
    
    // Selection bounds for gizmo positioning
    public static Bounds bounds { get; }
}
```

### Selection Types:
1. **Object Selection**: Entire ProBuilderMesh objects
2. **Element Selection**: Vertices, edges, or faces within meshes
3. **Mixed Selection**: Combination of objects and elements

### Selection Persistence:
The system maintains selection state across:
- **Edit Mode Changes**: Preserve selection when switching tools
- **Scene Changes**: Restore selection after scene reload
- **Undo Operations**: Proper selection handling with undo/redo

## Scene View Integration

### EditorSceneViewPicker
**Location**: `Editor/EditorCore/EditorSceneViewPicker.cs`

Handles mouse interaction and element picking in the Scene View.

### Picking Algorithm:
1. **Ray Casting**: Cast ray from mouse position
2. **Mesh Intersection**: Find intersection with ProBuilder meshes
3. **Element Identification**: Determine which vertex/edge/face was hit
4. **Selection Update**: Modify selection based on input modifiers

### Input Handling:
```csharp
// Mouse input processing
private void OnSceneGUI(SceneView sceneView)
{
    Event evt = Event.current;
    
    if (evt.type == EventType.MouseDown)
        HandleMouseDown(evt);
    else if (evt.type == EventType.MouseDrag)
        HandleMouseDrag(evt);
    else if (evt.type == EventType.MouseUp)
        HandleMouseUp(evt);
}
```

### Rectangle Selection:
Supports drag-selection of multiple elements:
1. **Start Drag**: Record initial mouse position
2. **Track Drag**: Update selection rectangle
3. **Element Testing**: Test which elements fall within rectangle
4. **Complete Selection**: Apply final selection state

## Tool System

ProBuilder integrates with Unity's EditorTools system for unified tool handling.

### Base Tool Classes:

#### EditorTool Integration:
```csharp
public abstract class ProBuilderTool : EditorTool
{
    public abstract void OnToolGUI(EditorWindow window);
    public virtual bool IsAvailable() => true;
    public virtual void OnActivated() { }
    public virtual void OnWillBeDeactivated() { }
}
```

### Manipulation Tools:

#### Position Tool
**Location**: `Editor/EditorCore/PositionTool.cs`
- **Function**: Move vertices, edges, or faces
- **Handles**: Unity's position handles with ProBuilder integration
- **Constraints**: Grid snapping, axis locking

#### Vertex Manipulation Tool  
**Location**: `Editor/EditorCore/VertexManipulationTool.cs`
- **Function**: Direct vertex editing with visual feedback
- **Features**: Multi-selection, proportional editing
- **Visualization**: Custom gizmos for selected elements

### Shape Tools:

#### Draw Shape Tool
**Location**: `Editor/EditorCore/DrawShapeTool.cs`
- **Function**: Create new shapes by drawing in Scene View
- **Interaction**: Click-and-drag to define shape bounds
- **Preview**: Real-time preview of shape being created

#### Poly Shape Tool
**Location**: `Editor/EditorCore/PolyShapeTool.cs`  
- **Function**: Create custom polygonal shapes
- **Workflow**: Click to place vertices, close to finish
- **Features**: Bezier curves, hole creation

## Gizmo and Handle System

### EditorHandleDrawing
**Location**: `Editor/EditorCore/EditorHandleDrawing.cs`

Provides custom drawing functions for ProBuilder's 3D handles and gizmos.

### Handle Types:
1. **Vertex Handles**: Small spheres for vertex selection
2. **Edge Handles**: Lines with midpoint indicators  
3. **Face Handles**: Filled polygons with normal indicators
4. **Center Handles**: Pivot points for transformations

### Drawing Optimization:
- **Culling**: Skip drawing handles outside view frustum
- **LOD**: Reduce detail for distant elements
- **Batching**: Group similar handle types for efficient rendering

### Custom Gizmos:
```csharp
public static void DrawVertexGizmos(ProBuilderMesh mesh, IEnumerable<int> vertices)
{
    foreach(int vertex in vertices)
    {
        Vector3 worldPos = mesh.transform.TransformPoint(mesh.positions[vertex]);
        
        // Draw selection indicator
        if (IsSelected(vertex))
            DrawSelectedVertexGizmo(worldPos);
        else
            DrawUnselectedVertexGizmo(worldPos);
    }
}
```

## Menu Action System

**Location**: `Editor/MenuActions/`

ProBuilder uses an extensible action system for tools and operations.

### MenuAction Base Class:
```csharp
[ProBuilderMenuAction]
public abstract class MenuAction
{
    public abstract ActionResult DoAction();
    public virtual bool enabled => true;
    public virtual string tooltip => "";
    public abstract string menuTitle { get; }
}
```

### Action Categories:
1. **Geometry Actions**: Extrude, subdivide, merge
2. **Selection Actions**: Select loops, grow selection
3. **Material Actions**: Apply materials, UV operations
4. **Export Actions**: OBJ export, mesh conversion

### Dynamic Menu Building:
Actions are automatically discovered and added to menus:
```csharp
// Attribute-based registration
[ProBuilderMenuAction]
public class ExtrudeFaces : MenuAction
{
    public override string menuTitle => "Extrude Faces";
    
    public override ActionResult DoAction()
    {
        return MeshSelection.selectedFaceCount > 0 
            ? ExtrudeSelectedFaces() 
            : ActionResult.UserCanceled;
    }
}
```

## UI Components

### ProBuilder Windows:

#### Shape Tool Window
- **Purpose**: Configure shape parameters
- **Features**: Real-time preview, parameter validation
- **Integration**: Updates shapes as parameters change

#### Material Palette
**Location**: `Editor/EditorCore/MaterialPalette.cs`
- **Purpose**: Quick material assignment
- **Features**: Drag-and-drop, material preview
- **Storage**: Persistent material collections

#### UV Editor
**Location**: `Editor/EditorCore/UVEditor.cs`
- **Purpose**: Visual UV coordinate editing
- **Features**: 2D UV view, manual coordinate adjustment
- **Tools**: UV selection, transformation, stitching

### Overlay System:
**Location**: `Editor/Overlays/`

ProBuilder uses Unity's Overlay system for floating UI panels:
- **Tool Settings**: Context-sensitive tool parameters
- **Element Info**: Display counts and statistics
- **Quick Actions**: Common operations in floating panels

## Preference System

**Location**: `Editor/EditorCore/PreferencesInternal.cs`

Manages user preferences and editor settings.

### Preference Categories:
1. **Tool Behavior**: Default tool settings, interaction modes
2. **Visual Settings**: Gizmo colors, handle sizes
3. **Performance**: Culling distances, update frequencies
4. **Shortcuts**: Keyboard bindings for actions

### Settings Storage:
```csharp
[UserSetting("Mesh Editing", "Handle Size")]
internal static Pref<float> s_HandleSize = new Pref<float>("mesh.handleSize", 2.5f);

[UserSetting("General", "Show Scene Info")]  
internal static Pref<bool> s_ShowSceneInfo = new Pref<bool>("editor.showSceneInfo", false);
```

## Performance Considerations

### Update Optimization:
- **Dirty Flags**: Track what needs updating
- **Frame Budgeting**: Spread expensive operations across frames
- **Conditional Updates**: Only update visible elements

### Memory Management:
- **Handle Pooling**: Reuse handle objects
- **Texture Caching**: Cache gizmo textures and materials
- **Selection Caching**: Cache selection bounds and counts

### Scene View Optimization:
```csharp
// Example: Conditional gizmo drawing
if (ShouldDrawHandles())
{
    // Only draw if handles are visible and relevant
    DrawElementHandles();
}
```

## Integration with Unity Systems

### EditorTools Framework:
ProBuilder tools inherit from Unity's `EditorTool` base class:
```csharp
[EditorTool("ProBuilder/Position Tool")]
public class PositionTool : ProBuilderTool
{
    public override void OnToolGUI(EditorWindow window)
    {
        // Tool-specific GUI handling
    }
}
```

### Undo System Integration:
All operations properly integrate with Unity's undo:
```csharp
public override ActionResult DoAction()
{
    Undo.RecordObjects(Selection.transforms, "Action Name");
    
    // Perform operation
    
    ProBuilderEditor.Refresh();
    return ActionResult.Success;
}
```

### Scene View Integration:
Custom drawing and interaction in Scene View:
```csharp
[InitializeOnLoad]
public class SceneViewIntegration
{
    static SceneViewIntegration()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    static void OnSceneGUI(SceneView sceneView)
    {
        // Custom scene view drawing and interaction
    }
}
```

## Event System

### Core Events:
```csharp
// Selection changes
ProBuilderEditor.selectionUpdated += OnSelectionChanged;

// Tool mode changes  
ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

// Mesh modifications
ProBuilderEditor.afterMeshModification += OnMeshModified;
```

### Event Flow:
1. **User Input** → Scene View handlers
2. **Selection Change** → Update UI and gizmos  
3. **Tool Activation** → Configure tool-specific UI
4. **Mesh Modification** → Refresh displays and validate

## Debugging and Diagnostics

### Debug Tools:
**Location**: `Debug/`

1. **Mesh Validation**: Check for topology errors
2. **Performance Profiling**: Measure operation times
3. **Selection Debugging**: Visualize selection state
4. **Handle Debugging**: Show handle bounds and interactions

### Debug Visualization:
```csharp
#if PROBUILDER_DEBUG
    // Visualize internal data structures
    DebugDrawWingedEdges(mesh);
    DebugDrawSharedVertices(mesh);
#endif
```

This editor architecture provides a robust foundation for 3D modeling tools while maintaining good performance and integration with Unity's editor systems.