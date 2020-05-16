// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityFx.Outline.URP
{
	/// <summary>
	/// 
	/// </summary>
	internal class OutlinePass : ScriptableRenderPass
	{
		private OutlineResources _outlineResources;
		private OutlineLayerCollection _outlineLayers;

		private List<OutlineRenderObject> _renderObjects = new List<OutlineRenderObject>();
		private RenderTargetIdentifier _rt;
		private RenderTargetIdentifier _depth;
		private RenderTextureDescriptor _rtDesc;

		public void Setup(OutlineResources resources, OutlineLayerCollection layers, RenderTargetIdentifier rt, RenderTargetIdentifier depth)
		{
			_outlineResources = resources;
			_outlineLayers = layers;
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

			using (var renderer = new OutlineRenderer(cmd, _outlineResources, _rt, _depth, _rtDesc))
			{
				_renderObjects.Clear();
				_outlineLayers.GetRenderObjects(_renderObjects);

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
