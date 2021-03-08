using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    static class EditorShapeUtility
    {
        [UserSetting]
        public static Pref<bool> s_ResetUserPrefs = new Pref<bool>("ShapeComponent.ResetSettings", true);

        static Dictionary<string, Shape> s_Prefs = new Dictionary<string, Shape>();

        static Dictionary<string, Shape> prefs
        {
            get
            {
                if(s_ResetUserPrefs.value)
                    ResetPrefs();

                return s_Prefs;
            }
        }

        static Type[] s_AvailableShapeTypes = null;

        public static Type[] availableShapeTypes
        {
            get
            {
                if(s_AvailableShapeTypes == null)
                    s_AvailableShapeTypes = TypeCache.GetTypesWithAttribute<ShapeAttribute>().Where(t => t.BaseType == typeof(Shape)).ToArray();
                return s_AvailableShapeTypes;
            }
        }

        static string[] s_ShapeTypes;

        public static string[] shapeTypes
        {
            get
            {
                if( s_ShapeTypes == null)
                    s_ShapeTypes = availableShapeTypes.Select(x => ((ShapeAttribute)System.Attribute.GetCustomAttribute(x, typeof(ShapeAttribute))).name)
                        .ToArray();
                return s_ShapeTypes;
            }
        }

        public static string GetName<T>(T shape) where T : Shape
        {
            var type = shape == null ? typeof(T) : shape.GetType();
            if (Attribute.GetCustomAttribute(type, typeof(ShapeAttribute)) is ShapeAttribute attrib)
                return attrib.name;
            var str = type.ToString();
            return str.Substring(str.LastIndexOf('.') + 1);
        }

        static int s_MaxContentPerGroup = 6;

        public static int MaxContentPerGroup
        {
            get => s_MaxContentPerGroup;
        }

        static List<GUIContent[]> s_ShapeTypesGUILists;

        public static List<GUIContent[]> shapeTypesGUI
        {
            get
            {
                if(s_ShapeTypesGUILists == null)
                {
                    s_ShapeTypesGUILists = new List<GUIContent[]>();
                    string[] shapeTypeNames = availableShapeTypes.Select(x => ((ShapeAttribute)System.Attribute.GetCustomAttribute(x, typeof(ShapeAttribute))).name).ToArray();
                    GUIContent[] shapeTypesGUI = null;

                    int i;
                    for(i = 0; i < shapeTypeNames.Length; i++)
                    {
                        if(i % s_MaxContentPerGroup == 0)
                        {
                            if(shapeTypesGUI != null) s_ShapeTypesGUILists.Add(shapeTypesGUI);
                            int maxLength = Mathf.Min(s_MaxContentPerGroup, ( shapeTypeNames.Length - i ));
                            shapeTypesGUI = new GUIContent[maxLength];
                        }
                        var name = shapeTypeNames[i];
                        var texture = IconUtility.GetIcon("Tools/ShapeTool/" + name);
                        if(texture != null)
                            shapeTypesGUI[i % s_MaxContentPerGroup] = new GUIContent(texture, name);
                        else
                            shapeTypesGUI[i % s_MaxContentPerGroup] = new GUIContent(name, name);
                    }

                    if(shapeTypesGUI != null) s_ShapeTypesGUILists.Add(shapeTypesGUI);

                }
                return s_ShapeTypesGUILists;
            }
        }

        static EditorShapeUtility()
        {
            ResetPrefs();
        }

        static void ResetPrefs()
        {
            var types = TypeCache.GetTypesDerivedFrom<Shape>();

            foreach(var type in types)
            {
                if(typeof(Shape).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var name = "ShapeBuilder." + type.Name;
                    var pref = ProBuilderSettings.Get(name, SettingsScope.Project, (Shape) Activator.CreateInstance(type));
                    if(pref == null)
                        pref = (Shape) Activator.CreateInstance(type);

                    if(s_Prefs.ContainsKey(name))
                        s_Prefs[name] = pref;
                    else
                        s_Prefs.Add(name, pref);
                }
            }

            s_ResetUserPrefs.value = false;
        }

        public static void SaveParams<T>(T shape) where T : Shape
        {
            var name = "ShapeBuilder." + shape.GetType().Name;
            if (prefs.TryGetValue(name, out var data))
            {
                data.CopyShape(shape);
                prefs[name] = data;
                ProBuilderSettings.Set(name, data);
            }
        }

        public static void CopyLastParams(Shape shape, Type type)
        {
            if (!typeof(Shape).IsAssignableFrom(type))
            {
                throw new ArgumentException(nameof(type));
            }

            if(shape == null)
            {
                try
                {
                    shape = Activator.CreateInstance(type) as Shape;
                }
                catch
                {
                    Debug.LogError(
                        $"Cannot create shape of type {type.ToString()} because it doesn't have a default constructor.");
                }
            }

            var name = "ShapeBuilder." + type.Name;
            if (prefs.TryGetValue(name, out var data))
            {
                if (data != null)
                    shape.CopyShape(data);
            }
        }

        public static Shape CreateShape(Type type)
        {
            Shape shape = null;
            try
            {
                shape = Activator.CreateInstance(type) as Shape;
            }
            catch
            {
                Debug.LogError($"Cannot create shape of type { type.ToString() } because it doesn't have a default constructor.");
            }

            if(shape == null)
                return null;

            CopyLastParams(shape, type);

            return shape;
        }

        public sealed class FaceData
        {
            public Vector3 CenterPosition;
            public Vector3 Normal;
            public Vector3[] Points;
            public bool IsValid = true;

            public bool IsVisible
            {
                get
                {
                    Vector3 worldDir = Handles.matrix.MultiplyVector(Normal).normalized;

                    Vector3 cameraDir;
                    if (Camera.current.orthographic)
                        cameraDir = Camera.current.transform.forward;
                    else
                        cameraDir = (Handles.matrix.MultiplyPoint(CenterPosition) - Camera.current.transform.position).normalized;

                    return Vector3.Dot(cameraDir, worldDir) < 0;
                }
            }

            public FaceData()
            {
                Points = new Vector3[4];
            }

            public void SetData(Vector3 centerPosition, Vector3 normal)
            {
                CenterPosition = centerPosition;
                Normal = normal;
            }
        }

        public static void UpdateFaces(Bounds bounds, Vector3 scale, FaceData[] faces)
        {
            if(faces.Length != 6)
                faces = new FaceData[6];

            Vector3 extents = bounds.extents;

            Vector3 pointX0Y0Z0 = Vector3.Scale(new Vector3(-extents.x, -extents.y, -extents.z), scale);
            Vector3 pointX1Y0Z0 = Vector3.Scale(new Vector3(extents.x, -extents.y, -extents.z), scale);
            Vector3 pointX0Y1Z0 = Vector3.Scale(new Vector3(-extents.x, extents.y, -extents.z), scale);
            Vector3 pointX0Y0Z1 = Vector3.Scale(new Vector3(-extents.x, -extents.y, extents.z), scale);
            Vector3 pointX1Y1Z0 = Vector3.Scale(new Vector3(extents.x, extents.y, -extents.z), scale);
            Vector3 pointX1Y0Z1 = Vector3.Scale(new Vector3(extents.x, -extents.y, extents.z), scale);
            Vector3 pointX0Y1Z1 = Vector3.Scale(new Vector3(-extents.x, extents.y, extents.z), scale);
            Vector3 pointX1Y1Z1 = Vector3.Scale(new Vector3(extents.x, extents.y, extents.z), scale);

            var signs = Math.Sign(bounds.size);
            var pos = Vector3.zero;

            // -X
            pos = -new Vector3(extents.x * scale.x, 0, 0);
            faces[0].SetData(pos, -( scale.x * signs.x * Vector3.right ).normalized);
            faces[0].Points[0] = pointX0Y1Z1;
            faces[0].Points[1] = pointX0Y0Z1;
            faces[0].Points[2] = pointX0Y0Z0;
            faces[0].Points[3] = pointX0Y1Z0;
            faces[0].IsValid = Mathf.Abs(extents.x) > Mathf.Epsilon;

            // +X
            pos = new Vector3(extents.x * scale.x, 0, 0);
            faces[1].SetData(pos, ( scale.x * signs.x * Vector3.right ).normalized);
            faces[1].Points[0] = pointX1Y1Z1;
            faces[1].Points[1] = pointX1Y0Z1;
            faces[1].Points[2] = pointX1Y0Z0;
            faces[1].Points[3] = pointX1Y1Z0;
            faces[1].IsValid = Mathf.Abs(extents.x) > Mathf.Epsilon;

            // -Y
            pos = -new Vector3(0, extents.y * scale.y, 0);
            faces[2].SetData(pos, -( scale.y * signs.y * Vector3.up ).normalized);
            faces[2].Points[0] = pointX1Y0Z1;
            faces[2].Points[1] = pointX0Y0Z1;
            faces[2].Points[2] = pointX0Y0Z0;
            faces[2].Points[3] = pointX1Y0Z0;
            faces[2].IsValid = Mathf.Abs(extents.y) > Mathf.Epsilon;

            // +Y
            pos = new Vector3(0, extents.y * scale.y, 0);
            faces[3].SetData(pos, ( scale.y * signs.y * Vector3.up ).normalized);
            faces[3].Points[0] = pointX1Y1Z1;
            faces[3].Points[1] = pointX0Y1Z1;
            faces[3].Points[2] = pointX0Y1Z0;
            faces[3].Points[3] = pointX1Y1Z0;
            faces[3].IsValid = Mathf.Abs(extents.y) > Mathf.Epsilon;

            // -Z
            pos = -new Vector3(0, 0, extents.z * scale.z);
            faces[4].SetData(pos, -(scale.z * signs.z * Vector3.forward).normalized);
            faces[4].Points[0] = pointX1Y1Z0;
            faces[4].Points[1] = pointX1Y0Z0;
            faces[4].Points[2] = pointX0Y0Z0;
            faces[4].Points[3] = pointX0Y1Z0;
            faces[4].IsValid = Mathf.Abs(extents.z) > Mathf.Epsilon;

            // +Z
            pos = new Vector3(0, 0, extents.z * scale.z);
            faces[5].SetData(pos, (scale.z * signs.z * Vector3.forward).normalized);
            faces[5].Points[0] = pointX1Y1Z1;
            faces[5].Points[1] = pointX1Y0Z1;
            faces[5].Points[2] = pointX0Y0Z1;
            faces[5].Points[3] = pointX0Y1Z1;
            faces[5].IsValid = Mathf.Abs(extents.z) > Mathf.Epsilon;
        }

        internal static bool PointerIsInFace(FaceData face)
        {
            if(!face.IsVisible)
                return false;

            Vector2[] face2D = new Vector2[4];
            for(int i = 0; i < face.Points.Length; i++)
            {
                face2D[i] = HandleUtility.WorldToGUIPoint(face.Points[i]);
            }
            return PointInQuad2D(Event.current.mousePosition, face2D);
        }

        static bool PointInQuad2D(Vector2 point, Vector2[] quadPoints)
        {
            bool inQuad = true;

            Vector2[] points = { point, quadPoints[2], quadPoints[3] };
            inQuad &= SameSide(quadPoints[0], quadPoints[1], points);
            points[1] =  quadPoints[0]; // { point, quadPoints[0], quadPoints[3]};
            inQuad &= SameSide(quadPoints[1], quadPoints[2], points);
            points[2] =  quadPoints[1]; // { point, quadPoints[0], quadPoints[1]};
            inQuad &= SameSide(quadPoints[2], quadPoints[3], points);
            points[1] =  quadPoints[2]; // { point, quadPoints[2], quadPoints[1]};
            inQuad &= SameSide(quadPoints[3], quadPoints[0], points);

            return inQuad;
        }

        static bool SameSide(Vector2 pStart, Vector2 pEnd, Vector2[] points)
        {
            if(points.Length < 2)
                return true;

            var cpRef = Vector3.Cross(pEnd - pStart, points[0] - pStart);
            for(int i = 1; i < points.Length; i++)
            {
                var cp = Vector3.Cross(pEnd - pStart, points[i] - pStart);
                if(Vector3.Dot(cpRef, cp) < 0)
                    return false;
            }
            return true;
        }

    }
}

