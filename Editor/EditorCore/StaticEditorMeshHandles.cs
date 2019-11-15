using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;
using System.Reflection;
using UnityEngine.Rendering;

namespace UnityEditor.ProBuilder
{
    partial class EditorMeshHandles
    {
        const float k_DefaultGizmoSize = .2f;

        internal static void DrawGizmo(Vector3 position, Quaternion rotation, float size = -1f)
        {
            var p = position;
            size = HandleUtility.GetHandleSize(p) * size < 0f ? .5f : size;

            using (var lineDrawer = new LineDrawingScope(Color.green, -1f, CompareFunction.Always))
            {
                lineDrawer.DrawLine(p, p + rotation * Vector3.up * size);
                lineDrawer.color = Color.red;
                lineDrawer.DrawLine(p, p + rotation * Vector3.right * size);
                lineDrawer.color = Color.blue;
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
                lineDrawer.color = Color.red;
                lineDrawer.DrawLine(p, p + matrix.MultiplyVector(Vector3.right) * size);
                lineDrawer.color = Color.blue;
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

        internal struct PointDrawingScope : IDisposable
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
                m_Mesh = Get().m_MeshPool.Get();
                m_Points = new List<Vector3>(64);
                m_Indices = new List<int>(64);
                m_Matrix = Matrix4x4.identity;
                Begin();
            }

            void Begin()
            {
                Get().m_VertMaterial.SetColor("_Color", color);
                Get().m_VertMaterial.SetInt("_HandleZTest", (int)zTest);

                if (!Get().m_VertMaterial.SetPass(0))
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
                    Get().m_MeshPool.Put(m_Mesh);
            }

            public void Draw(Vector3 point)
            {
                m_Points.Add(point);
            }
        }

        internal class LineDrawingScope : IDisposable
        {
            bool m_Wire;
            Color m_Color;
            float m_Thickness;
            CompareFunction m_ZTest;
            bool m_IsDisposed;

            public Color color
            {
                get { return m_Color; }

                set
                {
                    if (!m_Wire)
                        End();

                    m_Color = value;

                    if (!m_Wire)
                        Begin();
                }
            }

            public float thickness
            {
                get { return m_Thickness; }

                set
                {
                    End();
                    if (value < Mathf.Epsilon)
                        m_Thickness = s_EdgeLineSize;
                    else
                        m_Thickness = value;
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

            public LineDrawingScope(Color color, float thickness = -1f, CompareFunction zTest = CompareFunction.LessEqual)
            {
                m_Color = color;
                m_Thickness = thickness < 0f ? s_EdgeLineSize : thickness;
                m_ZTest = zTest;
                Begin();
            }

            void Begin()
            {
                m_Wire = thickness < k_MinLineWidthForGeometryShader || Get().m_LineMaterial == null;

                if (!m_Wire)
                {
                    Get().m_LineMaterial.SetColor("_Color", color);
                    Get().m_LineMaterial.SetFloat("_Scale", thickness * EditorGUIUtility.pixelsPerPoint);
                    Get().m_LineMaterial.SetInt("_HandleZTest", (int)zTest);
                }

                if (m_Wire || !Get().m_LineMaterial.SetPass(0))
                {
#if UNITY_2019_1_OR_NEWER
                    HandleUtility.ApplyWireMaterial(zTest);
#else
                    if (s_ApplyWireMaterial == null)
                    {
                        s_ApplyWireMaterial = typeof(HandleUtility).GetMethod(
                                "ApplyWireMaterial",
                                BindingFlags.Static | BindingFlags.NonPublic,
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

                GL.PushMatrix();
                GL.Begin(m_Wire || BuiltinMaterials.geometryShadersSupported ? GL.LINES : GL.QUADS);
            }

            void End()
            {
                GL.End();
                GL.PopMatrix();
            }

            public void DrawLine(Vector3 a, Vector3 b)
            {
                if (m_Wire)
                {
                    GL.Color(color);
                    GL.Vertex(a);
                    GL.Vertex(b);
                }
                else if (!BuiltinMaterials.geometryShadersSupported)
                {
                    Vector3 c = b + (b - a);

                    GL.Color(new Color(b.x, b.y, b.z, 1f));
                    GL.Vertex(a);
                    GL.Color(new Color(b.x, b.y, b.z, -1f));
                    GL.Vertex(a);
                    GL.Color(new Color(c.x, c.y, c.z, -1f));
                    GL.Vertex(b);
                    GL.Color(new Color(c.x, c.y, c.z, 1f));
                    GL.Vertex(b);
                }
                else
                {
                    GL.Vertex(a);
                    GL.Vertex(b);
                }
            }

            public void Dispose()
            {
                if (m_IsDisposed)
                    return;
                m_IsDisposed = true;

                End();
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
                Get().m_FaceMaterial.SetColor("_Color", color);
                Get().m_FaceMaterial.SetInt("_HandleZTest", (int)zTest);

                if (!Get().m_FaceMaterial.SetPass(0))
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
