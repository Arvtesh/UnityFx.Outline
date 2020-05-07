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
	public struct OutlineRenderer : IDisposable
	{
		#region data

		private const int _hPassId = 0;
		private const int _vPassId = 1;

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
			: this(commandBuffer, rt, rt, Vector2Int.zero)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		/// <param name="commandBuffer">A <see cref="CommandBuffer"/> to render the effect to. It should be cleared manually (if needed) before passing to this method.</param>
		/// <param name="rt">Render target.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="commandBuffer"/> is <see langword="null"/>.</exception>
		public OutlineRenderer(CommandBuffer commandBuffer, RenderTargetIdentifier rt)
			: this(commandBuffer, rt, rt, Vector2Int.zero)
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
			: this(commandBuffer, src, dst, Vector2Int.zero)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		/// <param name="commandBuffer">A <see cref="CommandBuffer"/> to render the effect to. It should be cleared manually (if needed) before passing to this method.</param>
		/// <param name="src">Source image. Can be the same as <paramref name="dst"/>.</param>
		/// <param name="dst">Render target.</param>
		/// <param name="rtSize">Size of the temporaty render textures.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="commandBuffer"/> is <see langword="null"/>.</exception>
		public OutlineRenderer(CommandBuffer commandBuffer, RenderTargetIdentifier src, RenderTargetIdentifier dst, Vector2Int rtSize)
		{
			if (commandBuffer == null)
			{
				throw new ArgumentNullException("commandBuffer");
			}

			var cx = rtSize.x > 0 ? rtSize.x : -1;
			var cy = rtSize.y > 0 ? rtSize.y : -1;
			var rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? RenderTextureFormat.R8 : RenderTextureFormat.Default;

			_source = src;
			_destination = dst;

			_commandBuffer = commandBuffer;
			_commandBuffer.BeginSample(EffectName);
			_commandBuffer.GetTemporaryRT(_maskRtId, cx, cy, 0, FilterMode.Bilinear, rtFormat);
			_commandBuffer.GetTemporaryRT(_hPassRtId, cx, cy, 0, FilterMode.Bilinear, rtFormat);

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
		/// Renders outline around a single object. This version allows enumeration of <paramref name="renderers"/> with no GC allocations.
		/// </summary>
		/// <param name="renderers">One or more renderers representing a single object to be outlined.</param>
		/// <param name="resources">Outline resources.</param>
		/// <param name="settings">Outline settings.</param>
		/// <param name="renderingPath">Rendering path used by the target camera (used is <see cref="OutlineRenderFlags.EnableDepthTesting"/> is set).</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments is <see langword="null"/>.</exception>
		/// <seealso cref="Render(IEnumerable{Renderer}, OutlineResources, IOutlineSettings)"/>
		/// <seealso cref="Render(Renderer, OutlineResources, IOutlineSettings)"/>
		public void Render(IList<Renderer> renderers, OutlineResources resources, IOutlineSettings settings, RenderingPath renderingPath = RenderingPath.UsePlayerSettings)
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

			if (renderers.Count > 0)
			{
				Init(resources, settings);
				RenderObject(resources, settings, renderers, renderingPath);
				RenderHPass(resources, settings);
				RenderVPassBlend(resources, settings);
			}
		}

		/// <summary>
		/// Renders outline around a single object.
		/// </summary>
		/// <param name="renderers">One or more renderers representing a single object to be outlined.</param>
		/// <param name="resources">Outline resources.</param>
		/// <param name="settings">Outline settings.</param>
		/// <param name="renderingPath">Rendering path used by the target camera (used is <see cref="OutlineRenderFlags.EnableDepthTesting"/> is set).</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments is <see langword="null"/>.</exception>
		/// <seealso cref="Render(IList{Renderer}, OutlineResources, IOutlineSettings)"/>
		/// <seealso cref="Render(Renderer, OutlineResources, IOutlineSettings)"/>
		public void Render(IEnumerable<Renderer> renderers, OutlineResources resources, IOutlineSettings settings, RenderingPath renderingPath = RenderingPath.UsePlayerSettings)
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
			RenderObject(resources, settings, renderers, renderingPath);
			RenderHPass(resources, settings);
			RenderVPassBlend(resources, settings);
		}

		/// <summary>
		/// Renders outline around a single object.
		/// </summary>
		/// <param name="renderer">A <see cref="Renderer"/> representing an object to be outlined.</param>
		/// <param name="resources">Outline resources.</param>
		/// <param name="settings">Outline settings.</param>
		/// <param name="renderingPath">Rendering path used by the target camera (used is <see cref="OutlineRenderFlags.EnableDepthTesting"/> is set).</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments is <see langword="null"/>.</exception>
		/// <seealso cref="Render(IList{Renderer}, OutlineResources, IOutlineSettings)"/>
		/// <seealso cref="Render(IEnumerable{Renderer}, OutlineResources, IOutlineSettings)"/>
		public void Render(Renderer renderer, OutlineResources resources, IOutlineSettings settings, RenderingPath renderingPath = RenderingPath.UsePlayerSettings)
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
			RenderObject(resources, settings, renderer, renderingPath);
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
			// Shader parameter overrides (shared between all passes).
			var props = resources.Properties;

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

			// Gauss samples.
			_commandBuffer.SetGlobalFloatArray(resources.GaussSamplesId, resources.GetGaussSamples(settings.OutlineWidth));
		}

		private void RenderObjectClear(OutlineRenderFlags flags, RenderingPath renderingPath)
		{
			if ((flags & OutlineRenderFlags.EnableDepthTesting) != 0)
			{
				// Have to use BuiltinRenderTextureType.ResolvedDepth for deferred, BuiltinRenderTextureType.Depth for forward.
				var depthTextureId = (renderingPath == RenderingPath.DeferredShading || renderingPath == RenderingPath.DeferredLighting) ?
					BuiltinRenderTextureType.ResolvedDepth : BuiltinRenderTextureType.Depth;

				// NOTE: Use the camera depth buffer when rendering the mask. Shader only reads from the depth buffer (ZWrite Off).
#if UNITY_2018_2_OR_NEWER
				_commandBuffer.SetRenderTarget(_maskRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, depthTextureId, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
#else
				_commandBuffer.SetRenderTarget(_maskRtId, depthTextureId);
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

		private void RenderObject(OutlineResources resources, IOutlineSettings settings, IList<Renderer> renderers, RenderingPath renderingPath)
		{
			RenderObjectClear(settings.OutlineRenderMode, renderingPath);

			for (var i = 0; i < renderers.Count; ++i)
			{
				var r = renderers[i];

				if (r && r.enabled && r.gameObject.activeInHierarchy)
				{
					// NOTE: Accessing Renderer.sharedMaterials triggers GC.Alloc. That's why we use a temporary
					// list of materials, cached with the outline resources.
					r.GetSharedMaterials(resources.TmpMaterials);

					for (var j = 0; j < resources.TmpMaterials.Count; ++j)
					{
						_commandBuffer.DrawRenderer(r, resources.RenderMaterial, j);
					}
				}
			}
		}

		private void RenderObject(OutlineResources resources, IOutlineSettings settings, IEnumerable<Renderer> renderers, RenderingPath renderingPath)
		{
			RenderObjectClear(settings.OutlineRenderMode, renderingPath);

			// NOTE: Calling IEnumerable.GetEnumerator() triggers GC.Alloc.
			foreach (var r in renderers)
			{
				if (r && r.enabled && r.gameObject.activeInHierarchy)
				{
					// NOTE: Accessing Renderer.sharedMaterials triggers GC.Alloc. That's why we use a temporary
					// list of materials, cached with the outline resources.
					r.GetSharedMaterials(resources.TmpMaterials);

					for (var j = 0; j < resources.TmpMaterials.Count; ++j)
					{
						_commandBuffer.DrawRenderer(r, resources.RenderMaterial, j);
					}
				}
			}
		}

		private void RenderObject(OutlineResources resources, IOutlineSettings settings, Renderer renderer, RenderingPath renderingPath)
		{
			RenderObjectClear(settings.OutlineRenderMode, renderingPath);

			if (renderer && renderer.gameObject.activeInHierarchy && renderer.enabled)
			{
				// NOTE: Accessing Renderer.sharedMaterials triggers GC.Alloc. That's why we use a temporary
				// list of materials, cached with the outline resources.
				renderer.GetSharedMaterials(resources.TmpMaterials);

				for (var i = 0; i < resources.TmpMaterials.Count; ++i)
				{
					_commandBuffer.DrawRenderer(renderer, resources.RenderMaterial, i);
				}
			}
		}

		private void RenderHPass(OutlineResources resources, IOutlineSettings settings)
		{
			// Set source texture as _MainTex to match Blit behavior.
			_commandBuffer.SetGlobalTexture(_mainRtId, _maskRtId);

			// Set destination texture as render target.
#if UNITY_2018_2_OR_NEWER
			_commandBuffer.SetRenderTarget(_hPassRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
			_commandBuffer.SetRenderTarget(_hPassRtId);
#endif

			// Blit fullscreen triangle.
			if ((settings.OutlineRenderMode & OutlineRenderFlags.UseLegacyRenderer) == 0 && SystemInfo.graphicsShaderLevel >= 35)
			{
				_commandBuffer.DrawProcedural(Matrix4x4.identity, resources.OutlineMaterial, _hPassId, MeshTopology.Triangles, 3, 1, resources.Properties);
			}
			else
			{
				_commandBuffer.DrawMesh(resources.FullscreenTriangleMesh, Matrix4x4.identity, resources.OutlineMaterial, 0, _hPassId, resources.Properties);
			}
		}

		private void RenderVPassBlend(OutlineResources resources, IOutlineSettings settings)
		{
			// Set source texture as _MainTex to match Blit behavior.
			_commandBuffer.SetGlobalTexture(_mainRtId, _hPassRtId);

			// Set destination texture as render target.
#if UNITY_2018_2_OR_NEWER
			_commandBuffer.SetRenderTarget(_destination, _source.Equals(_destination) ? RenderBufferLoadAction.Load : RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
			_commandBuffer.SetRenderTarget(_destination);
#endif

			// Blit fullscreen triangle.
			if ((settings.OutlineRenderMode & OutlineRenderFlags.UseLegacyRenderer) == 0 && SystemInfo.graphicsShaderLevel >= 35)
			{
				_commandBuffer.DrawProcedural(Matrix4x4.identity, resources.OutlineMaterial, _vPassId, MeshTopology.Triangles, 3, 1, resources.Properties);
			}
			else
			{
				_commandBuffer.DrawMesh(resources.FullscreenTriangleMesh, Matrix4x4.identity, resources.OutlineMaterial, 0, _vPassId, resources.Properties);
			}
		}

		#endregion
	}
}
