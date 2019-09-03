// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
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
	public sealed class OutlineLayerCollection : ScriptableObject, IList<OutlineLayer>
	{
		#region data

		[SerializeField, HideInInspector]
		private List<OutlineLayer> _layers = new List<OutlineLayer>();
		private EventHandler _changedDelegate;

		#endregion

		#region interface

		internal event EventHandler Changed;

		internal void Init()
		{
			_changedDelegate = OnChanged;

			foreach (var layer in _layers)
			{
				layer.Awake();
				layer.Changed += _changedDelegate;
			}
		}

		internal void Reset()
		{
			foreach (var layer in _layers)
			{
				layer.Reset();
			}
		}

		#endregion

		#region ScriptableObject

#if UNITY_EDITOR

		private void OnValidate()
		{
			if (_changedDelegate == null)
			{
				_changedDelegate = OnChanged;
			}
		}

#endif

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
					_layers[layerIndex].Changed -= _changedDelegate;
					_layers[layerIndex] = value;
					_layers[layerIndex].Changed += _changedDelegate;

					RaiseChanged();
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

			_layers.Insert(index, layer);
			layer.Changed += _changedDelegate;

			RaiseChanged();
		}

		/// <inheritdoc/>
		public void RemoveAt(int index)
		{
			if (index >= 0 && index < _layers.Count)
			{
				_layers[index].Changed -= _changedDelegate;
				_layers.RemoveAt(index);

				RaiseChanged();
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

			_layers.Add(layer);

			layer.Changed += _changedDelegate;
			RaiseChanged();
		}

		/// <inheritdoc/>
		public bool Remove(OutlineLayer layer)
		{
			if (layer != null)
			{
				layer.Changed -= _changedDelegate;
			}

			if (_layers.Remove(layer))
			{
				RaiseChanged();
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public void Clear()
		{
			foreach (var layer in _layers)
			{
				layer.Changed -= _changedDelegate;
			}

			_layers.Clear();
			RaiseChanged();
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

		public List<OutlineLayer>.Enumerator GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		IEnumerator<OutlineLayer> IEnumerable<OutlineLayer>.GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		#endregion

		#region implementation

		private void RaiseChanged()
		{
			if (Changed != null)
			{
				Changed(this, EventArgs.Empty);
			}
		}

		private void OnChanged(object sender, EventArgs args)
		{
			RaiseChanged();
		}

		#endregion
	}
}
