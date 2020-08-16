// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityFx.Outline.URP
{
	internal class OutlineVBlurBlendPass : ScriptableRenderPass
	{
		private readonly OutlineFeature _feature;

		private RenderTargetHandle _src;

		public OutlineVBlurBlendPass(OutlineFeature feature, RenderTargetHandle src)
		{
			_feature = feature;
			_src = src;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var cmd = CommandBufferPool.Get("VPassBlend");
			var resources = _feature.OutlineResources;
			var settings = _feature.OutlineSettings;
			var mat = resources.OutlineMaterial;
			var props = resources.GetProperties(settings);

			cmd.SetGlobalFloatArray(resources.GaussSamplesId, resources.GetGaussSamples(settings.OutlineWidth));
			OutlineRenderer.Blit(cmd, _src.Identifier(), resources, OutlineResources.OutlineShaderVPassId, mat, props);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}
