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

		private RenderTargetIdentifier _rt;
		private RenderTargetIdentifier _depth;
		private RenderTextureDescriptor _rtDesc;

		public OutlinePass(OutlineFeature feature)
		{
			_feature = feature;
		}

		public void Setup(RenderTargetIdentifier rt, RenderTargetIdentifier depth)
		{
			_rt = rt;
			_depth = depth;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			_rtDesc = cameraTextureDescriptor;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var cmd = CommandBufferPool.Get(OutlineResources.EffectName);

			using (var renderer = new OutlineRenderer(cmd, _feature.OutlineResources, _rt, _depth, _rtDesc))
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
