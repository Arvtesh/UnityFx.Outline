// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

namespace UnityFx.Outline.PostProcessing
{
	[Preserve]
	public sealed class OutlineEffectRenderer : PostProcessEffectRenderer<Outline>
	{
		private OutlineResources _defaultResources;

		public override void Render(PostProcessRenderContext context)
		{
			OutlineResources resources;

			if (settings.Resources.value != null)
			{
				resources = settings.Resources;
			}
			else
			{
				if (!_defaultResources)
				{
					_defaultResources = ScriptableObject.CreateInstance<OutlineResources>();
					_defaultResources.ResetToDefaults();
					_defaultResources.FullscreenTriangleMesh = RuntimeUtilities.fullscreenTriangle;
				}

				resources = _defaultResources;
			}

			if (resources && resources.IsValid)
			{
				using (var renderer = new OutlineRenderer(context.command, context.source, context.destination))
				{
					settings.Layers.value.Render(renderer, resources);
				}
			}
		}

		public override void Release()
		{
			if (_defaultResources)
			{
				ScriptableObject.Destroy(_defaultResources);
			}

			base.Release();
		}
	}
}
