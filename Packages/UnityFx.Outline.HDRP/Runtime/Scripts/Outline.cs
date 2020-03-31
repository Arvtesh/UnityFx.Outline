// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityFx.Outline.HDRP
{
	[Serializable]
	[VolumeComponentMenu("Post-processing/UnityFx/Outline")]
	public sealed class Outline : CustomPostProcessVolumeComponent, IPostProcessComponent
	{
		#region data

#pragma warning disable 0649

		[SerializeField, HideInInspector]
		private OutlineResources _defaultResources;
		[SerializeField]
		private VolumeParameter<OutlineResources> _resources = new VolumeParameter<OutlineResources>();
		[SerializeField]
		private VolumeParameter<OutlineLayerCollection> _layers = new VolumeParameter<OutlineLayerCollection>();

#pragma warning restore 0649

		#endregion

		#region interface

		#endregion

		#region CustomPostProcessVolumeComponent

		public override CustomPostProcessInjectionPoint injectionPoint
		{
			get
			{
				return CustomPostProcessInjectionPoint.AfterPostProcess;
			}
		}

		public override void Setup()
		{
		}

		public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
		{
			using (var renderer = new OutlineRenderer(cmd, source, destination))
			{
				_layers.value.Render(renderer, _resources.value);
			}
		}

		public override void Cleanup()
		{
		}

		#endregion

		#region ScriptableObject

		protected override void OnEnable()
		{
			// NOTE: This should go before base.OnEnable().
			if (!_resources.value)
			{
				_resources.value = _defaultResources;
			}

			base.OnEnable();
		}

		#endregion

		#region IPostProcessComponent

		public bool IsActive()
		{
			if (_resources == null || _layers == null)
			{
				return false;
			}

			var r = _resources.value;

			if (r == null || !r.IsValid)
			{
				return false;
			}

			var l = _layers.value;

			if (l == null || l.Count == 0)
			{
				return false;
			}

			return true;
		}

		#endregion
	}
}
