using NUnit.Framework;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;


/// <summary>
/// Test the construction of the contextual menu in the Scene View.
/// It doesn't cover the cases where a MenuAction has a file menu entry.
/// </summary>
[ProBuilderMenuAction]
public class ConfigurableMenuAction : MenuAction
{
    internal const string actionName = "Action Without File Menu Entry";
    internal static bool userHasFileMenuEntry { get; set; }
    internal static SelectMode userSelectMode { get; set; }
    internal static bool userEnabled { get;  set; }

    public override ToolbarGroup group
    {
        get { return ToolbarGroup.Geometry; }
    }

    public override Texture2D icon => null;

    public override string iconPath => string.Empty;

    public override TooltipContent tooltip => new TooltipContent(
        actionName,
        @"This action should not have a file menu entry."
    );

    public ConfigurableMenuAction()
    {
    }

    protected override ActionResult PerformActionImplementation()
    {
        return ActionResult.Success;
    }

    public override SelectMode validSelectModes => userSelectMode;
    public override bool enabled => userEnabled;
    protected internal override bool hasFileMenuEntry => userHasFileMenuEntry;
}

public class ContextualMenuTests
{
    // Cases:
    // - MenuAction with hasFileMenuEntry = false should show in the Contextual Menu.
    // - MenuAction with hasFileMenuEntry = true should not show in the File Menu.
    // - MenuAction with validSelectModes not matching the current selection should be disabled.
    // - MenuAction with validSelectModes matching the current selection should be enabled.
    // - MenuAction with enabled = false should be disabled in the Contextual Menu.
    // - MenuAction with enabled = true should be enabled in the Contextual Menu.

    ProBuilderMesh m_PBMesh;

    [SetUp]
    public void Setup()
    {
        m_PBMesh = ShapeFactory.Instantiate(typeof(UnityEngine.ProBuilder.Shapes.Plane));
    }

    public void TearDown()
    {
        if (m_PBMesh)
            Object.DestroyImmediate(m_PBMesh.gameObject);
    }

    [Test]
    [TestCase(true, ExpectedResult = false)]
    [TestCase(false, ExpectedResult = true)]
    public bool MenuActionWithoutMenuItem_hasFileMenuEntry(bool hasFileMenuEntry)
    {
        MeshSelection.SetSelection(m_PBMesh.gameObject);
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ToolManager.SetActiveContext<PositionToolContext>();
        Tools.current = Tool.Move;
        ProBuilderEditor.selectMode = SelectMode.Face;
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ConfigurableMenuAction.userHasFileMenuEntry = hasFileMenuEntry;
        ConfigurableMenuAction.userSelectMode = SelectMode.Any;
        ConfigurableMenuAction.userEnabled = true;

        DropdownMenu menu = new DropdownMenu();
        PositionToolContext ctx = Resources.FindObjectsOfTypeAll<PositionToolContext>()?[0];
        Assume.That(ctx, Is.Not.Null);

        ctx.PopulateMenu(menu);
        menu.PrepareForDisplay(null);
        DropdownMenuAction foundInMenu = null;
        foreach (var t in menu.MenuItems())
        {
            if (t is not DropdownMenuAction menuAction)
                continue;

            if (menuAction.name == ConfigurableMenuAction.actionName)
            {
                foundInMenu = menuAction;
                break;
            }
        }

        Assert.That(foundInMenu, Is.Not.Null, "MenuAction should be present in the Contextual Menu regardless of hasFileMenuEntry value.");
        return (foundInMenu.status == DropdownMenuAction.Status.Normal);
    }

    [Test]
    [TestCase(SelectMode.Edge, ExpectedResult = false)]
    [TestCase(SelectMode.Face, ExpectedResult = true)]
    [TestCase(SelectMode.Vertex, ExpectedResult = false)]
    public bool MenuAction_SelectModeSetToFace_EnabledOnlyForFaceSelection(SelectMode mode)
    {
        MeshSelection.SetSelection(m_PBMesh.gameObject);
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ToolManager.SetActiveContext<PositionToolContext>();
        Tools.current = Tool.Move;
        ProBuilderEditor.selectMode = mode;
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ConfigurableMenuAction.userHasFileMenuEntry = false;
        ConfigurableMenuAction.userSelectMode = SelectMode.Face;
        ConfigurableMenuAction.userEnabled = true;

        DropdownMenu menu = new DropdownMenu();
        PositionToolContext ctx = Resources.FindObjectsOfTypeAll<PositionToolContext>()?[0];
        Assume.That(ctx, Is.Not.Null);

        ctx.PopulateMenu(menu);
        menu.PrepareForDisplay(null);
        DropdownMenuAction foundInMenu = null;
        foreach (var t in menu.MenuItems())
        {
            if (t is not DropdownMenuAction menuAction)
                continue;

            if (menuAction.name == ConfigurableMenuAction.actionName)
            {
                foundInMenu = menuAction;
                break;
            }
        }

        // MenuAction is expected to be present in the menu only when the mode matches.
        return (foundInMenu != null);
    }

    [Test]
    [TestCase(true, ExpectedResult = true)]
    [TestCase(false, ExpectedResult = false)]
    public bool MenuAction_enabledPropertyIsFollowedByMenu(bool enabled)
    {
        MeshSelection.SetSelection(m_PBMesh.gameObject);
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ToolManager.SetActiveContext<PositionToolContext>();
        Tools.current = Tool.Move;
        ProBuilderEditor.selectMode = SelectMode.Face;
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        ConfigurableMenuAction.userHasFileMenuEntry = false;
        ConfigurableMenuAction.userSelectMode = SelectMode.Face;
        ConfigurableMenuAction.userEnabled = enabled;

        DropdownMenu menu = new DropdownMenu();
        PositionToolContext ctx = Resources.FindObjectsOfTypeAll<PositionToolContext>()?[0];
        Assume.That(ctx, Is.Not.Null);

        ctx.PopulateMenu(menu);
        menu.PrepareForDisplay(null);
        DropdownMenuAction foundInMenu = null;
        foreach (var t in menu.MenuItems())
        {
            if (t is not DropdownMenuAction menuAction)
                continue;

            if (menuAction.name == ConfigurableMenuAction.actionName)
            {
                foundInMenu = menuAction;
                break;
            }
        }
        return (foundInMenu.status == DropdownMenuAction.Status.Normal);
    }
}
