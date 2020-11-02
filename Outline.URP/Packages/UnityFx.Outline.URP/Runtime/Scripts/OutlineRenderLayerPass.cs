// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityFx.Outline.URP
{
	internal class OutlineRenderLayerPass : ScriptableRenderPass
	{
		private readonly OutlineFeature _feature;

		private List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
		private FilteringSettings _filteringSettings;
		private RenderStateBlock _renderStateBlock;

		private ScriptableRenderer _renderer;

		public OutlineRenderLayerPass(OutlineFeature feature, int layerMask)
		{
			_feature = feature;
			_filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
			_renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

		}

		public void Setup(ScriptableRenderer renderer)
		{
			_renderer = renderer;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			cameraTextureDescriptor.colorFormat = OutlineRenderer.RtFormat;
			cmd.GetTemporaryRT(_feature.MaskTexId, cameraTextureDescriptor);
			ConfigureTarget(_feature.MaskTex, _renderer.cameraDepth);
			ConfigureClear(ClearFlag.Color, Color.clear);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
			var drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);
			drawingSettings.overrideMaterial = _feature.OutlineResources.RenderMaterial;

			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings, ref _renderStateBlock);
		}
	}
}
