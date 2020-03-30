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
		}

		public override void Cleanup()
		{
		}

		#endregion

		#region IPostProcessComponent

		public bool IsActive()
		{
			// TODO
			return true;
		}

		#endregion
	}
}
