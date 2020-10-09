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

            public SimpleTuple<Face, Vector2>[] faceAndScale
            {
                get { return m_FaceAndScale; }
            }

            public TranslateTextureSelection(ProBuilderMesh mesh, PivotPoint pivot)
                : base(mesh, pivot)
            {
                var faces = mesh.faces;

                m_FaceAndScale = mesh.selectedFaceIndexes.Select(x =>
                    new SimpleTuple<Face, Vector2>(faces[x], UvUnwrapping.GetUVTransform(mesh, faces[x]).scale))
                        .ToArray();
            }
        }

        internal override MeshAndElementSelection GetElementSelection(ProBuilderMesh mesh, PivotPoint pivot)
        {
            return new TranslateTextureSelection(mesh, pivot);
        }

        protected override void DoToolGUI()
        {
            if (!isEditing)
                m_Position = Vector3.zero;

            EditorHandleUtility.PushMatrix();

            Handles.matrix = Matrix4x4.TRS(m_HandlePosition, m_HandleRotation, Vector3.one);

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

                m_Position = EditorSnapping.MoveSnap(m_Position);

                // invert `y` because to users it's confusing that "up" in UV space visually moves the texture down
                var delta = new Vector4(m_Position.x, -m_Position.y, 0f, 0f);

                foreach (var value in elementSelection)
                {
                    var selection = value as TranslateTextureSelection;

                    if (selection == null)
                        continue;

                    // Account for object scale
                    delta *= k_Vector3Magnitude / selection.mesh.transform.lossyScale.magnitude;

                    var origins = selection.origins;
                    var positions = selection.textures;

                    // Translating faces is treated as a special case because we want the textures in scene to visually
                    // match the movement of the translation handle. When UVs are scaled, they have the appearance of
                    // moving faster or slower (even though they are translating the correct distances). To avoid this,
                    // we cache the UV scale of each face and modify the translation delta accordingly. This isn't perfect,
                    // as it will not be able to find the scale for sheared or otherwise distorted face UVs. However, for
                    // most cases it maps quite well.
                    if (ProBuilderEditor.selectMode == SelectMode.TextureFace)
                    {
                        foreach (var face in selection.faceAndScale)
                        {
                            var faceDelta = new Vector4(delta.x / face.item2.x, delta.y / face.item2.y, 0f, 0f);

                            foreach (var index in face.item1.distinctIndexes)
                                positions[index] = origins[index] + faceDelta;
                        }
                    }
                    else
                    {
                        foreach (var group in value.elementGroups)
                        {
                            foreach (var index in group.indices)
                                positions[index] = origins[index] + delta;
                        }
                    }

                    selection.mesh.mesh.SetUVs(k_TextureChannel, positions);
                }
            }

            EditorHandleUtility.PopMatrix();
        }
    }
}
