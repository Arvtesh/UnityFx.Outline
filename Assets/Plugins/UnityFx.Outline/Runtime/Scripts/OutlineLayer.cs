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
	public sealed class OutlineLayer : ICollection<GameObject>, ISerializationCallbackReceiver
	{
		#region data

		private readonly int _colorNameId = Shader.PropertyToID(OutlineRenderer.ColorParamName);
		private readonly int _widthNameId = Shader.PropertyToID(OutlineRenderer.WidthParamName);

		private Dictionary<GameObject, Renderer[]> _outlineObjects = new Dictionary<GameObject, Renderer[]>();
		private Dictionary<OutlineResources, Material> _renderMaterials;
		private Dictionary<OutlineResources, Material> _postProcessMaterials;

		[SerializeField]
		private Color _outlineColor = Color.red;
		[SerializeField]
		[Range(OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth)]
		private int _outlineWidth = 4;

		private bool _changed;

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets outline color for the layer.
		/// </summary>
		/// <seealso cref="OutlineWidth"/>
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
				}
			}
		}

		/// <summary>
		/// Gets or sets outline width in pixels. Only positive values are allowed.
		/// </summary>
		/// <seealso cref="OutlineColor"/>
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
				}
			}
		}

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

		/// <summary>
		/// Renders the layer with the <paramref name="renderer"/> passed.
		/// </summary>
		internal void Render(OutlineRenderer renderer, OutlineResourceCache resources)
		{
			var renderMaterial = resources.GetRenderMaterial(this);
			var hPassMaterial = resources.GetHPassMaterial(this);
			var vPassMaterial = resources.GetVPassMaterial(this);

			hPassMaterial.SetInt(_widthNameId, _outlineWidth);
			vPassMaterial.SetInt(_widthNameId, _outlineWidth);
			vPassMaterial.SetColor(_colorNameId, _outlineColor);

			foreach (var kvp in _outlineObjects)
			{
				if (kvp.Key)
				{
					renderer.RenderSingleObject(kvp.Value, renderMaterial, hPassMaterial, vPassMaterial);
				}
			}

			_changed = false;
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
