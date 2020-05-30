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
	public sealed class OutlineBehaviour : MonoBehaviour, IOutlineSettings
	{
		#region data

#pragma warning disable 0649

		[SerializeField, Tooltip(OutlineResources.OutlineResourcesTooltip)]
		private OutlineResources _outlineResources;
		[SerializeField, HideInInspector]
		private OutlineSettingsInstance _outlineSettings;
		[SerializeField, HideInInspector]
		private int _layerMask;
		[SerializeField, Tooltip("If set, list of object renderers is updated on each frame. Enable if the object has child renderers which are enabled/disabled frequently.")]
		private bool _updateRenderers;

#pragma warning restore 0649

		private Dictionary<Camera, CommandBuffer> _cameraMap = new Dictionary<Camera, CommandBuffer>();
		private List<Camera> _camerasToRemove = new List<Camera>();
		private OutlineRendererCollection _renderers;

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets resources used by the effect implementation.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if setter argument is <see langword="null"/>.</exception>
		/// <seealso cref="OutlineSettings"/>
		public OutlineResources OutlineResources
		{
			get
			{
				return _outlineResources;
			}
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(OutlineResources));
				}

				_outlineResources = value;
			}
		}

		/// <summary>
		/// Gets or sets outline settings. Set this to non-<see langword="null"/> value to share settings with other components.
		/// </summary>
		/// <seealso cref="OutlineResources"/>
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

		/// <summary>
		/// Gets or sets layer mask to use for ignored <see cref="Renderer"/> components in this game object.
		/// </summary>
		public int IgnoreLayerMask
		{
			get
			{
				return _layerMask;
			}
			set
			{
				if (_layerMask != value)
				{
					_layerMask = value;
					_renderers?.Reset(false, value);
				}
			}
		}

		/// <summary>
		/// Gets outline renderers. By default all child <see cref="Renderer"/> components are used for outlining.
		/// </summary>
		/// <seealso cref="UpdateRenderers"/>
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
		public ICollection<Camera> Cameras => _cameraMap.Keys;

		/// <summary>
		/// Updates renderer list.
		/// </summary>
		/// <seealso cref="OutlineRenderers"/>
		public void UpdateRenderers()
		{
			_renderers?.Reset(false, _layerMask);
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			CreateRenderersIfNeeded();
			CreateSettingsIfNeeded();
		}

		private void OnEnable()
		{
			Camera.onPreRender += OnCameraPreRender;
		}

		private void OnDisable()
		{
			Camera.onPreRender -= OnCameraPreRender;

			foreach (var kvp in _cameraMap)
			{
				if (kvp.Key)
				{
					kvp.Key.RemoveCommandBuffer(OutlineRenderer.RenderEvent, kvp.Value);
				}

				kvp.Value.Dispose();
			}

			_cameraMap.Clear();
		}

		private void Update()
		{
			if (_outlineResources != null && _renderers != null)
			{
				_camerasToRemove.Clear();

				if (_updateRenderers)
				{
					_renderers.Reset(false, _layerMask);
				}

				foreach (var kvp in _cameraMap)
				{
					var camera = kvp.Key;
					var cmdBuffer = kvp.Value;

					if (camera)
					{
						cmdBuffer.Clear();

						if (_renderers.Count > 0)
						{
							using (var renderer = new OutlineRenderer(cmdBuffer, _outlineResources, camera.actualRenderingPath))
							{
								renderer.Render(_renderers.GetList(), _outlineSettings, name);
							}
						}
					}
					else
					{
						cmdBuffer.Dispose();
						_camerasToRemove.Add(camera);
					}
				}

				foreach (var camera in _camerasToRemove)
				{
					_cameraMap.Remove(camera);
				}
			}
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			CreateRenderersIfNeeded();
			CreateSettingsIfNeeded();
		}

		private void Reset()
		{
			if (_renderers != null)
			{
				_renderers.Reset(false, _layerMask);
			}
		}

#endif

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
		public OutlineRenderFlags OutlineRenderMode
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineRenderMode;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineRenderMode = value;
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

		private void OnCameraPreRender(Camera camera)
		{
			if (camera)
			{
				if (_outlineSettings.RequiresCameraDepth)
				{
					camera.depthTextureMode |= DepthTextureMode.Depth;
				}

				if (!_cameraMap.ContainsKey(camera))
				{
					var cmdBuf = new CommandBuffer();
					cmdBuf.name = string.Format("{0} - {1}", GetType().Name, name);
					camera.AddCommandBuffer(OutlineRenderer.RenderEvent, cmdBuf);

					_cameraMap.Add(camera, cmdBuf);
				}
			}
		}

		private void CreateSettingsIfNeeded()
		{
			if (_outlineSettings == null)
			{
				_outlineSettings = new OutlineSettingsInstance();
			}
		}

		private void CreateRenderersIfNeeded()
		{
			if (_renderers == null)
			{
				_renderers = new OutlineRendererCollection(gameObject);
				_renderers.Reset(false, _layerMask);
			}
		}

		#endregion
	}
}
