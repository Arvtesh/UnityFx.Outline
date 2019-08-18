// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Outline helpers.
	/// </summary>
	internal static class OutlineHelpers
	{
		public const string RenderTextureName = "_MainTex";
		public const string EffectName = "Outline";
		public const string ColorParamName = "_Color";
		public const string WidthParamName = "_Width";
		public const CameraEvent RenderEvent = CameraEvent.BeforeImageEffects;
		public const int MinWidth = 1;
		public const int MaxWidth = 32;

		public static readonly int RenderTextureId = Shader.PropertyToID(RenderTextureName);

		public static void RenderBegin(CommandBuffer commandBuffer)
		{
			Debug.Assert(commandBuffer != null);

			commandBuffer.BeginSample(EffectName);
			commandBuffer.Clear();
			commandBuffer.GetTemporaryRT(RenderTextureId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		}

		public static void RenderEnd(CommandBuffer commandBuffer)
		{
			Debug.Assert(commandBuffer != null);

			commandBuffer.ReleaseTemporaryRT(RenderTextureId);
			commandBuffer.EndSample(EffectName);
		}

		public static void RenderSingleObject(Renderer[] renderers, Material renderMaterial, Material postProcessMaterial, CommandBuffer commandBuffer, RenderTargetIdentifier dst)
		{
			Debug.Assert(renderers != null);
			Debug.Assert(renderMaterial != null);
			Debug.Assert(postProcessMaterial != null);
			Debug.Assert(commandBuffer != null);

			var rt = new RenderTargetIdentifier(RenderTextureId);

			commandBuffer.SetRenderTarget(rt);
			commandBuffer.ClearRenderTarget(false, true, Color.black);

			foreach (var renderer in renderers)
			{
				if (renderer)
				{
					for (var i = 0; i < renderer.sharedMaterials.Length; ++i)
					{
						commandBuffer.DrawRenderer(renderer, renderMaterial, i);
					}
				}
			}

			commandBuffer.Blit(rt, dst, postProcessMaterial);
		}
	}
}
