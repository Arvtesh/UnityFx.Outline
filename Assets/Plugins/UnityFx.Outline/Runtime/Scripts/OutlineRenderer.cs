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
	/// <seealso cref="OutlineMaterialSet"/>
	public struct OutlineRenderer : IDisposable
	{
		#region data

		private static readonly int _maskRtId = Shader.PropertyToID("_MaskTex");
		private static readonly int _hPassRtId = Shader.PropertyToID("_HPassTex");

		private readonly RenderTargetIdentifier _source;
		private readonly RenderTargetIdentifier _destination;
		private readonly CommandBuffer _commandBuffer;

		private bool _disposed;

		#endregion

		#region interface

		/// <summary>
		/// A <see cref="CameraEvent"/> outline rendering should be assosiated with.
		/// </summary>
		public const CameraEvent RenderEvent = CameraEvent.AfterImageEffects;

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

			_disposed = false;
			_source = src;
			_destination = dst;

			_commandBuffer = commandBuffer;
			_commandBuffer.BeginSample(EffectName);
			_commandBuffer.GetTemporaryRT(_maskRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
			_commandBuffer.GetTemporaryRT(_hPassRtId, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		}

		/// <summary>
		/// Adds commands for rendering single outline object.
		/// </summary>
		public void RenderSingleObject(IEnumerable<Renderer> renderers, OutlineMaterialSet materials)
		{
			if (renderers == null)
			{
				throw new ArgumentNullException("renderers");
			}

			if (materials == null)
			{
				throw new ArgumentNullException("materials");
			}

			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			Init(materials);
			RenderObject(renderers, materials);
			Blit(_maskRtId, _hPassRtId, materials.HPassMaterial);
			Blit(_source, _destination, materials.VPassBlendMaterial);
		}

		/// <summary>
		/// Adds commands for rendering single outline object.
		/// </summary>
		public void RenderSingleObject(Renderer renderer, OutlineMaterialSet materials)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException("renderer");
			}

			if (materials == null)
			{
				throw new ArgumentNullException("materials");
			}

			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			Init(materials);
			RenderObject(renderer, materials);
			Blit(_maskRtId, _hPassRtId, materials.HPassMaterial);
			Blit(_source, _destination, materials.VPassBlendMaterial);
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
			if (!_disposed)
			{
				_disposed = true;
				_commandBuffer.ReleaseTemporaryRT(_hPassRtId);
				_commandBuffer.ReleaseTemporaryRT(_maskRtId);
				_commandBuffer.EndSample(EffectName);
			}
		}

		#endregion

		#region implementation

		private void Init(OutlineMaterialSet materials)
		{
			_commandBuffer.SetGlobalFloatArray(materials.GaussSamplesNameId, materials.GaussSamples);

#if UNITY_2018_2_OR_NEWER
			_commandBuffer.SetRenderTarget(_maskRtId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
			_commandBuffer.SetRenderTarget(_maskRtId);
#endif
			_commandBuffer.ClearRenderTarget(false, true, Color.clear);

		}

		private void RenderObject(IEnumerable<Renderer> renderers, OutlineMaterialSet materials)
		{
			foreach (var r in renderers)
			{
				if (r && r.enabled && r.gameObject.activeInHierarchy)
				{
					for (var j = 0; j < r.sharedMaterials.Length; ++j)
					{
						_commandBuffer.DrawRenderer(r, materials.RenderMaterial, j);
					}
				}
			}
		}

		private void RenderObject(Renderer renderer, OutlineMaterialSet materials)
		{
			if (renderer && renderer.gameObject.activeInHierarchy && renderer.enabled)
			{
				for (var i = 0; i < renderer.sharedMaterials.Length; ++i)
				{
					_commandBuffer.DrawRenderer(renderer, materials.RenderMaterial, i);
				}
			}
		}

		private void Blit(RenderTargetIdentifier source, RenderTargetIdentifier destination, Material mat)
		{
#if UNITY_2018_2_OR_NEWER
			_commandBuffer.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
#else
			_commandBuffer.SetRenderTarget(destination);
#endif

			// NOTE: Have to clear render target before blitting to avoid Tile GPU perf. warnings.
			// https://forum.unity.com/threads/rendertexture-not-working-on-mobile.484105/#post-3153721
			if (!source.Equals(destination))
			{
				_commandBuffer.ClearRenderTarget(true, true, Color.clear);
			}

			// TODO: Use DrawMesh with special copy material to render one full-screen triangle instead of 2 triangles used in Blit.
			_commandBuffer.Blit(source, BuiltinRenderTextureType.CurrentActive, mat);
		}

		#endregion
	}
}
