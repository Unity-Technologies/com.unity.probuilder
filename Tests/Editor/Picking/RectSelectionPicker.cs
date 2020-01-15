using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.TestTools;
using UObject = UnityEngine.Object;

/// <summary>
/// This class ensures we generate proper lookup textures when we need them during a marquee select.
/// These textures must not render anything than our picking objects.
/// The setup phase will create a set of objects put at the front of a camera. As there is no selectable object,
/// we should only get white texture as a result.
/// Issues will likely to appear if changes have been made in graphics pipeline.
/// This set of tests should run against standard pipeline, URP and HDRP.
/// </summary>
class RectSelectionPicker
{
    ProBuilderMesh[] selectables;
    GameObject[] sceneObjects;
    Camera camera;

    void Setup()
    {
        camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
        camera.transform.position = Vector3.zero;
        camera.transform.rotation = Quaternion.identity;
        
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0, 5f);

        GameObject light = new GameObject("Light", typeof(Light));

        sceneObjects = new GameObject[]
        {
            cube,
            light
        };

        selectables = new ProBuilderMesh[]
        {
        };

        foreach (SceneView sv in SceneView.sceneViews)
            sv.drawGizmos = true;
    }

    void Cleanup()
    {
        for (int i = 0; i < sceneObjects.Length; ++i)
        {
            UObject.DestroyImmediate(sceneObjects[i].gameObject);
        }

        for (int i = 0; i < selectables.Length; i++)
        {
            UObject.DestroyImmediate(selectables[i].gameObject);
        }

        UObject.DestroyImmediate(camera.gameObject);
    }

    [Test]
    public void RectSelection_LookupTexture_RenderPickingObjectsOnly()
    {
        Setup();

        Rect selectionRect = new Rect(camera.pixelRect);
        selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
        selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

        Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, int>>();

        Texture2D tex = SelectionPickerRenderer.RenderSelectionPickerTexture(
            camera,
            selectables,
            true,
            out map,
            (int)selectionRect.width,
            (int)selectionRect.height
            );

        Assert.That(tex.GetPixels(), Is.All.EqualTo(Color.white), "Lookup textures is not entirely white. Must have rendered something wrong.");       
        
        Cleanup();
    }

    [Test]
    public void RectSelection_FaceLookupTexture_RenderPickingObjectsOnly()
    {
        Setup();

        Rect selectionRect = new Rect(camera.pixelRect);
        selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
        selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

        Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>> map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>>();

        Texture2D tex = SelectionPickerRenderer.RenderSelectionPickerTexture(
            camera,
            selectables,
            out map,
            (int)selectionRect.width,
            (int)selectionRect.height
            );

        Assert.That(tex.GetPixels(), Is.All.EqualTo(Color.white), "Lookup textures is not entirely white. Must have rendered something wrong.");

        Cleanup();
    }

    [Test]
    public void RectSelection_EdgeLookupTexture_RenderPickingObjectsOnly()
    {
        Setup();

        Rect selectionRect = new Rect(camera.pixelRect);
        selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
        selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

        Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>>();

        Texture2D tex = SelectionPickerRenderer.RenderSelectionPickerTexture(
            camera,
            selectables,
            true,
            out map,
            (int)selectionRect.width,
            (int)selectionRect.height
            );

        Assert.That(tex.GetPixels(), Is.All.EqualTo(Color.white), "Lookup textures is not entirely white. Must have rendered something wrong.");

        Cleanup();
    }
}
