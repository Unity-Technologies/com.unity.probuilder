using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Reflection;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Access the built-in materials that ProBuilder uses.
	/// </summary>
	public static class BuiltinMaterials
	{
		static Shader s_SelectionPickerShader;

		static Material s_DefaultMaterial;
		static Material s_FacePickerMaterial;
		static Material s_VertexPickerMaterial;
		static Material s_EdgePickerMaterial;
		static Material s_UnityDefaultDiffuse;
		static Material s_UnlitVertexColorMaterial;

		/// <summary>
		/// Shader used in selection picking functions.
		/// </summary>
		internal static Shader SelectionPickerShader
		{
			get
			{
				if (s_SelectionPickerShader == null)
					s_SelectionPickerShader = (Shader)Shader.Find("Hidden/ProBuilder/SelectionPicker");
				return s_SelectionPickerShader;
			}
		}

		/// <summary>
		/// The default ProBuilder material.
		/// <br />
		/// <br />
		/// When using the Scriptable Render Pipeline this returns the default material for that pipeline.
		/// </summary>
		public static Material DefaultMaterial
		{
			get
			{
				if (s_DefaultMaterial == null)
				{
					var pipe = GraphicsSettings.renderPipelineAsset;

					if (pipe != null)
					{
						s_DefaultMaterial = pipe.GetDefaultMaterial();
					}
					else
					{
						s_DefaultMaterial = (Material)Resources.Load("Materials/ProBuilderDefault", typeof(Material));

						if (s_DefaultMaterial == null || !s_DefaultMaterial.shader.isSupported)
							s_DefaultMaterial = UnityDefaultDiffuse;
					}
				}

				return s_DefaultMaterial;
			}
		}

		/// <summary>
		/// Material used for face picking functions.
		/// </summary>
		internal static Material FacePickerMaterial
		{
			get
			{
				if (s_FacePickerMaterial == null)
				{
					var facePickerShader = Shader.Find("Hidden/ProBuilder/FacePicker");

					if (facePickerShader == null)
						Log.Error("pb_FacePicker.shader not found! Re-import ProBuilder to fix.");

					if (s_FacePickerMaterial == null)
						s_FacePickerMaterial = new Material(facePickerShader);
					else
						s_FacePickerMaterial.shader = facePickerShader;
				}

				return s_FacePickerMaterial;
			}
		}

		/// <summary>
		/// Material used for vertex picking functions.
		/// </summary>
		internal static Material VertexPickerMaterial
		{
			get
			{
				if (s_VertexPickerMaterial == null)
				{
					s_VertexPickerMaterial = Resources.Load<Material>("Materials/VertexPicker");

					var vertexPickerShader = Shader.Find("Hidden/ProBuilder/VertexPicker");

					if (vertexPickerShader == null)
						Log.Error("pb_VertexPicker.shader not found! Re-import ProBuilder to fix.");

					if (s_VertexPickerMaterial == null)
						s_VertexPickerMaterial = new Material(vertexPickerShader);
					else
						s_VertexPickerMaterial.shader = vertexPickerShader;
				}

				return s_VertexPickerMaterial;
			}
		}

		/// <summary>
		/// Material used for edge picking functions.
		/// </summary>
		internal static Material EdgePickerMaterial
		{
			get
			{
				if (s_EdgePickerMaterial == null)
				{
					s_EdgePickerMaterial = Resources.Load<Material>("Materials/EdgePicker");

					var edgePickerShader = Shader.Find("Hidden/ProBuilder/EdgePicker");

					if (edgePickerShader == null)
						Log.Error("pb_EdgePicker.shader not found! Re-import ProBuilder to fix.");

					if (s_EdgePickerMaterial == null)
						s_EdgePickerMaterial = new Material(edgePickerShader);
					else
						s_EdgePickerMaterial.shader = edgePickerShader;
				}

				return s_EdgePickerMaterial;
			}
		}

		/// <summary>
		/// The ProBuilder "Trigger" entity type material.
		/// </summary>
		internal static Material TriggerMaterial
		{
			get { return (Material)Resources.Load("Materials/Trigger", typeof(Material)); }
		}

		/// <summary>
		/// The ProBuilder "Collider" entity type material.
		/// </summary>
		internal static Material ColliderMaterial
		{
			get { return (Material)Resources.Load("Materials/Collider", typeof(Material)); }
		}

		/// <summary>
		/// The ProBuilder "NoDraw" material. Faces with this material are hidden when the game is played.
		/// </summary>
		[Obsolete("NoDraw is no longer supported.")]
		internal static Material NoDrawMaterial
		{
			get { return (Material)Resources.Load("Materials/NoDraw", typeof(Material)); }
		}

		/// <summary>
		/// Default Unity diffuse material.
		/// </summary>
		internal static Material UnityDefaultDiffuse
		{
			get
			{
				if (s_UnityDefaultDiffuse == null)
				{
					var mi = typeof(Material).GetMethod("GetDefaultMaterial", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

					if (mi != null)
						s_UnityDefaultDiffuse = mi.Invoke(null, null) as Material;

					if (s_UnityDefaultDiffuse == null)
					{
						var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
						s_UnityDefaultDiffuse = go.GetComponent<MeshRenderer>().sharedMaterial;
						UnityEngine.Object.DestroyImmediate(go);
					}
				}

				return s_UnityDefaultDiffuse;
			}
		}

		/// <summary>
		/// An unlit vertex color material.
		/// </summary>
		internal static Material UnlitVertexColor
		{
			get
			{
				if (s_UnlitVertexColorMaterial == null)
					s_UnlitVertexColorMaterial = (Material)Resources.Load("Materials/UnlitVertexColor", typeof(Material));

				return s_UnlitVertexColorMaterial;
			}
		}
	}
}
