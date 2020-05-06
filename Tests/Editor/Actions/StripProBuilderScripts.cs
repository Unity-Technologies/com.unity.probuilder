using UObject = UnityEngine.Object;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor;

public class StripProBuilderScripts
{
    bool m_OpenedWindow = false;
    ProBuilderMesh m_cube;
    GameObject m_cube_obj;

    [Test]
    public void Strip_ProBuilder_Scripts()
    {
        // make sure the ProBuilder window is open
        if (ProBuilderEditor.instance == null)
        {
            ProBuilderEditor.MenuOpenWindow();
            m_OpenedWindow = true;
        }

        UVEditor.MenuOpenUVEditor();

        m_cube = ShapeGenerator.CreateShape(ShapeType.Cube);
        m_cube_obj = m_cube.gameObject;
        UnityEditor.ProBuilder.EditorUtility.InitObject(m_cube);

        Assume.That(m_cube_obj.GetComponent<ProBuilderMesh>() != null);
        Assume.That(m_cube_obj.GetComponent<PolyShape>() != null);

        EditorApplication.ExecuteMenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Actions/Strip All ProBuilder Scripts in Scene");

        Assert.That(m_cube_obj.GetComponent<ProBuilderMesh>() == null);
        Assert.That(m_cube_obj.GetComponent<PolyShape>() == null);


        // close editor window if we had to open it
        if (m_OpenedWindow && ProBuilderEditor.instance != null)
        {
            ProBuilderEditor.instance.Close();
        }

        UObject.DestroyImmediate(m_cube_obj);
    }
}
