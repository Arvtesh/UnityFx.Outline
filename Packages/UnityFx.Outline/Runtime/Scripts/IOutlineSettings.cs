// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// Generic outline settings.
	/// </summary>
	public interface IOutlineSettings : IEquatable<IOutlineSettings>
	{
		/// <summary>
		/// Gets or sets outline color.
		/// </summary>
		/// <seealso cref="OutlineWidth"/>
		/// <seealso cref="OutlineRenderMode"/>
		Color OutlineColor { get; set; }

		/// <summary>
		/// Gets or sets outline width in pixels. Allowed range is [<see cref="OutlineRenderer.MinWidth"/>, <see cref="OutlineRenderer.MaxWidth"/>].
		/// </summary>
		/// <seealso cref="OutlineColor"/>
		/// <seealso cref="OutlineRenderMode"/>
		int OutlineWidth { get; set; }

		/// <summary>
		/// Gets or sets outline intensity value. Allowed range is [<see cref="OutlineRenderer.MinIntensity"/>, <see cref="OutlineRenderer.MaxIntensity"/>].
		/// This is used for blurred oulines only (i.e. <see cref="OutlineRenderMode"/> is set to <see cref="OutlineRenderFlags.Blurred"/>).
		/// </summary>
		/// <seealso cref="OutlineRenderMode"/>
		/// <seealso cref="OutlineColor"/>
		/// <seealso cref="OutlineWidth"/>
		float OutlineIntensity { get; set; }

		/// <summary>
		/// Gets or sets outline render mode.
		/// </summary>
		/// <seealso cref="OutlineWidth"/>
		/// <seealso cref="OutlineColor"/>
		/// <seealso cref="OutlineIntensity"/>
		OutlineRenderFlags OutlineRenderMode { get; set; }
	}
}
