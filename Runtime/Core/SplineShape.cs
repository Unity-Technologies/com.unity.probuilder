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

    [Min(0.05f)]
    public float m_SegmentsLength = 0.5f;

    public bool m_UseEndCaps = true;

    SplineContainer m_SplineContainer;
    Spline m_Spline;

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
        m_SplineContainer = GetComponent<SplineContainer>();
        m_Spline = m_SplineContainer.Spline;
        m_Spline.EditType = SplineType.Bezier;
        m_SplineContainer.Spline.changed += SplineChanged;

        Refresh();
    }

    void SplineChanged()
    {
        var newKnotPos = m_SplineContainer.Spline[m_SplineContainer.Spline.KnotCount - 1].Position;
        var length = SplineUtility.CalculateSplineLength(m_Spline);
        if(math.length(newKnotPos) > 0.0f && length > 0.0f)
            UpdateSplineMesh();
    }

    void UpdateSplineMesh()
    {
        if(m_Radius > 0
           && m_SidesCount > 0
           && m_SegmentsLength > 0)
            Refresh();
    }

    /// <summary>
    /// Rebuild the ProBuilderMesh with the extruded spline.
    /// </summary>
    public void Refresh()
    {
        if(m_SplineContainer == null)
            return;

        if (m_SplineContainer.Spline.KnotCount < 2)
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
        float length = SplineUtility.CalculateSplineLength(m_Spline);
        if(length == 0)
        {
            mesh.ToMesh();
            return;
        }

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

        int segmentsCount = (int) (length / m_SegmentsLength) + 1;

        var vertexCount = m_SidesCount * ( segmentsCount + 1 );
        var faceCount = m_Spline.Closed ?
                                m_SidesCount * 2 * ( segmentsCount + 1 )
                                : m_SidesCount * 2 * segmentsCount;

        if(!m_Spline.Closed && m_UseEndCaps)
        {
            vertexCount += 2;
            faceCount += 2 * m_SidesCount;
        }

        Vector3[] vertices = new Vector3[vertexCount];
        Face[] faces = new Face[faceCount];

        int vertexIndex = 0;
        for(int i = 0; i < segmentsCount + 1; i++)
        {
            var index = (float)i / (float)segmentsCount;
            if(index > 1)
                index = 1f;

            var center = SplineUtility.EvaluateSplinePosition(m_Spline,index);
            float3 tangent = SplineUtility.EvaluateSplineDirection(m_Spline,index);

            var rightDir = math.normalize(math.cross(new float3(0, 1, 0), tangent));
            var upDir = math.normalize(math.cross(tangent, rightDir));

            for(int j = 0; j < m_SidesCount; j++)
                vertices[vertexIndex++] = (Vector3) center + circle[j].x * (Vector3) rightDir + circle[j].y * (Vector3) upDir;

            if(i == 0)
                vertices[vertexCount-2] = center;
            if(i == segmentsCount)
                vertices[vertexCount-1] = center;
        }

        var maxSegmentCount = m_Spline.Closed ? segmentsCount + 1 : segmentsCount;
        for(int i = 0; i < maxSegmentCount; i++)
        {
            for(int j = 0; j < m_SidesCount; j++)
            {
                faces[2 * (j + i * m_SidesCount)] =
                    new Face(
                        new int[3]
                        {
                            (j + i * m_SidesCount)%vertices.Length,
                            ((j + 1)%m_SidesCount + i * m_SidesCount)%vertices.Length,
                            (j + (i + 1) * m_SidesCount)%vertices.Length
                        });
                faces[2 * (j + i * m_SidesCount) + 1] =
                    new Face(new int[3]
                    {
                        ((j + 1)%m_SidesCount + i * m_SidesCount)%vertices.Length,
                        ((j + 1)%m_SidesCount + (i + 1) * m_SidesCount)%vertices.Length,
                        (j + (i + 1) * m_SidesCount)%vertices.Length
                    });
            }
        }

        if(!m_Spline.Closed && m_UseEndCaps)
        {
            var offset = m_SidesCount * 2 * segmentsCount;
            //Build end caps
            //Start cap
            for(int i = 0; i < m_SidesCount; i++)
            {
                faces[offset + i] = new Face(new int[3] { vertexCount-2, (i+1)%(m_SidesCount), i });
            }

            //End cap
            offset += m_SidesCount;
            for(int i = 0; i < m_SidesCount; i++)
            {
                faces[offset + i] = new Face(new int[3] { vertexCount - 1,
                    vertexCount - 2 - m_SidesCount + i,
                    vertexCount - 2 - m_SidesCount + (i + 1)%m_SidesCount});
            }
        }

        mesh.RebuildWithPositionsAndFaces(vertices, faces);
    }
}
