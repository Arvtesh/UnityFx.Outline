// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A serializable collection of outline layers.
	/// </summary>
	/// <seealso cref="OutlineLayer"/>
	/// <seealso cref="OutlineEffect"/>
	[CreateAssetMenu(fileName = "OutlineLayerCollection", menuName = "UnityFx/Outline Layer Collection")]
	public class OutlineLayerCollection : ScriptableObject, IList<OutlineLayer>
	{
		#region data

		[SerializeField]
		private List<OutlineLayer> _layers;

		#endregion

		#region interface

		internal IList<OutlineLayer> Layers
		{
			get
			{
				if (_layers == null)
				{
					_layers = new List<OutlineLayer>();
				}

				return _layers;
			}
		}

		#endregion

		#region IList

		public OutlineLayer this[int index]
		{
			get
			{
				return _layers[index];
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("layer");
				}

				_layers[index] = value;
			}
		}

		public int IndexOf(OutlineLayer layer)
		{
			if (layer != null)
			{
				return _layers.IndexOf(layer);
			}

			return -1;
		}

		public void Insert(int index, OutlineLayer layer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}

			_layers.Insert(index, layer);
		}

		public void RemoveAt(int index)
		{
			_layers.RemoveAt(index);
		}

		#endregion

		#region ICollection

		public int Count
		{
			get
			{
				return _layers.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public void Add(OutlineLayer layer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}

			_layers.Add(layer);
		}

		public bool Remove(OutlineLayer layer)
		{
			return _layers.Remove(layer);
		}

		public void Clear()
		{
			_layers.Clear();
		}

		public bool Contains(OutlineLayer layer)
		{
			if (layer == null)
			{
				return false;
			}

			return _layers.Contains(layer);
		}

		public void CopyTo(OutlineLayer[] array, int arrayIndex)
		{
			_layers.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable

		public IEnumerator<OutlineLayer> GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		#endregion
	}
}
