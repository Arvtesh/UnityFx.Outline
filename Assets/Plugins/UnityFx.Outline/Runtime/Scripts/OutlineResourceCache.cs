// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// Shared outline resource cache.
	/// </summary>
	internal class OutlineResourceCache
	{
		private OutlineResources _outlineResources;
		private Dictionary<object, Material> _renderMaterials;
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

			Material mat;

			if (_renderMaterials == null)
			{
				_renderMaterials = new Dictionary<object, Material>();
			}

			if (!_renderMaterials.TryGetValue(obj, out mat))
			{
				mat = new Material(_outlineResources.RenderShader);
				_renderMaterials.Add(obj, mat);
			}

			return mat;
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
				if (_renderMaterials != null)
				{
					if (_outlineResources.RenderShader)
					{
						foreach (var m in _renderMaterials.Values)
						{
							m.shader = _outlineResources.RenderShader;
						}
					}
					else
					{
						_renderMaterials.Clear();
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
			if (_renderMaterials != null)
			{
				_renderMaterials.Clear();
			}

			if (_postProcessMaterials != null)
			{
				_postProcessMaterials.Clear();
			}
		}
	}
}
