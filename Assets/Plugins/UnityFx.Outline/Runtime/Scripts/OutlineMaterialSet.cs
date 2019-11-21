// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A set of materials needed to render outlines.
	/// </summary>
	/// <seealso cref="OutlineRenderer"/>
	public sealed class OutlineMaterialSet  : IOutlineSettings, IDisposable
	{
		#region data

		private const string _renderMaterialName = "Outline - SimpleRender";
		private const string _hPassMaterialName = "Outline - HPassRender";
		private const string _vPassMaterialName = "Outline - VPassBlendRender";

		private readonly OutlineResources _outlineResources;
		private readonly Material _renderMaterial;
		private readonly Material _hPassMaterial;
		private readonly Material _vPassMaterial;
		private readonly float[] _gaussSamples = new float[OutlineRenderer.MaxWidth];

		private Color _color;
		private int _width = OutlineRenderer.MinWidth;
		private float _intensity = OutlineRenderer.MinIntensity;
		private OutlineMode _mode;
		private bool _disposed;

		#endregion

		#region interface

		/// <summary>
		/// NameID of the outline color shader parameter.
		/// </summary>
		public readonly int ColorNameId = Shader.PropertyToID("_Color");

		/// <summary>
		/// NameID of the outline width shader parameter.
		/// </summary>
		public readonly int WidthNameId = Shader.PropertyToID("_Width");

		/// <summary>
		/// NameID of the outline intensity shader parameter.
		/// </summary>
		public readonly int IntensityNameId = Shader.PropertyToID("_Intensity");

		/// <summary>
		/// NameID of the outline width shader parameter.
		/// </summary>
		public readonly int GaussSamplesNameId = Shader.PropertyToID("_GaussSamples");

		/// <summary>
		/// Gets resources used by the effect implementation.
		/// </summary>
		public OutlineResources OutlineResources
		{
			get
			{
				return _outlineResources;
			}
		}

		/// <summary>
		/// Gets material for <see cref="OutlineResources.RenderShader"/>.
		/// </summary>
		/// <seealso cref="HPassMaterial"/>
		/// <seealso cref="VPassBlendMaterial"/>
		public Material RenderMaterial
		{
			get
			{
				return _renderMaterial;
			}
		}

		/// <summary>
		/// Gets material for <see cref="OutlineResources.HPassShader"/>.
		/// </summary>
		/// <seealso cref="VPassBlendMaterial"/>
		/// <seealso cref="RenderMaterial"/>
		public Material HPassMaterial
		{
			get
			{
				return _hPassMaterial;
			}
		}

		/// <summary>
		/// Gets material for <see cref="OutlineResources.VPassBlendShader"/>.
		/// </summary>
		/// <seealso cref="HPassMaterial"/>
		/// <seealso cref="RenderMaterial"/>
		public Material VPassBlendMaterial
		{
			get
			{
				return _vPassMaterial;
			}
		}

		/// <summary>
		/// Gets Gauss samples for blur calculations.
		/// </summary>
		public float[] GaussSamples
		{
			get
			{
				return _gaussSamples;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineMaterialSet"/> class.
		/// </summary>
		/// <remarks>
		/// The preferred way of creating instances of <see cref="OutlineMaterialSet"/> is calling <see cref="OutlineResources.CreateMaterialSet"/> method.
		/// </remarks>
		public OutlineMaterialSet(OutlineResources resources)
		{
			if (resources == null)
			{
				throw new ArgumentNullException("resources");
			}

			_outlineResources = resources;
			_renderMaterial = CreateRenderMaterial(resources.RenderShader);
			_hPassMaterial = CreateHPassMaterial(resources.HPassShader);
			_vPassMaterial = CreateVPassMaterial(resources.VPassBlendShader);

			_hPassMaterial.SetInt(WidthNameId, _width);
			_vPassMaterial.SetInt(WidthNameId, _width);
			_vPassMaterial.SetColor(ColorNameId, _color);
			_vPassMaterial.SetFloat(IntensityNameId, OutlineRenderer.SolidIntensity);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineMaterialSet"/> class.
		/// </summary>
		internal OutlineMaterialSet(OutlineResources resources, Material renderMaterial)
		{
			Debug.Assert(resources);
			Debug.Assert(resources.IsValid);
			Debug.Assert(renderMaterial);

			_outlineResources = resources;
			_renderMaterial = renderMaterial;
			_hPassMaterial = CreateHPassMaterial(resources.HPassShader);
			_vPassMaterial = CreateVPassMaterial(resources.VPassBlendShader);

			_hPassMaterial.SetInt(WidthNameId, _width);
			_vPassMaterial.SetInt(WidthNameId, _width);
			_vPassMaterial.SetColor(ColorNameId, _color);
			_vPassMaterial.SetFloat(IntensityNameId, OutlineRenderer.SolidIntensity);
		}

		/// <summary>
		/// Resets materials state.
		/// </summary>
		/// <seealso cref="SetColor(Color)"/>
		/// <seealso cref="SetWidth(int)"/>
		/// <seealso cref="SetMode(OutlineMode)"/>
		public void Reset(IOutlineSettings settings)
		{
			ThrowIfDisposed();

			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			SetColor(settings.OutlineColor);
			SetIntensity(settings.OutlineIntensity);
			SetWidth(settings.OutlineWidth);
			SetMode(settings.OutlineMode);
			UpdateGaussSamples();
		}

		internal static Material CreateRenderMaterial(Shader shader)
		{
			return new Material(shader)
			{
				name = _renderMaterialName,
				hideFlags = HideFlags.HideAndDontSave
			};
		}

		internal static Material CreateHPassMaterial(Shader shader)
		{
			return new Material(shader)
			{
				name = _hPassMaterialName,
				hideFlags = HideFlags.HideAndDontSave
			};
		}

		internal static Material CreateVPassMaterial(Shader shader)
		{
			return new Material(shader)
			{
				name = _vPassMaterialName,
				hideFlags = HideFlags.HideAndDontSave
			};
		}

		#endregion

		#region IOutlineSettings

		/// <inheritdoc/>
		public Color OutlineColor
		{
			get
			{
				return _color;
			}
			set
			{
				ThrowIfDisposed();
				SetColor(value);
			}
		}

		/// <inheritdoc/>
		public int OutlineWidth
		{
			get
			{
				return _width;
			}
			set
			{
				ThrowIfDisposed();

				if (_width != value)
				{
					SetWidth(value);
					UpdateGaussSamples();
				}
			}
		}

		/// <inheritdoc/>
		public float OutlineIntensity
		{
			get
			{
				return _intensity;
			}
			set
			{
				ThrowIfDisposed();

				if (_intensity != value)
				{
					SetIntensity(value);
				}
			}
		}

		/// <inheritdoc/>
		public OutlineMode OutlineMode
		{
			get
			{
				return _mode;
			}
			set
			{
				ThrowIfDisposed();

				if (_mode != value)
				{
					SetMode(value);
				}
			}
		}

		#endregion

		#region IDisposable

		/// <inheritdoc/>
		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				if (_renderMaterial)
				{
					UnityEngine.Object.DestroyImmediate(_renderMaterial);
				}

				if (_hPassMaterial)
				{
					UnityEngine.Object.DestroyImmediate(_hPassMaterial);
				}

				if (_vPassMaterial)
				{
					UnityEngine.Object.DestroyImmediate(_vPassMaterial);
				}
			}
		}

		#endregion

		#region implementation

		private void SetColor(Color color)
		{
			_color = color;
			_vPassMaterial.SetColor(ColorNameId, color);
		}

		private void SetWidth(int width)
		{
			_width = Mathf.Clamp(width, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);
			_hPassMaterial.SetInt(WidthNameId, _width);
			_vPassMaterial.SetInt(WidthNameId, _width);
		}

		private void SetIntensity(float intensity)
		{
			_intensity = Mathf.Clamp(intensity, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);
			_vPassMaterial.SetFloat(IntensityNameId, _intensity);
		}

		private void SetMode(OutlineMode mode)
		{
			_mode = mode;

			if (mode == OutlineMode.Solid)
			{
				_vPassMaterial.SetFloat(IntensityNameId, OutlineRenderer.SolidIntensity);
			}
			else
			{
				_vPassMaterial.SetFloat(IntensityNameId, _intensity);
			}
		}

		private void UpdateGaussSamples()
		{
			OutlineRenderer.GetGaussSamples(_width, _gaussSamples);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion
	}
}
