// Copyright (C) 2019-2021 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// Extension methods for <see cref="IOutlineSettings"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class OutlineSettingsExtensions
	{
		/// <summary>
		/// Gets a value indicating whether outline should use alpha testing.
		/// </summary>
		/// <seealso cref="IsDepthTestingEnabled(IOutlineSettings)"/>
		/// <seealso cref="IsBlurEnabled(IOutlineSettings)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlphaTestingEnabled(this IOutlineSettings settings)
		{
			return (settings.OutlineRenderMode & OutlineRenderFlags.EnableAlphaTesting) != 0;
		}

		/// <summary>
		/// Gets a value indicating whether outline should use depth testing.
		/// </summary>
		/// <seealso cref="IsAlphaTestingEnabled(IOutlineSettings)"/>
		/// <seealso cref="IsBlurEnabled(IOutlineSettings)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDepthTestingEnabled(this IOutlineSettings settings)
		{
			return (settings.OutlineRenderMode & OutlineRenderFlags.EnableDepthTesting) != 0;
		}

		/// <summary>
		/// Gets a value indicating whether outline frame should be blurred.
		/// </summary>
		/// <seealso cref="IsAlphaTestingEnabled(IOutlineSettings)"/>
		/// <seealso cref="IsDepthTestingEnabled(IOutlineSettings)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBlurEnabled(this IOutlineSettings settings)
		{
			return (settings.OutlineRenderMode & OutlineRenderFlags.Blurred) != 0;
		}
	}
}
