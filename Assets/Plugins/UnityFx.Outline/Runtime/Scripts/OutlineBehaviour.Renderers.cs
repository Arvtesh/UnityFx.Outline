// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	partial class OutlineBehaviour
	{
		#region interface
		#endregion

		#region implementation

		[ExecuteInEditMode]
		[DisallowMultipleComponent]
		private class OutlineRendererHelper : MonoBehaviour
		{
			private OutlineBehaviour _parent;

			public void SetParent(OutlineBehaviour parent)
			{
				_parent = parent;
			}

			private void OnWillRenderObject()
			{
				if (isActiveAndEnabled && _parent)
				{
					_parent.OnWillRenderObject();
				}
			}
		}

		private sealed class RendererCollection : IList<Renderer>
		{
			#region data

			private readonly List<Renderer> _renderers = new List<Renderer>();
			private readonly OutlineBehaviour _parent;
			private readonly GameObject _go;

			#endregion

			#region interface

			internal RendererCollection(OutlineBehaviour parent)
			{
				Debug.Assert(parent);

				_parent = parent;
				_go = parent.gameObject;
			}

			public void Reset()
			{
				foreach (var r in _renderers)
				{
					Release(r);
				}

				_renderers.Clear();
				_parent.GetComponentsInChildren(true, _renderers);

				foreach (var r in _renderers)
				{
					Init(r);
				}
			}

			#endregion

			#region IList

			public Renderer this[int index]
			{
				get
				{
					return _renderers[index];
				}
				set
				{
					if (index < 0 || index >= _renderers.Count)
					{
						throw new ArgumentOutOfRangeException("index");
					}

					Validate(value);
					Release(_renderers[index]);
					Init(value);

					_renderers[index] = value;
				}
			}

			public int IndexOf(Renderer renderer)
			{
				return _renderers.IndexOf(renderer);
			}

			public void Insert(int index, Renderer renderer)
			{
				if (index < 0 || index >= _renderers.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}

				Validate(renderer);
				Init(renderer);

				_renderers.Insert(index, renderer);
			}

			public void RemoveAt(int index)
			{
				if (index >= 0 && index < _renderers.Count)
				{
					Release(_renderers[index]);
					_renderers.RemoveAt(index);
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
				Init(renderer);

				_renderers.Add(renderer);
			}

			public bool Remove(Renderer renderer)
			{
				if (_renderers.Remove(renderer))
				{
					Release(renderer);
					return true;
				}

				return false;
			}

			public void Clear()
			{
				foreach (var r in _renderers)
				{
					Release(r);
				}

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

			private void Init(Renderer r)
			{
				if (r && r.gameObject != _go)
				{
					var c = r.GetComponent<OutlineRendererHelper>();

					if (c == null)
					{
						c = r.gameObject.AddComponent<OutlineRendererHelper>();
					}

					c.SetParent(_parent);
				}
			}

			private void Release(Renderer r)
			{
				if (r)
				{
					var c = r.GetComponent<OutlineRendererHelper>();

					if (c)
					{
						DestroyImmediate(c);
					}
				}
			}

			#endregion
		}

		private void CreateRenderersIfNeeded()
		{
			if (_renderers == null)
			{
				_renderers = new RendererCollection(this);
				_renderers.Reset();
			}
		}

		#endregion
	}
}
