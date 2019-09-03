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
		private bool _changed;

		#endregion

		#region interface

		public event EventHandler Changed;

		public OutlineMaterialSet OutlineMaterials
		{
			get
			{
				return _materials;
			}
		}

		internal void Awake()
		{
			ResetSettings(true);
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

				SetChanged();
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
					if (_outlineSettings != null)
					{
						_outlineSettings.Changed -= OnSettingsChanged;
					}

					_outlineSettings = value;

					ResetSettings(true);
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
				if (_outlineColor != value)
				{
					_outlineColor = value;

					if (_materials != null)
					{
						_materials.OutlineColor = value;
					}

					SetChanged();
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

					if (_materials != null)
					{
						_materials.OutlineWidth = value;
					}

					SetChanged();
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
				value = Mathf.Clamp(value, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);

				if (_outlineIntensity != value)
				{
					_outlineIntensity = value;

					if (_materials != null)
					{
						_materials.OutlineIntensity = value;
					}

					SetChanged();
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

					if (_materials != null)
					{
						_materials.OutlineMode = value;
					}

					SetChanged();
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
			if (_outlineSettings != null)
			{
				_outlineSettings.Changed -= OnSettingsChanged;
			}

			if (_materials != null)
			{
				_materials.Dispose();
				_materials = null;
			}
		}

		#endregion

		#region implementation

		private void OnSettingsChanged(object sender, EventArgs e)
		{
			Debug.Assert(_outlineSettings);
			ResetSettings(false);
		}

		private void ResetSettings(bool subscribeToEvents)
		{
			if (_outlineSettings != null)
			{
				if (subscribeToEvents)
				{
					_outlineSettings.Changed += OnSettingsChanged;
				}

				_outlineColor = _outlineSettings.OutlineColor;
				_outlineWidth = _outlineSettings.OutlineWidth;
				_outlineIntensity = _outlineSettings.OutlineIntensity;
				_outlineMode = _outlineSettings.OutlineMode;

				if (_materials != null)
				{
					_materials.Reset(this);
				}

				SetChanged();
			}
		}

		private void SetChanged()
		{
			_changed = true;

			if (Changed != null)
			{
				Changed(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
