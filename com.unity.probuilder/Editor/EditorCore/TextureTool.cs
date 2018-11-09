using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

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

		protected class MeshAndTextures : MeshAndElementGroupPair
		{
			List<Vector4> m_Origins;
			List<Vector4> m_Textures;

			public List<Vector4> textures
			{
				get { return m_Textures; }
			}

			public List<Vector4> origins
			{
				get { return m_Origins; }
			}

			public MeshAndTextures(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation) : base(mesh, pivot, orientation, k_CollectCoincidentVertices)
			{
				m_Textures = new List<Vector4>();
				mesh.GetUVs(k_TextureChannel, m_Textures);
				m_Origins = new List<Vector4>(m_Textures);

				foreach (var group in elementGroups)
				{
					var bounds = Bounds2D.Center(m_Origins, group.indices);
					group.preApplyMatrix = Matrix4x4.Translate(-bounds);
				}
			}
		}

		protected override void OnToolDisengaged()
		{
			var isFaceMode = ProBuilderEditor.selectMode.ContainsFlag(SelectMode.TextureFace | SelectMode.Face);

			foreach (var mesh in meshAndElementGroupPairs)
			{
				if (!(mesh is MeshAndTextures))
					continue;

				if (isFaceMode)
				{
					foreach (var face in mesh.mesh.selectedFacesInternal)
						face.manualUV = true;
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

				var textures = ((MeshAndTextures)mesh).textures;
				mesh.mesh.SetUVs(k_TextureChannel, textures);
			}
		}

		protected override MeshAndElementGroupPair GetMeshAndElementGroupPair(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation)
		{
			return new MeshAndTextures(mesh, pivot, orientation);
		}
	}
}
