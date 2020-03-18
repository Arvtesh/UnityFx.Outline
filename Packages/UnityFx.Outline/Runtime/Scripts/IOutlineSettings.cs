// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// Defines outline settings.
	/// </summary>
	public interface IOutlineSettings : IEquatable<IOutlineSettings>
	{
		/// <summary>
		/// Gets or sets outline color.
		/// </summary>
		/// <seealso cref="OutlineWidth"/>
		/// <seealso cref="OutlineMode"/>
		Color OutlineColor { get; set; }

		/// <summary>
		/// Gets or sets outline width in pixels. Allowed range is [<see cref="OutlineRenderer.MinWidth"/>, <see cref="OutlineRenderer.MaxWidth"/>].
		/// </summary>
		/// <seealso cref="OutlineColor"/>
		/// <seealso cref="OutlineMode"/>
		int OutlineWidth { get; set; }

		/// <summary>
		/// Gets or sets outline intensity value. Allowed range is [<see cref="OutlineRenderer.MinIntensity"/>, <see cref="OutlineRenderer.MaxIntensity"/>].
		/// </summary>
		float OutlineIntensity { get; set; }

		/// <summary>
		/// Gets or sets outline mode.
		/// </summary>
		/// <seealso cref="OutlineWidth"/>
		/// <seealso cref="OutlineColor"/>
		OutlineMode OutlineMode { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether depth-testing is enabled.
		/// </summary>
		bool DepthTestEnabled { get; set; }
	}
}
