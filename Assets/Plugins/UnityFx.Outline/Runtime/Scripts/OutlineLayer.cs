// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// A single outline layer.
	/// </summary>
	/// <seealso cref="OutlineEffect"/>
	public sealed class OutlineLayer : ICollection<GameObject>
	{
		#region data

		private readonly Material _renderMaterial;
		private readonly Material _postProcessMaterial;
		private readonly Dictionary<GameObject, Renderer[]> _outlineObjects = new Dictionary<GameObject, Renderer[]>();

		private Color _outlineColor = Color.green;
		private int _outlineWidth = 5;
		private bool _changed = true;

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
					_postProcessMaterial.SetColor(OutlineHelpers.ColorParamName, value);
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
				value = Mathf.Clamp(value, 1, 32);

				if (_outlineWidth != value)
				{
					_outlineWidth = value;
					_postProcessMaterial.SetInt(OutlineHelpers.WidthParamName, value);
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
		/// Gets the material used for outline rendering.
		/// </summary>
		internal Material PostProcessMaterial
		{
			get
			{
				return _postProcessMaterial;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineLayer"/> class.
		/// </summary>
		internal OutlineLayer(Material renderMaterial, Material postProcessMaterial)
		{
			Debug.Assert(renderMaterial);
			Debug.Assert(postProcessMaterial);

			_renderMaterial = renderMaterial;
			_postProcessMaterial = postProcessMaterial;
			_postProcessMaterial.SetColor(OutlineHelpers.ColorParamName, _outlineColor);
			_postProcessMaterial.SetInt(OutlineHelpers.WidthParamName, _outlineWidth);
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
		/// Renders the layer into the <paramref name="commandBuffer"/> passed.
		/// </summary>
		internal void FillCommandBuffer(CommandBuffer commandBuffer, RenderTargetIdentifier dst)
		{
			foreach (var kvp in _outlineObjects)
			{
				if (kvp.Key)
				{
					OutlineHelpers.RenderSingleObject(kvp.Value, _renderMaterial, _postProcessMaterial, commandBuffer, dst);
				}
			}

			_changed = false;
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
