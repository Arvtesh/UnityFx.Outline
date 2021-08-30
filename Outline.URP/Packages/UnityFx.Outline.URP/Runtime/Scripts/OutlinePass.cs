// Copyright (C) 2019-2021 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

namespace UnityFx.Outline.URP
{
	internal class OutlinePass : ScriptableRenderPass
	{
		private const string _profilerTag = "OutlinePass";
		private static readonly ProfilingSampler _profilingSampler = new ProfilingSampler(_profilerTag);

		private readonly OutlineFeature _feature;
		private readonly List<OutlineRenderObject> _renderObjects = new List<OutlineRenderObject>();
		private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>();

		private ScriptableRenderer _renderer;

		public OutlinePass(OutlineFeature feature, string[] shaderTags)
		{
			_feature = feature;

			if (shaderTags != null && shaderTags.Length > 0)
			{
				foreach (var passName in shaderTags)
				{
					_shaderTagIdList.Add(new ShaderTagId(passName));
				}
			}
			else
			{
				_shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
				_shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
				_shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			}
		}

		public void Setup(ScriptableRenderer renderer)
		{
			_renderer = renderer;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var outlineResources = _feature.OutlineResources;
			var outlineSettings = _feature.OutlineSettings;
			var camData = renderingData.cameraData;

			if (_feature.OutlineLayerMask != 0)
			{
				var cmd = CommandBufferPool.Get(_feature.FeatureName);
				var filteringSettings = new FilteringSettings(RenderQueueRange.all, _feature.OutlineLayerMask, _feature.OutlineRenderingLayerMask);
				var renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
				var sortingCriteria = camData.defaultOpaqueSortFlags;
				var drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);
				var depthTexture = new RenderTargetIdentifier("_CameraDepthTexture");

				drawingSettings.enableDynamicBatching = true;
				drawingSettings.overrideMaterial = outlineResources.RenderMaterial;

				if (outlineSettings.IsAlphaTestingEnabled())
				{
					drawingSettings.overrideMaterialPassIndex = OutlineResources.RenderShaderAlphaTestPassId;
					cmd.SetGlobalFloat(outlineResources.AlphaCutoffId, outlineSettings.OutlineAlphaCutoff);
				}
				else
				{
					drawingSettings.overrideMaterialPassIndex = OutlineResources.RenderShaderDefaultPassId;
				}

				using (new ProfilingScope(cmd, _profilingSampler))
				{
					using (var renderer = new OutlineRenderer(cmd, outlineResources, _renderer.cameraColorTarget, depthTexture/*_renderer.cameraDepth*/, camData.cameraTargetDescriptor))
					{
						renderer.RenderObjectClear(outlineSettings.OutlineRenderMode);
						context.ExecuteCommandBuffer(cmd);

						context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);

						cmd.Clear();
						renderer.RenderOutline(outlineSettings);
					}
				}

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}

			if (_feature.OutlineLayers)
			{
				var cmd = CommandBufferPool.Get(OutlineResources.EffectName);
				var depthTexture = new RenderTargetIdentifier("_CameraDepthTexture");

				using (var renderer = new OutlineRenderer(cmd, outlineResources, _renderer.cameraColorTarget, depthTexture /*_renderer.cameraDepth*/, camData.cameraTargetDescriptor))
				{
					_renderObjects.Clear();
					_feature.OutlineLayers.GetRenderObjects(_renderObjects);
					renderer.Render(_renderObjects);
				}

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
		}
	}
}
