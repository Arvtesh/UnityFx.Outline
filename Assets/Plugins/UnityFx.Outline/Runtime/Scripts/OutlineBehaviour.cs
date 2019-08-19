// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Attach this script to a <see cref="GameObject"/> to add outline effect.
	/// </summary>
	/// <seealso cref="OutlineEffect"/>
	public class OutlineBehaviour : MonoBehaviour
	{
		#region data

#pragma warning disable 0649

		[SerializeField]
		private OutlineResources _outlineResources;
		[SerializeField]
		private Color _outlineColor = Color.green;
		[SerializeField]
		private int _outlineWidth = 5;

#pragma warning restore 0649

		private Material _renderMaterial;
		private Material _postProcessMaterial;
		private Renderer[] _renderers;
		private CommandBuffer _commandBuffer;

		private Dictionary<Camera, CommandBuffer> _cameraMap = new Dictionary<Camera, CommandBuffer>();
		private bool _changed = true;

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
					throw new ArgumentNullException("Resources");
				}

				if (_outlineResources != value)
				{
					_outlineResources = value;
					_renderMaterial.shader = value.RenderShader;
					_postProcessMaterial.shader = value.PostProcessShader;
					_changed = true;
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
					_postProcessMaterial.SetColor(OutlineRenderer.ColorParamName, value);
					_changed = true;
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
					_postProcessMaterial.SetInt(OutlineRenderer.WidthParamName, value);
					_changed = true;
				}
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			_renderMaterial = new Material(_outlineResources.RenderShader);

			_postProcessMaterial = new Material(_outlineResources.PostProcessShader);
			_postProcessMaterial.SetColor(OutlineRenderer.ColorParamName, _outlineColor);
			_postProcessMaterial.SetInt(OutlineRenderer.WidthParamName, _outlineWidth);

			_renderers = GetComponentsInChildren<Renderer>();
		}

		private void OnEnable()
		{
			_commandBuffer = new CommandBuffer();
			_commandBuffer.name = OutlineRenderer.EffectName;
			_changed = true;
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

		private void Update()
		{
			if (_changed)
			{
				FillCommandBuffer();
				_changed = false;
			}
		}

		private void OnWillRenderObject()
		{
			if (gameObject.activeInHierarchy && isActiveAndEnabled)
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

		private void FillCommandBuffer()
		{
			using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
			{
				renderer.RenderSingleObject(_renderers, _renderMaterial, _postProcessMaterial);
			}
		}

		#endregion
	}
}
