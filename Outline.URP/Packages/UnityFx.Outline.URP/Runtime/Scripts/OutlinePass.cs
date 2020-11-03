// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityFx.Outline.URP
{
	internal class OutlinePass : ScriptableRenderPass
	{
		private readonly OutlineFeature _feature;
		private readonly List<OutlineRenderObject> _renderObjects = new List<OutlineRenderObject>();
		private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };

		private RenderTextureDescriptor _cameraTextureDescriptor;
		private ScriptableRenderer _renderer;

		public OutlinePass(OutlineFeature feature)
		{
			_feature = feature;
		}

		public void Setup(ScriptableRenderer renderer)
		{
			_renderer = renderer;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			_cameraTextureDescriptor = cameraTextureDescriptor;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var outlineResources = _feature.OutlineResources;
			var outlineSettings = _feature.OutlineSettings;

			if (_feature.OutlineLayerMask != 0)
			{
				var cmd = CommandBufferPool.Get(_feature.FeatureName);

				using (var renderer = new OutlineRenderer(cmd, outlineResources, _renderer.cameraColorTarget, _renderer.cameraDepth, _cameraTextureDescriptor))
				{
					renderer.RenderObjectClear(outlineSettings.OutlineRenderMode);
					context.ExecuteCommandBuffer(cmd);
					cmd.Clear();

					var filteringSettings = new FilteringSettings(RenderQueueRange.all, _feature.OutlineLayerMask);
					var renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
					var sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
					var drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);

					drawingSettings.enableDynamicBatching = false;
					drawingSettings.overrideMaterial = outlineResources.RenderMaterial;

					context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
					renderer.RenderOutline(outlineSettings);
				}

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}

			if (_feature.OutlineLayers)
			{
				var cmd = CommandBufferPool.Get(OutlineResources.EffectName);

				using (var renderer = new OutlineRenderer(cmd, outlineResources, _renderer.cameraColorTarget, _renderer.cameraDepth, _cameraTextureDescriptor))
				{
					_renderObjects.Clear();
					_feature.OutlineLayers.GetRenderObjects(_renderObjects);

					foreach (var obj in _renderObjects)
					{
						renderer.Render(obj);
					}
				}

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
		}
	}
}
