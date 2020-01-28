// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnityFx.Outline
{
	partial class OutlineLayer
	{
		#region interface
		#endregion

		#region implementation

		private sealed class RendererCollection : ICollection<Renderer>
		{
			#region data

			private readonly List<Renderer> _renderers = new List<Renderer>();
			private readonly GameObject _go;

			#endregion

			#region interface

			internal RendererCollection(GameObject parent)
			{
				Debug.Assert(parent);
				_go = parent;
			}

			internal RendererCollection(GameObject parent, int ignoreMask)
			{
				Debug.Assert(parent);

				_go = parent;
				Reset(ignoreMask);
			}

			public void Reset(int ignoreLayerMask)
			{
				_renderers.Clear();

				var renderers = _go.GetComponentsInChildren<Renderer>();

				if (renderers != null)
				{
					if (ignoreLayerMask != 0)
					{
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
						foreach (var renderer in renderers)
						{
							_renderers.Add(renderer);
						}
					}
				}
			}

			#endregion

			#region ICollection

			public int Count
			{
				get
				{
					return _renderers.Count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}

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
				if (renderer == null)
				{
					throw new ArgumentNullException("renderer");
				}

				if (!renderer.transform.IsChildOf(_go.transform))
				{
					throw new ArgumentException(string.Format("Only children of the {0} are allowed.", _go.name), "renderer");
				}
			}

			#endregion
		}

		#endregion
	}
}
