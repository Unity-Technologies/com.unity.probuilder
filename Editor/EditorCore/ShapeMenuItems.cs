using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class ShapeMenuItems
    {
        const int k_Priority = 9;

        [MenuItem("GameObject/ProBuilder/Cube", false, k_Priority)]
        static void CreateCube() => CreateShape(ShapeType.Cube);

        [MenuItem("GameObject/ProBuilder/Stair", false, k_Priority)]
        static void CreateStair() => CreateShape(ShapeType.Stair);

        [MenuItem("GameObject/ProBuilder/Curved Stair", false, k_Priority)]
        static void CreateCurvedStair() => CreateShape(ShapeType.CurvedStair);

        [MenuItem("GameObject/ProBuilder/Prism", false, k_Priority)]
        static void CreatePrism() => CreateShape(ShapeType.Prism);

        [MenuItem("GameObject/ProBuilder/Cylinder", false, k_Priority)]
        static void CreateCylinder() => CreateShape(ShapeType.Cylinder);

        [MenuItem("GameObject/ProBuilder/Plane", false, k_Priority)]
        static void CreatePlane() => CreateShape(ShapeType.Plane);

        [MenuItem("GameObject/ProBuilder/Door", false, k_Priority)]
        static void CreateDoor() => CreateShape(ShapeType.Door);

        [MenuItem("GameObject/ProBuilder/Pipe", false, k_Priority)]
        static void CreatePipe() => CreateShape(ShapeType.Pipe);

        [MenuItem("GameObject/ProBuilder/Cone", false, k_Priority)]
        static void CreateCone() => CreateShape(ShapeType.Cone);

        [MenuItem("GameObject/ProBuilder/Sprite", false, k_Priority)]
        static void CreateSprite() => CreateShape(ShapeType.Sprite);

        [MenuItem("GameObject/ProBuilder/Arch", false, k_Priority)]
        static void CreateArch() => CreateShape(ShapeType.Arch);

        [MenuItem("GameObject/ProBuilder/Sphere", false, k_Priority)]
        static void CreateSphere() => CreateShape(ShapeType.Sphere);

        [MenuItem("GameObject/ProBuilder/Torus", false, k_Priority)]
        static void CreateTorus() => CreateShape(ShapeType.Torus);

        static void CreateShape(ShapeType shape)
        {
            var res = ShapeGenerator.CreateShape(shape, EditorUtility.newShapePivotLocation);
            Undo.RegisterCreatedObjectUndo(res.gameObject, $"Create {shape}");
            EditorUtility.InitObject(res);
        }
    }
}
