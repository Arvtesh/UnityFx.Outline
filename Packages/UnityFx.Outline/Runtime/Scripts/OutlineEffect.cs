// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Renders outlines at specific camera. Should be attached to camera to function.
	/// </summary>
	/// <seealso cref="OutlineLayer"/>
	/// <seealso cref="OutlineBehaviour"/>
	/// <seealso cref="OutlineSettings"/>
	/// <seealso cref="https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/"/>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public sealed partial class OutlineEffect : MonoBehaviour
	{
		#region data

		[SerializeField, Tooltip("Sets outline resources to use. Do not change the defaults unless you know what you're doing.")]
		private OutlineResources _outlineResources;
		[SerializeField, Tooltip("Collection of outline layers to use. This can be used to share outline settings between multiple cameras.")]
		private OutlineLayerCollection _outlineLayers;
		[SerializeField, HideInInspector]
		private CameraEvent _cameraEvent = OutlineRenderer.RenderEvent;

		private CommandBuffer _commandBuffer;

#if UNITY_EDITOR

		private int _commandBufferUpdateCounter;

#endif

		#endregion

		#region interface

#if UNITY_EDITOR

		/// <summary>
		/// Gets number of the command buffer updates since its creation. Only available in editor.
		/// </summary>
		public int NumberOfCommandBufferUpdates
		{
			get
			{
				return _commandBufferUpdateCounter;
			}
		}

#endif

		/// <summary>
		/// Gets or sets resources used by the effect implementation.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if setter argument is <see langword="null"/>.</exception>
		public OutlineResources OutlineResources
		{
			get
			{
				return _outlineResources;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("OutlineResources");
				}

				_outlineResources = value;
			}
		}

		/// <summary>
		/// Gets or sets outline layers.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if setter argument is <see langword="null"/>.</exception>
		/// <seealso cref="ShareLayersWith(OutlineEffect)"/>
		public OutlineLayerCollection OutlineLayers
		{
			get
			{
				CreateLayersIfNeeded();
				return _outlineLayers;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("OutlineLayers");
				}

				_outlineLayers = value;
			}
		}

		/// <summary>
		/// Gets or sets <see cref="CameraEvent"/> used to render the outlines.
		/// </summary>
		public CameraEvent RenderEvent
		{
			get
			{
				return _cameraEvent;
			}
			set
			{
				if (_cameraEvent != value)
				{
					if (_commandBuffer != null)
					{
						var camera = GetComponent<Camera>();

						if (camera)
						{
							camera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
							camera.AddCommandBuffer(value, _commandBuffer);
						}
					}

					_cameraEvent = value;
				}
			}
		}

		/// <summary>
		/// Shares <see cref="OutlineLayers"/> with another <see cref="OutlineEffect"/> instance.
		/// </summary>
		/// <param name="other">Effect to share <see cref="OutlineLayers"/> with.</param>
		/// <seealso cref="OutlineLayers"/>
		public void ShareLayersWith(OutlineEffect other)
		{
			if (other)
			{
				CreateLayersIfNeeded();

				other._outlineLayers = _outlineLayers;
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
		}

		private void OnEnable()
		{
			var camera = GetComponent<Camera>();

			if (camera)
			{
				_commandBuffer = new CommandBuffer
				{
					name = string.Format("{0} - {1}", GetType().Name, name)
				};

				camera.depthTextureMode |= DepthTextureMode.Depth;

#if UNITY_EDITOR

				_commandBufferUpdateCounter = 0;

#endif

				camera.AddCommandBuffer(_cameraEvent, _commandBuffer);
			}
		}

		private void OnDisable()
		{
			var camera = GetComponent<Camera>();

			if (camera)
			{
				camera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
			}

			if (_commandBuffer != null)
			{
				_commandBuffer.Dispose();
				_commandBuffer = null;
			}
		}

		private void Update()
		{
			if (_outlineLayers)
			{
				FillCommandBuffer();
			}
		}

		private void OnDestroy()
		{
			// TODO: Find a way to do this once per OutlineLayerCollection instance.
			if (_outlineLayers)
			{
				_outlineLayers.Reset();
			}
		}

#if UNITY_EDITOR

		private void Reset()
		{
			_outlineLayers = null;
		}

#endif

		#endregion

		#region implementation

		private void FillCommandBuffer()
		{
			_commandBuffer.Clear();

			if (_outlineResources && _outlineResources.IsValid)
			{
				using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
				{
					_outlineLayers.Render(renderer, _outlineResources);
				}
			}

#if UNITY_EDITOR

			_commandBufferUpdateCounter++;

#endif
		}

		private void CreateLayersIfNeeded()
		{
			if (ReferenceEquals(_outlineLayers, null))
			{
				_outlineLayers = ScriptableObject.CreateInstance<OutlineLayerCollection>();
				_outlineLayers.name = "OutlineLayers";
			}
		}

		#endregion
	}
}
