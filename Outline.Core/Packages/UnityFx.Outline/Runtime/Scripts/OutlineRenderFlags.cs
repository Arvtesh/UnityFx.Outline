// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Outline
{
	/// <summary>
	/// Enumerates outline render modes.
	/// </summary>
	[Flags]
	public enum OutlineRenderFlags
	{
		/// <summary>
		/// Outline frame is a solid line.
		/// </summary>
		None = 0,

		/// <summary>
		/// Outline frame is blurred.
		/// </summary>
		Blurred = 1,

		/// <summary>
		/// Enables depth testing when rendering object outlines. Only visible parts of objects are outlined.
		/// </summary>
		EnableDepthTesting = 2,

		/// <summary>
		/// Enabled alpha testing when rendering outlines.
		/// </summary>
		EnableAlphaTesting = 4
	}
}
