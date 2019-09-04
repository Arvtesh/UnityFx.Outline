// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
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

		private EventHandler _changedDelegate;
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
				if (_outlineLayers == null)
				{
					_outlineLayers = ScriptableObject.CreateInstance<OutlineLayerCollection>();
				}

				return _outlineLayers;
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
				if (_outlineLayers == null)
				{
					_outlineLayers = ScriptableObject.CreateInstance<OutlineLayerCollection>();
				}

				other._outlineLayers = _outlineLayers;
				other._changed = true;
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
#if UNITY_EDITOR

			if (_outlineLayers)
			{
				_outlineLayers.UpdateChanged();
			}

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
				using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
				{
					for (var i = 0; i < _outlineLayers.Count; ++i)
					{
						if (_outlineLayers[i] != null)
						{
							_outlineLayers[i].Render(renderer, _outlineResources);
						}
					}
				}
			}
			else
			{
				_commandBuffer.Clear();
			}

			_changed = false;
		}

		private void OnChanged(object sender, EventArgs args)
		{
			_changed = true;
		}

		#endregion
	}
}
