// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityFx.Outline.HDRP
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class OutlinePass : CustomPass
	{
		#region data

#pragma warning disable 0649

		[SerializeField, Tooltip(OutlineResources.OutlineResourcesTooltip)]
		private OutlineResources _outlineResources;
		[SerializeField, Tooltip(OutlineResources.OutlineLayerCollectionTooltip)]
		private OutlineLayerCollection _outlineLayers;

#pragma warning restore 0649

		private List<OutlineRenderObject> _renderList;

		#endregion

		#region CustomPass

		protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
		{
			if (_renderList is null)
			{
				_renderList = new List<OutlineRenderObject>();
			}
		}

		protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
		{
			if (_outlineResources && _outlineLayers)
			{
				GetCameraBuffers(out var colorBuffer, out var depthBuffer);

				using (var renderer = new OutlineRenderer(cmd, _outlineResources, colorBuffer.nameID, depthBuffer.nameID, colorBuffer.rt.descriptor))
				{
					_renderList.Clear();
					_outlineLayers.GetRenderObjects(_renderList);

					foreach (var obj in _renderList)
					{
						renderer.Render(obj);
					}
				}
			}
		}

		#endregion
	}
}
