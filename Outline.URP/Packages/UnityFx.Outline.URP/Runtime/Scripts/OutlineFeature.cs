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
	public class OutlineFeature : ScriptableRendererFeature
	{
#pragma warning disable 0649

		[SerializeField]
		private OutlineResources _outlineResources;
		[SerializeField]
		private OutlineLayerCollection _outlineLayers;

#pragma warning restore 0649

		private OutlinePass _outlinePass;

		public override void Create()
		{
			_outlinePass = new OutlinePass();
			_outlinePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			_outlinePass.Setup(_outlineResources, renderer.cameraDepth, _outlineLayers);
			renderer.EnqueuePass(_outlinePass);
		}
	}
}
