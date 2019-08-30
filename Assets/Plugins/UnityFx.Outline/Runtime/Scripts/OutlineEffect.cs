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
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public sealed class OutlineEffect : MonoBehaviour
	{
		#region data

		[SerializeField]
		private OutlineResources _outlineResources;
		[SerializeField]
		private OutlineLayerCollection _outlineLayers;

		private IList<OutlineLayer> _layers;
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
				}
			}
		}

		/// <summary>
		/// Gets outline layers.
		/// </summary>
		/// <seealso cref="ShareLayersWith(OutlineEffect)"/>
		public IList<OutlineLayer> OutlineLayers
		{
			get
			{
				if (_layers == null)
				{
					if (_outlineLayers)
					{
						_layers = _outlineLayers.Layers;
					}
					else
					{
						_layers = new List<OutlineLayer>();
					}
				}

				return _layers;
			}
		}

		/// <summary>
		/// Shares <see cref="OutlineLayers"/> with another <see cref="OutlineEffect"/> instace.
		/// </summary>
		/// <param name="other">Effect to share <see cref="OutlineLayers"/> with.</param>
		/// <seealso cref="OutlineLayers"/>
		public void ShareLayersWith(OutlineEffect other)
		{
			if (other)
			{
				other._layers = OutlineLayers;
				other._outlineLayers = _outlineLayers;
				other._changed = true;
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			if (_outlineLayers)
			{
				_layers = _outlineLayers.Layers;
			}
		}

		private void OnValidate()
		{
			if (_outlineLayers)
			{
				_layers = _outlineLayers.Layers;
			}
			else
			{
				_layers = null;
			}

			_changed = true;
		}

		private void Reset()
		{
			_outlineLayers = null;
			_layers = null;
			_changed = true;
		}

		private void OnEnable()
		{
			var camera = GetComponent<Camera>();

			if (camera)
			{
				_commandBuffer = new CommandBuffer();
				_commandBuffer.name = string.Format("{0} - {1}", GetType().Name, name);
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
							_layers[i].Render(renderer, _outlineResources);
						}
					}
				}

				_changed = false;
			}
			else
			{
				_commandBuffer.Clear();
			}
		}

		#endregion
	}
}
