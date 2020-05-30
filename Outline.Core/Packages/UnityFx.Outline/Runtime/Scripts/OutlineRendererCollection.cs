// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	internal class OutlineRendererCollection : ICollection<Renderer>
	{
		#region data

		private readonly List<Renderer> _renderers = new List<Renderer>();
		private readonly GameObject _go;

		#endregion

		#region interface

		internal OutlineRendererCollection(GameObject go)
		{
			Debug.Assert(go);
			_go = go;
		}

		internal IReadOnlyList<Renderer> GetList()
		{
			return _renderers;
		}

		internal void Reset(bool includeInactive)
		{
			_go.GetComponentsInChildren(includeInactive, _renderers);
		}

		internal void Reset(bool includeInactive, int ignoreLayerMask)
		{
			_renderers.Clear();

			if (ignoreLayerMask != 0)
			{
				var renderers = _go.GetComponentsInChildren<Renderer>(includeInactive);

				foreach (var renderer in renderers)
				{
					if (((1 << renderer.gameObject.layer) & ignoreLayerMask) == 0)
					{
						_renderers.Add(renderer);
					}
				}
			}
			else
			{
				_go.GetComponentsInChildren(includeInactive, _renderers);
			}
		}

		#endregion

		#region ICollection

		public int Count => _renderers.Count;

		public bool IsReadOnly => false;

		public void Add(Renderer renderer)
		{
			Validate(renderer);

			_renderers.Add(renderer);
		}

		public bool Remove(Renderer renderer)
		{
			return _renderers.Remove(renderer);
		}

		public void Clear()
		{
			_renderers.Clear();
		}

		public bool Contains(Renderer renderer)
		{
			return _renderers.Contains(renderer);
		}

		public void CopyTo(Renderer[] array, int arrayIndex)
		{
			_renderers.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable

		public IEnumerator<Renderer> GetEnumerator()
		{
			return _renderers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _renderers.GetEnumerator();
		}

		#endregion

		#region implementation

		private void Validate(Renderer renderer)
		{
			if (renderer is null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

			if (!renderer.transform.IsChildOf(_go.transform))
			{
				throw new ArgumentException(string.Format("Only children of the {0} are allowed.", _go.name), nameof(renderer));
			}
		}

		#endregion
	}
}
