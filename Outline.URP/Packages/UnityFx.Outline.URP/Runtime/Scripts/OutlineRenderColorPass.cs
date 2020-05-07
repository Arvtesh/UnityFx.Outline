// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityFx.Outline.URP
{
	/// <summary>
	/// 
	/// </summary>
	internal class OutlineRenderColorPass : ScriptableRenderPass
	{
		private RenderTargetHandle _rt;
		private RenderTargetIdentifier _depth;

		public OutlineRenderColorPass()
		{
			_rt.Init("_MaskTex");
		}

		public void SetDepth(RenderTargetIdentifier depth)
		{
			_depth = depth;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			var rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? RenderTextureFormat.R8 : RenderTextureFormat.Default;
			cmd.GetTemporaryRT(_rt.id, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, rtFormat);
			ConfigureClear(ClearFlag.Color, Color.clear);
			ConfigureTarget(_rt.Identifier(), _depth);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			// Here you can implement the rendering logic.
			// Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
			// https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
			// You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
			
		}

		public override void FrameCleanup(CommandBuffer cmd)
		{
			cmd.ReleaseTemporaryRT(_rt.id);
		}
	}
}
