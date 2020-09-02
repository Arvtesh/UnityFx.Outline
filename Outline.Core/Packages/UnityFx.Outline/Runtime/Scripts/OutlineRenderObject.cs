// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// A single outline object + its outline settings.
	/// </summary>
	public readonly struct OutlineRenderObject : IEquatable<OutlineRenderObject>
	{
		#region data

		private readonly string _tag;
		private readonly IReadOnlyList<Renderer> _renderers;
		private readonly IOutlineSettings _outlineSettings;

		#endregion

		#region interface

		/// <summary>
		/// Gets the object tag name.
		/// </summary>
		public string Tag => _tag;

		/// <summary>
		/// Gets renderers for the object.
		/// </summary>
		public IReadOnlyList<Renderer> Renderers => _renderers;

		/// <summary>
		/// Gets outline settings for this object.
		/// </summary>
		public IOutlineSettings OutlineSettings => _outlineSettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderObject"/> struct.
		/// </summary>
		public OutlineRenderObject(IReadOnlyList<Renderer> renderers, IOutlineSettings outlineSettings, string tag = null)
		{
			_renderers = renderers;
			_outlineSettings = outlineSettings;
			_tag = tag;
		}

		#endregion

		#region IEquatable

		/// <inheritdoc/>
		public bool Equals(OutlineRenderObject other)
		{
			return string.CompareOrdinal(_tag, other._tag) == 0 && _renderers == other._renderers && _outlineSettings == other._outlineSettings;
		}

		#endregion
	}
}
