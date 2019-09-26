// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A collection of <see cref="GameObject"/> instances that share outline settings. An <see cref="OutlineLayer"/>
	/// can only belong to one <see cref="OutlineLayerCollection"/> at time.
	/// </summary>
	/// <seealso cref="OutlineLayerCollection"/>
	/// <seealso cref="OutlineEffect"/>
	[Serializable]
	public sealed partial class OutlineLayer : ICollection<GameObject>, IOutlineSettingsEx, IChangeTracking
	{
		#region data

		[SerializeField, HideInInspector]
		private OutlineSettingsInstance _settings = new OutlineSettingsInstance();
		[SerializeField, HideInInspector]
		private string _name;
		[SerializeField, HideInInspector]
		private int _zOrder;
		[SerializeField, HideInInspector]
		private bool _enabled = true;

		private OutlineLayerCollection _parentCollection;
		private Dictionary<GameObject, RendererCollection> _outlineObjects = new Dictionary<GameObject, RendererCollection>();
		private bool _changed;

		#endregion

		#region interface

		/// <summary>
		/// Gets the layer name.
		/// </summary>
		public string Name
		{
			get
			{
				if (string.IsNullOrEmpty(_name))
				{
					return "OutlineLayer #" + Index.ToString();
				}

				return _name;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the layer is enabled.
		/// </summary>
		/// <seealso cref="Priority"/>
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					_changed = true;
				}
			}
		}

		/// <summary>
		/// Gets or sets the layer priority. Layers with greater <see cref="Priority"/> values are rendered on top of layers with lower priority.
		/// Layers with equal priorities are rendered according to index in parent collection.
		/// </summary>
		/// <seealso cref="Enabled"/>
		public int Priority
		{
			get
			{
				return _zOrder;
			}
			set
			{
				if (_zOrder != value)
				{
					if (_parentCollection != null)
					{
						_parentCollection.SetOrderChanged();
					}

					_zOrder = value;
					_changed = true;
				}
			}
		}

		/// <summary>
		/// Gets index of the layer in parent collection.
		/// </summary>
		public int Index
		{
			get
			{
				if (_parentCollection != null)
				{
					return _parentCollection.IndexOf(this);
				}

				return -1;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineLayer"/> class.
		/// </summary>
		public OutlineLayer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineLayer"/> class.
		/// </summary>
		public OutlineLayer(string name)
		{
			_name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineLayer"/> class.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is <see langword="null"/>.</exception>
		public OutlineLayer(OutlineSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			_settings.OutlineSettings = settings;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineLayer"/> class.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/> is <see langword="null"/>.</exception>
		public OutlineLayer(string name, OutlineSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			_name = name;
			_settings.OutlineSettings = settings;
		}

		/// <summary>
		/// Adds a new object to the layer.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="go"/> is <see langword="null"/>.</exception>
		public void Add(GameObject go, int ignoreLayerMask)
		{
			if (go == null)
			{
				throw new ArgumentNullException("go");
			}

			if (!_outlineObjects.ContainsKey(go))
			{
				_outlineObjects.Add(go, new RendererCollection(go, ignoreLayerMask));
				_changed = true;
			}
		}

		/// <summary>
		/// Adds a new object to the layer.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="go"/> is <see langword="null"/>.</exception>
		public void Add(GameObject go, string ignoreLayer)
		{
			Add(go, 1 << LayerMask.NameToLayer(ignoreLayer));
		}

		/// <summary>
		/// Attempts to get renderers assosiated with the specified <see cref="GameObject"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="go"/> is <see langword="null"/>.</exception>
		public bool TryGetRenderers(GameObject go, out ICollection<Renderer> renderers)
		{
			if (go == null)
			{
				throw new ArgumentNullException("go");
			}

			RendererCollection result;

			if (_outlineObjects.TryGetValue(go, out result))
			{
				renderers = result;
				return true;
			}

			renderers = null;
			return false;
		}

		#endregion

		#region internals

		internal string NameTag
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		internal OutlineLayerCollection ParentCollection
		{
			get
			{
				return _parentCollection;
			}
		}

		internal void Reset()
		{
			_settings.SetResources(null);
			_outlineObjects.Clear();
		}

		internal void UpdateChanged()
		{
			_settings.UpdateChanged();
		}

		internal void SetCollection(OutlineLayerCollection collection)
		{
			if (_parentCollection == null || collection == null || _parentCollection == collection)
			{
				_parentCollection = collection;
			}
			else
			{
				throw new InvalidOperationException("OutlineLayer can only belong to a single OutlineLayerCollection.");
			}
		}

		internal void Render(OutlineRenderer renderer, OutlineResources resources)
		{
			if (_enabled)
			{
				_settings.SetResources(resources);

				foreach (var kvp in _outlineObjects)
				{
					if (kvp.Key && kvp.Key.activeInHierarchy)
					{
						renderer.RenderSingleObject(kvp.Value, _settings.OutlineMaterials);
					}
				}
			}
		}

		#endregion

		#region IOutlineSettingsEx

		/// <summary>
		/// Gets or sets outline settings. Set this to non-<see langword="null"/> value to share settings with other components.
		/// </summary>
		public OutlineSettings OutlineSettings
		{
			get
			{
				return _settings.OutlineSettings;
			}
			set
			{
				_settings.OutlineSettings = value;
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
			if (go != null && _outlineObjects.Remove(go))
			{
				_changed = true;
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public bool Contains(GameObject go)
		{
			if (ReferenceEquals(go, null))
			{
				return false;
			}

			return _outlineObjects.ContainsKey(go);
		}

		/// <inheritdoc/>
		public void Clear()
		{
			if (_outlineObjects.Count > 0)
			{
				_outlineObjects.Clear();
				_changed = true;
			}
		}

		/// <inheritdoc/>
		public void CopyTo(GameObject[] array, int arrayIndex)
		{
			_outlineObjects.Keys.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable

		/// <inheritdoc/>
		public IEnumerator<GameObject> GetEnumerator()
		{
			return _outlineObjects.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _outlineObjects.Keys.GetEnumerator();
		}

		#endregion

		#region IChangeTracking

		/// <inheritdoc/>
		public bool IsChanged
		{
			get
			{
				return _changed || _settings.IsChanged;
			}
		}

		/// <inheritdoc/>
		public void AcceptChanges()
		{
			_settings.AcceptChanges();
			_changed = false;
		}

		#endregion

		#region Object

		public override string ToString()
		{
			var text = new StringBuilder();

			if (string.IsNullOrEmpty(_name))
			{
				text.Append("OutlineLayer");
			}
			else
			{
				text.Append(_name);
			}

			if (_parentCollection != null)
			{
				text.Append(" #");
				text.Append(_parentCollection.IndexOf(this));
			}

			if (_zOrder > 0)
			{
				text.Append(" z");
				text.Append(_zOrder);
			}

			if (_outlineObjects.Count > 0)
			{
				text.Append(" (");

				foreach (var go in _outlineObjects.Keys)
				{
					text.Append(go.name);
					text.Append(", ");
				}

				text.Remove(text.Length - 2, 2);
				text.Append(")");
			}

			return string.Format("{0}", text);
		}

		#endregion

		#region implementation
		#endregion
	}
}
