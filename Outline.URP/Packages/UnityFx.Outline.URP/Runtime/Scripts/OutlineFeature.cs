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
		#region data

#pragma warning disable 0649

		[SerializeField, Tooltip(OutlineResources.OutlineResourcesTooltip)]
		private OutlineResources _outlineResources;
		[SerializeField, Tooltip(OutlineResources.OutlineLayerCollectionTooltip)]
		private OutlineLayerCollection _outlineLayers;
		[SerializeField]
		private OutlineSettingsWithLayerMask _outlineSettings;
		[SerializeField]
		private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

#pragma warning restore 0649

		private RenderTargetHandle _renderTexture;
		private RenderTargetHandle _hpassTexture;

		private OutlinePass _outlinePass;
		private OutlineRenderLayerPass _outlineRenderPass;
		private OutlineHBlurPass _outlineHBlurPass;
		private OutlineVBlurBlendPass _outlineVBlurBlendPass;

		#endregion

		#region interface

		internal OutlineResources OutlineResources => _outlineResources;

		internal OutlineLayerCollection OutlineLayers => _outlineLayers;

		internal IOutlineSettings OutlineSettings => _outlineSettings;

		#endregion

		#region ScriptableRendererFeature

		/// <inheritdoc/>
		public override void Create()
		{
			_renderTexture.Init(OutlineResources.MaskTexName);
			_hpassTexture.Init(OutlineResources.TempTexName);

			_outlinePass = new OutlinePass(this)
			{
				renderPassEvent = _renderPassEvent
			};

			_outlineRenderPass = new OutlineRenderLayerPass(this, _outlineSettings.OutlineLayerMask, _renderTexture)
			{
				renderPassEvent = _renderPassEvent
			};

			_outlineHBlurPass = new OutlineHBlurPass(this, _renderTexture, _hpassTexture)
			{
				renderPassEvent = _renderPassEvent
			};

			_outlineVBlurBlendPass = new OutlineVBlurBlendPass(this, _hpassTexture)
			{
				renderPassEvent = _renderPassEvent
			};
		}

		/// <inheritdoc/>
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (_outlineResources && _outlineResources.IsValid)
			{
				if (_outlineSettings.OutlineLayerMask != 0)
				{
					_outlineRenderPass.Setup(renderer);
					renderer.EnqueuePass(_outlineRenderPass);
					renderer.EnqueuePass(_outlineHBlurPass);
					renderer.EnqueuePass(_outlineVBlurBlendPass);
				}

				if (_outlineLayers)
				{
					_outlinePass.Setup(renderer);
					renderer.EnqueuePass(_outlinePass);
				}
			}
		}

		#endregion
	}
}
