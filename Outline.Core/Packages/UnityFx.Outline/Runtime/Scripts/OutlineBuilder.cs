// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A helper behaviour for managing content of <see cref="OutlineLayerCollection"/> via Unity Editor.
	/// </summary>
	public sealed class OutlineBuilder : MonoBehaviour
	{
		#region data

#pragma warning disable 0649

		[SerializeField, Tooltip("Collection of outline layers to manage.")]
		private OutlineLayerCollection _outlineLayers;

#pragma warning restore 0649

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets a collection of layers to manage.
		/// </summary>
		public OutlineLayerCollection OutlineLayers { get => _outlineLayers; set => _outlineLayers = value; }

		/// <summary>
		/// Clears content of all layers.
		/// </summary>
		/// <seealso cref="OutlineLayers"/>
		public void Clear()
		{
			_outlineLayers?.ClearLayerContent();
		}

		#endregion

		#region MonoBehaviour

#if UNITY_EDITOR

		private void Reset()
		{
			var effect = GetComponent<OutlineEffect>();

			if (effect)
			{
				_outlineLayers = effect.OutlineLayersInternal;
			}
		}

#endif

		#endregion
	}
}
