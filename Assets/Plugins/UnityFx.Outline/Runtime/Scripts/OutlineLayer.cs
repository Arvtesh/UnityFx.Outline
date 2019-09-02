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
	/// <seealso cref="OutlineSettings"/>
	public sealed class OutlineLayer : ICollection<GameObject>, IOutlineSettings
	{
		#region data

		private readonly OutlineSettings _settings;

		private Dictionary<GameObject, Renderer[]> _outlineObjects = new Dictionary<GameObject, Renderer[]>();
		private OutlineMaterialSet _materials;

		#endregion

		#region interface

		/// <summary>
		/// Raised when the layer is changed.
		/// </summary>
		public event EventHandler Changed;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineLayer"/> class.
		/// </summary>
		public OutlineLayer()
		{
			_settings = ScriptableObject.CreateInstance<OutlineSettings>();
			_settings.Changed += OnSettingsChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineLayer"/> class.
		/// </summary>
		public OutlineLayer(OutlineSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			_settings = settings;
			_settings.Changed += OnSettingsChanged;
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

				if (Changed != null)
				{
					Changed(this, EventArgs.Empty);
				}
			}
		}

		internal void Render(OutlineRenderer renderer, OutlineResources resources)
		{
			if (_materials == null || _materials.OutlineResources != resources)
			{
				_materials = resources.CreateMaterialSet();
			}

			_materials.Reset(_settings);

			foreach (var kvp in _outlineObjects)
			{
				if (kvp.Key)
				{
					renderer.RenderSingleObject(kvp.Value, _materials);
				}
			}
		}

		#endregion

		#region IOutlineSettings

		/// <inheritdoc/>
		public Color OutlineColor
		{
			get
			{
				return _settings.OutlineColor;
			}
			set
			{
				_settings.OutlineColor = value;
			}
		}

		/// <inheritdoc/>
		public int OutlineWidth
		{
			get
			{
				return _settings.OutlineWidth;
			}
			set
			{
				_settings.OutlineWidth = value;
			}
		}

		/// <inheritdoc/>
		public float OutlineIntensity
		{
			get
			{
				return _settings.OutlineIntensity;
			}
			set
			{
				_settings.OutlineIntensity = value;
			}
		}

		/// <inheritdoc/>
		public OutlineMode OutlineMode
		{
			get
			{
				return _settings.OutlineMode;
			}
			set
			{
				_settings.OutlineMode = value;
			}
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
				if (Changed != null)
				{
					Changed(this, EventArgs.Empty);
				}

				return true;
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

			if (Changed != null)
			{
				Changed(this, EventArgs.Empty);
			}
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

		private void OnSettingsChanged(object sender, EventArgs e)
		{
			if (Changed != null)
			{
				Changed(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
