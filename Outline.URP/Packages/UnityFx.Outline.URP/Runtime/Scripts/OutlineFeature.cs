// Copyright (C) 2019-2021 Alexander Bogarsukov. All rights reserved.
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
		[SerializeField]
		public string[] _shaderPassNames;

#pragma warning restore 0649

		private OutlinePass _outlinePass;
		private string _featureName;

		#endregion

		#region interface

		internal OutlineResources OutlineResources => _outlineResources;

		internal OutlineLayerCollection OutlineLayers => _outlineLayers;

		internal IOutlineSettings OutlineSettings => _outlineSettings;

		internal int OutlineLayerMask => _outlineSettings.OutlineLayerMask;

		internal uint OutlineRenderingLayerMask => _outlineSettings.OutlineRenderingLayerMask;

		internal string FeatureName => _featureName;

		#endregion

		#region ScriptableRendererFeature

		/// <inheritdoc/>
		public override void Create()
		{
			if (_outlineSettings != null)
			{
				_featureName = OutlineResources.EffectName + '-' + _outlineSettings.OutlineLayerMask;
			}
			else
			{
				_featureName = OutlineResources.EffectName;
			}

			_outlinePass = new OutlinePass(this, _shaderPassNames)
			{
				renderPassEvent = _renderPassEvent
			};
		}

		/// <inheritdoc/>
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (_outlineResources && _outlineResources.IsValid)
			{
				_outlinePass.Setup(renderer);
				renderer.EnqueuePass(_outlinePass);
			}
		}

		#endregion
	}
}
