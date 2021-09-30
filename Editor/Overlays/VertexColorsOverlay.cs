using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), k_Name, true)]
public class VertexColorsOverlay : Overlay
{
    public static VertexColorsOverlay instance;

    const string k_Name = "Vertex Color Palette";

    string k_UxmlPath = "Packages/com.unity.probuilder/Editor/Overlays/UXML/vertexcolors-menu.uxml";
    static VisualTreeAsset s_TreeAsset;

    VisualElement m_VertexColorElement = new VisualElement();

    List<Button> m_colorButtons = new List<Button>();

    bool m_Visible = false;
    bool visible
    {
        set
        {
            m_Visible = value;
            displayed = value;
        }
        get
        {
            return m_Visible && ToolManager.activeContextType == typeof(ProBuilderToolContext);
        }
    }

    public VertexColorsOverlay()
    {
        //this.m_HasMenuEntry = false;
        //this.floating = true;
        this.layout = Layout.Panel;
        //ToolManager.activeContextChanged += OnActiveContextChanged;
    }

    // void OnActiveContextChanged()
    // {
    //     this.displayed = visible;
    // }

    public override void OnCreated()
    {
        if(s_TreeAsset == null)
            s_TreeAsset = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath(k_UxmlPath, typeof(VisualTreeAsset));

        if(s_TreeAsset != null)
            s_TreeAsset.CloneTree(m_VertexColorElement);

        var quitButton = m_VertexColorElement.Q<Button>("Exit");
        quitButton.clicked += Hide;

        var palette = VertexColorPalette.GetLastUsedColorPalette();

        for(int i = 0; i < 16; i++)
        {
            var colorButton = m_VertexColorElement.Q<VisualElement>("ColorButton"+i);
            var button = colorButton.Q<Button>("ColorButton");
            var colorArea = colorButton.Q<VisualElement>("ColorArea");
            var alphaArea = colorButton.Q<VisualElement>("AlphaArea");

            var paletteColor = palette.colors[i];
            colorArea.style.backgroundColor = paletteColor;
            alphaArea.style.backgroundColor = Color.Lerp(Color.black, Color.white, paletteColor.a);

            m_colorButtons.Add(button);
        }

        instance = this;
    }

    public override void OnWillBeDestroyed()
    {
    }

    public override VisualElement CreatePanelContent()
    {
        return m_VertexColorElement;
    }

    public void Show()
    {
        visible = true;
    }

    public void Hide()
    {
        visible = false;
    }
}
