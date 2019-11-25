// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Helper low-level class for building outline <see cref="CommandBuffer"/>.
	/// </summary>
	/// <remarks>
	/// This class is used by higher level outline implementations (<see cref="OutlineEffect"/> and <see cref="OutlineBehaviour"/>).
	/// It implements <see cref="IDisposable"/> to be used inside <see langword="using"/> block as shown in the code sample. Disposing
	/// <see cref="OutlineRenderer"/> does not dispose the <see cref="CommandBuffer"/>.
	/// </remarks>
	/// <example>
	/// using (var renderer = new OutlineRenderer(commandBuffer, BuiltinRenderTextureType.CameraTarget))
	/// {
	/// 	renderer.RenderSingleObject(outlineRenderers, renderMaterial, postProcessMaterial);
	/// }
	/// </example>
	/// <seealso cref="OutlineResources"/>
	public struct OutlineRenderer : IDisposable
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
		/// A <see cref="CameraEvent"/> outline rendering should be assosiated with.
		/// </summary>
		public const CameraEvent RenderEvent = CameraEvent.BeforeImageEffects;

		/// <summary>
		/// Name of the outline effect.
		/// </summary>
		public const string EffectName = "Outline";

		/// <summary>
		/// Minimum value of outline width parameter.
		/// </summary>
		public const int MinWidth = 1;

		/// <summary>
		/// Maximum value of outline width parameter.
		/// </summary>
		public const int MaxWidth = 32;

		/// <summary>
		/// Minimum value of outline intensity parameter.
		/// </summary>
		public const int MinIntensity = 1;

		/// <summary>
		/// Maximum value of outline intensity parameter.
		/// </summary>
		public const int MaxIntensity = 64;

		/// <summary>
		/// Value of outline intensity parameter that is treated as solid fill.
		/// </summary>
		public const int SolidIntensity = 100;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		public OutlineRenderer(CommandBuffer commandBuffer, BuiltinRenderTextureType rt)
			: this(commandBuffer, rt, rt)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		public OutlineRenderer(CommandBuffer commandBuffer, RenderTargetIdentifier rt)
			: this(commandBuffer, rt, rt)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OutlineRenderer"/> struct.
		/// </summary>
		public OutlineRenderer(CommandBuffer commandBuffer, RenderTargetIdentifier src, RenderTargetIdentifier dst)
		{
			Debug.Assert(commandBuffer != null);

			_source = src;
			_destination = dst;

			_commandBuffer = commandBuffer;
			_commandBuffer.BeginSample(EffectName);
			_commandBuffer.GetTemporaryRT(_maskRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
			_commandBuffer.GetTemporaryRT(_hPassRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		}

		/// <summary>
		/// Renders outline around a single object.
		/// </summary>
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
			RenderObject(renderers, resources.RenderMaterial);
			RenderHPass(resources, settings);
			RenderVPassBlend(resources, settings);
		}

		/// <summary>
		/// Renders outline around a single object.
		/// </summary>
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
			RenderObject(renderer, resources.RenderMaterial);
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

		/// <inheritdoc/>
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
			_commandBuffer.SetGlobalFloatArray(resources.GaussSamplesNameId, resources.GetGaussSamples(settings.OutlineWidth));
		}

		private void RenderObject(IEnumerable<Renderer> renderers, Material mat)
		{
#if UNITY_2018_2_OR_NEWER
			_commandBuffer.SetRenderTarget(_maskRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
			_commandBuffer.SetRenderTarget(_maskRtId);
#endif
			_commandBuffer.ClearRenderTarget(false, true, Color.clear);

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

		private void RenderObject(Renderer renderer, Material mat)
		{
#if UNITY_2018_2_OR_NEWER
			_commandBuffer.SetRenderTarget(_maskRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
			_commandBuffer.SetRenderTarget(_maskRtId);
#endif
			_commandBuffer.ClearRenderTarget(false, true, Color.clear);

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
			props.SetFloat(resources.WidthNameId, settings.OutlineWidth);

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

			props.SetFloat(resources.WidthNameId, settings.OutlineWidth);
			props.SetColor(resources.ColorNameId, settings.OutlineColor);

			if (settings.OutlineMode == OutlineMode.Solid)
			{
				props.SetFloat(resources.IntensityNameId, SolidIntensity);
			}
			else
			{
				props.SetFloat(resources.IntensityNameId, settings.OutlineIntensity);
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
