using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    class TextureMoveTool : TextureTool
    {
        static readonly float k_Vector3Magnitude = Vector3.one.magnitude;

        Vector3 m_Position = Vector3.zero;

        protected class TranslateTextureSelection : MeshAndTextures
        {
            SimpleTuple<Face, Vector2>[] m_FaceAndScale;

            public TranslateTextureSelection(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation)
                : base(mesh, pivot, orientation)
            {
                var faces = mesh.faces;

                m_FaceAndScale = mesh.selectedFaceIndexes.Select(x =>
                {
                    return new SimpleTuple<Face, Vector2>(faces[x], Vector2.one);
                }).ToArray();
            }
        }

        protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
        {
            if (!isEditing)
                m_Position = Vector3.zero;

            EditorHandleUtility.PushMatrix();

            Handles.matrix = Matrix4x4.TRS(handlePosition, handleRotation, Vector3.one);

            EditorGUI.BeginChangeCheck();

            Handles.color = Color.blue;

            m_Position = Handles.Slider2D(m_Position,
                    Vector3.forward,
                    Vector3.right,
                    Vector3.up,
                    HandleUtility.GetHandleSize(m_Position) * .2f,
                    Handles.RectangleHandleCap,
                    0f,
                    false);

            Handles.color = Color.green;

            m_Position = Handles.Slider(m_Position, Vector3.up);

            Handles.color = Color.red;

            m_Position = Handles.Slider(m_Position, Vector3.right);

            Handles.color = Color.white;

            if (EditorGUI.EndChangeCheck())
            {
                if (!isEditing)
                    BeginEdit("Translate Textures");

                if (relativeSnapEnabled)
                {
                    m_Position.x = Snapping.SnapValue(m_Position.x, relativeSnapX);
                    m_Position.y = Snapping.SnapValue(m_Position.y, relativeSnapY);
                }
                else if (progridsSnapEnabled)
                {
                    m_Position.x = Snapping.SnapValue(m_Position.x, progridsSnapValue);
                    m_Position.y = Snapping.SnapValue(m_Position.y, progridsSnapValue);
                }

                // invert `y` because to users it's confusing that "up" in UV space visually moves the texture down
                var delta = new Vector4(m_Position.x, -m_Position.y, 0f, 0f);

                foreach (var mesh in elementSelection)
                {
                    if (!(mesh is MeshAndTextures))
                        continue;

                    // Account for object scale
                    delta *= k_Vector3Magnitude / mesh.mesh.transform.lossyScale.magnitude;

                    var origins = ((MeshAndTextures)mesh).origins;
                    var positions = ((MeshAndTextures)mesh).textures;

                    foreach (var group in mesh.elementGroups)
                    {
                        foreach (var index in group.indices)
                            positions[index] = origins[index] + delta;
                    }

                    mesh.mesh.mesh.SetUVs(k_TextureChannel, positions);
                }
            }

            EditorHandleUtility.PopMatrix();
        }
    }
}
