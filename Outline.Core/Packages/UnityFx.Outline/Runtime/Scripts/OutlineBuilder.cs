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

		[Serializable]
		internal class ContentItem
		{
			public GameObject Go;
			public int LayerIndex;
		}

#pragma warning disable 0649

		[SerializeField, Tooltip(OutlineResources.OutlineLayerCollectionTooltip)]
		private OutlineLayerCollection _outlineLayers;
		[SerializeField, HideInInspector]
		private List<ContentItem> _content;

#pragma warning restore 0649

		#endregion

		#region interface

		internal List<ContentItem> Content { get => _content; set => _content = value; }

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

		private void OnEnable()
		{
			if (_outlineLayers && _content != null)
			{
				foreach (var item in _content)
				{
					if (item.LayerIndex >= 0 && item.LayerIndex < _outlineLayers.Count && item.Go)
					{
						_outlineLayers.GetOrAddLayer(item.LayerIndex).Add(item.Go);
					}
				}
			}
		}

#if UNITY_EDITOR

		private void Reset()
		{
			var effect = GetComponent<OutlineEffect>();

			if (effect)
			{
				_outlineLayers = effect.OutlineLayersInternal;
			}
		}

		private void OnDestroy()
		{
			_outlineLayers?.ClearLayerContent();
		}

#endif

		#endregion
	}
}
