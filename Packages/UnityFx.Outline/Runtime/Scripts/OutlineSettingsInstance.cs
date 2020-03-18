// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine;

namespace UnityFx.Outline
{
	[Serializable]
	internal class OutlineSettingsInstance : IOutlineSettingsEx, IChangeTracking
	{
		#region data

#pragma warning disable 0649

		// NOTE: There are custom editors for public components, so no need to show these in default inspector.
		[SerializeField, HideInInspector]
		private OutlineSettings _outlineSettings;
		[SerializeField, HideInInspector]
		private Color _outlineColor = Color.red;
		[SerializeField, HideInInspector]
		private int _outlineWidth = 4;
		[SerializeField, HideInInspector]
		private float _outlineIntensity = 2;
		[SerializeField, HideInInspector]
		private OutlineMode _outlineMode;
		[SerializeField, HideInInspector]
		private bool _depthTestEnabled;

#pragma warning restore 0649

		private OutlineResources _resources;
		private bool _changed = true;

		#endregion

		#region interface

		public OutlineResources OutlineResources
		{
			get
			{
				return _resources;
			}
		}

		internal OutlineSettingsInstance()
		{
		}

		internal OutlineSettingsInstance(OutlineResources resources)
		{
			_resources = resources;
		}

		internal void SetResources(OutlineResources resources)
		{
			if (resources != _resources)
			{
				_resources = resources;
				_changed = true;
			}
		}

		internal void UpdateChanged()
		{
			if (_outlineSettings != null)
			{
				if (_outlineColor != _outlineSettings.OutlineColor ||
					_outlineWidth != _outlineSettings.OutlineWidth ||
					_outlineIntensity != _outlineSettings.OutlineIntensity ||
					_outlineMode != _outlineSettings.OutlineMode ||
					_depthTestEnabled != _outlineSettings.DepthTestEnabled)
				{
					_outlineColor = _outlineSettings.OutlineColor;
					_outlineWidth = _outlineSettings.OutlineWidth;
					_outlineIntensity = _outlineSettings.OutlineIntensity;
					_outlineMode = _outlineSettings.OutlineMode;
					_depthTestEnabled = _outlineSettings.DepthTestEnabled;
					_changed = true;
				}
			}
		}

		#endregion

		#region IOutlineSettingsEx

		public OutlineSettings OutlineSettings
		{
			get
			{
				return _outlineSettings;
			}
			set
			{
				if (_outlineSettings != value)
				{
					_outlineSettings = value;

					if (_outlineSettings != null)
					{
						_outlineColor = _outlineSettings.OutlineColor;
						_outlineWidth = _outlineSettings.OutlineWidth;
						_outlineIntensity = _outlineSettings.OutlineIntensity;
						_outlineMode = _outlineSettings.OutlineMode;
						_depthTestEnabled = _outlineSettings.DepthTestEnabled;
						_changed = true;
					}
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
				return _outlineColor;
			}
			set
			{
				ThrowIfSettingsAssigned();

				if (_outlineColor != value)
				{
					_outlineColor = value;
					_changed = true;
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
				ThrowIfSettingsAssigned();

				value = Mathf.Clamp(value, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);

				if (_outlineWidth != value)
				{
					_outlineWidth = value;
					_changed = true;
				}
			}
		}

		/// <inheritdoc/>
		public float OutlineIntensity
		{
			get
			{
				return _outlineIntensity;
			}
			set
			{
				ThrowIfSettingsAssigned();

				value = Mathf.Clamp(value, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);

				if (_outlineIntensity != value)
				{
					_outlineIntensity = value;
					_changed = true;
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
				ThrowIfSettingsAssigned();

				if (_outlineMode != value)
				{
					_outlineMode = value;
					_changed = true;
				}
			}
		}

		/// <inheritdoc/>
		public bool DepthTestEnabled
		{
			get
			{
				return _depthTestEnabled;
			}
			set
			{
				ThrowIfSettingsAssigned();

				if (_depthTestEnabled != value)
				{
					_depthTestEnabled = value;
					_changed = true;
				}
			}
		}

		#endregion

		#region IChangeTracking

		/// <inheritdoc/>
		public bool IsChanged
		{
			get
			{
				return _changed;
			}
		}

		/// <inheritdoc/>
		public void AcceptChanges()
		{
			_changed = false;
		}

		#endregion

		#region IEquatable

		public bool Equals(IOutlineSettings other)
		{
			return OutlineSettings.Equals(this, other);
		}

		#endregion

		#region implementation

		private void ThrowIfSettingsAssigned()
		{
			if (_outlineSettings)
			{
				throw new InvalidOperationException("The outline parameters cannot be altered when OutlineSettings is set.");
			}
		}

		#endregion
	}
}
