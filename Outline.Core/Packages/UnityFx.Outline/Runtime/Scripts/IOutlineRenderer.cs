// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Outline
{
	public interface IOutlineRenderer
	{
		/// <summary>
		/// Renders outline around a single object represented with a collection of renderers.
		/// </summary>
		void Render(IReadOnlyList<Renderer> renderers, OutlineResources resources, IOutlineSettings settings, RenderingPath renderingPath);
	}
}
