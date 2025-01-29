using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using PBPlane = UnityEngine.ProBuilder.Shapes.Plane;
using PBSprite = UnityEngine.ProBuilder.Shapes.Sprite;

namespace UnityEditor.ProBuilder
{
    static class ShapeMenuItems
    {
        const int k_Priority = 9;

        [MenuItem("GameObject/ProBuilder/Cube", false, k_Priority)]
        static void CreateCube()
        {
            CreateShape(ShapeType.Cube);
            var shape = CreateDefaultShape(typeof(Cube), new Vector3(1f, 1f, 1f));
            
            // var uvs = new List<Vector4>();
            //
            // shape.mesh.GetUVs(0, uvs);
            // for (int i = 0; i < uvs.Count; ++i)
            //     uvs[i] += new Vector4(0.5f, 0.5f, 0f, 0f);
            // shape.mesh.SetUVs(0, uvs);
            // shape.mesh.RefreshUV(shape.mesh.faces);

            foreach (var face in shape.mesh.facesInternal)
                face.uv = new AutoUnwrapSettings(face.uv) { offset = new Vector2(-.5f, -.5f) };
            shape.mesh.RefreshUV(shape.mesh.faces);
        }

        [MenuItem("GameObject/ProBuilder/Stair", false, k_Priority)]
        static void CreateStair()
        {
            CreateShape(ShapeType.Stair);
            CreateDefaultShape(typeof(Stairs), new Vector3(2f, 2.5f, 4f));
        }

        [MenuItem("GameObject/ProBuilder/Curved Stair", false, k_Priority)]
        static void CreateCurvedStair()
        {
            CreateShape(ShapeType.CurvedStair);
            CreateDefaultShape(typeof(Stairs), new Vector3(8f, 2.5f, 4f), "CurvedStair", (shape =>
            {
                if (shape is Stairs stairs)
                {
                    stairs.stepsCount = 8;
                    stairs.circumference = 180f;
                    stairs.innerRadius = 2f;
                }
            } ));
        }

        [MenuItem("GameObject/ProBuilder/Prism", false, k_Priority)]
        static void CreatePrism()
        {
            CreateShape(ShapeType.Prism);
            CreateDefaultShape(typeof(Prism), new Vector3(1f, 1f, 1f));
        }

        [MenuItem("GameObject/ProBuilder/Cylinder", false, k_Priority)]
        static void CreateCylinder()
        {
            CreateShape(ShapeType.Cylinder);
            CreateDefaultShape(typeof(Cylinder), new Vector3(2f, 2f, 2f));
        }

        [MenuItem("GameObject/ProBuilder/Plane", false, k_Priority)]
        static void CreatePlane()
        {
            CreateShape(ShapeType.Plane);
            CreateDefaultShape(typeof(PBPlane), new Vector3(5f, 1f, 5f));
        }

        [MenuItem("GameObject/ProBuilder/Door", false, k_Priority)]
        static void CreateDoor()
        {
            CreateShape(ShapeType.Door);
            CreateDefaultShape(typeof(Door), new Vector3(3f, 2.5f, 1f));
        }

        [MenuItem("GameObject/ProBuilder/Pipe", false, k_Priority)]
        static void CreatePipe()
        {
            CreateShape(ShapeType.Pipe);
            CreateDefaultShape(typeof(Pipe), new Vector3(2f, 2f, 2f));
        }

        [MenuItem("GameObject/ProBuilder/Cone", false, k_Priority)]
        static void CreateCone()
        {
            CreateShape(ShapeType.Cone);
            CreateDefaultShape(typeof(Cone), new Vector3(1f, 1f, 1f));
        }

        [MenuItem("GameObject/ProBuilder/Sprite", false, k_Priority)]
        static void CreateSprite()
        {
            CreateShape(ShapeType.Sprite);
            CreateDefaultShape(typeof(PBSprite), new Vector3(1f, 0f, 1f));
        }

        [MenuItem("GameObject/ProBuilder/Arch", false, k_Priority)]
        static void CreateArch()
        {
            CreateShape(ShapeType.Arch);
            CreateDefaultShape(typeof(Arch), new Vector3(4f, 2f, 1f));
        }

        [MenuItem("GameObject/ProBuilder/Sphere", false, k_Priority)]
        static void CreateSphere()
        {
            CreateShape(ShapeType.Sphere);
            CreateDefaultShape(typeof(Sphere), new Vector3(1f, 1f, 1f));
        }

        [MenuItem("GameObject/ProBuilder/Torus", false, k_Priority)]
        static void CreateTorus()
        {
            CreateShape(ShapeType.Torus);
            CreateDefaultShape(typeof(Torus), new Vector3(2f, 0.6f, 2f));
        }

        static void CreateShape(ShapeType shape)
        {
            var res = ShapeGenerator.CreateShape(shape);
            Undo.RegisterCreatedObjectUndo(res.gameObject, $"Create {shape}");
            EditorUtility.InitObject(res);
        }
        
        static ProBuilderShape CreateDefaultShape(Type shapeType, Vector3 size, string nameOverride = "", Action<Shape> shapeBuiltinParamPostProcess = null)
        {
            var shape = EditorShapeUtility.CreateShape(shapeType, copyLastParams: false);
            shape.SetParametersToBuiltInShape();
            shapeBuiltinParamPostProcess?.Invoke(shape);
            
            var name = $"{shapeType.Name}";
            if (!string.IsNullOrEmpty(nameOverride))
                name = nameOverride;
            
            var shapeComp = new GameObject(name, typeof(ProBuilderShape)).GetComponent<ProBuilderShape>();
            Undo.RegisterCreatedObjectUndo(shapeComp.gameObject, $"Create {name}");
            shapeComp.SetShape(shape);
            shapeComp.size = size;
            shapeComp.shapeRotation = Quaternion.identity;
            shapeComp.mesh.renderer.sharedMaterial = EditorMaterialUtility.GetUserMaterial();
            
            EditorUtility.InitObject(shapeComp.mesh);
            shapeComp.UpdateShape();
            
            ProBuilderEditor.Refresh(false);
            SceneView.RepaintAll();

            return shapeComp;
        }
    }
}
