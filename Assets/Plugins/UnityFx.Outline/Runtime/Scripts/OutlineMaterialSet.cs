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
	public sealed class OutlineMaterialSet  : IOutlineSettings
	{
		#region data

		private readonly OutlineResources _outlineResources;
		private readonly Material _renderMaterial;
		private readonly Material _hPassMaterial;
		private readonly Material _vPassMaterial;
		private readonly float[] _gaussSamples = new float[OutlineRenderer.MaxWidth];

		private Color _color;
		private int _width;
		private float _intensity;
		private OutlineMode _mode;

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
			_outlineResources = resources;
			_renderMaterial = new Material(resources.RenderShader);
			_hPassMaterial = new Material(resources.HPassShader);
			_vPassMaterial = new Material(resources.VPassBlendShader);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineMaterialSet"/> class.
		/// </summary>
		internal OutlineMaterialSet(OutlineResources resources, Material renderMaterial)
		{
			_outlineResources = resources;
			_renderMaterial = renderMaterial;
			_hPassMaterial = new Material(resources.HPassShader);
			_vPassMaterial = new Material(resources.VPassBlendShader);
		}

		/// <summary>
		/// Resets materials state.
		/// </summary>
		/// <seealso cref="SetColor(Color)"/>
		/// <seealso cref="SetWidth(int)"/>
		/// <seealso cref="SetMode(OutlineMode)"/>
		public void Reset(IOutlineSettings settings)
		{
			SetColor(settings.OutlineColor);
			SetIntensity(settings.OutlineIntensity);
			SetWidth(settings.OutlineWidth);
			SetMode(settings.OutlineMode);
			UpdateGaussSamples();
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
				Debug.Assert(value >= OutlineRenderer.MinWidth);
				Debug.Assert(value <= OutlineRenderer.MaxWidth);

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
				Debug.Assert(value >= OutlineRenderer.MinIntensity);
				Debug.Assert(value <= OutlineRenderer.MaxIntensity);

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
				if (_mode != value)
				{
					SetMode(value);
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
			_width = width;

			_hPassMaterial.SetInt(WidthNameId, width);
			_vPassMaterial.SetInt(WidthNameId, width);
		}

		private void SetIntensity(float intensity)
		{
			_intensity = intensity;
			_vPassMaterial.SetFloat(IntensityNameId, intensity);
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

		#endregion
	}
}
