// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	internal struct OutlineRenderer : IDisposable
	{
		#region data

		private readonly int _renderTextureId;
		private readonly RenderTargetIdentifier _renderTarget;
		private readonly CommandBuffer _commandBuffer;

		#endregion

		#region interface

		public const CameraEvent RenderEvent = CameraEvent.BeforeImageEffects;

		public const string EffectName = "Outline";
		public const string ColorParamName = "_Color";
		public const string WidthParamName = "_Width";

		public const int MinWidth = 1;
		public const int MaxWidth = 32;

		public OutlineRenderer(CommandBuffer commandBuffer, BuiltinRenderTextureType dst)
			: this(commandBuffer, new RenderTargetIdentifier(dst))
		{
		}

		public OutlineRenderer(CommandBuffer commandBuffer, RenderTargetIdentifier dst)
		{
			Debug.Assert(commandBuffer != null);

			_renderTextureId = Shader.PropertyToID("_MainTex");
			_renderTarget = dst;

			_commandBuffer = commandBuffer;
			_commandBuffer.BeginSample(EffectName);
			_commandBuffer.Clear();
			_commandBuffer.GetTemporaryRT(_renderTextureId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		}

		public void RenderSingleObject(Renderer[] renderers, Material renderMaterial, Material postProcessMaterial)
		{
			Debug.Assert(renderers != null);
			Debug.Assert(renderMaterial != null);
			Debug.Assert(postProcessMaterial != null);

			var rt = new RenderTargetIdentifier(_renderTextureId);

			_commandBuffer.SetRenderTarget(rt);
			_commandBuffer.ClearRenderTarget(false, true, Color.black);

			foreach (var renderer in renderers)
			{
				if (renderer)
				{
					for (var i = 0; i < renderer.sharedMaterials.Length; ++i)
					{
						_commandBuffer.DrawRenderer(renderer, renderMaterial, i);
					}
				}
			}

			_commandBuffer.Blit(rt, _renderTarget, postProcessMaterial);
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			_commandBuffer.ReleaseTemporaryRT(_renderTextureId);
			_commandBuffer.EndSample(EffectName);
		}

		#endregion

		#region implementation
		#endregion
	}
}
