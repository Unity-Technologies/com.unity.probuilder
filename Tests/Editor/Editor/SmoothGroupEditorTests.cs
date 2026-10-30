using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UObject = UnityEngine.Object;

public class SmoothGroupEditorTests
{
    ProBuilderMesh m_Cube;
    SmoothGroupEditor m_Window;

    [SetUp]
    public void SetUp()
    {
        m_Cube = ShapeFactory.Instantiate<Cube>();
        MeshSelection.SetSelection(m_Cube.gameObject);
        MeshSelection.OnObjectSelectionChanged();
        m_Window = EditorWindow.GetWindow<SmoothGroupEditor>();
    }

    [TearDown]
    public void TearDown()
    {
        if (m_Window != null)
            m_Window.Close();

        Selection.activeGameObject = null;

        if (m_Cube != null)
            UObject.DestroyImmediate(m_Cube.gameObject);
    }

    [Test]
    public void DestroyingDisplayedMesh_DoesNotThrowWhenWindowRepaints()
    {
        UObject.DestroyImmediate(m_Cube.gameObject);

        m_Window.SendEvent(new Event { type = EventType.Layout });
        m_Window.SendEvent(new Event { type = EventType.Repaint });
    }
}
