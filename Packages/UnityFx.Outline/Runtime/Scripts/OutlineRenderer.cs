// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Helper class for outline rendering with <see cref="CommandBuffer"/>.
	/// </summary>
	/// <remarks>
	/// <para>The class can be used on its own or as part of a higher level systems. It is used
	/// by higher level outline implementations (<see cref="OutlineEffect"/> and
	/// <see cref="OutlineBehaviour"/>). It is fully compatible with Unity post processing stack as well.</para>
	/// <para>The class implements <see cref="IDisposable"/> to be used inside <see langword="using"/>
	/// block as shown in the code samples. Disposing <see cref="OutlineRenderer"/> does not dispose
	/// the corresponding <see cref="CommandBuffer"/>.</para>
	/// <para>Command buffer is not cleared before rendering. It is user responsibility to do so if needed.</para>
	/// </remarks>
	/// <example>
	/// var commandBuffer = new CommandBuffer();
	/// 
	/// using (var renderer = new OutlineRenderer(commandBuffer, BuiltinRenderTextureType.CameraTarget))
	/// {
	/// 	renderer.Render(renderers, resources, settings);
	/// }
	///
	/// camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
	/// </example>
	/// <example>
	/// [Preserve]
	/// public class OutlineEffectRenderer : PostProcessEffectRenderer<Outline>
	/// {
	/// 	public override void Init()
	/// 	{
	/// 		base.Init();
	///
	/// 		// Reuse fullscreen triangle mesh from PostProcessing (do not create own).
	/// 		settings.OutlineResources.FullscreenTriangleMesh = RuntimeUtilities.fullscreenTriangle;
	/// 	}
	///
	/// 	public override void Render(PostProcessRenderContext context)
	/// 	{
	/// 		var resources = settings.OutlineResources;
	/// 		var layers = settings.OutlineLayers;
	///
	/// 		if (resources && resources.IsValid && layers)
	/// 		{
	/// 			// No need to setup property sheet parameters, all the rendering staff is handled by the OutlineRenderer.
	/// 			using (var renderer = new OutlineRenderer(context.command, context.source, context.destination))
	/// 			{
	/// 				layers.Render(renderer, resources);
	/// 			}
	/// 		}
	/// 	}
	/// }
	/// </example>
	/// <seealso cref="OutlineResources"/>
	public readonly struct OutlineRenderer : IDisposable
	{
		#region data

		private static readonly int _mainRtId = Shader.PropertyToID("_MainTex");
		private static readonly int _maskRtId = Shader.PropertyToID("_MaskTex");
		private static readonly int _hPassRtId = Shader.PropertyToID("_HPassTex");

		private readonly RenderTargetIdentifier _source;
		private readonly RenderTargetIdentifier _destination;
		private readonly CommandBuffer _commandBuffer;

		#endregion

		#region interface

		/// <summary>
		/// A default <see cref="CameraEvent"/> outline rendering should be assosiated with.
		/// </summary>
		public const CameraEvent RenderEvent = CameraEvent.BeforeImageEffects;

		/// <summary>
		/// Name of the outline effect.
		/// </summary>
		public const string EffectName = "Outline";

		/// <summary>
		/// Minimum value of outline width parameter.
		/// </summary>
		/// <seealso cref="MaxWidth"/>
		public const int MinWidth = 1;

		/// <summary>
		/// Maximum value of outline width parameter.
		/// </summary>
		/// <seealso cref="MinWidth"/>
		public const int MaxWidth = 32;

		/// <summary>
		/// Minimum value of outline intensity parameter.
		/// </summary>
		/// <seealso cref="MaxIntensity"/>
		/// <seealso cref="SolidIntensity"/>
		public const int MinIntensity = 1;

		/// <summary>
		/// Maximum value of outline intensity parameter.
		/// </summary>
		/// <seealso cref="MinIntensity"/>
		/// <seealso cref="SolidIntensity"/>
		public const int MaxIntensity = 64;

		/// <summary>
		/// Value of outline intensity parameter that is treated as solid fill.
		/// </summary>
		/// <seealso cref="MinIntensity"/>
		/// <seealso cref="MaxIntensity"/>
		public const int SolidIntensity = 100;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		/// <param name="commandBuffer">A <see cref="CommandBuffer"/> to render the effect to. It should be cleared manually (if needed) before passing to this method.</param>
		/// <param name="rt">Render target.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="commandBuffer"/> is <see langword="null"/>.</exception>
		public OutlineRenderer(CommandBuffer commandBuffer, BuiltinRenderTextureType rt)
			: this(commandBuffer, rt, rt)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		/// <param name="commandBuffer">A <see cref="CommandBuffer"/> to render the effect to. It should be cleared manually (if needed) before passing to this method.</param>
		/// <param name="rt">Render target.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="commandBuffer"/> is <see langword="null"/>.</exception>
		public OutlineRenderer(CommandBuffer commandBuffer, RenderTargetIdentifier rt)
			: this(commandBuffer, rt, rt)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		/// <param name="commandBuffer">A <see cref="CommandBuffer"/> to render the effect to. It should be cleared manually (if needed) before passing to this method.</param>
		/// <param name="src">Source image. Can be the same as <paramref name="dst"/>.</param>
		/// <param name="dst">Render target.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="commandBuffer"/> is <see langword="null"/>.</exception>
		public OutlineRenderer(CommandBuffer commandBuffer, RenderTargetIdentifier src, RenderTargetIdentifier dst)
		{
			if (commandBuffer == null)
			{
				throw new ArgumentNullException("commandBuffer");
			}

			_source = src;
			_destination = dst;

			_commandBuffer = commandBuffer;
			_commandBuffer.BeginSample(EffectName);
			_commandBuffer.GetTemporaryRT(_maskRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
			_commandBuffer.GetTemporaryRT(_hPassRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);

			// Need to copy src content into dst if they are not the same. For instance this is the case when rendering
			// the outline effect as part of Unity Post Processing stack.
			if (!src.Equals(dst))
			{
				if (SystemInfo.copyTextureSupport > CopyTextureSupport.None)
				{
					_commandBuffer.CopyTexture(src, dst);
				}
				else
				{
#if UNITY_2018_2_OR_NEWER
					_commandBuffer.SetRenderTarget(dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
					_commandBuffer.SetRenderTarget(dst);
#endif
					_commandBuffer.Blit(src, BuiltinRenderTextureType.CurrentActive);
				}
			}
		}

		/// <summary>
		/// Renders outline around a single object.
		/// </summary>
		/// <param name="renderers">One or more renderers representing a single object to be outlined.</param>
		/// <param name="resources">Outline resources.</param>
		/// <param name="settings">Outline settings.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments is <see langword="null"/>.</exception>
		public void Render(IEnumerable<Renderer> renderers, OutlineResources resources, IOutlineSettings settings)
		{
			if (renderers == null)
			{
				throw new ArgumentNullException("renderers");
			}

			if (resources == null)
			{
				throw new ArgumentNullException("resources");
			}

			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			Init(resources, settings);
			RenderObject(renderers, settings, resources.RenderMaterial);
			RenderHPass(resources, settings);
			RenderVPassBlend(resources, settings);
		}

		/// <summary>
		/// Renders outline around a single object.
		/// </summary>
		/// <param name="renderer">A <see cref="Renderer"/> representing an object to be outlined.</param>
		/// <param name="resources">Outline resources.</param>
		/// <param name="settings">Outline settings.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments is <see langword="null"/>.</exception>
		public void Render(Renderer renderer, OutlineResources resources, IOutlineSettings settings)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException("renderers");
			}

			if (resources == null)
			{
				throw new ArgumentNullException("resources");
			}

			if (settings == null)
			{
				throw new ArgumentNullException("settings");
			}

			Init(resources, settings);
			RenderObject(renderer, settings, resources.RenderMaterial);
			RenderHPass(resources, settings);
			RenderVPassBlend(resources, settings);
		}

		/// <summary>
		/// Calculates value of Gauss function for the specified <paramref name="x"/> and <paramref name="stdDev"/> values.
		/// </summary>
		/// <seealso href="https://en.wikipedia.org/wiki/Gaussian_blur"/>
		/// <seealso href="https://en.wikipedia.org/wiki/Normal_distribution"/>
		public static float Gauss(float x, float stdDev)
		{
			var stdDev2 = stdDev * stdDev * 2;
			var a = 1 / Mathf.Sqrt((float)Math.PI * stdDev2);
			var gauss = a * Mathf.Pow((float)Math.E, -x * x / stdDev2);

			return gauss;
		}

		/// <summary>
		/// Samples Gauss function for the specified <paramref name="width"/>.
		/// </summary>
		/// <seealso href="https://en.wikipedia.org/wiki/Normal_distribution"/>
		public static float[] GetGaussSamples(int width, float[] samples)
		{
			// NOTE: According to '3 sigma' rule there is no reason to have StdDev less then width / 3.
			// In practice blur looks best when StdDev is within range [width / 3,  width / 2].
			var stdDev = width * 0.5f;

			if (samples == null)
			{
				samples = new float[MaxWidth];
			}

			for (var i = 0; i < width; i++)
			{
				samples[i] = Gauss(i, stdDev);
			}

			return samples;
		}

		#endregion

		#region IDisposable

		/// <summary>
		/// Finalizes the effect rendering and releases temporary textures used. Should only be called once.
		/// </summary>
		public void Dispose()
		{
			_commandBuffer.ReleaseTemporaryRT(_hPassRtId);
			_commandBuffer.ReleaseTemporaryRT(_maskRtId);
			_commandBuffer.EndSample(EffectName);
		}

		#endregion

		#region implementation

		private void Init(OutlineResources resources, IOutlineSettings settings)
		{
			_commandBuffer.SetGlobalFloatArray(resources.GaussSamplesId, resources.GetGaussSamples(settings.OutlineWidth));
		}

		private void RenderObjectClear(bool depthTestEnabled)
		{
			if (depthTestEnabled)
			{
				// NOTE: Use the camera depth buffer when rendering the mask. Shader only reads from the depth buffer (ZWrite Off).
#if UNITY_2018_2_OR_NEWER
				_commandBuffer.SetRenderTarget(_maskRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, BuiltinRenderTextureType.Depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
#else
				_commandBuffer.SetRenderTarget(_maskRtId, BuiltinRenderTextureType.Depth);
#endif
			}
			else
			{
#if UNITY_2018_2_OR_NEWER
				_commandBuffer.SetRenderTarget(_maskRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
				_commandBuffer.SetRenderTarget(_maskRtId);
#endif
			}

			_commandBuffer.ClearRenderTarget(false, true, Color.clear);
		}

		private void RenderObject(IEnumerable<Renderer> renderers, IOutlineSettings settings, Material mat)
		{
			RenderObjectClear((settings.OutlineRenderMode & OutlineRenderFlags.EnableDepthTesting) != 0);

			foreach (var r in renderers)
			{
				if (r && r.enabled && r.gameObject.activeInHierarchy)
				{
					for (var j = 0; j < r.sharedMaterials.Length; ++j)
					{
						_commandBuffer.DrawRenderer(r, mat, j);
					}
				}
			}
		}

		private void RenderObject(Renderer renderer, IOutlineSettings settings, Material mat)
		{
			RenderObjectClear((settings.OutlineRenderMode & OutlineRenderFlags.EnableDepthTesting) != 0);

			if (renderer && renderer.gameObject.activeInHierarchy && renderer.enabled)
			{
				for (var i = 0; i < renderer.sharedMaterials.Length; ++i)
				{
					_commandBuffer.DrawRenderer(renderer, mat, i);
				}
			}
		}

		private void RenderHPass(OutlineResources resources, IOutlineSettings settings)
		{
			// Setup shader parameter overrides.
			var props = resources.HPassProperties;
			props.SetFloat(resources.WidthId, settings.OutlineWidth);

			// Set source texture as _MainTex to match Blit behavior.
			_commandBuffer.SetGlobalTexture(_mainRtId, _maskRtId);

			// Set destination texture as render target.
#if UNITY_2018_2_OR_NEWER
			_commandBuffer.SetRenderTarget(_hPassRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
			_commandBuffer.SetRenderTarget(_hPassRtId);
#endif

			// Blit fullscreen triangle.
			_commandBuffer.DrawMesh(resources.FullscreenTriangleMesh, Matrix4x4.identity, resources.HPassMaterial, 0, 0, props);
		}

		private void RenderVPassBlend(OutlineResources resources, IOutlineSettings settings)
		{
			// Setup shader parameter overrides.
			var props = resources.VPassBlendProperties;

			props.SetFloat(resources.WidthId, settings.OutlineWidth);
			props.SetColor(resources.ColorId, settings.OutlineColor);

			if ((settings.OutlineRenderMode & OutlineRenderFlags.Blurred) != 0)
			{
				props.SetFloat(resources.IntensityId, settings.OutlineIntensity);
			}
			else
			{
				props.SetFloat(resources.IntensityId, SolidIntensity);
			}

			// Set source texture as _MainTex to match Blit behavior.
			_commandBuffer.SetGlobalTexture(_mainRtId, _source);

			// Set destination texture as render target.
#if UNITY_2018_2_OR_NEWER
			_commandBuffer.SetRenderTarget(_destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
			_commandBuffer.SetRenderTarget(_destination);
#endif

			// Blit fullscreen triangle.
			_commandBuffer.DrawMesh(resources.FullscreenTriangleMesh, Matrix4x4.identity, resources.VPassBlendMaterial, 0, 0, props);
		}

		#endregion
	}
}
