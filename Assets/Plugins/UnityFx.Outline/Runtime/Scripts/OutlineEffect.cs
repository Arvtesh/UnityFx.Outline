// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
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

		[SerializeField]
		private OutlineResources _outlineResources;
		[SerializeField]
		private OutlineLayerCollection _outlineLayers;
		[SerializeField, HideInInspector]
		private CameraEvent _cameraEvent = OutlineRenderer.RenderEvent;

		private CommandBuffer _commandBuffer;
		private bool _changed;

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

				if (_outlineResources != value)
				{
					_outlineResources = value;
					_changed = true;
				}
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

				if (_outlineLayers != value)
				{
					_outlineLayers = value;
					_changed = true;
				}
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
				other._changed = true;
			}
		}

		/// <summary>
		/// Detects changes in nested assets and updates outline if needed. The actual update might not be invoked until the next frame.
		/// </summary>
		public void UpdateChanged()
		{
			if (_outlineLayers)
			{
				_outlineLayers.UpdateChanged();
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

				_changed = true;

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
#if UNITY_EDITOR

			UpdateChanged();

#endif

			if (_outlineLayers && (_changed || _outlineLayers.IsChanged))
			{
				FillCommandBuffer();
			}
		}

		private void LateUpdate()
		{
			// TODO: Find a way to do this once per OutlineLayerCollection instance.
			if (_outlineLayers)
			{
				_outlineLayers.AcceptChanges();
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

		private void OnValidate()
		{
			_changed = true;
		}

		private void Reset()
		{
			_outlineLayers = null;
			_changed = true;
		}

#endif

		#endregion

		#region implementation

		private void FillCommandBuffer()
		{
			if (_outlineResources && _outlineResources.IsValid)
			{
				_commandBuffer.Clear();

				using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
				{
					_outlineLayers.Render(renderer, _outlineResources);
				}
			}
			else
			{
				_commandBuffer.Clear();
			}

			_changed = false;

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
