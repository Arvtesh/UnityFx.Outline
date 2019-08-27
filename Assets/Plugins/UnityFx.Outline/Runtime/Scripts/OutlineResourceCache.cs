// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	internal class OutlineResourceCache
	{
		private OutlineResources _outlineResources;
		private Material _renderMaterial;
		private Dictionary<object, Material> _postProcessMaterials;

		public OutlineResources OutlineResources
		{
			get
			{
				return _outlineResources;
			}
			set
			{
				if (_outlineResources != value)
				{
					_outlineResources = value;
					UpdateResources();
				}
			}
		}

		public Material GetRenderMaterial(object obj)
		{
			Debug.Assert(obj != null);
			Debug.Assert(_outlineResources != null);

			if (_renderMaterial == null)
			{
				_renderMaterial = new Material(_outlineResources.RenderShader);
			}

			return _renderMaterial;
		}

		public Material GetPostProcessMaterial(object obj)
		{
			Debug.Assert(obj != null);
			Debug.Assert(_outlineResources != null);

			Material mat;

			if (_postProcessMaterials == null)
			{
				_postProcessMaterials = new Dictionary<object, Material>();
			}

			if (!_postProcessMaterials.TryGetValue(obj, out mat))
			{
				mat = new Material(_outlineResources.PostProcessShader);
				_postProcessMaterials.Add(obj, mat);
			}

			return mat;
		}

		public void UpdateResources()
		{
			if (_outlineResources)
			{
				if (_renderMaterial != null)
				{
					if (_outlineResources.RenderShader)
					{
						_renderMaterial.shader = _outlineResources.RenderShader;
					}
					else
					{
						_renderMaterial = null;
					}
				}

				if (_postProcessMaterials != null)
				{
					if (_outlineResources.PostProcessShader)
					{
						foreach (var m in _postProcessMaterials.Values)
						{
							m.shader = _outlineResources.PostProcessShader;
						}
					}
					else
					{
						_postProcessMaterials.Clear();
					}
				}
			}
			else
			{
				Clear();
			}
		}

		public void Clear()
		{
			if (_postProcessMaterials != null)
			{
				_postProcessMaterials.Clear();
			}

			_renderMaterial = null;
		}
	}
}
