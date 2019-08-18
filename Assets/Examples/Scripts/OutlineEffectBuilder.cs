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
		private int _outlineWidth = 5;
		[SerializeField]
		private Color _outlineColor = Color.red;

#pragma warning restore 0649

		private OutlineEffect _outlineEffect;
		private OutlineLayer _outlineLayer;

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			_outlineEffect = GetComponent<OutlineEffect>();
			_outlineLayer = _outlineEffect.AddLayer();
		}

		private void Update()
		{
			foreach (var go in _outlineGos)
			{
				_outlineLayer.OutlineColor = _outlineColor;
				_outlineLayer.OutlineWidth = _outlineWidth;
				_outlineLayer.Add(go);
			}
		}

		#endregion
	}
}
