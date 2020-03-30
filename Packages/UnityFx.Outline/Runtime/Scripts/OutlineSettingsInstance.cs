// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline
{
	[Serializable]
	internal class OutlineSettingsInstance : IOutlineSettingsEx
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
		private OutlineRenderFlags _outlineMode;

#pragma warning restore 0649

		private OutlineResources _resources;

		#endregion

		#region interface

		public OutlineResources OutlineResources
		{
			get
			{
				return _resources;
			}
			set
			{
				_resources = value;
			}
		}

		public bool RequiresCameraDepth
		{
			get
			{
				var renderMode = _outlineMode;

				if (!ReferenceEquals(_outlineSettings, null))
				{
					renderMode = _outlineSettings.OutlineRenderMode;
				}

				return (renderMode & OutlineRenderFlags.EnableDepthTesting) != 0;
			}
		}

		internal OutlineSettingsInstance()
		{
		}

		internal OutlineSettingsInstance(OutlineResources resources)
		{
			_resources = resources;
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
				_outlineSettings = value;
			}
		}

		#endregion

		#region IOutlineSettings

		/// <inheritdoc/>
		public Color OutlineColor
		{
			get
			{
				if (!ReferenceEquals(_outlineSettings, null))
				{
					return _outlineSettings.OutlineColor;
				}

				return _outlineColor;
			}
			set
			{
				_outlineColor = value;
			}
		}

		/// <inheritdoc/>
		public int OutlineWidth
		{
			get
			{
				if (!ReferenceEquals(_outlineSettings, null))
				{
					return _outlineSettings.OutlineWidth;
				}

				return _outlineWidth;
			}
			set
			{
				_outlineWidth = Mathf.Clamp(value, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);
			}
		}

		/// <inheritdoc/>
		public float OutlineIntensity
		{
			get
			{
				if (!ReferenceEquals(_outlineSettings, null))
				{
					return _outlineSettings.OutlineIntensity;
				}

				return _outlineIntensity;
			}
			set
			{
				_outlineIntensity = Mathf.Clamp(value, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);
			}
		}

		/// <inheritdoc/>
		public OutlineRenderFlags OutlineRenderMode
		{
			get
			{
				if (!ReferenceEquals(_outlineSettings, null))
				{
					return _outlineSettings.OutlineRenderMode;
				}

				return _outlineMode;
			}
			set
			{
				_outlineMode = value;
			}
		}

		#endregion

		#region IEquatable

		public bool Equals(IOutlineSettings other)
		{
			return OutlineSettings.Equals(this, other);
		}

		#endregion

		#region implementation
		#endregion
	}
}
