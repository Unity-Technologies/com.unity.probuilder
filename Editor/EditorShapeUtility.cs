using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEditor.ProBuilder
{
    internal static class EditorShapeUtility
    {
        static Dictionary<string, Shape> s_Prefs = new Dictionary<string, Shape>();

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
            var types = TypeCache.GetTypesDerivedFrom<Shape>();

            foreach (var type in types)
            {
                if (typeof(Shape).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var name = "ShapeBuilder." + type.Name;
                    var pref = ProBuilderSettings.Get(name, SettingsScope.Project, (Shape)Activator.CreateInstance(type));
                    s_Prefs.Add(name, pref);
                }
            }
        }

        public static void SaveParams<T>(T shape) where T : Shape
        {
            var name = "ShapeBuilder." + shape.GetType().Name;
            if (s_Prefs.TryGetValue(name, out var data))
            {
                data = shape;
                s_Prefs[name] = data;
                ProBuilderSettings.Set(name, data);
            }
        }

        public static Shape GetLastParams(Type type)
        {
            if (!typeof(Shape).IsAssignableFrom(type))
            {
                throw new ArgumentException(nameof(type));
            }
            var name = "ShapeBuilder." + type.Name;
            if (s_Prefs.TryGetValue(name, out var data))
            {
                if (data != null)
                    return (Shape)data;
            }
            try
            {
                return Activator.CreateInstance(type) as Shape;
            }
            catch
            {
                Debug.LogError($"Cannot create shape of type { type.ToString() } because it doesn't have a default constructor.");
            }
            return default;
        }

        public static Shape CreateShape(Type type, Shape refShape = null)
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

            if(refShape == null)
                shape = GetLastParams(shape.GetType());
            else
                shape.CopyShapeParameters(refShape);

            return shape;
        }

        public sealed class FaceData
        {
            public Vector3 CenterPosition;
            public Vector3 Normal;
            public Vector3[] Points;
            public Color m_Color = Color.white;

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

                if(Normal == Vector3.up || Normal == Vector3.down)
                    m_Color = Color.green;
                else if(Normal == Vector3.right || Normal == Vector3.left)
                    m_Color = Color.red;
                else if(Normal == Vector3.forward || Normal == Vector3.back)
                    m_Color = Color.blue;
            }
        }

        public static void UpdateFaces(Bounds bounds, FaceData[] faces)
        {
            if(faces.Length != 6)
                faces = new FaceData[6];

            Vector3 extents = bounds.extents;

            Vector3 pointX0Y0Z0 = new Vector3(-extents.x, -extents.y, -extents.z);
            Vector3 pointX1Y0Z0 = new Vector3(extents.x, -extents.y, -extents.z);
            Vector3 pointX0Y1Z0 = new Vector3(-extents.x, extents.y, -extents.z);
            Vector3 pointX0Y0Z1 = new Vector3(-extents.x, -extents.y, extents.z);
            Vector3 pointX1Y1Z0 = new Vector3(extents.x, extents.y, -extents.z);
            Vector3 pointX1Y0Z1 = new Vector3(extents.x, -extents.y, extents.z);
            Vector3 pointX0Y1Z1 = new Vector3(-extents.x, extents.y, extents.z);
            Vector3 pointX1Y1Z1 = new Vector3(extents.x, extents.y, extents.z);

            // -X
            var pos = -new Vector3(extents.x, 0, 0);
            faces[0].SetData(pos, -Vector3.right);
            faces[0].Points[0] = pointX0Y1Z1;
            faces[0].Points[1] = pointX0Y0Z1;
            faces[0].Points[2] = pointX0Y0Z0;
            faces[0].Points[3] = pointX0Y1Z0;

            // +X
            pos = new Vector3(extents.x, 0, 0);
            faces[1].SetData(pos, Vector3.right);
            faces[1].Points[0] = pointX1Y1Z1;
            faces[1].Points[1] = pointX1Y0Z1;
            faces[1].Points[2] = pointX1Y0Z0;
            faces[1].Points[3] = pointX1Y1Z0;

            // -Y
            pos = -new Vector3(0, extents.y, 0);
            faces[2].SetData(pos, -Vector3.up);
            faces[2].Points[0] = pointX1Y0Z1;
            faces[2].Points[1] = pointX0Y0Z1;
            faces[2].Points[2] = pointX0Y0Z0;
            faces[2].Points[3] = pointX1Y0Z0;

            // +Y
            pos = new Vector3(0, extents.y, 0);
            faces[3].SetData(pos, Vector3.up);
            faces[3].Points[0] = pointX1Y1Z1;
            faces[3].Points[1] = pointX0Y1Z1;
            faces[3].Points[2] = pointX0Y1Z0;
            faces[3].Points[3] = pointX1Y1Z0;

            // -Z
            pos = - new Vector3(0, 0, extents.z);
            faces[4].SetData(pos, -Vector3.forward);
            faces[4].Points[0] = pointX1Y1Z0;
            faces[4].Points[1] = pointX1Y0Z0;
            faces[4].Points[2] = pointX0Y0Z0;
            faces[4].Points[3] = pointX0Y1Z0;

            // +Z
            pos = new Vector3(0, 0, extents.z);
            faces[5].SetData(pos, Vector3.forward);
            faces[5].Points[0] = pointX1Y1Z1;
            faces[5].Points[1] = pointX1Y0Z1;
            faces[5].Points[2] = pointX0Y0Z1;
            faces[5].Points[3] = pointX0Y1Z1;

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

