using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UObject = UnityEngine.Object;

class TempMenuItems : EditorWindow
{
    static float GetEdgeRotation(ProBuilderMesh mesh, Edge edge)
    {
        var dir = mesh.texturesInternal[edge.b] - mesh.texturesInternal[edge.a];
        return Vector2.SignedAngle(Vector2.up, dir);
    }

    [MenuItem("Tools/Temp Menu Item &d", false, 1000)]
    static void MenuInit()
    {
        var rotation = 35f;

        var mesh = ShapeGenerator.CreateShape(ShapeType.Sprite);
        var face = mesh.faces.First();
        var edge = face.edgesInternal.First();

        // Verify that UVs are actually rotated
//        Assume.That(face.manualUV, Is.EqualTo(false));

        var unwrap = face.uv;
        unwrap.rotation = rotation;
        face.uv = unwrap;

        mesh.Refresh(RefreshMask.UV);

        Debug.Log("rotation: " + face.uv.rotation + " -> " + GetEdgeRotation(mesh, edge));
//        Assume.That(GetEdgeRotation(mesh, edge), Is.EqualTo(rotation));

        var origins = mesh.textures.ToArray();

        face.uv = AutoUnwrapSettings.defaultAutoUnwrapSettings;
        face.manualUV = true;

//        Assume.That(face.uv.rotation, Is.EqualTo(0f));
//        Assume.That(face.manualUV, Is.EqualTo(true));

        UVEditing.SetAutoAndAlignUnwrapParamsToUVs(mesh, new [] { face });

//        Assert.That(face.uv.rotation, Is.EqualTo(rotation));
    }

    [MenuItem("Tools/Recompile")]
    static void Recompile()
    {
        if (ScriptingSymbolManager.ContainsDefine("PROBUILDER_RECOMPILE_FLAG"))
            ScriptingSymbolManager.RemoveScriptingDefine("PROBUILDER_RECOMPILE_FLAG");
        else
            ScriptingSymbolManager.AddScriptingDefine("PROBUILDER_RECOMPILE_FLAG");
    }
}
