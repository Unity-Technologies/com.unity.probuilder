using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    [CustomEditor(typeof(ShapeComponent))]
    public class ShapeComponentEditor : Editor
    {
        SerializedProperty m_shape;
        static int s_CurrentIndex = 0;
        string[] m_ShapeTypes;

        TypeCache.TypeCollection m_AvailableShapeTypes;

        private void OnEnable()
        {
            m_AvailableShapeTypes = TypeCache.GetTypesDerivedFrom<Shape>();
            m_ShapeTypes = m_AvailableShapeTypes.Select(x => x.ToString()).ToArray();
            m_shape = serializedObject.FindProperty("m_shape");
            var fullName = m_shape.managedReferenceFullTypename;
            var typeName = fullName.Substring(fullName.LastIndexOf(' ') + 1);
            Type type = null;
            foreach(var shapeType in m_AvailableShapeTypes)
            {
                if(shapeType.ToString() == typeName)
                {
                    type = shapeType;
                    break;
                }
            }
            s_CurrentIndex = m_AvailableShapeTypes.IndexOf(type);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            s_CurrentIndex = EditorGUILayout.Popup(s_CurrentIndex, m_ShapeTypes);
            if (EditorGUI.EndChangeCheck())
            {
                foreach(var target in targets)
                {
                    ((ShapeComponent)target).SetShape(m_AvailableShapeTypes[s_CurrentIndex]);
                }
            }
            EditorGUILayout.PropertyField(m_shape, true);
            serializedObject.ApplyModifiedProperties();
        }
    }

    [AddComponentMenu("")]
    [RequireComponent(typeof(ProBuilderMesh))]
    public class ShapeComponent : MonoBehaviour
    {
        [SerializeReference]
        Shape m_shape = new Cube();

        ProBuilderMesh m_Mesh;
        Transform m_Transform;

        [HideInInspector]
        [SerializeField]
        Vector3 m_Size;

        public Vector3 size {
            get { return m_Size; }
            set { m_Size = value; }
        }

        public ProBuilderMesh mesh {
            get { return m_Mesh == null ? m_Mesh = GetComponent<ProBuilderMesh>() : m_Mesh; }
        }

        public Transform transform {
            get { return m_Transform == null ? m_Transform = GetComponent<Transform>() : m_Transform; }
        }

        // Bounds where center is in world space, size is mesh.bounds.size
        internal Bounds meshFilterBounds {
            get {
                var mb = mesh.mesh.bounds;
                return new Bounds(transform.TransformPoint(mb.center), mb.size);
            }
        }

        public void Rebuild(Bounds bounds, Quaternion rotation)
        {
            size = Math.Abs(bounds.size);
            transform.position = bounds.center;
            transform.rotation = rotation;
            RebuildMesh();
        }

        public void Rebuild()
        {
            RebuildMesh();
        }

        private void RebuildMesh()
        {
            m_shape.RebuildMesh(mesh, size);
        }


        public void SetShape(Type type)
        {
            if (type.IsAssignableFrom(typeof(Shape)))
                throw new ArgumentException("Type needs to derive from Shape");

            m_shape = Activator.CreateInstance(type) as Shape;
            Rebuild();
        }

        public void SetShape<T>() where T : Shape, new()
        {
            m_shape = new T();
            Rebuild();
            FitToSize();
        }

        // Assumes that mesh origin is {0,0,0}
        protected void FitToSize()
        {
            if (mesh.vertexCount < 1)
                return;
            var actual = mesh.mesh.bounds.size;
            var scale = size.DivideBy(actual);
            var positions = mesh.positionsInternal;
            for (int i = 0, c = mesh.vertexCount; i < c; i++)
                positions[i].Scale(scale);
            mesh.ToMesh();
            mesh.Rebuild();
        }
    }
}
