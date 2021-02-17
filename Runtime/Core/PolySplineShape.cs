using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;
using Spline = UnityEngine.Splines.Spline;

[DisallowMultipleComponent, ExcludeFromPreset, ExcludeFromObjectFactory]
[RequireComponent(typeof(ProBuilderMesh))]
public class PolySplineShape : MonoBehaviour
{
    [Min(0.01f)]
    public float m_Height = 1f;

    [Min(1)]
    public int m_SplineCount = 3;

    [Min(3)]
    public int m_SidesCount = 12;

    ProBuilderMesh m_Mesh;

    bool m_Init = false;

    public ProBuilderMesh mesh
    {
        get
        {
            if (m_Mesh == null)
                m_Mesh = GetComponent<ProBuilderMesh>();

            return m_Mesh;
        }

        set => m_Mesh = value;
    }

    int m_CurrentSplineCount;
    List<SplineContainer> m_Splines = new List<SplineContainer>();

    internal bool m_IsPBEditorDirty = false;
    internal bool isPBEditorDirty
    {
        get => m_IsPBEditorDirty;
        set => m_IsPBEditorDirty = false;
    }

    public void Init()
    {
        UpdateSplines();
        RefreshMesh();
    }

    void SplineOnchanged()
    {
        RefreshMesh();
    }

    void OnValidate()
    {
        if(!m_Init)
            return;

        UpdateSplines();
        RebuildMesh();
    }

    SplineContainer CreateNewSpline(int index)
    {
        GameObject newSplineGO = new GameObject("Spline-" + index);
        newSplineGO.transform.parent = this.transform;
        newSplineGO.transform.localPosition = Vector3.zero;
        var splineContainer = newSplineGO.AddComponent<SplineContainer>();
        var spline = splineContainer.Spline;
        spline.EditType = SplineType.Bezier;
        spline.Closed = true;
        spline.AddKnot(new BezierKnot(new float3(0,0,1), new float3(-0.5f,0,1), new float3(0.5f,0,1)));
        spline.AddKnot(new BezierKnot(new float3(1,0,0), new float3(1,0,0.5f), new float3(1,0,-0.5f)));
        spline.AddKnot(new BezierKnot(new float3(0,0,-1), new float3(0.5f,0,-1), new float3(-0.5f,0,-1)));
        spline.AddKnot(new BezierKnot(new float3(-1,0,0), new float3(-1,0,-0.5f), new float3(-1,0,0.5f)));
        spline.changed += SplineOnchanged;

        return splineContainer;
    }

    void UpdateSplines()
    {
        if(m_SplineCount != m_CurrentSplineCount)
        {
            while(m_SplineCount < m_Splines.Count)
            {
                var splineToRemove = m_Splines[m_Splines.Count - 1].gameObject;
                m_Splines.RemoveAt(m_Splines.Count - 1);
                DestroyImmediate(splineToRemove);
            }

            while(m_SplineCount > m_Splines.Count)
            {
                m_Splines.Add(CreateNewSpline(m_Splines.Count));
            }

            m_CurrentSplineCount = m_SplineCount;
        }
        UpdateSplinesPosition();
    }

    void UpdateSplinesPosition()
    {
        for(int i = 0; i < m_CurrentSplineCount; i++)
        {
            var spline = m_Splines[i].Spline;
            var height = m_CurrentSplineCount > 1 ? m_Height * i / ( m_CurrentSplineCount - 1f ) : 0;
            for(int knotIndex = 0; knotIndex < spline.KnotCount; knotIndex++)
            {
                BezierKnot knot = spline[knotIndex];
                knot.Position = new float3(knot.Position.x, height, knot.Position.z);
                knot.TangentIn = new float3(knot.TangentIn.x, height, knot.TangentIn.z);
                knot.TangentOut = new float3(knot.TangentOut.x, height, knot.TangentOut.z);
                spline[knotIndex] = knot;
            }
        }
    }

    void RefreshMesh()
    {
        if(m_Splines == null || m_Splines.Count == 0)
            return;

        if (m_Splines[0].Spline.KnotCount < 2)
        {
            mesh.Clear();
            mesh.ToMesh();
            mesh.Refresh();
        }
        else
        {
            mesh.Clear();
            RebuildMesh();
            m_IsPBEditorDirty = true;
        }
    }

    void RebuildMesh()
    {
        float length = SplineUtility.CalculateSplineLength(m_Splines[0].Spline);
        if(length == 0)
        {
            mesh.ToMesh();
            return;
        }

        Vector3[] vertices = new Vector3[0];
        Face[] faces = new Face[0];

        int segmentsCount = m_SidesCount;
        if(m_Splines.Count == 0)
        {
            vertices = new Vector3[m_SidesCount + 1];
            faces = new Face[m_SidesCount];

            vertices[0] = Vector3.zero;
            for(int i = 0; i < segmentsCount; i++)
            {
                var index = (float) i / (float) segmentsCount;
                if(index > 1)
                    index = 1f;

                vertices[i + 1] = SplineUtility.EvaluateSplinePosition(m_Splines[0].Spline, index);
                vertices[0] += vertices[i + 1];

                faces[i] = new Face(new int[3] { 0, i + 1, i != segmentsCount - 1 ? i + 2 : 1 });
            }

            vertices[0] = vertices[0] / segmentsCount;
        }
        else
        {
            vertices = new Vector3[m_SplineCount * m_SidesCount + 2 /*top and bottom centers*/];
            faces = new Face[2 * m_SidesCount /*Top and bottom faces*/ + 2 * m_SidesCount * (m_SplineCount - 1)/*Side faces*/];
            //faces = new Face[2 * m_SidesCount * (m_SplineCount - 1)/*Side faces*/];

            //Down face
            vertices[0] = Vector3.zero;
            var lastIndex = vertices.Length - 1;
            vertices[lastIndex] = Vector3.zero;

            int vertexindex = 1;
            int faceindex = 0;
            for(int splineIndex = 0; splineIndex < m_Splines.Count; splineIndex++)
            {
                for(int i = 0; i < segmentsCount; i++)
                {
                    var index = (float) i / (float) segmentsCount;
                    if(index > 1)
                        index = 1f;

                    var vertex = SplineUtility.EvaluateSplinePosition(m_Splines[splineIndex].Spline, index);

                    vertices[vertexindex] = vertex;
                    if(splineIndex == 0)
                        vertices[0] += vertices[vertexindex];

                    if(splineIndex == m_SplineCount - 1)
                        vertices[lastIndex] += vertices[vertexindex];
                    if(splineIndex != m_SplineCount - 1)
                    {
                        var nextIndex = i != segmentsCount - 1 ? vertexindex + 1 : vertexindex + 1 - segmentsCount;
                        var upperIndex = vertexindex + segmentsCount;
                        var upperNextIndex = i != segmentsCount - 1 ? vertexindex + segmentsCount + 1 : vertexindex + 1;
                        faces[2 * faceindex] = new Face(new int[3] { vertexindex, upperNextIndex, upperIndex });
                        faces[2 * faceindex + 1] = new Face(new int[3] { upperNextIndex, vertexindex, nextIndex });
                        faceindex++;
                    }

                    vertexindex++;
                }
            }

            vertices[0] = vertices[0] / segmentsCount;
            vertices[lastIndex] = vertices[lastIndex] / segmentsCount;

            for(int i = 0; i < segmentsCount; i++)
            {
                //Down Face
                faces[2 * faceindex + i] = new Face(new int[3] { 0, i != segmentsCount - 1 ? i + 2 : 1, i + 1 });
                //Up Face
                faces[2 * faceindex + i + segmentsCount] = new Face(new int[3]
                {
                    lastIndex,
                    lastIndex - segmentsCount + i,
                    i != segmentsCount - 1 ? lastIndex - segmentsCount + i + 1 : lastIndex - segmentsCount
                });
            }
        }

        mesh.RebuildWithPositionsAndFaces(vertices, faces);
        mesh.ToMesh();
        mesh.Refresh();

        m_Init = true;
    }

}
