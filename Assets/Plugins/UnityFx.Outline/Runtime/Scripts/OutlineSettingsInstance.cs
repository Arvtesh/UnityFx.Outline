// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine;

namespace UnityFx.Outline
{
	[Serializable]
	internal class OutlineSettingsInstance : IOutlineSettingsEx, IChangeTracking, IDisposable
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

#pragma warning restore 0649

		private OutlineMaterialSet _materials;
		private bool _changed = true;

		#endregion

		#region interface

		public OutlineMaterialSet OutlineMaterials
		{
			get
			{
				return _materials;
			}
		}

		internal void SetResources(OutlineResources resources)
		{
			if (resources == null)
			{
				if (_materials != null)
				{
					_materials.Dispose();
					_materials = null;
				}
			}
			else if (_materials == null || _materials.OutlineResources != resources)
			{
				_materials = resources.CreateMaterialSet();
				_materials.Reset(this);
				_changed = true;
			}
		}

		internal void UpdateChanged()
		{
			if (_outlineSettings != null)
			{
				_outlineColor = _outlineSettings.OutlineColor;
				_outlineWidth = _outlineSettings.OutlineWidth;
				_outlineIntensity = _outlineSettings.OutlineIntensity;
				_outlineMode = _outlineSettings.OutlineMode;
			}

			if (_materials != null)
			{
				if (_outlineColor != _materials.OutlineColor ||
					_outlineWidth != _materials.OutlineWidth ||
					_outlineIntensity != _materials.OutlineIntensity ||
					_outlineMode != _materials.OutlineMode)
				{
					_materials.Reset(this);
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

						if (_materials != null)
						{
							_materials.Reset(this);
						}

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
				ThrowIfSettingsAssigned();

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

					if (_materials != null)
					{
						_materials.OutlineIntensity = value;
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
				ThrowIfSettingsAssigned();

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

		#region IDisposable

		public void Dispose()
		{
			if (_materials != null)
			{
				_materials.Dispose();
				_materials = null;
			}
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
