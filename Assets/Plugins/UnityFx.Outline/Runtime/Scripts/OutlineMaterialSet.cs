// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A set of materials needed to render outline.
	/// </summary>
	public class OutlineMaterialSet
	{
		#region data

		private readonly int _colorNameId = Shader.PropertyToID(OutlineRenderer.ColorParamName);
		private readonly int _widthNameId = Shader.PropertyToID(OutlineRenderer.WidthParamName);

		private readonly OutlineResources _outlineResources;
		private readonly Material _renderMaterial;
		private readonly Material _hPassMaterial;
		private readonly Material _vPassMaterial;

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
		public OutlineMaterialSet(OutlineResources resources, Material renderMaterial)
		{
			_outlineResources = resources;
			_renderMaterial = renderMaterial;
			_hPassMaterial = new Material(resources.HPassShader);
			_vPassMaterial = new Material(resources.VPassBlendShader);
		}

		/// <summary>
		/// Sets outline color value.
		/// </summary>
		public void SetColor(Color color)
		{
			_vPassMaterial.SetColor(_colorNameId, color);
		}

		/// <summary>
		/// Sets outline width value.
		/// </summary>
		public void SetWidth(int width)
		{
			_hPassMaterial.SetInt(_widthNameId, width);
			_vPassMaterial.SetInt(_widthNameId, width);
		}

		/// <summary>
		/// Sets outline mode value.
		/// </summary>
		public void SetMode(OutlineMode mode)
		{
			OutlineRenderer.SetupMeterialKeywords(_hPassMaterial, mode);
			OutlineRenderer.SetupMeterialKeywords(_vPassMaterial, mode);
		}

		#endregion
	}
}
