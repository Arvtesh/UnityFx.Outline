// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// Extended outline settings.
	/// </summary>
	public interface IOutlineSettingsEx : IOutlineSettings
	{
		/// <summary>
		/// Gets or sets serializable outline settings. Set this to non-<see langword="null"/> value to share settings with other components.
		/// </summary>
		OutlineSettings OutlineSettings { get; set; }
	}
}
