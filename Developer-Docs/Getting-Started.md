# Getting Started with ProBuilder Development

This guide provides practical information for developers working on ProBuilder, covering common development scenarios, debugging techniques, and best practices.

## Development Environment Setup

### Prerequisites:
- **Unity 6000.0+**: ProBuilder requires Unity 6000.0 or later
- **Package Manager**: Development typically done through Package Manager workflow
- **Git Knowledge**: Understanding of Git for version control
- **C# Proficiency**: Strong C# skills for Unity development

### Project Structure Understanding:
```
com.unity.probuilder/
├── Runtime/           # Game-ready code (no editor dependencies)
├── Editor/           # Editor-only code (UnityEditor namespace)
├── External/         # Third-party libraries  
├── Tests/            # Unit and integration tests
├── Debug/            # Debug tools and utilities
└── Developer-Docs/   # This documentation
```

### Build and Test:
ProBuilder follows Unity's package development patterns:
- **Assembly Definitions**: Separate assemblies for Runtime, Editor, and External code
- **Unity Test Framework**: Unit tests in Tests/ directory
- **Package Validation**: Automated validation for package requirements

## Common Development Scenarios

### 1. Fixing a Bug

#### Step 1: Identify the Component
Determine which part of the system is affected:

**UI/Interaction Issues** → `Editor/EditorCore/`
- Selection not working: Check `MeshSelection.cs`, `EditorSceneViewPicker.cs`
- Tool problems: Look in specific tool classes (`PositionTool.cs`, etc.)
- Menu actions: Check `MenuActions/` directory

**Mesh Operation Issues** → `Runtime/MeshOperations/`
- Extrusion problems: `ExtrudeElements.cs`
- Subdivision issues: `Subdivision.cs`, `ConnectElements.cs`
- Boolean operations: `External/CSG/`
- Triangulation: `Triangulation.cs`, `External/Poly2Tri/`

**Core Data Issues** → `Runtime/Core/`
- Mesh corruption: `ProBuilderMesh.cs`, `MeshValidation.cs`
- Selection problems: `SharedVertex.cs`, `Face.cs`
- UV issues: `AutoUnwrapSettings.cs`, `UVEditing.cs`

#### Step 2: Reproduce the Issue
Create minimal test case:
```csharp
[Test]
public void TestExtrudeFaceIssue()
{
    // Create simple test mesh
    var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
    var face = mesh.faces[0];
    
    // Perform operation that should work
    var result = mesh.Extrude(new[] { face }, ExtrudeMethod.FaceNormal, 1f);
    
    // Verify result
    Assert.IsNotNull(result);
    Assert.IsTrue(mesh.faces.Length > 6); // Should have more faces after extrude
}
```

#### Step 3: Debug and Fix
Use ProBuilder's debugging tools:
```csharp
// Enable verbose logging
Log.SetLogLevel(LogLevel.All);

// Validate mesh state
if (!mesh.IsValid())
{
    Debug.LogError("Mesh validation failed: " + mesh.GetValidationErrors());
}

// Check topology
var wingedEdges = WingedEdge.GetWingedEdges(mesh);
if (wingedEdges.Any(we => we.opposite == null))
{
    Debug.LogWarning("Non-manifold edges detected");
}
```

### 2. Adding a New Feature

#### Step 1: Determine Scope
Choose appropriate layer for your feature:

**New Mesh Operation** → `Runtime/MeshOperations/`
Example: Adding a new extrusion mode
```csharp
public static class ExtrudeElements
{
    public static Face[] ExtrudeWithTwist(ProBuilderMesh mesh, 
                                        IEnumerable<Face> faces, 
                                        float distance, 
                                        float twistAngle)
    {
        // Implementation here
    }
}
```

**New Editor Tool** → `Editor/EditorCore/`
Example: Adding a measurement tool
```csharp
[EditorTool("ProBuilder/Measure Tool")]
public class MeasureTool : ProBuilderTool
{
    public override void OnToolGUI(EditorWindow window)
    {
        // Tool implementation
    }
}
```

**New Menu Action** → `Editor/MenuActions/`
Example: Adding a new geometry operation
```csharp
[ProBuilderMenuAction]
public class CreateSpiral : MenuAction
{
    public override string menuTitle => "Create Spiral";
    
    public override ActionResult DoAction()
    {
        // Action implementation
    }
}
```

#### Step 2: Follow Established Patterns
Study existing similar features:
- **Extrusion patterns**: Look at `ExtrudeElements.cs`
- **Selection patterns**: Check `ElementSelection.cs`
- **UI patterns**: Examine existing tool implementations

#### Step 3: Add Tests
Create comprehensive tests for new functionality:
```csharp
[TestFixture]
public class SpiralCreationTests
{
    [Test]
    public void CreateSpiral_ValidParameters_CreatesValidMesh()
    {
        // Test implementation
    }
    
    [Test]
    public void CreateSpiral_InvalidParameters_HandlesGracefully()
    {
        // Error handling test
    }
}
```

### 3. Performance Optimization

#### Identify Bottlenecks:
Use Unity Profiler to find performance issues:
```csharp
// Add profiler markers to your code
using Unity.Profiling;

static readonly ProfilerMarker s_ExtrudeFacesMarker = new ProfilerMarker("ExtrudeFaces");

public static Face[] Extrude(...)
{
    using (s_ExtrudeFacesMarker.Auto())
    {
        // Your operation here
    }
}
```

#### Common Optimization Areas:
1. **Excessive Memory Allocation**: Use object pooling
2. **Redundant Calculations**: Cache expensive computations
3. **Inefficient Algorithms**: Replace with faster alternatives
4. **Unnecessary Updates**: Use dirty flags and batch operations

#### Example Optimization:
```csharp
// Before: Inefficient per-vertex normal calculation
foreach (var vertex in vertices)
{
    vertex.normal = CalculateVertexNormal(vertex);
}

// After: Batch normal calculation
var normals = CalculateVertexNormals(vertices);
for (int i = 0; i < vertices.Length; i++)
{
    vertices[i].normal = normals[i];
}
```

## Debugging Techniques

### 1. Visual Debugging

#### Gizmo Drawing:
```csharp
void OnDrawGizmos()
{
    if (Application.isPlaying && debugMesh != null)
    {
        // Draw vertex positions
        Gizmos.color = Color.red;
        foreach (var pos in debugMesh.positions)
        {
            Gizmos.DrawSphere(transform.TransformPoint(pos), 0.1f);
        }
        
        // Draw face normals
        Gizmos.color = Color.blue;
        foreach (var face in debugMesh.faces)
        {
            var center = debugMesh.GetFaceCenter(face);
            var normal = debugMesh.GetFaceNormal(face);
            Gizmos.DrawRay(transform.TransformPoint(center), 
                          transform.TransformDirection(normal));
        }
    }
}
```

#### Scene View Debugging:
```csharp
[InitializeOnLoad]
public class ProBuilderDebugDraw
{
    static ProBuilderDebugDraw()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    static void OnSceneGUI(SceneView sceneView)
    {
        // Custom debug visualization
        foreach (var mesh in FindObjectsOfType<ProBuilderMesh>())
        {
            DrawMeshDebugInfo(mesh);
        }
    }
}
```

### 2. Logging and Validation

#### Structured Logging:
```csharp
using UnityEngine.ProBuilder;

// Use ProBuilder's logging system
Log.Info("Starting mesh operation with {0} faces", faces.Length);
Log.Warning("Non-manifold edge detected at {0}", edgePosition);
Log.Error("Mesh validation failed: {0}", validationError);
```

#### Mesh Validation:
```csharp
public static bool ValidateMeshOperation(ProBuilderMesh mesh, string operationName)
{
    var errors = new List<string>();
    
    // Check basic mesh integrity
    if (mesh.faces == null || mesh.positions == null)
        errors.Add("Null mesh data");
        
    // Check face validity
    foreach (var face in mesh.faces)
    {
        if (face.indexes.Any(i => i >= mesh.vertexCount))
            errors.Add($"Face index out of bounds: {face}");
    }
    
    // Check shared vertex consistency
    if (!SharedVertex.AreConsistent(mesh.sharedVertices, mesh.vertexCount))
        errors.Add("Shared vertex inconsistency");
    
    if (errors.Any())
    {
        Log.Error($"{operationName} validation failed:\n{string.Join("\n", errors)}");
        return false;
    }
    
    return true;
}
```

### 3. Unit Test Debugging

#### Test-Driven Development:
```csharp
[TestFixture]
public class MeshOperationTests
{
    ProBuilderMesh testMesh;
    
    [SetUp]
    public void Setup()
    {
        testMesh = ShapeGenerator.CreateShape(ShapeType.Cube);
    }
    
    [Test]
    public void ExtrudeFace_SingleFace_CreatesCorrectTopology()
    {
        var originalFaceCount = testMesh.faceCount;
        var face = testMesh.faces[0];
        
        var result = testMesh.Extrude(new[] { face }, ExtrudeMethod.FaceNormal, 1f);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Length);
        Assert.Greater(testMesh.faceCount, originalFaceCount);
        
        // Validate mesh integrity
        Assert.IsTrue(ValidateMeshOperation(testMesh, "ExtrudeFace"));
    }
}
```

## Best Practices

### 1. Code Organization

#### Follow Namespace Conventions:
```csharp
// Runtime code
namespace UnityEngine.ProBuilder
{
    // Core classes, mesh operations
}

namespace UnityEngine.ProBuilder.MeshOperations  
{
    // Specific mesh algorithms
}

// Editor code
namespace UnityEditor.ProBuilder
{
    // Editor tools and UI
}
```

#### Use Proper Access Modifiers:
```csharp
// Public API - external users
public class ProBuilderMesh : MonoBehaviour

// Internal API - within ProBuilder
internal static class InternalUtility

// Private implementation details
private void UpdateSharedVertexLookup()
```

### 2. Error Handling

#### Graceful Degradation:
```csharp
public static ActionResult SubdivideFaces(ProBuilderMesh mesh, Face[] faces)
{
    try
    {
        if (!ValidateInput(mesh, faces))
            return ActionResult.UserCanceled;
            
        var result = PerformSubdivision(mesh, faces);
        
        if (!ValidateMeshOperation(mesh, "Subdivide"))
        {
            Log.Warning("Subdivision produced invalid geometry, reverting");
            return ActionResult.Failure;
        }
        
        return ActionResult.Success;
    }
    catch (Exception e)
    {
        Log.Error($"Subdivision failed: {e.Message}");
        return ActionResult.Failure;
    }
}
```

#### User-Friendly Messages:
```csharp
public override ActionResult DoAction()
{
    if (MeshSelection.selectedFaceCount == 0)
        return new ActionResult(ActionResult.Status.UserCanceled, 
                              "No faces selected for extrusion");
                              
    if (MeshSelection.selectedFaceCount > 1000)
        return new ActionResult(ActionResult.Status.UserCanceled,
                              "Too many faces selected (limit: 1000)");
}
```

### 3. Performance Guidelines

#### Memory Management:
```csharp
// Use object pooling for frequently allocated objects
private static readonly ObjectPool<List<int>> s_IntListPool = 
    new ObjectPool<List<int>>(() => new List<int>(), 
                             list => list.Clear());

public static void SomeOperation()
{
    var tempList = s_IntListPool.Get();
    try
    {
        // Use tempList
    }
    finally
    {
        s_IntListPool.Release(tempList);
    }
}
```

#### Batch Operations:
```csharp
// Prefer batch operations over individual element processing
public static void SetVertexColors(ProBuilderMesh mesh, 
                                 IEnumerable<int> vertices, 
                                 Color color)
{
    mesh.SetVertexColors(vertices.Select(i => new KeyValuePair<int, Color>(i, color)));
    mesh.RefreshColors();  // Single refresh call
}
```

### 4. Integration Patterns

#### Undo Integration:
```csharp
[ProBuilderMenuAction]
public class MyAction : MenuAction
{
    public override ActionResult DoAction()
    {
        var selectedMeshes = MeshSelection.topInternal.ToArray();
        
        // Record state for undo
        Undo.RecordObjects(selectedMeshes, menuTitle);
        
        // Perform operation
        foreach (var mesh in selectedMeshes)
        {
            DoOperationOnMesh(mesh);
        }
        
        // Refresh displays
        ProBuilderEditor.Refresh();
        
        return ActionResult.Success;
    }
}
```

## Testing Strategies

### 1. Unit Tests
Focus on individual algorithms and data structures:
```csharp
[Test]
public void SharedVertex_MergeVertices_MaintainsTopology()
{
    // Test shared vertex operations
}

[Test]  
public void Triangulation_ConvexPolygon_ProducesValidTriangles()
{
    // Test triangulation algorithms
}
```

### 2. Integration Tests
Test complete workflows:
```csharp
[UnityTest]
public IEnumerator CreateCube_ExtrudeFace_ProducesValidMesh()
{
    // Create mesh, perform operations, validate result
    yield return null;
}
```

### 3. Performance Tests
Measure operation performance:
```csharp
[Test, Performance]
public void ExtrudeFaces_1000Faces_CompletesInReasonableTime()
{
    // Performance benchmarking
}
```

## Resources and References

### Documentation:
- [Unity Editor Scripting](https://docs.unity3d.com/Manual/editor-EditorWindows.html)
- [Unity Package Development](https://docs.unity3d.com/Manual/CustomPackages.html)
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

### Algorithms:
- [Computational Geometry](https://en.wikipedia.org/wiki/Computational_geometry)
- [Mesh Processing](http://www.cs.cmu.edu/~kmcrane/Projects/ModelRepository/)
- [Real-Time Rendering](http://www.realtimerendering.com/)

### Tools:
- **Unity Profiler**: Performance analysis
- **Unity Test Runner**: Automated testing
- **Git**: Version control integration
- **Visual Studio/Rider**: IDE with Unity support

This guide should provide a solid foundation for ProBuilder development work. Remember to always validate your changes thoroughly and follow the established patterns and conventions.