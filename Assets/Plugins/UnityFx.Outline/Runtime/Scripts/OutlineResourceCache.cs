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
		private Dictionary<object, Material> _hPassMeterials;
		private Dictionary<object, Material> _vPassMeterials;

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

		public Material GetHPassMaterial(object obj)
		{
			Debug.Assert(obj != null);
			Debug.Assert(_outlineResources != null);

			Material mat;

			if (_hPassMeterials == null)
			{
				_hPassMeterials = new Dictionary<object, Material>();
			}

			if (!_hPassMeterials.TryGetValue(obj, out mat))
			{
				mat = new Material(_outlineResources.HPassShader);
				_hPassMeterials.Add(obj, mat);
			}

			return mat;
		}

		public Material GetVPassMaterial(object obj)
		{
			Debug.Assert(obj != null);
			Debug.Assert(_outlineResources != null);

			Material mat;

			if (_vPassMeterials == null)
			{
				_vPassMeterials = new Dictionary<object, Material>();
			}

			if (!_vPassMeterials.TryGetValue(obj, out mat))
			{
				mat = new Material(_outlineResources.VPassBlendShader);
				_vPassMeterials.Add(obj, mat);
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

				if (_hPassMeterials != null)
				{
					if (_outlineResources.HPassShader)
					{
						foreach (var m in _hPassMeterials.Values)
						{
							m.shader = _outlineResources.HPassShader;
						}
					}
					else
					{
						_hPassMeterials.Clear();
					}
				}

				if (_vPassMeterials != null)
				{
					if (_outlineResources.VPassBlendShader)
					{
						foreach (var m in _vPassMeterials.Values)
						{
							m.shader = _outlineResources.VPassBlendShader;
						}
					}
					else
					{
						_vPassMeterials.Clear();
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
			if (_hPassMeterials != null)
			{
				_hPassMeterials.Clear();
			}

			if (_vPassMeterials != null)
			{
				_vPassMeterials.Clear();
			}

			_renderMaterial = null;
		}
	}
}
