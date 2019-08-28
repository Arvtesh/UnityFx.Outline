// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Defines outline settings.
	/// </summary>
	public interface IOutlineSettings
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
		/// Gets or sets outline mode.
		/// </summary>
		/// <seealso cref="OutlineWidth"/>
		/// <seealso cref="OutlineColor"/>
		OutlineMode OutlineMode { get; set; }
	}
}
