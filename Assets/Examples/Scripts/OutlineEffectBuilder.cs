// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline.Examples
{
	/// <summary>
	/// Helperr class for managing outlines from editor.
	/// </summary>
	[RequireComponent(typeof(OutlineEffect))]
	public class OutlineEffectBuilder : MonoBehaviour
	{
		#region data

#pragma warning disable 0649

		[SerializeField]
		private GameObject[] _outlineGos;

#pragma warning restore 0649

		private OutlineEffect _outlineEffect;
		private OutlineLayer _outlineLayer;

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			if (_outlineEffect == null)
			{
				_outlineEffect = GetComponent<OutlineEffect>();
			}

			if (_outlineLayer == null)
			{
				if (_outlineEffect.OutlineLayers.Count > 0)
				{
					_outlineLayer = _outlineEffect.OutlineLayers[0];
				}
				else
				{
					_outlineLayer = new OutlineLayer();
					_outlineEffect.OutlineLayers.Add(_outlineLayer);
				}
			}

			foreach (var go in _outlineGos)
			{
				if (go)
				{
					_outlineLayer.Add(go);
				}
			}
		}

		private void OnValidate()
		{
			if (_outlineEffect == null)
			{
				_outlineEffect = GetComponent<OutlineEffect>();
			}

			if (_outlineEffect.OutlineLayers.Count > 0)
			{
				_outlineLayer = _outlineEffect.OutlineLayers[0];
			}
			else
			{
				_outlineLayer = new OutlineLayer();
				_outlineEffect.OutlineLayers.Add(_outlineLayer);
			}

			foreach (var go in _outlineGos)
			{
				if (go)
				{
					_outlineLayer.Add(go);
				}
			}
		}

		#endregion
	}
}
