// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	/// <summary>
	/// Post-effect script. Should be attached to camera.
	/// </summary>
	/// <seealso cref="OutlineLayer"/>
	/// <seealso cref="OutlineBehaviour"/>
	/// <seealso cref="https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/"/>
	[RequireComponent(typeof(Camera))]
	public sealed class OutlineEffect : MonoBehaviour
	{
		#region data

		[SerializeField]
		private OutlineResources _outlineResources;

		private IList<OutlineLayer> _layers;
		private OutlineResourceCache _resourceCache;
		private CommandBuffer _commandBuffer;
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

					if (_resourceCache != null)
					{
						_resourceCache.OutlineResources = _outlineResources;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets outline layers.
		/// </summary>
		public IList<OutlineLayer> OutlineLayers
		{
			get
			{
				if (_layers == null)
				{
					_layers = new List<OutlineLayer>();
				}

				return _layers;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("OutlineLayers");
				}

				if (_layers != value)
				{
					_layers = value;
					_changed = true;
				}
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			if (_resourceCache == null)
			{
				_resourceCache = new OutlineResourceCache();
				_resourceCache.OutlineResources = _outlineResources;
			}
		}

		private void OnValidate()
		{
			if (_resourceCache != null)
			{
				_resourceCache.OutlineResources = _outlineResources;
				_changed = true;
			}
		}

		private void OnEnable()
		{
			var camera = GetComponent<Camera>();

			if (camera)
			{
				_commandBuffer = new CommandBuffer();
				_commandBuffer.name = OutlineRenderer.EffectName;
				_changed = true;

				camera.AddCommandBuffer(OutlineRenderer.RenderEvent, _commandBuffer);
			}
		}

		private void OnDisable()
		{
			var camera = GetComponent<Camera>();

			if (camera)
			{
				camera.RemoveCommandBuffer(OutlineRenderer.RenderEvent, _commandBuffer);
			}

			if (_commandBuffer != null)
			{
				_commandBuffer.Dispose();
				_commandBuffer = null;
			}

			_resourceCache.Clear();
		}

		private void Update()
		{
			if (_layers != null)
			{
				if (_changed)
				{
					FillCommandBuffer();
				}
				else
				{
					var needUpdate = false;

					for (var i = 0; i < _layers.Count; ++i)
					{
						if (_layers[i] != null && _layers[i].IsChanged)
						{
							needUpdate = true;
							break;
						}
					}

					if (needUpdate)
					{
						FillCommandBuffer();
					}
				}
			}
		}

		#endregion

		#region implementation

		private void FillCommandBuffer()
		{
			if (_outlineResources && _outlineResources.IsValid)
			{
				using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
				{
					for (var i = 0; i < _layers.Count; ++i)
					{
						if (_layers[i] != null)
						{
							_layers[i].Render(renderer, _resourceCache);
						}
					}
				}

				_changed = false;
			}
			else
			{
				_commandBuffer.Clear();
				_resourceCache.Clear();
			}
		}

		#endregion
	}
}
