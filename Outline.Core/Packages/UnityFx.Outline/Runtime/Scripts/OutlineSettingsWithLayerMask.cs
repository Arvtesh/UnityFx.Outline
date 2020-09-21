// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline
{
	[Serializable]
	internal class OutlineSettingsWithLayerMask : OutlineSettingsInstance
	{
		#region data

#pragma warning disable 0649

		// NOTE: There are custom editors for public components, so no need to show these in default inspector.
		[SerializeField, HideInInspector]
		private LayerMask _outlineLayerMask;

#pragma warning restore 0649

		#endregion

		#region interface

		public int OutlineLayerMask => _outlineLayerMask;

		#endregion

		#region implementation
		#endregion
	}
}
