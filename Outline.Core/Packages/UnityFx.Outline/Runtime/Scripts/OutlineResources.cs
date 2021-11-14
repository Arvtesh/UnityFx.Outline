// Copyright (C) 2019-2021 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// This asset is used to store references to shaders and other resources needed at runtime without having to use a Resources folder.
	/// </summary>
	/// <seealso cref="OutlineRenderer"/>
	[CreateAssetMenu(fileName = "OutlineResources", menuName = "UnityFx/Outline/Outline Resources")]
	public sealed class OutlineResources : ScriptableObject
	{
		#region data

		[SerializeField]
		private Shader _renderShader;
		[SerializeField]
		private Shader _outlineShader;

		private Material _renderMaterial;
		private Material _outlineMaterial;
		private MaterialPropertyBlock _props;
		private Mesh _fullscreenTriangleMesh;
		private float[][] _gaussSamples;
		private bool _useDrawMesh;

		#endregion

		#region interface

		/// <summary>
		/// Minimum value of outline width parameter.
		/// </summary>
		/// <seealso cref="MaxWidth"/>
		public const int MinWidth = 1;

		/// <summary>
		/// Maximum value of outline width parameter.
		/// </summary>
		/// <remarks>
		/// If the value is changed here, it should be adjusted in Outline.shader as well.
		/// </remarks>
		/// <seealso cref="MinWidth"/>
		public const int MaxWidth = 32;

		/// <summary>
		/// Minimum value of outline intensity parameter.
		/// </summary>
		/// <seealso cref="MaxIntensity"/>
		/// <seealso cref="SolidIntensity"/>
		public const int MinIntensity = 1;

		/// <summary>
		/// Maximum value of outline intensity parameter.
		/// </summary>
		/// <seealso cref="MinIntensity"/>
		/// <seealso cref="SolidIntensity"/>
		public const int MaxIntensity = 64;

		/// <summary>
		/// Value of outline intensity parameter that is treated as solid fill.
		/// </summary>
		/// <seealso cref="MinIntensity"/>
		/// <seealso cref="MaxIntensity"/>
		public const int SolidIntensity = 100;

		/// <summary>
		/// Minimum value of outline alpha cutoff parameter.
		/// </summary>
		/// <seealso cref="MaxAlphaCutoff"/>
		public const float MinAlphaCutoff = 0;

		/// <summary>
		/// Maximum value of outline alpha cutoff parameter.
		/// </summary>
		/// <seealso cref="MinAlphaCutoff"/>
		public const float MaxAlphaCutoff = 1;

		/// <summary>
		/// Name of _MainTex shader parameter.
		/// </summary>
		public const string MainTexName = "_MainTex";

		/// <summary>
		/// Name of _MaskTex shader parameter.
		/// </summary>
		public const string MaskTexName = "_MaskTex";

		/// <summary>
		/// Name of _TempTex shader parameter.
		/// </summary>
		public const string TempTexName = "_TempTex";

		/// <summary>
		/// Name of _Color shader parameter.
		/// </summary>
		public const string ColorName = "_Color";

		/// <summary>
		/// Name of _Width shader parameter.
		/// </summary>
		public const string WidthName = "_Width";

		/// <summary>
		/// Name of _Intensity shader parameter.
		/// </summary>
		public const string IntensityName = "_Intensity";

		/// <summary>
		/// Name of _Cutoff shader parameter.
		/// </summary>
		public const string AlphaCutoffName = "_Cutoff";

		/// <summary>
		/// Name of _GaussSamples shader parameter.
		/// </summary>
		public const string GaussSamplesName = "_GaussSamples";

		/// <summary>
		/// Name of the _USE_DRAWMESH shader feature.
		/// </summary>
		public const string UseDrawMeshFeatureName = "_USE_DRAWMESH";

		/// <summary>
		/// Name of the outline effect.
		/// </summary>
		public const string EffectName = "Outline";

		/// <summary>
		/// Tooltip text for <see cref="OutlineResources"/> field.
		/// </summary>
		public const string OutlineResourcesTooltip = "Outline resources to use (shaders, materials etc). Do not change defaults unless you know what you're doing.";

		/// <summary>
		/// Tooltip text for <see cref="OutlineLayerCollection"/> field.
		/// </summary>
		public const string OutlineLayerCollectionTooltip = "Collection of outline layers to use. This can be used to share outline settings between multiple cameras.";

		/// <summary>
		/// Tooltip text for outline <see cref="LayerMask"/> field.
		/// </summary>
		public const string OutlineLayerMaskTooltip = "Layer mask for outined objects.";

		/// <summary>
		/// Tooltip text for outline <see cref="LayerMask"/> field.
		/// </summary>
		public const string OutlineRenderingLayerMaskTooltip = "Rendering layer mask for outined objects.";

		/// <summary>
		/// Index of the default pass in <see cref="RenderShader"/>.
		/// </summary>
		public const int RenderShaderDefaultPassId = 0;

		/// <summary>
		/// Index of the alpha-test pass in <see cref="RenderShader"/>.
		/// </summary>
		public const int RenderShaderAlphaTestPassId = 1;

		/// <summary>
		/// Index of the HPass in <see cref="OutlineShader"/>.
		/// </summary>
		public const int OutlineShaderHPassId = 0;

		/// <summary>
		/// Index of the VPass in <see cref="OutlineShader"/>.
		/// </summary>
		public const int OutlineShaderVPassId = 1;

		/// <summary>
		/// SRP not supported message.
		/// </summary>
		internal const string SrpNotSupported = "{0} works with built-in render pipeline only. It does not support SRP (including URP and HDRP).";

		/// <summary>
		/// Post-processing not supported message.
		/// </summary>
		internal const string PpNotSupported = "{0} does not support Unity Post-processing stack v2. It might not work as expected.";

		/// <summary>
		/// Hashed name of _MainTex shader parameter.
		/// </summary>
		public readonly int MainTexId = Shader.PropertyToID(MainTexName);

		/// <summary>
		/// Texture identifier for _MainTex shader parameter.
		/// </summary>
		public readonly RenderTargetIdentifier MainTex = new RenderTargetIdentifier(MainTexName);

		/// <summary>
		/// Hashed name of _MaskTex shader parameter.
		/// </summary>
		public readonly int MaskTexId = Shader.PropertyToID(MaskTexName);

		/// <summary>
		/// Texture identifier for _MaskTex shader parameter.
		/// </summary>
		public readonly RenderTargetIdentifier MaskTex = new RenderTargetIdentifier(MaskTexName);

		/// <summary>
		/// Hashed name of _TempTex shader parameter.
		/// </summary>
		public readonly int TempTexId = Shader.PropertyToID(TempTexName);

		/// <summary>
		/// Texture identifier for _TempTex shader parameter.
		/// </summary>
		public readonly RenderTargetIdentifier TempTex = new RenderTargetIdentifier(TempTexName);

		/// <summary>
		/// Hashed name of _Color shader parameter.
		/// </summary>
		public readonly int ColorId = Shader.PropertyToID(ColorName);

		/// <summary>
		/// Hashed name of _Width shader parameter.
		/// </summary>
		public readonly int WidthId = Shader.PropertyToID(WidthName);

		/// <summary>
		/// Hashed name of _Intensity shader parameter.
		/// </summary>
		public readonly int IntensityId = Shader.PropertyToID(IntensityName);

		/// <summary>
		/// Hashed name of _Cutoff shader parameter.
		/// </summary>
		public readonly int AlphaCutoffId = Shader.PropertyToID(AlphaCutoffName);

		/// <summary>
		/// Hashed name of _GaussSamples shader parameter.
		/// </summary>
		public readonly int GaussSamplesId = Shader.PropertyToID(GaussSamplesName);

		/// <summary>
		/// Temp materials list. Used by <see cref="OutlineRenderer"/> to avoid GC allocations.
		/// </summary>
		internal readonly List<Material> TmpMaterials = new List<Material>();

		/// <summary>
		/// Gets a <see cref="Shader"/> that renders objects outlined with a solid while color.
		/// </summary>
		public Shader RenderShader
		{
			get
			{
				return _renderShader;
			}
		}

		/// <summary>
		/// Gets a <see cref="Shader"/> that renders outline around the mask, that was generated with <see cref="RenderShader"/>.
		/// </summary>
		public Shader OutlineShader
		{
			get
			{
				return _outlineShader;
			}
		}

		/// <summary>
		/// Gets a <see cref="RenderShader"/>-based material.
		/// </summary>
		public Material RenderMaterial
		{
			get
			{
				if (_renderMaterial == null)
				{
					UnityEngine.Debug.Assert(_renderShader != null, "No RenderShader is set in outline resources.", this);

					_renderMaterial = new Material(_renderShader)
					{
						name = "Outline - RenderColor",
						hideFlags = HideFlags.HideAndDontSave
					};
				}

				return _renderMaterial;
			}
		}

		/// <summary>
		/// Gets a <see cref="OutlineShader"/>-based material.
		/// </summary>
		public Material OutlineMaterial
		{
			get
			{
				if (_outlineMaterial == null)
				{
					UnityEngine.Debug.Assert(_outlineShader != null, "No OutlineShader is set in outline resources.", this);

					_outlineMaterial = new Material(_outlineShader)
					{
						name = "Outline - Main",
						hideFlags = HideFlags.HideAndDontSave
					};

					if (_useDrawMesh)
					{
						_outlineMaterial.EnableKeyword(UseDrawMeshFeatureName);
					}
				}

				return _outlineMaterial;
			}
		}

		/// <summary>
		/// Gets a <see cref="MaterialPropertyBlock"/> for <see cref="VPassBlendMaterial"/>.
		/// </summary>
		public MaterialPropertyBlock Properties
		{
			get
			{
				if (_props is null)
				{
					_props = new MaterialPropertyBlock();
				}

				return _props;
			}
		}

		/// <summary>
		/// Gets or sets a fullscreen triangle mesh. The mesh is lazy-initialized on the first access.
		/// </summary>
		/// <remarks>
		/// This is used by <see cref="OutlineRenderer"/> to avoid Blit() calls and use DrawMesh() passing
		/// this mesh as the first argument. When running on a device with Shader Model 3.5 support this
		/// should not be used at all, as the vertices are generated in vertex shader with DrawProcedural() call.
		/// </remarks>
		/// <seealso cref="OutlineRenderer"/>
		public Mesh FullscreenTriangleMesh
		{
			get
			{
				if (_fullscreenTriangleMesh == null)
				{
					_fullscreenTriangleMesh = new Mesh()
					{
						name = "Outline - FullscreenTriangle",
						hideFlags = HideFlags.HideAndDontSave,
						vertices = new Vector3[] { new Vector3(-1, -1, 0), new Vector3(3, -1, 0), new Vector3(-1, 3, 0) },
						triangles = new int[] { 0, 1, 2 }
					};

					_fullscreenTriangleMesh.UploadMeshData(true);
				}

				return _fullscreenTriangleMesh;
			}
			set
			{
				_fullscreenTriangleMesh = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether <see cref="FullscreenTriangleMesh"/> is used for image effects rendering even when procedural rendering is available.
		/// </summary>
		public bool UseFullscreenTriangleMesh
		{
			get
			{
				return _useDrawMesh;
			}
			set
			{
				if (_useDrawMesh != value)
				{
					_useDrawMesh = value;

					if (_outlineMaterial)
					{
						if (_useDrawMesh)
						{
							_outlineMaterial.EnableKeyword(UseDrawMeshFeatureName);
						}
						else
						{
							_outlineMaterial.DisableKeyword(UseDrawMeshFeatureName);
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the instance is in valid state.
		/// </summary>
		public bool IsValid => RenderShader && OutlineShader;

		/// <summary>
		/// Returns a <see cref="MaterialPropertyBlock"/> instance initialized with values from <paramref name="settings"/>.
		/// </summary>
		public MaterialPropertyBlock GetProperties(IOutlineSettings settings)
		{
			if (_props is null)
			{
				_props = new MaterialPropertyBlock();
			}

			_props.SetFloat(WidthId, settings.OutlineWidth);
			_props.SetColor(ColorId, settings.OutlineColor);

			if ((settings.OutlineRenderMode & OutlineRenderFlags.Blurred) != 0)
			{
				_props.SetFloat(IntensityId, settings.OutlineIntensity);
			}
			else
			{
				_props.SetFloat(IntensityId, SolidIntensity);
			}

			return _props;
		}

		/// <summary>
		/// Gets cached gauss samples for the specified outline <paramref name="width"/>.
		/// </summary>
		public float[] GetGaussSamples(int width)
		{
			var index = Mathf.Clamp(width, 1, MaxWidth) - 1;

			if (_gaussSamples is null)
			{
				_gaussSamples = new float[MaxWidth][];
			}

			if (_gaussSamples[index] is null)
			{
				_gaussSamples[index] = GetGaussSamples(width, null);
			}

			return _gaussSamples[index];
		}

		/// <summary>
		/// Resets the resources to defaults.
		/// </summary>
		public void ResetToDefaults()
		{
			_renderShader = Shader.Find("Hidden/UnityFx/OutlineColor");
			_outlineShader = Shader.Find("Hidden/UnityFx/Outline");
		}

		/// <summary>
		/// Calculates value of Gauss function for the specified <paramref name="x"/> and <paramref name="stdDev"/> values.
		/// </summary>
		/// <seealso href="https://en.wikipedia.org/wiki/Gaussian_blur"/>
		/// <seealso href="https://en.wikipedia.org/wiki/Normal_distribution"/>
		public static float Gauss(float x, float stdDev)
		{
			var stdDev2 = stdDev * stdDev * 2;
			var a = 1 / Mathf.Sqrt(Mathf.PI * stdDev2);
			var gauss = a * Mathf.Pow((float)Math.E, -x * x / stdDev2);

			return gauss;
		}

		/// <summary>
		/// Samples Gauss function for the specified <paramref name="width"/>.
		/// </summary>
		/// <seealso href="https://en.wikipedia.org/wiki/Normal_distribution"/>
		public static float[] GetGaussSamples(int width, float[] samples)
		{
			// NOTE: According to '3 sigma' rule there is no reason to have StdDev less then width / 3.
			// In practice blur looks best when StdDev is within range [width / 3,  width / 2].
			var stdDev = width * 0.5f;

			if (samples is null)
			{
				samples = new float[MaxWidth];
			}

			for (var i = 0; i < width; i++)
			{
				samples[i] = Gauss(i, stdDev);
			}

			return samples;
		}

		/// <summary>
		/// Writes a console warning if SRP is detected.
		/// </summary>
		public static void LogSrpNotSupported(UnityEngine.Object obj)
		{
			if (GraphicsSettings.renderPipelineAsset)
			{
				UnityEngine.Debug.LogWarningFormat(obj, SrpNotSupported, obj.GetType().Name);
			}
		}

		/// <summary>
		/// Writes a console warning if Post Processing Stack v2 is detected.
		/// </summary>
		[Conditional("UNITY_POST_PROCESSING_STACK_V2")]
		public static void LogPpNotSupported(UnityEngine.Object obj)
		{
			UnityEngine.Debug.LogWarningFormat(obj, PpNotSupported, obj.GetType().Name);
		}

		#endregion

		#region ScriptableObject

		private void OnValidate()
		{
			if (_renderMaterial)
			{
				_renderMaterial.shader = _renderShader;
			}

			if (_outlineMaterial)
			{
				_outlineMaterial.shader = _outlineShader;
			}
		}

		#endregion
	}
}
