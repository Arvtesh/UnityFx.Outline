// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A set of materials needed to render outlines.
	/// </summary>
	public class OutlineMaterialSet
	{
		#region data

		private readonly int _colorNameId = Shader.PropertyToID(OutlineRenderer.ColorParamName);
		private readonly int _widthNameId = Shader.PropertyToID(OutlineRenderer.WidthParamName);
		private readonly int _gaussSmaplesId = Shader.PropertyToID(OutlineRenderer.GaussSamplesParamName);

		private readonly OutlineResources _outlineResources;
		private readonly Material _renderMaterial;
		private readonly Material _hPassMaterial;
		private readonly Material _vPassMaterial;

		private int _width;
		private float[] _gaussSamples;

		#endregion

		#region interface

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
			SetWidth(settings.OutlineWidth);
			SetMode(settings.OutlineMode);
		}

		/// <summary>
		/// Sets outline color value.
		/// </summary>
		/// <seealso cref="SetWidth(int)"/>
		/// <seealso cref="SetMode(OutlineMode)"/>
		public void SetColor(Color color)
		{
			_vPassMaterial.SetColor(_colorNameId, color);
		}

		/// <summary>
		/// Sets outline width value.
		/// </summary>
		/// <seealso cref="SetColor(Color)"/>
		/// <seealso cref="SetMode(OutlineMode)"/>
		public void SetWidth(int width)
		{
			Debug.Assert(width >= OutlineRenderer.MinWidth);
			Debug.Assert(width <= OutlineRenderer.MaxWidth);

			if (width != _width)
			{
				_width = width;
				_gaussSamples = OutlineRenderer.GetGaussSamples(width, _gaussSamples);

				_hPassMaterial.SetInt(_widthNameId, width);
				_vPassMaterial.SetInt(_widthNameId, width);

				_hPassMaterial.SetFloatArray(_gaussSmaplesId, _gaussSamples);
				_vPassMaterial.SetFloatArray(_gaussSmaplesId, _gaussSamples);
			}
			
		}

		/// <summary>
		/// Sets outline mode value.
		/// </summary>
		/// <seealso cref="SetWidth(int)"/>
		/// <seealso cref="SetColor(Color)"/>
		public void SetMode(OutlineMode mode)
		{
			if (mode == OutlineMode.Solid)
			{
				_hPassMaterial.EnableKeyword(OutlineRenderer.ModeSolidKeyword);
				_vPassMaterial.EnableKeyword(OutlineRenderer.ModeSolidKeyword);

				_hPassMaterial.DisableKeyword(OutlineRenderer.ModeBlurredKeyword);
				_vPassMaterial.DisableKeyword(OutlineRenderer.ModeBlurredKeyword);
			}
			else
			{
				_hPassMaterial.EnableKeyword(OutlineRenderer.ModeBlurredKeyword);
				_vPassMaterial.EnableKeyword(OutlineRenderer.ModeBlurredKeyword);

				_hPassMaterial.DisableKeyword(OutlineRenderer.ModeSolidKeyword);
				_vPassMaterial.DisableKeyword(OutlineRenderer.ModeSolidKeyword);
			}
		}

		#endregion
	}
}
