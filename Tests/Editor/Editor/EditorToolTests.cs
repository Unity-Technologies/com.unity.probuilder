using NUnit.Framework;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

public class EditorToolTests
{
    [SetUp]
    public void SetUp()
    {
        var windows = Resources.FindObjectsOfTypeAll<ProBuilderEditor>();
        for (int i = windows.Length - 1; i > -1; i--)
            windows[i].Close();
    }

    [Test]
    public void SetToolKeepsContext()
    {
        var editor = EditorWindow.GetWindow<ProBuilderEditor>();
        Assume.That(editor, Is.Not.Null);
        ProBuilderEditor.selectMode = SelectMode.Face;
        Assume.That(ProBuilderEditor.selectMode, Is.EqualTo(SelectMode.Face));
        Assume.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderMoveTool)));
        Tools.current = Tool.Rotate;
        Assert.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderRotateTool)));
    }

    [Test]
    public void SetSelectModeSetsTool()
    {
        var editor = EditorWindow.GetWindow<ProBuilderEditor>();
        Assume.That(editor, Is.Not.Null);
        ProBuilderEditor.selectMode = SelectMode.None;
        Assume.That(ProBuilderEditor.selectMode, Is.EqualTo(SelectMode.None));
        Tools.current = Tool.Move;

        ProBuilderEditor.selectMode = SelectMode.Face;
        Assert.That(ToolManager.activeToolType, Is.EqualTo(typeof(ProbuilderMoveTool)));
    }
}
