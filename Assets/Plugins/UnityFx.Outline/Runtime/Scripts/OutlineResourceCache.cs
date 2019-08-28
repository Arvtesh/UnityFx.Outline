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
		private Dictionary<object, OutlineMaterialSet> _materialsMap;

		public OutlineResources OutlineResources
		{
			get
			{
				return _outlineResources;
			}
			set
			{
				Debug.Assert(value != null);

				if (_outlineResources != value)
				{
					_outlineResources = value;
					Clear();
				}
			}
		}

		public OutlineMaterialSet GetMaterials(IOutlineSettings obj)
		{
			Debug.Assert(obj != null);
			Debug.Assert(_outlineResources != null);

			OutlineMaterialSet result;

			if (_materialsMap == null)
			{
				_materialsMap = new Dictionary<object, OutlineMaterialSet>();
			}

			if (!_materialsMap.TryGetValue(obj, out result))
			{
				if (_renderMaterial == null)
				{
					_renderMaterial = new Material(_outlineResources.RenderShader);
				}

				result = new OutlineMaterialSet(_outlineResources, _renderMaterial);
				_materialsMap.Add(obj, result);
			}

			return result;
		}

		public void Clear()
		{
			if (_materialsMap != null)
			{
				_materialsMap.Clear();
			}

			_renderMaterial = null;
		}
	}
}
