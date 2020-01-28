// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
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

		public static bool Equals(IOutlineSettings lhs, IOutlineSettings rhs)
		{
			if (lhs == null || rhs == null)
			{
				return false;
			}

			return lhs.OutlineColor == rhs.OutlineColor &&
				lhs.OutlineWidth == rhs.OutlineWidth &&
				lhs.OutlineMode == rhs.OutlineMode &&
				Mathf.Approximately(lhs.OutlineIntensity, rhs.OutlineIntensity);
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
				_outlineColor = value;
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
				_outlineWidth = Mathf.Clamp(value, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);
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
				_outlineIntensity = Mathf.Clamp(value, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);
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
				_outlineMode = value;
			}
		}

		#endregion

		#region IEquatable

		/// <inheritdoc/>
		public bool Equals(IOutlineSettings other)
		{
			return Equals(this, other);
		}

		#endregion

		#region Object

		/// <inheritdoc/>
		public override bool Equals(object other)
		{
			return Equals(this, other as IOutlineSettings);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
