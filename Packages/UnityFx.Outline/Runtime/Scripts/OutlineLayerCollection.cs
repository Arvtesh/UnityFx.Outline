// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A serializable collection of outline layers.
	/// </summary>
	/// <seealso cref="OutlineLayer"/>
	/// <seealso cref="OutlineEffect"/>
	/// <seealso cref="OutlineSettings"/>
	[CreateAssetMenu(fileName = "OutlineLayerCollection", menuName = "UnityFx/Outline/Outline Layer Collection")]
	public sealed class OutlineLayerCollection : ScriptableObject, IList<OutlineLayer>, IChangeTracking
	{
		#region data

		private class OutlineLayerComparer : IComparer<OutlineLayer>
		{
			public int Compare(OutlineLayer x, OutlineLayer y)
			{
				return x.Priority - y.Priority;
			}
		}

		[SerializeField, HideInInspector]
		private List<OutlineLayer> _layers = new List<OutlineLayer>();

		private List<OutlineLayer> _sortedLayers = new List<OutlineLayer>();
		private OutlineLayerComparer _sortComparer = new OutlineLayerComparer();
		private bool _orderChanged = true;
		private bool _changed = true;

		#endregion

		#region interface

		/// <summary>
		/// Gets layers ordered by <see cref="OutlineLayer.Priority"/>.
		/// </summary>
		public OutlineLayer[] SortedLayers
		{
			get
			{
				UpdateSortedLayersIfNeeded();
				return _sortedLayers.ToArray();
			}
		}

		/// <summary>
		/// Renders all layers.
		/// </summary>
		public void Render(OutlineRenderer renderer, OutlineResources resources)
		{
			UpdateSortedLayersIfNeeded();

			foreach (var layer in _sortedLayers)
			{
				layer.Render(renderer, resources);
			}
		}

		#endregion

		#region internals

		internal void SetOrderChanged()
		{
			_orderChanged = true;
			_changed = true;
		}

		internal void Reset()
		{
			foreach (var layer in _layers)
			{
				layer.Reset();
			}
		}

		internal void UpdateChanged()
		{
			foreach (var layer in _layers)
			{
				layer.UpdateChanged();
			}
		}

		#endregion

		#region ScriptableObject

		private void OnEnable()
		{
			foreach (var layer in _layers)
			{
				layer.SetCollection(this);
			}

			_orderChanged = true;
		}

		#endregion

		#region IList

		/// <inheritdoc/>
		public OutlineLayer this[int layerIndex]
		{
			get
			{
				return _layers[layerIndex];
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("layer");
				}

				if (layerIndex < 0 || layerIndex >= _layers.Count)
				{
					throw new ArgumentOutOfRangeException("layerIndex");
				}

				if (_layers[layerIndex] != value)
				{
					value.SetCollection(this);

					_layers[layerIndex].SetCollection(null);
					_layers[layerIndex] = value;

					_orderChanged = true;
					_changed = true;
				}
			}
		}

		/// <inheritdoc/>
		public int IndexOf(OutlineLayer layer)
		{
			if (layer != null)
			{
				return _layers.IndexOf(layer);
			}

			return -1;
		}

		/// <inheritdoc/>
		public void Insert(int index, OutlineLayer layer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}

			if (layer.ParentCollection != this)
			{
				layer.SetCollection(this);

				_layers.Insert(index, layer);

				_orderChanged = true;
				_changed = true;
			}
		}

		/// <inheritdoc/>
		public void RemoveAt(int index)
		{
			if (index >= 0 && index < _layers.Count)
			{
				_layers[index].SetCollection(null);
				_layers.RemoveAt(index);

				_orderChanged = true;
				_changed = true;
			}
		}

		#endregion

		#region ICollection

		/// <inheritdoc/>
		public int Count
		{
			get
			{
				return _layers.Count;
			}
		}

		/// <inheritdoc/>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <inheritdoc/>
		public void Add(OutlineLayer layer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}

			if (layer.ParentCollection != this)
			{
				layer.SetCollection(this);

				_layers.Add(layer);

				_orderChanged = true;
				_changed = true;
			}
		}

		/// <inheritdoc/>
		public bool Remove(OutlineLayer layer)
		{
			if (_layers.Remove(layer))
			{
				layer.SetCollection(null);

				_sortedLayers.Remove(layer);
				_changed = true;

				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void Clear()
		{
			if (_layers.Count > 0)
			{
				foreach (var layer in _layers)
				{
					layer.SetCollection(null);
				}

				_layers.Clear();
				_sortedLayers.Clear();
				_changed = true;
			}
		}

		/// <inheritdoc/>
		public bool Contains(OutlineLayer layer)
		{
			if (layer == null)
			{
				return false;
			}

			return _layers.Contains(layer);
		}

		/// <inheritdoc/>
		public void CopyTo(OutlineLayer[] array, int arrayIndex)
		{
			_layers.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable

		/// <inheritdoc/>
		public IEnumerator<OutlineLayer> GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		#endregion

		#region IChangeTracking

		/// <inheritdoc/>
		public bool IsChanged
		{
			get
			{
				if (_changed)
				{
					return true;
				}

				foreach (var layer in _layers)
				{
					if (layer.IsChanged)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <inheritdoc/>
		public void AcceptChanges()
		{
			foreach (var layer in _layers)
			{
				layer.AcceptChanges();
			}

			_changed = false;
		}

		#endregion

		#region implementation

		private void UpdateSortedLayersIfNeeded()
		{
			if (_orderChanged)
			{
				_sortedLayers.Clear();
				_sortedLayers.AddRange(_layers);
				_sortedLayers.Sort(_sortComparer);
				_orderChanged = false;
			}
		}

		#endregion
	}
}
