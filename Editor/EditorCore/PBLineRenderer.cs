using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UObject = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    internal class PBLineRenderer : IDisposable
    {
        // Line renderer for the current cut path
        Material m_Material;
        Mesh m_Mesh = null;
        readonly Color lineMaterialBaseColor = Color.white;
        readonly Color lineMaterialHighlightColor = Color.white;

        Transform m_Transform;

        bool m_DashedLine = false;

        float m_LineLength = 0.1f;
        float m_SpaceLength = 0.05f;

        /// <summary>
        /// Instantiate Line Materials, all are based on the same base Material with different colors
        /// </summary>
        /// <param name="baseColor">base color to apply to the line</param>
        /// <param name="highlightColor">highlight color to apply to the line</param>
        /// <returns></returns>
        public PBLineRenderer(Transform referenceTransform, bool dashed, Color baseColor, Color highlightColor)
        {
            m_Transform = referenceTransform;

            m_Mesh = new Mesh();

            m_Material = new Material(Shader.Find("Hidden/ProBuilder/ScrollHighlight"));
            m_Material.SetColor("_Base", baseColor);
            m_Material.SetColor("_Highlight", highlightColor);

            m_DashedLine = dashed;
        }

        public PBLineRenderer(Transform referenceTransform, bool dashed, Color baseColor) : this(referenceTransform, dashed, baseColor, baseColor) {}
        public PBLineRenderer(Transform referenceTransform, Color baseColor, Color highlightColor) : this(referenceTransform, false, baseColor, highlightColor) {}

        public PBLineRenderer(Transform referenceTransform, Color baseColor) : this(referenceTransform, false, baseColor, baseColor) {}

        public void SetDashedLineParameters(float lineLength, float space)
        {
            m_LineLength = lineLength;
            m_SpaceLength = space;
        }

        public void Clear()
        {
            if(m_Mesh)
                m_Mesh.Clear();
        }

        public void Dispose()
        {
            if(m_Mesh)
                UObject.DestroyImmediate(m_Mesh);
            if(m_Material)
                UObject.DestroyImmediate(m_Material);
        }

        public void DrawLineGUI()
        {
            m_Material.SetPass(0);
            Graphics.DrawMeshNow(m_Mesh, m_Transform.localToWorldMatrix, 0);
        }

        public void UpdateLineRenderer()
        {
            m_Material.SetFloat("_EditorTime", (float) EditorApplication.timeSinceStartup);
        }

        /// <summary>
        /// Draw the line connecting the points
        /// </summary>
        /// <param name="points">Positions of the points</param>
        /// <param name="lineTopology"></param>
        public void SetPositions(List<Vector3> points)
        {
            if(m_DashedLine)
                SetDashedLinePositions(points);
            else
                SetLinePositions(points);
        }


        /// <summary>
        /// Draw the line connecting the points
        /// </summary>
        /// <param name="points">Positions of the points</param>
        /// <param name="lineTopology"></param>
        void SetLinePositions(List<Vector3> points)
        {
            if(m_Mesh)
                m_Mesh.Clear();

            if (points.Count < 2)
                return;

            int vc = points.Count;

            Vector3[] ver = new Vector3[vc];
            Vector2[] uvs = new Vector2[vc];
            int[] indexes = new int[vc];
            int cnt = points.Count;
            float distance = 0f;

            for (int i = 0; i < vc; i++)
            {
                Vector3 a = points[i % cnt];
                Vector3 b = points[i < 1 ? 0 : i - 1];

                float d = Vector3.Distance(a, b);
                distance += d;

                ver[i] = points[i % cnt];
                uvs[i] = new Vector2(distance, 1f);
                indexes[i] = i;
            }

            m_Mesh.name = "PB Line";
            m_Mesh.vertices = ver;
            m_Mesh.uv = uvs;
            m_Mesh.SetIndices(indexes, MeshTopology.LineStrip, 0);

            m_Material.SetFloat("_LineDistance", distance);
        }

        /// <summary>
        /// Draw the dashed line connecting the points
        /// </summary>
        /// <param name="points">Positions of the points</param>
        /// <param name="lineTopology"></param>
        void SetDashedLinePositions(List<Vector3> points)
        {
            if(m_Mesh)
                m_Mesh.Clear();

            List<Vector3> vertices = new List<Vector3>();
            List<int> indexes = new List<int>();

            for(int i = 0; i < points.Count / 2; i++)
                UpdateDashedSegment(vertices, indexes, points[2*i], points[2*i+1]);

            m_Mesh.name = "DashedLine";
            m_Mesh.vertices = vertices.ToArray();
            m_Mesh.SetIndices(indexes, MeshTopology.Lines, 0);
        }

        void UpdateDashedSegment(List<Vector3> vertices, List<int> indexes, Vector3 fromPoint, Vector3 toPoint)
        {
            float d = Vector3.Distance(fromPoint, toPoint);
            Vector3 dir = ( toPoint - fromPoint ).normalized;
            int sections = (int)(d / (m_LineLength + m_SpaceLength));

            int offset = vertices.Count;
            for(int i = 0; i < sections; i++)
            {
                vertices.Add(fromPoint + i * (m_LineLength + m_SpaceLength) * dir);
                vertices.Add(fromPoint + (i * (m_LineLength + m_SpaceLength) + m_LineLength) * dir);

                indexes.Add(2*i + offset);
                indexes.Add(2*i+1 + offset);
            }

            vertices.Add(fromPoint + sections * (m_LineLength + m_SpaceLength) * dir);
            indexes.Add(2 * sections + offset);


            if(d - (sections * ( m_LineLength + m_SpaceLength )) > m_LineLength)
                vertices.Add(fromPoint + ( sections * ( m_LineLength + m_SpaceLength ) + m_LineLength ) * dir);
            else
                vertices.Add(toPoint);
            indexes.Add(2 * sections + 1 + offset);
        }

    }
}
