// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

namespace UnityFx.Outline.PostProcessing
{
	[Preserve]
	public sealed class OutlineEffectRenderer : PostProcessEffectRenderer<Outline>
	{
		private OutlineResources _defaultResources;
		private List<OutlineRenderObject> _objects = new List<OutlineRenderObject>();

		public override DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth;
		}

		public override void Render(PostProcessRenderContext context)
		{
			OutlineResources resources;

			if (settings.Resources.value)
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
				RuntimeUtilities.CopyTexture(context.command, context.source, context.destination);

				using (var renderer = new OutlineRenderer(context.command, resources, context.destination, context.camera.actualRenderingPath, new Vector2Int(context.width, context.height)))
				{
					_objects.Clear();
					settings.Layers.value.GetRenderObjects(_objects);

					foreach (var obj in _objects)
					{
						renderer.Render(obj);
					}
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
