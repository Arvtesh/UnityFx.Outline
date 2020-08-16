// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityFx.Outline.URP
{
	internal class OutlineRenderObjectsPass : ScriptableRenderPass
	{
		private readonly OutlineFeature _feature;

		private RenderTargetHandle _rt;
		private RenderTargetIdentifier _depth;

		private List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
		private FilteringSettings _filteringSettings;
		private RenderStateBlock _renderStateBlock;

		public OutlineRenderObjectsPass(OutlineFeature feature, int layerMask, RenderTargetHandle rt)
		{
			_feature = feature;
			_rt = rt;
			_filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
			_renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

		}

		public void Setup(RenderTargetIdentifier depth)
		{
			_depth = depth;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			cmd.GetTemporaryRT(_rt.id, cameraTextureDescriptor);
			ConfigureTarget(_rt.Identifier(), _depth);
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
