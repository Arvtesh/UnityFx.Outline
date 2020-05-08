// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
	public struct OutlineRenderer : IOutlineRenderer, IDisposable
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
			if (commandBuffer is null)
			{
				throw new ArgumentNullException(nameof(commandBuffer));
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
					_commandBuffer.SetRenderTarget(dst, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
					_commandBuffer.Blit(src, BuiltinRenderTextureType.CurrentActive);
				}
			}
		}

		#endregion

		#region IOutlineRenderer

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
		public void Render(IReadOnlyList<Renderer> renderers, OutlineResources resources, IOutlineSettings settings, RenderingPath renderingPath = RenderingPath.UsePlayerSettings)
		{
			if (renderers is null)
			{
				throw new ArgumentNullException(nameof(renderers));
			}

			if (resources is null)
			{
				throw new ArgumentNullException(nameof(resources));
			}

			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			if (renderers.Count > 0)
			{
				RenderObject(resources, settings, renderers, renderingPath);
				RenderOutline(resources, settings);
			}
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
			if (renderer is null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

			if (resources is null)
			{
				throw new ArgumentNullException(nameof(resources));
			}

			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			RenderObject(resources, settings, renderer, renderingPath);
			RenderOutline(resources, settings);
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

		private void RenderObjectClear(OutlineRenderFlags flags, RenderingPath renderingPath)
		{
			if ((flags & OutlineRenderFlags.EnableDepthTesting) != 0)
			{
				// Have to use BuiltinRenderTextureType.ResolvedDepth for deferred, BuiltinRenderTextureType.Depth for forward.
				var depthTextureId = (renderingPath == RenderingPath.DeferredShading || renderingPath == RenderingPath.DeferredLighting) ?
					BuiltinRenderTextureType.ResolvedDepth : BuiltinRenderTextureType.Depth;

				// NOTE: Use the camera depth buffer when rendering the mask. Shader only reads from the depth buffer (ZWrite Off).
				_commandBuffer.SetRenderTarget(_maskRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, depthTextureId, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
			}
			else
			{
				_commandBuffer.SetRenderTarget(_maskRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			}

			_commandBuffer.ClearRenderTarget(false, true, Color.clear);
		}

		private void RenderObject(OutlineResources resources, IOutlineSettings settings, IReadOnlyList<Renderer> renderers, RenderingPath renderingPath)
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

		private void RenderOutline(OutlineResources resources, IOutlineSettings settings)
		{
			var forceDrawMesh = (settings.OutlineRenderMode & OutlineRenderFlags.UseLegacyRenderer) != 0;
			var mat = resources.OutlineMaterial;
			var props = resources.GetProperties(settings);

			_commandBuffer.SetGlobalFloatArray(resources.GaussSamplesId, resources.GetGaussSamples(settings.OutlineWidth));

			// HPass
			_commandBuffer.SetRenderTarget(_hPassRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			Blit(_commandBuffer, _maskRtId, resources, _hPassId, mat, props, forceDrawMesh);

			// VPassBlend
			_commandBuffer.SetRenderTarget(_destination, _source.Equals(_destination) ? RenderBufferLoadAction.Load : RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			Blit(_commandBuffer, _hPassRtId, resources, _vPassId, mat, props, forceDrawMesh);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Blit(CommandBuffer cmdBuffer, RenderTargetIdentifier src, OutlineResources resources, int shaderPass, Material mat, MaterialPropertyBlock props, bool forceDrawMesh = false)
		{
			// Set source texture as _MainTex to match Blit behavior.
			cmdBuffer.SetGlobalTexture(_mainRtId, src);

			if (forceDrawMesh || SystemInfo.graphicsShaderLevel < 35)
			{
				cmdBuffer.DrawMesh(resources.FullscreenTriangleMesh, Matrix4x4.identity, mat, 0, shaderPass, props);
			}
			else
			{
				cmdBuffer.DrawProcedural(Matrix4x4.identity, mat, shaderPass, MeshTopology.Triangles, 3, 1, props);
			}
		}

		#endregion
	}
}
