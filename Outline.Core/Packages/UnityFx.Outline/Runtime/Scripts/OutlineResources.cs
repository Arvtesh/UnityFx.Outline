// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
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

		private Material _renderMaterial;
		private Material _hPassMaterial;
		private Material _vPassMaterial;
		private MaterialPropertyBlock _hPassProperties;
		private MaterialPropertyBlock _vPassProperties;
		private Mesh _fullscreenTriangleMesh;
		private float[][] _gaussSamples;

		#endregion

		#region interface

		/// <summary>
		/// Hashed name of _Color shader parameter.
		/// </summary>
		public readonly int ColorId = Shader.PropertyToID("_Color");

		/// <summary>
		/// Hashed name of _Width shader parameter.
		/// </summary>
		public readonly int WidthId = Shader.PropertyToID("_Width");

		/// <summary>
		/// Hashed name of _Intensity shader parameter.
		/// </summary>
		public readonly int IntensityId = Shader.PropertyToID("_Intensity");

		/// <summary>
		/// Hashed name of _GaussSamples shader parameter.
		/// </summary>
		public readonly int GaussSamplesId = Shader.PropertyToID("_GaussSamples");

		/// <summary>
		/// Temp materials list. Used by <see cref="OutlineRenderer"/> to avoid GC allocations.
		/// </summary>
		public readonly List<Material> TmpMaterials = new List<Material>();

		/// <summary>
		/// Gets or sets a <see cref="Shader"/> that renders objects outlined with a solid while color.
		/// </summary>
		public Shader RenderShader;

		/// <summary>
		/// Gets or sets a <see cref="Shader"/> that renders outline around the mask, that was generated with <see cref="RenderShader"/> (pass 1).
		/// </summary>
		public Shader HPassShader;

		/// <summary>
		/// Gets or sets a <see cref="Shader"/> that renders outline around the mask, that was generated with <see cref="RenderShader"/> and <see cref="HPassShader"/> (pass 2).
		/// </summary>
		public Shader VPassBlendShader;

		/// <summary>
		/// Gets a <see cref="RenderShader"/>-based material.
		/// </summary>
		public Material RenderMaterial
		{
			get
			{
				if (_renderMaterial == null)
				{
					_renderMaterial = new Material(RenderShader)
					{
						name = "Outline - SimpleRender",
						hideFlags = HideFlags.HideAndDontSave
					};
				}

				return _renderMaterial;
			}
		}

		/// <summary>
		/// Gets a <see cref="HPassShader"/>-based material.
		/// </summary>
		public Material HPassMaterial
		{
			get
			{
				if (_hPassMaterial == null)
				{
					_hPassMaterial = new Material(HPassShader)
					{
						name = "Outline - HPassRender",
						hideFlags = HideFlags.HideAndDontSave
					};
				}

				return _hPassMaterial;
			}
		}

		/// <summary>
		/// Gets a <see cref="VPassBlendShader"/>-based material.
		/// </summary>
		public Material VPassBlendMaterial
		{
			get
			{
				if (_vPassMaterial == null)
				{
					_vPassMaterial = new Material(VPassBlendShader)
					{
						name = "Outline - VPassBlendRender",
						hideFlags = HideFlags.HideAndDontSave
					};
				}

				return _vPassMaterial;
			}
		}

		/// <summary>
		/// Gets a <see cref="MaterialPropertyBlock"/> for <see cref="HPassMaterial"/>.
		/// </summary>
		public MaterialPropertyBlock HPassProperties
		{
			get
			{
				if (_hPassProperties == null)
				{
					_hPassProperties = new MaterialPropertyBlock();
				}

				return _hPassProperties;
			}
		}

		/// <summary>
		/// Gets a <see cref="MaterialPropertyBlock"/> for <see cref="VPassBlendMaterial"/>.
		/// </summary>
		public MaterialPropertyBlock VPassBlendProperties
		{
			get
			{
				if (_vPassProperties == null)
				{
					_vPassProperties = new MaterialPropertyBlock();
				}

				return _vPassProperties;
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
		/// Gets a value indicating whether the instance is in valid state.
		/// </summary>
		public bool IsValid
		{
			get
			{
				return RenderShader && HPassShader && VPassBlendShader;
			}
		}

		/// <summary>
		/// Gets cached gauss samples for the specified outline <paramref name="width"/>.
		/// </summary>
		public float[] GetGaussSamples(int width)
		{
			var index = Mathf.Clamp(width, 1, OutlineRenderer.MaxWidth) - 1;

			if (_gaussSamples == null)
			{
				_gaussSamples = new float[OutlineRenderer.MaxWidth][];
			}

			if (_gaussSamples[index] == null)
			{
				_gaussSamples[index] = OutlineRenderer.GetGaussSamples(width, null);
			}

			return _gaussSamples[index];
		}

		/// <summary>
		/// Resets the resources to defaults.
		/// </summary>
		public void ResetToDefaults()
		{
			RenderShader = Shader.Find("UnityFx/Outline/RenderColor");
			HPassShader = Shader.Find("UnityFx/Outline/HPass");
			VPassBlendShader = Shader.Find("UnityFx/Outline/VPassBlend");
		}

		#endregion
	}
}
