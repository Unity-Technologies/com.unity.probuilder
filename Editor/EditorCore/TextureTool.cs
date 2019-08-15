using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    abstract class TextureTool : VertexManipulationTool
    {
        const bool k_CollectCoincidentVertices = false;
        protected const int k_TextureChannel = 0;

        const string UnityMoveSnapX = "MoveSnapX";
        const string UnityMoveSnapY = "MoveSnapY";
        const string UnityMoveSnapZ = "MoveSnapZ";
        const string UnityScaleSnap = "ScaleSnap";
        const string UnityRotateSnap = "RotationSnap";

        protected static float relativeSnapX
        {
            get { return EditorPrefs.GetFloat(UnityMoveSnapX, 1f); }
        }

        protected static float relativeSnapY
        {
            get { return EditorPrefs.GetFloat(UnityMoveSnapY, 1f); }
        }

        protected static float relativeSnapZ
        {
            get { return EditorPrefs.GetFloat(UnityMoveSnapZ, 1f); }
        }

        protected static float relativeSnapScale
        {
            get { return EditorPrefs.GetFloat(UnityScaleSnap, .1f); }
        }

        protected static float relativeSnapRotation
        {
            get { return EditorPrefs.GetFloat(UnityRotateSnap, 15f); }
        }

        protected class MeshAndTextures : MeshAndElementSelection
        {
            List<Vector4> m_Origins;
            List<Vector4> m_Textures;

            Matrix4x4 m_PreApplyMatrix;
            Matrix4x4 m_PostApplyMatrix;

            public Matrix4x4 preApplyMatrix
            {
                get
                {
                    return m_PreApplyMatrix;
                }

                private set
                {
                    m_PreApplyMatrix = value;
                    m_PostApplyMatrix = value.inverse;
                }
            }

            public Matrix4x4 postApplyMatrix
            {
                get
                {
                    return m_PostApplyMatrix;
                }

                private set
                {
                    m_PostApplyMatrix = value;
                    m_PreApplyMatrix = value.inverse;
                }
            }

            public List<Vector4> textures
            {
                get { return m_Textures; }
            }

            public List<Vector4> origins
            {
                get { return m_Origins; }
            }

            public MeshAndTextures(ProBuilderMesh mesh, PivotPoint pivot) : base(mesh, k_CollectCoincidentVertices)
            {
                m_Textures = new List<Vector4>();
                mesh.GetUVs(k_TextureChannel, m_Textures);
                m_Origins = new List<Vector4>(m_Textures);
                preApplyMatrix = Matrix4x4.Translate(-Bounds2D.Center(m_Origins, mesh.selectedIndexesInternal));
            }
        }

        protected override void OnToolEngaged()
        {
            MeshSelection.InvalidateElementSelection();
        }

        protected override void OnToolDisengaged()
        {
            var isFaceMode = ProBuilderEditor.selectMode.ContainsFlag(SelectMode.TextureFace | SelectMode.Face);

            foreach (var mesh in elementSelection)
            {
                if (!(mesh is MeshAndTextures))
                    continue;

                var textures = ((MeshAndTextures)mesh).textures;
                mesh.mesh.SetUVs(k_TextureChannel, textures);

                if (isFaceMode)
                {
                    UVEditing.SetAutoAndAlignUnwrapParamsToUVs(mesh.mesh, mesh.mesh.selectedFacesInternal.Where(x => !x.manualUV));
                }
                else
                {
                    var indices = new HashSet<int>(mesh.elementGroups.SelectMany(x => x.indices));

                    foreach (var face in mesh.mesh.facesInternal)
                    {
                        foreach (var index in face.distinctIndexesInternal)
                        {
                            if (indices.Contains(index))
                            {
                                face.manualUV = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        internal override MeshAndElementSelection GetElementSelection(ProBuilderMesh mesh, PivotPoint pivot)
        {
            return new MeshAndTextures(mesh, pivot);
        }
    }
}
