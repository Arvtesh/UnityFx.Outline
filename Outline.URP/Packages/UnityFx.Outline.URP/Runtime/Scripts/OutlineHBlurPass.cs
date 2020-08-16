// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityFx.Outline.URP
{
	internal class OutlineHBlurPass : ScriptableRenderPass
	{
		private readonly OutlineFeature _feature;

		private RenderTargetHandle _src;
		private RenderTargetHandle _dst;

		public OutlineHBlurPass(OutlineFeature feature, RenderTargetHandle src, RenderTargetHandle dst)
		{
			_feature = feature;
			_src = src;
			_dst = dst;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			cmd.GetTemporaryRT(_dst.id, cameraTextureDescriptor);
			ConfigureTarget(_dst.Identifier());
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var cmd = CommandBufferPool.Get("HPass");
			var resources = _feature.OutlineResources;
			var settings = _feature.OutlineSettings;
			var mat = resources.OutlineMaterial;
			var props = resources.GetProperties(settings);

			cmd.SetGlobalFloatArray(resources.GaussSamplesId, resources.GetGaussSamples(settings.OutlineWidth));
			OutlineRenderer.Blit(cmd, _src.Identifier(), resources, OutlineResources.OutlineShaderHPassId, mat, props);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}
