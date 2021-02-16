using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;
using Unity.Mathematics;
using Spline = UnityEngine.Splines.Spline;

[AddComponentMenu("")]
[DisallowMultipleComponent, ExcludeFromPreset, ExcludeFromObjectFactory]
[RequireComponent(typeof(ProBuilderMesh))]
[RequireComponent(typeof(SplineContainer))]

public class SplineShape : MonoBehaviour
{
    [Min(0.01f)]
    public float m_Radius = 0.25f;

    [Range(3,36)]
    public int m_SidesCount = 4;

    public float m_SegmentsLength = 0.5f;

    SplineContainer m_Spline;

    ProBuilderMesh m_Mesh;

    public ProBuilderMesh mesh
    {
        get
        {
            if (m_Mesh == null)
                m_Mesh = GetComponent<ProBuilderMesh>();

            return m_Mesh;
        }

        set
        {
            m_Mesh = value;
        }
    }

    void OnValidate()
    {
        UpdateSplineMesh();
    }

    public void Init()
    {
        m_Spline = GetComponent<SplineContainer>();
        m_Spline.Spline.EditType = SplineType.Bezier;
        //m_Spline.Spline.AddKnot(new BezierKnot(Vector3.zero, Vector3.back, Vector3.forward));
        //m_Spline.Spline.AddKnot(new BezierKnot(0.5f * Vector3.forward, Vector3.back, Vector3.forward));
        //m_Spline.Spline.AddKnot(new BezierKnot(Vector3.forward, Vector3.back, Vector3.forward));
        m_Spline.Spline.changed += UpdateSplineMesh;

        Refresh();
    }

    void UpdateSplineMesh()
    {
        if(m_Radius > 0 && m_SidesCount > 0)
            Refresh();
    }

    /// <summary>
    /// Rebuild the ProBuilderMesh with the extruded spline.
    /// </summary>
    public void Refresh()
    {
        if(m_Spline == null)
            return;

        if (m_Spline.Spline.KnotCount < 2)
        {
            mesh.Clear();
            mesh.ToMesh();
            mesh.Refresh();
        }
        else
        {
            mesh.Clear();
            UpdateMesh();
            mesh.Refresh();
        }
    }

    void UpdateMesh()
    {
        Spline spline = m_Spline.Spline;

        Vector2[] circle = new Vector2[m_SidesCount];
        float radialStepAngle = 360f / m_SidesCount;
        // get a circle
        for (int i = 0; i < m_SidesCount; i++)
        {
            float angle0 = radialStepAngle * i * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle0) * m_Radius;
            float y = Mathf.Sin(angle0) * m_Radius;

            circle[i] = new Vector2(x, y);
        }

        float length = SplineUtility.CalculateSplineLength(spline);
        int segmentsCount = (int) (length / m_SegmentsLength) + 1;

        Vector3[] vertices = new Vector3[(m_SidesCount * (segmentsCount + 1) )];
        Face[] faces = new Face[m_SidesCount * 2 * segmentsCount];

        for(int i = 0; i < segmentsCount + 1; i++)
        {
            var index = (float)i / (float)segmentsCount;
            if(index > 1)
                index = 1f;

            var center = SplineUtility.EvaluateSplinePosition(spline,index);
            float3 tangent = SplineUtility.EvaluateSplineDirection(spline,index);

            var rightDir = math.normalize(math.cross(new float3(0, 1, 0), tangent));
            var upDir = math.normalize(math.cross(tangent, rightDir));

            for(int j = 0; j < m_SidesCount; j++)
            {
                vertices[j + i * m_SidesCount] = (Vector3) center + circle[j].x * (Vector3) rightDir +
                                                 circle[j].y * (Vector3) upDir;
            }
        }

        for(int i = 0; i < segmentsCount; i++)
        {
            for(int j = 0; j < m_SidesCount; j++)
            {
                faces[2 * (j + i * m_SidesCount)] =
                    new Face(
                        new int[3]
                        {
                            j + i * m_SidesCount,
                            (j + 1)%m_SidesCount + i * m_SidesCount,
                            j + (i + 1) * m_SidesCount
                        });
                faces[2 * (j + i * m_SidesCount) + 1] =
                    new Face(new int[3]
                    {
                        (j + 1)%m_SidesCount + i * m_SidesCount,

                        (j + 1)%m_SidesCount + (i + 1) * m_SidesCount,
                        j + (i + 1) * m_SidesCount
                    });
            }
        }

        // Vector3[] vertices = new Vector3[(m_SidesCount + 1) * 2];
        // Face[] faces = new Face[(m_SidesCount * 2)];
        //
        // //Build end caps
        // //Start cap
        // float3 tangent = SplineUtility.EvaluateSplineDirection(spline,0);
        // var rightDir = math.normalize(math.cross(new float3(0, 1, 0), tangent));
        // var upDir = math.normalize(math.cross(tangent, rightDir));
        //
        // vertices[0] = SplineUtility.EvaluateSplinePosition(spline,0f);
        // for(int i = 0; i < m_SidesCount; i++)
        // {
        //     vertices[i + 1] = vertices[0] + circle[i].x * (Vector3)rightDir + circle[i].y * (Vector3)upDir;
        //     faces[i] = new Face(new int[3] { 0, i + 1, 1 + (i + 1)%(m_SidesCount) });
        // }
        //
        // //End cap
        // tangent = SplineUtility.EvaluateSplineDirection(spline,1f);
        // rightDir = math.normalize(math.cross(new float3(0, 1, 0), tangent));
        // upDir = math.normalize(math.cross(tangent, rightDir));
        //
        // int offset = m_SidesCount + 1;
        // vertices[offset] = SplineUtility.EvaluateSplinePosition(spline, 1f);
        // for(int i = 0; i < m_SidesCount; i++)
        // {
        //     vertices[i + offset + 1] = vertices[offset] + circle[i].x * (Vector3)rightDir + circle[i].y * (Vector3)upDir;
        //     faces[m_SidesCount + i] = new Face(new int[3] { offset, offset + i + 1, offset + 1 + (i + 1)%(m_SidesCount) });
        // }

        mesh.RebuildWithPositionsAndFaces(vertices, faces);
    }
}
