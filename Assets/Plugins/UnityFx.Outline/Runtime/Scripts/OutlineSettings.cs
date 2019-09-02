// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// Outline settings.
	/// </summary>
	[CreateAssetMenu(fileName = "OutlineSettings", menuName = "UnityFx/Outline/Outline Settings")]
	public sealed class OutlineSettings : ScriptableObject, IOutlineSettings
	{
		#region data

		// NOTE: There is a custom editor for OutlineSettings, so no need to show these in default inspector.
		[SerializeField, HideInInspector]
		private Color _outlineColor = Color.red;
		[SerializeField, HideInInspector]
		private int _outlineWidth = 4;
		[SerializeField, HideInInspector]
		private float _outlineIntensity = 2;
		[SerializeField, HideInInspector]
		private OutlineMode _outlineMode;

		#endregion

		#region interface

		/// <summary>
		/// Raised when the settings are changed.
		/// </summary>
		public event EventHandler Changed;

		#endregion

		#region ScriptableObject
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

					if (Changed != null)
					{
						Changed(this, EventArgs.Empty);
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

					if (Changed != null)
					{
						Changed(this, EventArgs.Empty);
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
				value = Mathf.Clamp(value, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);

				if (_outlineIntensity != value)
				{
					_outlineIntensity = value;

					if (Changed != null)
					{
						Changed(this, EventArgs.Empty);
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

					if (Changed != null)
					{
						Changed(this, EventArgs.Empty);
					}
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
