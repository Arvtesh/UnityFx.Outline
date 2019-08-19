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
		[SerializeField]
		private Color _outlineColor = Color.red;
		[SerializeField]
		[Range(OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth)]
		private int _outlineWidth = 5;

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
				_outlineLayer = _outlineEffect.AddLayer();
			}
		}

		private void OnValidate()
		{
			if (_outlineEffect == null)
			{
				_outlineEffect = GetComponent<OutlineEffect>();
			}

			if (_outlineLayer == null)
			{
				_outlineLayer = _outlineEffect.AddLayer();
			}

			foreach (var go in _outlineGos)
			{
				if (go)
				{
					_outlineLayer.OutlineColor = _outlineColor;
					_outlineLayer.OutlineWidth = _outlineWidth;
					_outlineLayer.Add(go);
				}
			}
		}

		#endregion
	}
}
