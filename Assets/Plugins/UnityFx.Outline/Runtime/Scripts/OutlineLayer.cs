// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A collection of <see cref="GameObject"/> instances that share outlining settings.
	/// </summary>
	/// <seealso cref="OutlineEffect"/>
	[Serializable]
	public sealed class OutlineLayer : ICollection<GameObject>, IOutlineSettings, ISerializationCallbackReceiver
	{
		#region data

		private Dictionary<GameObject, Renderer[]> _outlineObjects = new Dictionary<GameObject, Renderer[]>();
		private OutlineMaterialSet _materials;

		[SerializeField]
		private Color _outlineColor = Color.red;
		[SerializeField]
		[Range(OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth)]
		private int _outlineWidth = 4;
		[SerializeField]
		private OutlineMode _outlineMode;

		private bool _changed;

		#endregion

		#region interface

		/// <summary>
		/// Gets a value indicating whether the layer contains unapplied changes.
		/// </summary>
		public bool IsChanged
		{
			get
			{
				return _changed;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineLayer"/> class.
		/// </summary>
		public OutlineLayer()
		{
		}

		/// <summary>
		/// Adds a new object to the layer.
		/// </summary>
		public void Add(GameObject go, int ignoreLayerMask)
		{
			if (go == null)
			{
				throw new ArgumentNullException("go");
			}

			if (!_outlineObjects.ContainsKey(go))
			{
				var renderers = go.GetComponentsInChildren<Renderer>();

				if (renderers != null)
				{
					if (renderers.Length > 0 && ignoreLayerMask != 0)
					{
						var filteredRenderers = new List<Renderer>(renderers.Length);

						for (var i = 0; i < renderers.Length; ++i)
						{
							if ((renderers[i].gameObject.layer & ignoreLayerMask) == 0)
							{
								filteredRenderers.Add(renderers[i]);
							}
						}

						renderers = filteredRenderers.ToArray();
					}
				}
				else
				{
					renderers = new Renderer[0];
				}

				_outlineObjects.Add(go, renderers);
				_changed = true;
			}
		}

		internal void Render(OutlineRenderer renderer, OutlineResources resources)
		{
			if (_materials == null || _materials.OutlineResources != resources)
			{
				_materials = resources.CreateMaterialSet();
				_materials.Reset(this);
			}

			foreach (var kvp in _outlineObjects)
			{
				if (kvp.Key)
				{
					renderer.RenderSingleObject(kvp.Value, _materials);
				}
			}

			_changed = false;
		}

		#endregion

		#region IOutlineSettings

		/// <inheritdoc/>
		public Color OutlineColor
		{
			get
			{
				return _outlineColor;
			}
			set
			{
				if (_outlineColor != value)
				{
					_outlineColor = value;
					_changed = true;

					if (_materials != null)
					{
						_materials.OutlineColor = value;
					}
				}
			}
		}

		/// <inheritdoc/>
		public int OutlineWidth
		{
			get
			{
				return _outlineWidth;
			}
			set
			{
				value = Mathf.Clamp(value, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);

				if (_outlineWidth != value)
				{
					_outlineWidth = value;
					_changed = true;

					if (_materials != null)
					{
						_materials.OutlineWidth = value;
					}
				}
			}
		}

		/// <inheritdoc/>
		public OutlineMode OutlineMode
		{
			get
			{
				return _outlineMode;
			}
			set
			{
				if (_outlineMode != value)
				{
					_outlineMode = value;
					_changed = true;

					if (_materials != null)
					{
						_materials.OutlineMode = value;
					}
				}
			}
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			if (_materials != null)
			{
				_materials.Reset(this);
			}

			_changed = true;
		}

		#endregion

		#region ISerializationCallbackReceiver

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			_outlineWidth = Mathf.Clamp(_outlineWidth, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);

			if (_outlineColor == Color.clear)
			{
				_outlineColor = Color.red;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			_changed = true;
		}

		#endregion

		#region ICollection

		/// <inheritdoc/>
		public int Count
		{
			get
			{
				return _outlineObjects.Count;
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
		public void Add(GameObject go)
		{
			Add(go, 0);
		}

		/// <inheritdoc/>
		public bool Remove(GameObject go)
		{
			if (_outlineObjects.Remove(go))
			{
				_changed = true;
			}

			return false;
		}

		/// <inheritdoc/>
		public bool Contains(GameObject go)
		{
			return _outlineObjects.ContainsKey(go);
		}

		/// <inheritdoc/>
		public void Clear()
		{
			_outlineObjects.Clear();
			_changed = true;
		}

		/// <inheritdoc/>
		public void CopyTo(GameObject[] array, int arrayIndex)
		{
			_outlineObjects.Keys.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable

		public IEnumerator<GameObject> GetEnumerator()
		{
			return _outlineObjects.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _outlineObjects.Keys.GetEnumerator();
		}

		#endregion

		#region implementation
		#endregion
	}
}
