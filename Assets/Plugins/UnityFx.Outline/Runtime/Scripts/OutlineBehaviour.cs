// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
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
	public sealed class OutlineBehaviour : MonoBehaviour
	{
		#region data

#pragma warning disable 0649

		[SerializeField]
		private OutlineResources _outlineResources;
		[SerializeField]
		private Color _outlineColor = Color.green;
		[SerializeField]
		[Range(OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth)]
		private int _outlineWidth = 5;

#pragma warning restore 0649

		private Material _renderMaterial;
		private Material _postProcessMaterial;
		private Renderer[] _renderers;
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

					if (_renderMaterial)
					{
						_renderMaterial.shader = value.RenderShader;
					}

					if (_postProcessMaterial)
					{
						_postProcessMaterial.shader = value.PostProcessShader;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets outline color for the layer.
		/// </summary>
		/// <seealso cref="OutlineWidth"/>
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

					if (_postProcessMaterial)
					{
						_postProcessMaterial.SetColor(OutlineRenderer.ColorParamName, value);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets outline width in pixels. Only positive values are allowed.
		/// </summary>
		/// <seealso cref="OutlineColor"/>
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

					if (_postProcessMaterial)
					{
						_postProcessMaterial.SetInt(OutlineRenderer.WidthParamName, value);
					}
				}
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			if (_renderers == null)
			{
				_renderers = GetComponentsInChildren<Renderer>();
				_changed = true;
			}
		}

		private void OnEnable()
		{
			if (_commandBuffer == null)
			{
				_commandBuffer = new CommandBuffer();
				_commandBuffer.name = OutlineRenderer.EffectName;
				_changed = true;
			}
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

			if (_commandBuffer != null)
			{
				_commandBuffer.Dispose();
				_commandBuffer = null;
			}

			_cameraMap.Clear();
		}

		private void OnValidate()
		{
			if (_commandBuffer == null)
			{
				_commandBuffer = new CommandBuffer();
				_commandBuffer.name = OutlineRenderer.EffectName;
				_changed = true;
			}

			if (_renderers == null)
			{
				_renderers = GetComponentsInChildren<Renderer>();
				_changed = true;
			}

			if (_postProcessMaterial)
			{
				_postProcessMaterial.SetInt(OutlineRenderer.WidthParamName, _outlineWidth);
				_postProcessMaterial.SetColor(OutlineRenderer.ColorParamName, _outlineColor);
				_changed = true;
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

		private void UpdateCommandBuffer()
		{
			if (_outlineResources != null && _renderers != null)
			{
				if (_renderMaterial == null && _outlineResources.RenderShader)
				{
					_renderMaterial = new Material(_outlineResources.RenderShader);
				}

				if (_postProcessMaterial == null && _outlineResources.PostProcessShader)
				{
					_postProcessMaterial = new Material(_outlineResources.PostProcessShader);
					_postProcessMaterial.SetColor(OutlineRenderer.ColorParamName, _outlineColor);
					_postProcessMaterial.SetInt(OutlineRenderer.WidthParamName, _outlineWidth);
				}

				if (_renderMaterial && _postProcessMaterial)
				{
					using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
					{
						renderer.RenderSingleObject(_renderers, _renderMaterial, _postProcessMaterial);
					}

					_changed = false;
				}
			}
		}

		#endregion
	}
}
