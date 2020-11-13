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
            shape = GetLastParams(shape.GetType());
            return shape;
        }

        public sealed class FaceData
        {
            public Vector3 CenterPosition;
            public Vector3 Normal;
            public EdgeData[] Edges;

            public bool IsVisible
            {
                get
                {
                    Vector3 worldDir = Handles.matrix.MultiplyVector(Normal).normalized;

                    Vector3 cameraDir;
                    if (Camera.current.orthographic)
                        cameraDir = -Camera.current.transform.forward;
                    else
                        cameraDir = (Camera.current.transform.position - Handles.matrix.MultiplyPoint(CenterPosition)).normalized;

                    return Vector3.Dot(cameraDir, worldDir) < 0;
                }
            }

            public FaceData()
            {
                Edges = new EdgeData[4];
            }

            public void SetData(Vector3 centerPosition, Vector3 normal)
            {
                CenterPosition = centerPosition;
                Normal = normal;
            }
        }

        public struct EdgeData
        {
            public Vector3 PointA;
            public Vector3 PointB;

            public Vector3 Center
            {
                get => ( (PointA + PointB) / 2.0f );
            }

            public EdgeData(Vector3 pointA, Vector3 pointB)
            {
                PointA = pointA;
                PointB = pointB;
            }
        }

        //Comparer for the edgesToDraw hashset
        public class EdgeDataComparer : IEqualityComparer<EdgeData>
        {
            public bool Equals(EdgeData edge1, EdgeData edge2)
            {
                bool result = edge1.PointA == edge2.PointA && edge1.PointB == edge2.PointB;
                result |= edge1.PointA == edge2.PointB && edge1.PointB == edge2.PointA;
                return result;
            }

            //Don't wan't to compare hashcode, only using equals
            public int GetHashCode(EdgeData edge) {return 0;}
        }

        public class BoundsState
        {
            public Matrix4x4 positionAndRotationMatrix;
            public Bounds boundsHandleValue;
        }

        public static void CopyColliderPropertiesToHandle(Transform transform, Bounds bounds, BoxBoundsHandle targetBoxBoundsHandle, bool isEditing, BoundsState activeBoundsState)
        {
            // when editing a shape, we don't bother doing the conversion from handle space bounds to model for the
            // active handle
            if (isEditing)
            {
                targetBoxBoundsHandle.center = activeBoundsState.boundsHandleValue.center;
                targetBoxBoundsHandle.size = activeBoundsState.boundsHandleValue.size;
                return;
            }

            var localToWorld = transform.localToWorldMatrix;
            var lossyScale = transform.lossyScale;

            targetBoxBoundsHandle.center = Handles.inverseMatrix * (localToWorld * bounds.center);
            targetBoxBoundsHandle.size = Vector3.Scale(bounds.size, lossyScale);
        }

        public static void CopyHandlePropertiesToCollider(BoxBoundsHandle boxBoundsHandle, BoundsState activeBoundsState)
        {
            Vector3 snappedHandleSize =
                ProBuilderSnapping.Snap(boxBoundsHandle.size, EditorSnapping.activeMoveSnapValue);
            //Find the scaling direction
            Vector3 centerDiffSign =
                ( boxBoundsHandle.center - activeBoundsState.boundsHandleValue.center ).normalized;
            Vector3 sizeDiffSign = ( boxBoundsHandle.size - activeBoundsState.boundsHandleValue.size ).normalized;
            Vector3 globalSign = Vector3.Scale(centerDiffSign, sizeDiffSign);
            //Set the center to the right position
            Vector3 center = activeBoundsState.boundsHandleValue.center +
                             Vector3.Scale(( snappedHandleSize - activeBoundsState.boundsHandleValue.size ) / 2f,
                                 globalSign);
            //Set new Bounding box value
            activeBoundsState.boundsHandleValue = new Bounds(center, snappedHandleSize);
        }

        public static void UpdateFaces(Bounds bounds, Vector3 center, FaceData[] faces,
            Dictionary<EdgeData, SimpleTuple<EdgeData, EdgeData>> edgeToNeighborsEdges)
        {
            if(faces.Length != 6)
                faces = new FaceData[6];

            Vector3 extents = bounds.extents;

            EdgeData edgeX1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z),
                                new Vector3(-extents.x, extents.y, extents.z));
            EdgeData edgeX2 = new EdgeData(new Vector3(extents.x, -extents.y, extents.z),
                                new Vector3(-extents.x, -extents.y, extents.z));
            EdgeData edgeX3 = new EdgeData(new Vector3(extents.x, extents.y, -extents.z),
                                new Vector3(-extents.x, extents.y, -extents.z));
            EdgeData edgeX4 = new EdgeData(new Vector3(extents.x, -extents.y, -extents.z),
                                new Vector3(-extents.x, -extents.y, -extents.z));

            EdgeData edgeY1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z),
                                new Vector3(extents.x, -extents.y, extents.z) );
            EdgeData edgeY2 = new EdgeData(new Vector3(-extents.x, extents.y, extents.z),
                                new Vector3(-extents.x, -extents.y, extents.z));
            EdgeData edgeY3 = new EdgeData(new Vector3(extents.x, extents.y, -extents.z),
                                new Vector3(extents.x, -extents.y, -extents.z));
            EdgeData edgeY4 = new EdgeData(new Vector3(-extents.x, extents.y, -extents.z),
                                new Vector3(-extents.x, -extents.y, -extents.z));

            EdgeData edgeZ1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z),
                                new Vector3(extents.x, extents.y, -extents.z));
            EdgeData edgeZ2 = new EdgeData(new Vector3(-extents.x, extents.y, extents.z),
                                new Vector3(-extents.x, extents.y, -extents.z));
            EdgeData edgeZ3 = new EdgeData(new Vector3(extents.x, -extents.y, extents.z),
                                new Vector3(extents.x, -extents.y, -extents.z));
            EdgeData edgeZ4 = new EdgeData(new Vector3(-extents.x, -extents.y, extents.z),
                                new Vector3(-extents.x, -extents.y, -extents.z));

            // -X
            var pos =  - new Vector3(extents.x, 0, 0);
            faces[0].SetData(pos, Vector3.right);
            faces[0].Edges[0] = edgeY2;
            faces[0].Edges[1] = edgeZ2;
            faces[0].Edges[2] = edgeZ4;
            faces[0].Edges[3] = edgeY4;

            // +X
            pos = new Vector3(extents.x, 0, 0);
            faces[1].SetData(pos, -Vector3.right);
            faces[1].Edges[0] = edgeY1;
            faces[1].Edges[1] = edgeZ1;
            faces[1].Edges[2] = edgeZ3;
            faces[1].Edges[3] = edgeY3;

            // -Y
            pos = - new Vector3(0, extents.y, 0);
            faces[2].SetData(pos, Vector3.up);
            faces[2].Edges[0] = edgeX2;
            faces[2].Edges[1] = edgeZ3;
            faces[2].Edges[2] = edgeZ4;
            faces[2].Edges[3] = edgeX4;

            // +Y
            pos = new Vector3(0, extents.y, 0);
            faces[3].SetData(pos, -Vector3.up);
            faces[3].Edges[0] = edgeX1;
            faces[3].Edges[1] = edgeZ1;
            faces[3].Edges[2] = edgeZ2;
            faces[3].Edges[3] = edgeX3;

            // -Z
            pos = - new Vector3(0, 0, extents.z);
            faces[4].SetData(pos, Vector3.forward);
            faces[4].Edges[0] = edgeX3;
            faces[4].Edges[1] = edgeY3;
            faces[4].Edges[2] = edgeY4;
            faces[4].Edges[3] = edgeX4;

            // +Z
            pos = new Vector3(0, 0, extents.z);
            faces[5].SetData(pos, -Vector3.forward);
            faces[5].Edges[0] = edgeX1;
            faces[5].Edges[1] = edgeY1;
            faces[5].Edges[2] = edgeY2;
            faces[5].Edges[3] = edgeX2;

            if(edgeToNeighborsEdges == null)
                return;

            if(edgeToNeighborsEdges.Count ==0)
            {
                edgeToNeighborsEdges.Add(edgeX1, new SimpleTuple<EdgeData, EdgeData>(edgeX2, edgeX3));
                edgeToNeighborsEdges.Add(edgeX2, new SimpleTuple<EdgeData, EdgeData>(edgeX4, edgeX1));
                edgeToNeighborsEdges.Add(edgeX3, new SimpleTuple<EdgeData, EdgeData>(edgeX1, edgeX4));
                edgeToNeighborsEdges.Add(edgeX4, new SimpleTuple<EdgeData, EdgeData>(edgeX3, edgeX2));

                edgeToNeighborsEdges.Add(edgeY1, new SimpleTuple<EdgeData, EdgeData>(edgeY3, edgeY2));
                edgeToNeighborsEdges.Add(edgeY2, new SimpleTuple<EdgeData, EdgeData>(edgeY1, edgeY4));
                edgeToNeighborsEdges.Add(edgeY3, new SimpleTuple<EdgeData, EdgeData>(edgeY4, edgeY1));
                edgeToNeighborsEdges.Add(edgeY4, new SimpleTuple<EdgeData, EdgeData>(edgeY2, edgeY3));

                edgeToNeighborsEdges.Add(edgeZ1, new SimpleTuple<EdgeData, EdgeData>(edgeZ2, edgeZ3));
                edgeToNeighborsEdges.Add(edgeZ2, new SimpleTuple<EdgeData, EdgeData>(edgeZ4, edgeZ1));
                edgeToNeighborsEdges.Add(edgeZ3, new SimpleTuple<EdgeData, EdgeData>(edgeZ1, edgeZ4));
                edgeToNeighborsEdges.Add(edgeZ4, new SimpleTuple<EdgeData, EdgeData>(edgeZ3, edgeZ2));
            }
            else
            {
                edgeToNeighborsEdges[edgeX1]= new SimpleTuple<EdgeData, EdgeData>(edgeX2, edgeX3);
                edgeToNeighborsEdges[edgeX2]= new SimpleTuple<EdgeData, EdgeData>(edgeX4, edgeX1);
                edgeToNeighborsEdges[edgeX3]= new SimpleTuple<EdgeData, EdgeData>(edgeX1, edgeX4);
                edgeToNeighborsEdges[edgeX4]= new SimpleTuple<EdgeData, EdgeData>(edgeX3, edgeX2);

                edgeToNeighborsEdges[edgeY1]= new SimpleTuple<EdgeData, EdgeData>(edgeY3, edgeY2);
                edgeToNeighborsEdges[edgeY2]= new SimpleTuple<EdgeData, EdgeData>(edgeY1, edgeY4);
                edgeToNeighborsEdges[edgeY3]= new SimpleTuple<EdgeData, EdgeData>(edgeY4, edgeY1);
                edgeToNeighborsEdges[edgeY4]= new SimpleTuple<EdgeData, EdgeData>(edgeY2, edgeY3);

                edgeToNeighborsEdges[edgeZ1]= new SimpleTuple<EdgeData, EdgeData>(edgeZ2, edgeZ3);
                edgeToNeighborsEdges[edgeZ2]= new SimpleTuple<EdgeData, EdgeData>(edgeZ4, edgeZ1);
                edgeToNeighborsEdges[edgeZ3]= new SimpleTuple<EdgeData, EdgeData>(edgeZ1, edgeZ4);
                edgeToNeighborsEdges[edgeZ4]= new SimpleTuple<EdgeData, EdgeData>(edgeZ3, edgeZ2);
            }
        }

    }
}

