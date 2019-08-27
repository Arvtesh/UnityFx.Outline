// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Helper low-level class for building outline <see cref="CommandBuffer"/>.
	/// </summary>
	/// <remarks>
	/// This class is used by higher level outline implementations (<see cref="OutlineEffect"/> and <see cref="OutlineBehaviour"/>).
	/// It implements <see cref="IDisposable"/> to be used with C# inside <see langword="using"/> block as shown in the code sample.
	/// </remarks>
	/// <example>
	/// using (var renderer = new OutlineRenderer(commandBuffer, BuiltinRenderTextureType.CameraTarget))
	/// {
	/// 	renderer.RenderSingleObject(outlineRenderers, renderMaterial, postProcessMaterial);
	/// }
	/// </example>
	/// <seealso cref="OutlineEffect"/>
	/// <seealso cref="OutlineBehaviour"/>
	public struct OutlineRenderer : IDisposable
	{
		#region data

		private readonly int _maskRtId;
		private readonly int _postProcessRtId;
		private readonly RenderTargetIdentifier _renderTarget;
		private readonly CommandBuffer _commandBuffer;

		#endregion

		#region interface

		/// <summary>
		/// A <see cref="CameraEvent"/> outline rendering should be assosiated with.
		/// </summary>
		public const CameraEvent RenderEvent = CameraEvent.BeforeImageEffects;

		/// <summary>
		/// Name of the outline effect.
		/// </summary>
		public const string EffectName = "Outline";

		/// <summary>
		/// Name of the outline color shader parameter.
		/// </summary>
		public const string ColorParamName = "_Color";

		/// <summary>
		/// Name of the outline width shader parameter.
		/// </summary>
		public const string WidthParamName = "_Width";

		/// <summary>
		/// Minimum value of outline width parameter.
		/// </summary>
		public const int MinWidth = 1;

		/// <summary>
		/// Maximum value of outline width parameter.
		/// </summary>
		public const int MaxWidth = 32;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		public OutlineRenderer(CommandBuffer commandBuffer, BuiltinRenderTextureType dst)
			: this(commandBuffer, new RenderTargetIdentifier(dst))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		public OutlineRenderer(CommandBuffer commandBuffer, RenderTargetIdentifier dst)
		{
			Debug.Assert(commandBuffer != null);

			_maskRtId = Shader.PropertyToID("_MaskTex");
			_postProcessRtId = Shader.PropertyToID("_PostProcessTex");
			_renderTarget = dst;

			_commandBuffer = commandBuffer;
			_commandBuffer.Clear();
			_commandBuffer.BeginSample(EffectName);
			_commandBuffer.GetTemporaryRT(_maskRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
			_commandBuffer.GetTemporaryRT(_postProcessRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		}

		/// <summary>
		/// Adds commands for rendering single outline object.
		/// </summary>
		public void RenderSingleObject(Renderer[] renderers, Material renderMaterial, Material hPassMaterial, Material vPassMaterial)
		{
			Debug.Assert(renderers != null);
			Debug.Assert(renderMaterial != null);
			Debug.Assert(hPassMaterial != null);
			Debug.Assert(vPassMaterial != null);

			_commandBuffer.SetRenderTarget(_maskRtId);
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

			_commandBuffer.SetGlobalTexture(_maskRtId, _maskRtId);
			_commandBuffer.Blit(_maskRtId, _postProcessRtId, hPassMaterial);
			_commandBuffer.Blit(_postProcessRtId, _renderTarget, vPassMaterial);
		}

		#endregion

		#region IDisposable

		/// <inheritdoc/>
		public void Dispose()
		{
			_commandBuffer.ReleaseTemporaryRT(_postProcessRtId);
			_commandBuffer.ReleaseTemporaryRT(_maskRtId);
			_commandBuffer.EndSample(EffectName);
		}

		#endregion

		#region implementation
		#endregion
	}
}
