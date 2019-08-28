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
		private readonly int _hPassRtId;
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
		/// Name of the outline gauss kernel table parameter.
		/// </summary>
		public const string GaussKernelParamName = "_GaussKernel";

		/// <summary>
		/// Name of the outline mode shader parameter.
		/// </summary>
		public const string ModeBlurredKeyword = "_MODE_BLURRED";

		/// <summary>
		/// Name of the outline mode shader parameter.
		/// </summary>
		public const string ModeSolidKeyword = "_MODE_SOLID";

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
			_hPassRtId = Shader.PropertyToID("_HPassTex");
			_renderTarget = dst;

			_commandBuffer = commandBuffer;
			_commandBuffer.Clear();
			_commandBuffer.BeginSample(EffectName);
			_commandBuffer.GetTemporaryRT(_maskRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
			_commandBuffer.GetTemporaryRT(_hPassRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		}

		/// <summary>
		/// Adds commands for rendering single outline object.
		/// </summary>
		public void RenderSingleObject(Renderer[] renderers, OutlineMaterialSet materials)
		{
			Debug.Assert(renderers != null);
			Debug.Assert(materials != null);

			_commandBuffer.SetRenderTarget(_maskRtId);
			_commandBuffer.ClearRenderTarget(false, true, Color.black);

			foreach (var renderer in renderers)
			{
				if (renderer)
				{
					for (var i = 0; i < renderer.sharedMaterials.Length; ++i)
					{
						_commandBuffer.DrawRenderer(renderer, materials.RenderMaterial, i);
					}
				}
			}

			_commandBuffer.SetGlobalTexture(_maskRtId, _maskRtId);
			_commandBuffer.Blit(_maskRtId, _hPassRtId, materials.HPassMaterial);
			_commandBuffer.Blit(_hPassRtId, _renderTarget, materials.VPassBlendMaterial);
		}

		/// <summary>
		/// Setups the meterial keywords for the <paramref name="mode"/> passed.
		/// </summary>
		public static void SetupMeterialKeywords(Material m, OutlineMode mode)
		{
			if (m)
			{
				if (mode == OutlineMode.Solid)
				{
					m.EnableKeyword(ModeSolidKeyword);
					m.DisableKeyword(ModeBlurredKeyword);
				}
				else
				{
					m.EnableKeyword(ModeBlurredKeyword);
					m.DisableKeyword(ModeSolidKeyword);
				}
			}
		}

		/// <summary>
		/// Calculates value of Gauss function for the specified <paramref name="x"/> and <paramref name="stdDev"/> values.
		/// </summary>
		public static float Gauss(float x, float stdDev)
		{
			var stdDev2 = stdDev * stdDev * 2;
			var a = 1 / Mathf.Sqrt((float)Math.PI * stdDev2);
			var gauss = a * Mathf.Pow((float)Math.E, -x * x / stdDev2);

			return gauss;
		}

		#endregion

		#region IDisposable

		/// <inheritdoc/>
		public void Dispose()
		{
			_commandBuffer.ReleaseTemporaryRT(_hPassRtId);
			_commandBuffer.ReleaseTemporaryRT(_maskRtId);
			_commandBuffer.EndSample(EffectName);
		}

		#endregion

		#region implementation
		#endregion
	}
}
