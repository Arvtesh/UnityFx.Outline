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
	/// <remarks>
	/// Add instance of this class to <see cref="ScriptableRendererData.rendererFeatures"/>. Configure
	/// and assign outline resources and layers collection. Make sure <see cref="UniversalRenderPipelineAsset.supportsCameraDepthTexture"/>
	/// is set if you use <see cref="OutlineRenderFlags.EnableDepthTesting"/>.
	/// </remarks>
	public class OutlineFeature : ScriptableRendererFeature
	{
#pragma warning disable 0649

		[SerializeField, Tooltip(OutlineResources.OutlineResourcesTooltip)]
		private OutlineResources _outlineResources;
		[SerializeField, Tooltip(OutlineResources.OutlineLayerCollectionTooltip)]
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
			if (_outlineResources && _outlineLayers)
			{
				_outlinePass.Setup(_outlineResources, _outlineLayers, renderer.cameraColorTarget, renderer.cameraDepth);
				renderer.EnqueuePass(_outlinePass);
			}
		}
	}
}
