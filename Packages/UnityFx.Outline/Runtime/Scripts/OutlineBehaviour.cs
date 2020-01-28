// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Attach this script to a <see cref="GameObject"/> to add outline effect. It can be configured in edit-time or in runtime via scripts.
	/// </summary>
	/// <seealso cref="OutlineEffect"/>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public sealed partial class OutlineBehaviour : MonoBehaviour, IOutlineSettingsEx
	{
		#region data

#pragma warning disable 0649

		[SerializeField]
		private OutlineResources _outlineResources;
		[SerializeField, HideInInspector]
		private OutlineSettingsInstance _outlineSettings;

#pragma warning restore 0649

		private RendererCollection _renderers;
		private CommandBuffer _commandBuffer;

		private Dictionary<Camera, CommandBuffer> _cameraMap = new Dictionary<Camera, CommandBuffer>();
		private float _cameraMapUpdateTimer;

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
					CreateSettingsIfNeeded();

					_outlineResources = value;
					_outlineSettings.SetResources(_outlineResources);
				}
			}
		}

		/// <summary>
		/// Gets outline renderers. By default all child <see cref="Renderer"/> components are used for outlining.
		/// </summary>
		public ICollection<Renderer> OutlineRenderers
		{
			get
			{
				CreateRenderersIfNeeded();
				return _renderers;
			}
		}

		/// <summary>
		/// Gets all cameras outline data is rendered to.
		/// </summary>
		public ICollection<Camera> Cameras
		{
			get
			{
				return _cameraMap.Keys;
			}
		}

		/// <summary>
		/// Detects changes in nested assets and updates outline if needed. The actual update might not be invoked until the next frame.
		/// </summary>
		public void UpdateChanged()
		{
			if (_outlineSettings != null)
			{
				_outlineSettings.UpdateChanged();
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			CreateRenderersIfNeeded();
			CreateSettingsIfNeeded();

			_outlineSettings.SetResources(_outlineResources);
		}

		private void OnDestroy()
		{
			if (_outlineSettings != null)
			{
				_outlineSettings.SetResources(null);
			}
		}

		private void OnEnable()
		{
			CreateCommandBufferIfNeeded();
		}

		private void OnDisable()
		{
			foreach (var kvp in _cameraMap)
			{
				if (kvp.Key)
				{
					kvp.Key.RemoveCommandBuffer(OutlineRenderer.RenderEvent, kvp.Value);
				}
			}

			_cameraMap.Clear();

			if (_commandBuffer != null)
			{
				_commandBuffer.Dispose();
				_commandBuffer = null;
			}
		}

		private void Update()
		{
			_cameraMapUpdateTimer += Time.deltaTime;

			if (_cameraMapUpdateTimer > 16)
			{
				RemoveDestroyedCameras();
				_cameraMapUpdateTimer = 0;
			}

#if UNITY_EDITOR

			UpdateChanged();

#endif

			if (_outlineResources != null && _renderers != null && (_outlineSettings.IsChanged || _commandBuffer.sizeInBytes == 0))
			{
				_commandBuffer.Clear();

				using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
				{
					renderer.Render(_renderers, _outlineSettings.OutlineResources, _outlineSettings);
				}

				_outlineSettings.AcceptChanges();

#if UNITY_EDITOR

				_commandBufferUpdateCounter++;

#endif
			}
		}

		private void OnWillRenderObject()
		{
			if (gameObject.activeInHierarchy && enabled)
			{
				var camera = Camera.current;

				if (camera)
				{
					if (!_cameraMap.ContainsKey(camera))
					{
						camera.AddCommandBuffer(OutlineRenderer.RenderEvent, _commandBuffer);
						_cameraMap.Add(camera, _commandBuffer);
					}
				}
			}
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			CreateRenderersIfNeeded();
			CreateCommandBufferIfNeeded();
			CreateSettingsIfNeeded();

			_outlineSettings.SetResources(_outlineResources);
		}

		private void Reset()
		{
			if (_outlineSettings != null)
			{
				_outlineSettings.SetResources(_outlineResources);
			}

			if (_renderers != null)
			{
				_renderers.Reset();
			}
		}

#endif

		#endregion

		#region IOutlineSettingsEx

		/// <summary>
		/// Gets or sets outline settings. Set this to non-<see langword="null"/> value to share settings with other components.
		/// </summary>
		public OutlineSettings OutlineSettings
		{
			get
			{
				if (_outlineSettings == null)
				{
					_outlineSettings = new OutlineSettingsInstance();
				}

				return _outlineSettings.OutlineSettings;
			}
			set
			{
				if (_outlineSettings == null)
				{
					_outlineSettings = new OutlineSettingsInstance();
				}

				_outlineSettings.OutlineSettings = value;
			}
		}

		#endregion

		#region IOutlineSettings

		/// <inheritdoc/>
		public Color OutlineColor
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineColor;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineColor = value;
			}
		}

		/// <inheritdoc/>
		public int OutlineWidth
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineWidth;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineWidth = value;
			}
		}

		/// <inheritdoc/>
		public float OutlineIntensity
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineIntensity;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineIntensity = value;
			}
		}

		/// <inheritdoc/>
		public OutlineMode OutlineMode
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineMode;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineMode = value;
			}
		}

		#endregion

		#region IEquatable

		/// <inheritdoc/>
		public bool Equals(IOutlineSettings other)
		{
			return OutlineSettings.Equals(_outlineSettings, other);
		}

		#endregion

		#region implementation

		private void RemoveDestroyedCameras()
		{
			List<Camera> camerasToRemove = null;

			foreach (var camera in _cameraMap.Keys)
			{
				if (camera == null)
				{
					if (camerasToRemove != null)
					{
						camerasToRemove.Add(camera);
					}
					else
					{
						camerasToRemove = new List<Camera>() { camera };
					}
				}
			}

			if (camerasToRemove != null)
			{
				foreach (var camera in camerasToRemove)
				{
					_cameraMap.Remove(camera);
				}
			}
		}

		private void CreateCommandBufferIfNeeded()
		{
			if (_commandBuffer == null)
			{
				_commandBuffer = new CommandBuffer();
				_commandBuffer.name = string.Format("{0} - {1}", GetType().Name, name);

#if UNITY_EDITOR

				_commandBufferUpdateCounter = 0;

#endif
			}
		}

		private void CreateSettingsIfNeeded()
		{
			if (_outlineSettings == null)
			{
				_outlineSettings = new OutlineSettingsInstance(_outlineResources);
			}
		}

		#endregion
	}
}
