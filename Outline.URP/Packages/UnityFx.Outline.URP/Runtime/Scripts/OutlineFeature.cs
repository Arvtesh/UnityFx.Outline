// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityFx.Outline.URP
{
	/// <summary>
	/// Outline feature (URP).
	/// </summary>
	[CreateAssetMenu(fileName = "OutlineFeature", menuName = "UnityFx/Outline/Outline (URP)")]
	public class OutlineFeature : ScriptableRendererFeature
	{
		private OutlineRenderColorPass _renderColorPass;
		private OutlinePass _outlinePass;

		public override void Create()
		{
			_renderColorPass = new OutlineRenderColorPass();
			_outlinePass = new OutlinePass();

			// Configures where the render pass should be injected.
			_renderColorPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
		}

		// Here you can inject one or multiple render passes in the renderer.
		// This method is called when setting up the renderer once per-camera.
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			_renderColorPass.SetDepth(renderer.cameraDepth);

			renderer.EnqueuePass(_renderColorPass);
			renderer.EnqueuePass(_outlinePass);
		}
	}
}
