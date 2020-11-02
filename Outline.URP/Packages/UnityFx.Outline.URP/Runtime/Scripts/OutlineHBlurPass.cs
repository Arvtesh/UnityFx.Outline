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

		public OutlineHBlurPass(OutlineFeature feature)
		{
			_feature = feature;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			cameraTextureDescriptor.colorFormat = OutlineRenderer.RtFormat;
			cmd.GetTemporaryRT(_feature.TempTexId, cameraTextureDescriptor);
			ConfigureTarget(_feature.TempTex);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var cmd = CommandBufferPool.Get(_feature.FeatureName);
			var resources = _feature.OutlineResources;
			var settings = _feature.OutlineSettings;
			var mat = resources.OutlineMaterial;
			var props = resources.GetProperties(settings);

			cmd.SetGlobalFloatArray(resources.GaussSamplesId, resources.GetGaussSamples(settings.OutlineWidth));
			OutlineRenderer.Blit(cmd, _feature.MaskTex, resources, OutlineResources.OutlineShaderHPassId, mat, props);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}
