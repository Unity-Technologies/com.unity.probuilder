using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;
using MaterialEditor = UnityEditor.ProBuilder.MaterialEditor;

[Overlay(typeof(SceneView), k_Id, k_Name, true)]
sealed class ProBuilderToolsOverlay : ToolbarOverlay
{
    const string k_Id = "probuilder-tools";
    const string k_Name = "ProBuilder Tools";

    public ProBuilderToolsOverlay()
        : base(
            "ProBuilder/ShapeTool",
                "ProBuilder/PolyShape",
                "ProBuilder/MaterialEditor",
                "ProBuilder/SmoothingEditor",
                "ProBuilder/UVEditor",
                "ProBuilder/VertexColor",
                "ProBuilder/ObjectActions"
            ) {}
}

[EditorToolbarElement("ProBuilder/ShapeTool", typeof(SceneView))]
sealed class ShapeToolElement : EditorToolbarToggle
{
    string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Tools/EditShape.png";

    MenuToolToggle m_Action;

    public ShapeToolElement()
    {
        m_Action = EditorToolbarLoader.GetInstance<NewShapeToggle>();

        name = m_Action.menuTitle;
        tooltip = m_Action.tooltip.summary;
        icon = m_Action.icon;

        value = false;

        RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

        this.RegisterValueChangedCallback(OnToggleValueChanged);
    }

    void OnAttachedToPanel(AttachToPanelEvent evt)
    {
        ToolManager.activeToolChanged += OnActiveToolChanged;
    }

    void OnDetachFromPanel(DetachFromPanelEvent evt)
    {
        ToolManager.activeToolChanged -= OnActiveToolChanged;
    }

    void OnActiveToolChanged()
    {
        if(value && !ToolManager.IsActiveTool(m_Action.Tool))
           value = false;
    }


    void OnToggleValueChanged(ChangeEvent<bool> toggleValue)
    {
        if(toggleValue.newValue)
            m_Action.PerformAction();
        else if(ToolManager.IsActiveTool(m_Action.Tool))
            m_Action.EndActivation();
    }
}


[EditorToolbarElement("ProBuilder/PolyShape", typeof(SceneView))]
sealed class PolyShapeElement : EditorToolbarToggle
{
     string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Tools/PolyShape/CreatePolyShape.png";

     MenuToolToggle m_Action;

     public PolyShapeElement()
         : base()
     {
         m_Action = EditorToolbarLoader.GetInstance<NewPolyShapeToggle>();

         name = m_Action.menuTitle;
         tooltip = m_Action.tooltip.summary;
         icon = m_Action.icon;

         value = false;

         RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
         RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

         this.RegisterValueChangedCallback(OnToggleValueChanged);
     }

     void OnAttachedToPanel(AttachToPanelEvent evt)
     {
         ToolManager.activeToolChanged += OnActiveToolChanged;
     }

     void OnDetachFromPanel(DetachFromPanelEvent evt)
     {
         ToolManager.activeToolChanged -= OnActiveToolChanged;
     }

     void OnActiveToolChanged()
     {
         if(value && ToolManager.activeToolType != m_Action.Tool.GetType())
             value = false;
     }


     void OnToggleValueChanged(ChangeEvent<bool> toggleValue)
     {
         if(toggleValue.newValue)
             m_Action.PerformAction();
         else if(ToolManager.activeToolType == m_Action.Tool.GetType())
             m_Action.EndActivation();
     }
}

[EditorToolbarElement("ProBuilder/SmoothingEditor", typeof(SceneView))]
sealed class SmoothingEditorElement : EditorToolbarButton
{
    static readonly string k_Name = L10n.Tr("Smoothing Groups");
    const string k_IconPath = "Toolbar/SelectBySmoothingGroup";
    static readonly string k_Tooltip = L10n.Tr("Opens the Material Editor window.\n\nThe Material Editor window applies materials to selected faces or objects.");

    public SmoothingEditorElement()
        : base(k_Name, IconUtility.GetIcon(k_IconPath, EditorGUIUtility.isProSkin ? IconSkin.Pro : IconSkin.Light), OnClicked)
    {
        tooltip = k_Tooltip;
    }

    static void OnClicked()
    {
        MaterialEditor.MenuOpenMaterialEditor();
    }
}

[EditorToolbarElement("ProBuilder/MaterialEditor", typeof(SceneView))]
sealed class MaterialEditorElement : EditorToolbarButton
{
    static readonly string k_Name = L10n.Tr("Material Editor");
    const string k_IconPath = "Toolbar/SelectByMaterial";
    static readonly string k_Tooltip = L10n.Tr("Opens the Material Editor window.\n\nThe Material Editor window applies materials to selected faces or objects.");

    public MaterialEditorElement()
        : base(k_Name, IconUtility.GetIcon(k_IconPath, EditorGUIUtility.isProSkin ? IconSkin.Pro : IconSkin.Light), OnClicked)
    {
        tooltip = k_Tooltip;
    }

    static void OnClicked()
    {
        MaterialEditor.MenuOpenMaterialEditor();
    }
}

[EditorToolbarElement("ProBuilder/UVEditor", typeof(SceneView))]
sealed class UVEditorElement : EditorToolbarButton
{
    static readonly string k_Name = L10n.Tr("UV Editor");
    const string k_IconPath = "Toolbar/SceneManipUVs";
    static readonly string k_Tooltip = L10n.Tr("Opens the UV Editor window.\n\nThe UV Editor allows you to change how textures are rendered on this mesh.");

    public UVEditorElement()
        : base(k_Name, IconUtility.GetIcon(k_IconPath, EditorGUIUtility.isProSkin ? IconSkin.Pro : IconSkin.Light), OnClicked)
    {
        tooltip = k_Tooltip;
    }

    static void OnClicked()
    {
        UVEditor.MenuOpenUVEditor();
    }
}

[EditorToolbarElement("ProBuilder/VertexColor", typeof(SceneView))]
sealed class VertexColorElement : EditorToolbarButton
{
    static readonly string k_Name = L10n.Tr("Vertex Colors");
    const string k_IconPath = "Toolbar/SelectByVertexColor";
    static readonly string k_Tooltip = L10n.Tr("Opens the Vertex Color Palette.\n\nApply using Face mode for hard-edged colors.\nApply using Edge or Vertex mode for soft, blended colors.");

    public VertexColorElement()
        : base(k_Name, IconUtility.GetIcon(k_IconPath, EditorGUIUtility.isProSkin ? IconSkin.Pro : IconSkin.Light), OnClicked)
    {
        tooltip = k_Tooltip;
    }

    static void OnClicked()
    {
        VertexColorPalette.MenuOpenWindow();
    }
}

[EditorToolbarElement("ProBuilder/ObjectActions", typeof(SceneView))]
sealed class ObjectActionDropDown : EditorToolbarDropdown
{
    static List<MenuAction> s_ObjectActions;

    ObjectActionDropDown()
    {
        name = "Object Actions";

        s_ObjectActions = EditorToolbarLoader.GetActions();
        s_ObjectActions = s_ObjectActions.FindAll(x => x.group == ToolbarGroup.Object || x.group == ToolbarGroup.Entity);

        clicked += OpenObjectActionsDropdown;
    }

    void OpenObjectActionsDropdown()
    {
        GenericMenu menu = new GenericMenu();
        for (var i = 0; i < s_ObjectActions.Count; i++)
        {
            if(s_ObjectActions[i].enabled)
            {
                int selected = i;
                menu.AddItem(
                    new GUIContent(s_ObjectActions[i].menuTitle),
                    false,
                    () =>
                    {
                        var action = s_ObjectActions[selected];
                            if((action.optionsState & MenuAction.MenuActionState.VisibleAndEnabled) ==
                                MenuAction.MenuActionState.VisibleAndEnabled)
                                action.OpenSettingsWindow();
                            else
                                EditorUtility.ShowNotification(action.PerformAction().notification);
                    });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(s_ObjectActions[i].menuTitle));
            }
        }
        menu.DropDown(worldBound, true);
    }
}
