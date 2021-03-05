using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace UnityEditor.ProBuilder
{
    static partial class EditorHandleDrawing
    {
        const float k_DefaultGizmoSize = .2f;

        internal static void DrawGizmo(Vector3 position, Quaternion rotation, float size = -1f)
        {
            var p = position;
            size = HandleUtility.GetHandleSize(p) * size < 0f ? .5f : size;

            using (var lineDrawer = new LineDrawingScope(Color.green, -1f, CompareFunction.Always))
            {
                lineDrawer.DrawLine(p, p + rotation * Vector3.up * size);
                lineDrawer.SetColor(Color.red);
                lineDrawer.DrawLine(p, p + rotation * Vector3.right * size);
                lineDrawer.SetColor(Color.blue);
                lineDrawer.DrawLine(p, p + rotation * Vector3.forward * size);
            }
        }

        internal static void DrawGizmo(Vector3 position, Matrix4x4 matrix, float size = -1f)
        {
            var p = matrix.MultiplyPoint3x4(position);
            size = HandleUtility.GetHandleSize(p) * size < 0f ? k_DefaultGizmoSize : size;

            using (var lineDrawer = new LineDrawingScope(Color.green, -1f, CompareFunction.Always))
            {
                lineDrawer.DrawLine(p, p + matrix.MultiplyVector(Vector3.up) * size);
                lineDrawer.SetColor(Color.red);
                lineDrawer.DrawLine(p, p + matrix.MultiplyVector(Vector3.right) * size);
                lineDrawer.SetColor(Color.blue);
                lineDrawer.DrawLine(p, p + matrix.MultiplyVector(Vector3.forward) * size);
            }
        }

        internal static void DrawTransformOriginGizmo(Matrix4x4 matrix, Vector3 direction,  float size = -1f)
        {
            var p = matrix.MultiplyPoint(Vector3.zero);
            var s = HandleUtility.GetHandleSize(p);
            var d = size < 0f ? k_DefaultGizmoSize : size;
            var e = Event.current.type;

            Handles.color = Color.gray;
            Handles.DotHandleCap(0, p, Quaternion.identity, s * dotCapSize, e);
            Handles.DotHandleCap(0, p + matrix.MultiplyVector(direction) * d, Quaternion.identity, s * dotCapSize, e);
            Handles.DrawLine(p, p + matrix.MultiplyVector(direction) * d);
            Handles.color = Color.white;
        }

        public struct PointDrawingScope : IDisposable
        {
            Color m_Color;
            CompareFunction m_ZTest;
            bool m_IsDisposed;
            Mesh m_Mesh;
            List<Vector3> m_Points;
            List<int> m_Indices;
            Matrix4x4 m_Matrix;

            public Color color
            {
                get { return m_Color; }
                set
                {
                    End();
                    m_Color = value;
                    Begin();
                }
            }

            public CompareFunction zTest
            {
                get { return m_ZTest; }

                set
                {
                    End();
                    m_ZTest = value;
                    Begin();
                }
            }

            public Matrix4x4 matrix
            {
                get { return m_Matrix; }
                set { m_Matrix = value; }
            }

            public PointDrawingScope(Color color, CompareFunction zTest = CompareFunction.LessEqual)
            {
                m_Color = color;
                m_ZTest = zTest;
                m_IsDisposed = false;
                m_Mesh = meshPool.Dequeue();
                m_Points = new List<Vector3>(64);
                m_Indices = new List<int>(64);
                m_Matrix = Matrix4x4.identity;
                Begin();
            }

            void Begin()
            {
                vertMaterial.SetColor("_Color", color);
                vertMaterial.SetFloat("_HandleZTest", (int)zTest);

                if (!vertMaterial.SetPass(0))
                    throw new Exception("Failed initializing vertex material.");

                m_Points.Clear();
            }

            void End()
            {
                m_Mesh.Clear();

                if (BuiltinMaterials.geometryShadersSupported)
                {
                    for (int i = 0, c = m_Points.Count; i < c; ++i)
                        m_Indices.Add(i);

                    m_Mesh.SetVertices(m_Points);

#if UNITY_2019_3_OR_NEWER
                    m_Mesh.SetIndices(m_Indices, MeshTopology.Points, 0, false);
#else
                    m_Mesh.SetIndices(m_Indices.ToArray(), MeshTopology.Points, 0, false);
#endif
                }
                else
                {
                    MeshHandles.CreatePointBillboardMesh(m_Points, m_Mesh);
                }

                Graphics.DrawMeshNow(m_Mesh, m_Matrix, 0);
            }

            public void Dispose()
            {
                if (m_IsDisposed)
                    return;

                m_IsDisposed = true;

                End();

                if(m_Mesh != null)
                    meshPool.Enqueue(m_Mesh);
            }

            public void Draw(Vector3 point)
            {
                m_Points.Add(point);
            }
        }

        public struct LineDrawingScope : IDisposable
        {
            bool m_Wire;
            bool m_LineTopology;
            Color m_Color;
            float m_Thickness;
            CompareFunction m_ZTest;
            Matrix4x4 m_Matrix;
            bool m_IsDisposed;

            Mesh m_LineMesh;
            List<Vector3> m_Positions;
            List<Vector4> m_Tangents;
            List<Color> m_Colors;
            List<int> m_Indices;

            public Color color
            {
                get { return m_Color; }
            }

            public void SetColor(Color color)
            {
                if (!m_Wire)
                    End();
                m_Color = color;
                if (!m_Wire)
                    Begin();
            }

            public float thickness
            {
                get { return m_Thickness; }
            }

            public CompareFunction zTest
            {
                get { return m_ZTest; }
            }

            public LineDrawingScope(Color color, float thickness = -1f, CompareFunction zTest = CompareFunction.LessEqual)
                : this(color, Matrix4x4.identity, thickness, zTest) { }

            public LineDrawingScope(Color color, Matrix4x4 matrix, float thickness = -1f, CompareFunction zTest = CompareFunction.LessEqual)
            {
                m_LineMesh = meshPool.Dequeue();
                m_IsDisposed = false;
                m_Matrix = matrix;
                m_Color = color;
                m_Thickness = thickness < 0f ? s_EdgeLineSize : thickness;
                m_ZTest = zTest;

                m_Positions = new List<Vector3>(4);
                m_Tangents = new List<Vector4>(4);
                m_Colors = new List<Color>(4);
                m_Indices = new List<int>(4);

                m_Wire = m_Thickness < k_MinLineWidthForGeometryShader || lineMaterial == null;
                m_LineTopology = m_Wire || BuiltinMaterials.geometryShadersSupported;

                Begin();
            }

            void Begin()
            {
                if (!m_Wire)
                {
                    lineMaterial.SetColor("_Color", color);
                    lineMaterial.SetFloat("_Scale", thickness * EditorGUIUtility.pixelsPerPoint);
                    lineMaterial.SetFloat("_HandleZTest", (int)zTest);
                }

                if (m_Wire || !lineMaterial.SetPass(0))
                {
#if UNITY_2019_1_OR_NEWER
                    HandleUtility.ApplyWireMaterial(zTest);
#else
                    if (s_ApplyWireMaterial == null)
                    {
                        s_ApplyWireMaterial = typeof(HandleUtility).GetMethod(
                                "ApplyWireMaterial",
                                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic,
                                null,
                                new System.Type[] { typeof(CompareFunction) },
                                null);

                        if (s_ApplyWireMaterial == null)
                            throw new Exception("Failed to find wire material, stopping draw lines.");
                    }

                    s_ApplyWireMaterialArgs[0] = zTest;
                    s_ApplyWireMaterial.Invoke(null, s_ApplyWireMaterialArgs);
#endif
                }
            }

            void End()
            {
                m_LineMesh.Clear();
                m_LineMesh.SetVertices(m_Positions);
                if(m_Wire)
                    m_LineMesh.SetColors(m_Colors);
                else
                    m_LineMesh.SetTangents(m_Tangents);
                m_LineMesh.subMeshCount = 1;
#if UNITY_2019_3_OR_NEWER
                m_LineMesh.SetIndices(m_Indices, m_LineTopology ? MeshTopology.Lines : MeshTopology.Quads, 0);
#else
                m_LineMesh.SetIndices(m_Indices.ToArray(), m_LineTopology ? MeshTopology.Lines : MeshTopology.Quads, 0);
#endif

                Graphics.DrawMeshNow(m_LineMesh, m_Matrix);

                m_Positions.Clear();
                m_Tangents.Clear();
                m_Colors.Clear();
                m_Indices.Clear();
            }

            public void DrawLine(Vector3 a, Vector3 b)
            {
                var count = m_Positions.Count;

                if (!m_Wire && !m_LineTopology)
                {
                    Vector3 c = b + (b - a);

                    m_Tangents.Add(new Vector4(b.x, b.y, b.z, 1f));
                    m_Positions.Add(a);
                    m_Indices.Add(count + 0);

                    m_Tangents.Add(new Vector4(b.x, b.y, b.z, -1f));
                    m_Positions.Add(a);
                    m_Indices.Add(count + 1);

                    m_Tangents.Add(new Vector4(c.x, c.y, c.z, -1f));
                    m_Positions.Add(b);
                    m_Indices.Add(count + 2);

                    m_Tangents.Add(new Vector4(c.x, c.y, c.z, 1f));
                    m_Positions.Add(b);
                    m_Indices.Add(count + 3);
                }
                else
                {
                    m_Colors.Add(color);
                    m_Positions.Add(a);
                    m_Indices.Add(count + 0);

                    m_Colors.Add(color);
                    m_Positions.Add(b);
                    m_Indices.Add(count + 1);
                }
            }

            public void Dispose()
            {
                if (m_IsDisposed)
                    return;
                m_IsDisposed = true;
                End();
                if(m_LineMesh != null)
                    meshPool.Enqueue(m_LineMesh);
            }
        }

        internal class TriangleDrawingScope : IDisposable
        {
            Color m_Color;
            CompareFunction m_ZTest;
            bool m_IsDisposed;

            public Color color
            {
                get { return m_Color; }
                set
                {
                    End();
                    m_Color = value;
                    Begin();
                }
            }

            public CompareFunction zTest
            {
                get { return m_ZTest; }

                set
                {
                    End();
                    m_ZTest = value;
                    Begin();
                }
            }

            public TriangleDrawingScope(Color color, CompareFunction zTest = CompareFunction.LessEqual)
            {
                m_Color = color;
                m_ZTest = zTest;
                Begin();
            }

            void Begin()
            {
                faceMaterial.SetColor("_Color", color);
                faceMaterial.SetFloat("_HandleZTest", (int)zTest);

                if (!faceMaterial.SetPass(0))
                    throw new Exception("Failed initializing face material.");

                GL.PushMatrix();
                GL.Begin(GL.TRIANGLES);
            }

            void End()
            {
                GL.End();
                GL.PopMatrix();
            }

            public void Dispose()
            {
                if (m_IsDisposed)
                    return;
                m_IsDisposed = true;
                End();
            }

            public void Draw(Vector3 a, Vector3 b, Vector3 c)
            {
                GL.Vertex(a);
                GL.Vertex(b);
                GL.Vertex(c);
            }
        }
    }
}
