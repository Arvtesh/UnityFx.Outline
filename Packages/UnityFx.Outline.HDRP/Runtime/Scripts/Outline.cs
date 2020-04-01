// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
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

		[Serializable]
		private class OutlineResourcesParameter : VolumeParameter<OutlineResources>
		{
		}

		[Serializable]
		private class OutlineLayersParameter : VolumeParameter<OutlineLayerCollection>
		{
		}

		[SerializeField, HideInInspector]
		private OutlineResources _defaultResources;
		[SerializeField]
		private OutlineResourcesParameter _resources = new OutlineResourcesParameter();
		[SerializeField]
		private OutlineLayersParameter _layers = new OutlineLayersParameter();

#pragma warning restore 0649

		#endregion

		#region interface

		public IList<OutlineLayer> OutlineLayers
		{
			get
			{
				return _layers.value;
			}
		}

		#endregion

		#region CustomPostProcessVolumeComponent

		public override CustomPostProcessInjectionPoint injectionPoint
		{
			get
			{
				return CustomPostProcessInjectionPoint.BeforePostProcess;
			}
		}

		public override void Setup()
		{
			base.Setup();
		}

		public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
		{
			using (var renderer = new OutlineRenderer(cmd, source, destination, destination.referenceSize))
			{
				_layers.value.Render(renderer, _resources.value);
			}
		}

		public override void Cleanup()
		{
			base.Cleanup();
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
