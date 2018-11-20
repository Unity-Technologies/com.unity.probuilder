﻿using System;
using UnityEngine.Rendering;
using System.Reflection;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Access the built-in materials that ProBuilder uses.
	/// </summary>
	static class BuiltinMaterials
	{
		static bool s_IsInitialized;

		internal const string faceShader = "Hidden/ProBuilder/FaceHighlight";
		internal const string lineShader = "Hidden/ProBuilder/LineBillboard";
		internal const string pointShader = "Hidden/ProBuilder/PointBillboard";
		// used when gpu doesn't support geometry shaders (metal, for example)
		internal const string wireShader = "Hidden/ProBuilder/FaceHighlight";
		internal const string dotShader = "Hidden/ProBuilder/VertexShader";

	    internal static readonly Color previewColor = new Color(.5f, .9f, 1f, .56f);

        static Shader s_SelectionPickerShader;

		static bool s_GeometryShadersSupported;

		static Material s_DefaultMaterial;
		static Material s_FacePickerMaterial;
		static Material s_VertexPickerMaterial;
		static Material s_EdgePickerMaterial;
		static Material s_UnityDefaultDiffuse;
		static Material s_UnlitVertexColorMaterial;
	    static Material s_ShapePreviewMaterial;

		static void Init()
		{
			if (s_IsInitialized)
				return;

			s_IsInitialized = true;

			var geo = Shader.Find(lineShader);
			s_GeometryShadersSupported = geo != null && geo.isSupported;

			// ProBuilder default
			if (GraphicsSettings.renderPipelineAsset != null)
			{
#if UNITY_2019_1_OR_NEWER
				s_DefaultMaterial = GraphicsSettings.renderPipelineAsset.defaultMaterial;
#else
				s_DefaultMaterial = GraphicsSettings.renderPipelineAsset.GetDefaultMaterial();
#endif
			}
			else
			{
				s_DefaultMaterial = (Material)Resources.Load("Materials/ProBuilderDefault", typeof(Material));

				if (s_DefaultMaterial == null || !s_DefaultMaterial.shader.isSupported)
					s_DefaultMaterial = GetLegacyDiffuse();
			}

			// SelectionPicker shader
			s_SelectionPickerShader = (Shader)Shader.Find("Hidden/ProBuilder/SelectionPicker");

			if ((s_FacePickerMaterial = Resources.Load<Material>("Materials/FacePicker")) == null)
			{
				Log.Error("FacePicker material not loaded... please re-install ProBuilder to fix this error.");
				s_FacePickerMaterial = new Material(Shader.Find("Hidden/ProBuilder/FacePicker"));
			}

			if ((s_VertexPickerMaterial = Resources.Load<Material>("Materials/VertexPicker")) == null)
			{
				Log.Error("VertexPicker material not loaded... please re-install ProBuilder to fix this error.");
				s_VertexPickerMaterial = new Material(Shader.Find("Hidden/ProBuilder/VertexPicker"));
			}

			if ((s_EdgePickerMaterial = Resources.Load<Material>("Materials/EdgePicker")) == null)
			{
				Log.Error("EdgePicker material not loaded... please re-install ProBuilder to fix this error.");
				s_EdgePickerMaterial = new Material(Shader.Find("Hidden/ProBuilder/EdgePicker"));
			}

			s_UnlitVertexColorMaterial = (Material) Resources.Load("Materials/UnlitVertexColor", typeof(Material));

		    s_ShapePreviewMaterial = new Material(s_DefaultMaterial.shader);
		    s_ShapePreviewMaterial.hideFlags = HideFlags.HideAndDontSave;

		    if (s_ShapePreviewMaterial.HasProperty("_MainTex"))
		        s_ShapePreviewMaterial.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");

		    if (s_ShapePreviewMaterial.HasProperty("_Color"))
		        s_ShapePreviewMaterial.SetColor("_Color", previewColor);

        }

		/// <summary>
		/// Does this platform support geometry shaders?
		/// </summary>
		public static bool geometryShadersSupported
		{
			get
			{
				Init();
				return s_GeometryShadersSupported;
			}
		}

		/// <summary>
		/// The default ProBuilder material.
		/// <br />
		/// <br />
		/// When using the Scriptable Render Pipeline this returns the default material for that pipeline.
		/// </summary>
		public static Material defaultMaterial
		{
			get
			{
				Init();
				return s_DefaultMaterial;
			}
		}

		/// <summary>
		/// Shader used in selection picking functions.
		/// </summary>
		internal static Shader selectionPickerShader
		{
			get
			{
				Init();
				return s_SelectionPickerShader;
			}
		}

		/// <summary>
		/// Material used for face picking functions.
		/// </summary>
		internal static Material facePickerMaterial
		{
			get
			{
				Init();
				return s_FacePickerMaterial;
			}
		}

		/// <summary>
		/// Material used for vertex picking functions.
		/// </summary>
		internal static Material vertexPickerMaterial
		{
			get
			{
				Init();
				return s_VertexPickerMaterial;
			}
		}

		/// <summary>
		/// Material used for edge picking functions.
		/// </summary>
		internal static Material edgePickerMaterial
		{
			get
			{
				Init();
				return s_EdgePickerMaterial;
			}
		}

		/// <summary>
		/// The ProBuilder "Trigger" entity type material.
		/// </summary>
		internal static Material triggerMaterial
		{
			get
			{
				Init();
				return (Material) Resources.Load("Materials/Trigger", typeof(Material));
			}
		}

		/// <summary>
		/// The ProBuilder "Collider" entity type material.
		/// </summary>
		internal static Material colliderMaterial
		{
			get
			{
				Init();
				return (Material) Resources.Load("Materials/Collider", typeof(Material));
			}
		}

		/// <summary>
		/// The ProBuilder "NoDraw" material. Faces with this material are hidden when the game is played.
		/// </summary>
		[Obsolete("NoDraw is no longer supported.")]
		internal static Material noDrawMaterial
		{
			get
			{
				Init();
				return (Material) Resources.Load("Materials/NoDraw", typeof(Material));
			}
		}

		/// <summary>
		/// Default Unity diffuse material.
		/// </summary>
		internal static Material GetLegacyDiffuse()
		{
			Init();

			if (s_UnityDefaultDiffuse == null)
			{
				var mi = typeof(Material).GetMethod("GetDefaultMaterial",
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

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

		/// <summary>
		/// An unlit vertex color material.
		/// </summary>
		internal static Material unlitVertexColor
		{
			get
			{
				Init();
				return s_UnlitVertexColorMaterial;
			}
		}

	    internal static Material ShapePreviewMaterial
        {
	        get
	        {
                Init();
	            return s_ShapePreviewMaterial;
	        }
	    }
	}
}
