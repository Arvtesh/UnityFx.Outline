// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityFx.Outline.PostProcessing
{
	[Serializable]
	[PostProcess(typeof(OutlineEffectRenderer), PostProcessEvent.BeforeStack, "UnityFx/Outline", false)]
	public sealed class Outline : PostProcessEffectSettings
	{
		[SerializeField]
		private OutlineResources _defaultResources;

		[Serializable]
		public class OutlineLayersParameter : ParameterOverride<OutlineLayerCollection>
		{
		}

		[Serializable]
		public class OutlineResourcesParameter : ParameterOverride<OutlineResources>
		{
		}

		// NOTE: PostProcessEffectSettings.OnEnable implementation requires the fields to be public to function properly.
		public OutlineResourcesParameter Resources = new OutlineResourcesParameter();
		public OutlineLayersParameter Layers = new OutlineLayersParameter();

		private void OnEnable()
		{
			if (Resources == null)
			{
				Resources = new OutlineResourcesParameter();
			}

			if (Resources.value == null)
			{
				if (_defaultResources)
				{
					Resources.value = _defaultResources;
				}
			}

			if (Layers == null)
			{
				Layers = new OutlineLayersParameter();
			}
		}

		public override bool IsEnabledAndSupported(PostProcessRenderContext context)
		{
#if UNITY_EDITOR

			// Don't render outline preview when the editor is not playing.
			if (!Application.isPlaying)
			{
				return false;
			}

#endif

			return base.IsEnabledAndSupported(context) && Layers != null && Layers.value != null;
		}
	}
}
