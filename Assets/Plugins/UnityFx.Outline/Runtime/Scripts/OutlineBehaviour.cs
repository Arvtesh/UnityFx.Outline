// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
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
	public sealed partial class OutlineBehaviour : MonoBehaviour, IOutlineSettings
	{
		#region data

#pragma warning disable 0649

		[SerializeField]
		private OutlineResources _outlineResources;

		// NOTE: There is a custom editor for OutlineSettings, so no need to show these in default inspector.
		[SerializeField, HideInInspector]
		private OutlineSettings _outlineSettings;
		[SerializeField, HideInInspector]
		private Color _outlineColor = Color.red;
		[SerializeField, HideInInspector]
		private int _outlineWidth = 4;
		[SerializeField, HideInInspector]
		private float _outlineIntensity = 2;
		[SerializeField, HideInInspector]
		private OutlineMode _outlineMode;

#pragma warning restore 0649

		private OutlineMaterialSet _materials;
		private RendererCollection _renderers;
		private CommandBuffer _commandBuffer;

		private Dictionary<Camera, CommandBuffer> _cameraMap = new Dictionary<Camera, CommandBuffer>();
		private float _cameraMapUpdateTimer;
		private bool _changed;

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets resources used by the effect implementation.
		/// </summary>
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
		/// Gets or sets outline settings.
		/// </summary>
		public OutlineSettings OutlineSettings
		{
			get
			{
				return _outlineSettings;
			}
			set
			{
				if (_outlineSettings != value)
				{
					if (_outlineSettings != null)
					{
						_outlineSettings.Changed -= OnSettingsChanged;
					}

					_outlineSettings = value;
					_changed = true;

					ResetOutlineSettings();
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

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			CreateRenderersIfNeeded();
			ResetOutlineSettings();

			_changed = true;
		}

		private void OnDestroy()
		{
			if (_outlineSettings != null)
			{
				_outlineSettings.Changed -= OnSettingsChanged;
			}

			if (_renderers != null)
			{
				_renderers.Clear();
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

			if (_changed)
			{
				UpdateCommandBuffer();
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

			if (_outlineSettings != null)
			{
				_outlineColor = _outlineSettings.OutlineColor;
				_outlineWidth = _outlineSettings.OutlineWidth;
				_outlineIntensity = _outlineSettings.OutlineIntensity;
				_outlineMode = _outlineSettings.OutlineMode;
				_changed = true;
			}

			_changed = true;
		}

		private void Reset()
		{
			if (_outlineSettings != null)
			{
				_outlineSettings.Changed -= OnSettingsChanged;
			}

			_renderers.Reset();
			_changed = true;
		}

#endif

		#endregion

		#region IOutlineSettings

		/// <inheritdoc/>
		public Color OutlineColor
		{
			get
			{
				return _outlineColor;
			}
			set
			{
				if (_outlineColor != value)
				{
					_outlineColor = value;
					_changed = true;
				}
			}
		}

		/// <inheritdoc/>
		public int OutlineWidth
		{
			get
			{
				return _outlineWidth;
			}
			set
			{
				value = Mathf.Clamp(value, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);

				if (_outlineWidth != value)
				{
					_outlineWidth = value;
					_changed = true;
				}
			}
		}

		/// <inheritdoc/>
		public float OutlineIntensity
		{
			get
			{
				return _outlineIntensity;
			}
			set
			{
				value = Mathf.Clamp(value, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);

				if (_outlineIntensity != value)
				{
					_outlineIntensity = value;
					_changed = true;
				}
			}
		}

		/// <inheritdoc/>
		public OutlineMode OutlineMode
		{
			get
			{
				return _outlineMode;
			}
			set
			{
				if (_outlineMode != value)
				{
					_outlineMode = value;
					_changed = true;
				}
			}
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
			}
		}

		private void ResetOutlineSettings()
		{
			if (_outlineSettings != null)
			{
				_outlineSettings.Changed += OnSettingsChanged;
				_outlineColor = _outlineSettings.OutlineColor;
				_outlineWidth = _outlineSettings.OutlineWidth;
				_outlineIntensity = _outlineSettings.OutlineIntensity;
				_outlineMode = _outlineSettings.OutlineMode;
			}
		}

		private void UpdateCommandBuffer()
		{
			if (_outlineResources != null && _renderers != null)
			{
				if (_materials == null || _materials.OutlineResources != _outlineResources)
				{
					_materials = _outlineResources.CreateMaterialSet();
				}

				_materials.Reset(this);

				using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
				{
					renderer.RenderSingleObject(_renderers, _materials);
				}

				_changed = false;
			}
		}

		private void OnSettingsChanged(object sender, EventArgs e)
		{
			if (_outlineSettings != null)
			{
				_outlineColor = _outlineSettings.OutlineColor;
				_outlineWidth = _outlineSettings.OutlineWidth;
				_outlineIntensity = _outlineSettings.OutlineIntensity;
				_outlineMode = _outlineSettings.OutlineMode;
				_changed = true;
			}
		}

		#endregion
	}
}
