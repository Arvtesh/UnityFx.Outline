// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
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
	/// <seealso href="https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/"/>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public sealed partial class OutlineEffect : MonoBehaviour
	{
		#region data

		[SerializeField, Tooltip(OutlineResources.OutlineResourcesTooltip)]
		private OutlineResources _outlineResources;
		[SerializeField, Tooltip(OutlineResources.OutlineLayerCollectionTooltip)]
		private OutlineLayerCollection _outlineLayers;
		[SerializeField, HideInInspector]
		private CameraEvent _cameraEvent = OutlineRenderer.RenderEvent;

		private Camera _camera;
		private CommandBuffer _commandBuffer;
		private List<OutlineRenderObject> _renderObjects = new List<OutlineRenderObject>(16);

		#endregion

		#region interface

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
				if (value is null)
				{
					throw new ArgumentNullException(nameof(OutlineResources));
				}

				_outlineResources = value;
			}
		}

		/// <summary>
		/// Gets collection of outline layers.
		/// </summary>
		public OutlineLayerCollection OutlineLayers
		{
			get
			{
				return _outlineLayers;
			}
			set
			{
				_outlineLayers = value;
			}
		}

		/// <summary>
		/// Gets outline layers (for internal use only).
		/// </summary>
		internal OutlineLayerCollection OutlineLayersInternal => _outlineLayers;

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
		/// Adds the <see cref="GameObject"/> passed to the first outline layer. Creates the layer if needed.
		/// </summary>
		/// <param name="go">The <see cref="GameObject"/> to add and render outline for.</param>
		/// <seealso cref="AddGameObject(GameObject, int)"/>
		public void AddGameObject(GameObject go)
		{
			AddGameObject(go, 0);
		}

		/// <summary>
		/// Adds the <see cref="GameObject"/> passed to the specified outline layer. Creates the layer if needed.
		/// </summary>
		/// <param name="go">The <see cref="GameObject"/> to add and render outline for.</param>
		/// <seealso cref="AddGameObject(GameObject)"/>
		public void AddGameObject(GameObject go, int layerIndex)
		{
			if (layerIndex < 0)
			{
				throw new ArgumentOutOfRangeException("layerIndex");
			}

			CreateLayersIfNeeded();

			while (_outlineLayers.Count <= layerIndex)
			{
				_outlineLayers.Add(new OutlineLayer());
			}

			_outlineLayers[layerIndex].Add(go);
		}

		/// <summary>
		/// Removes the specified <see cref="GameObject"/> from <see cref="OutlineLayers"/>.
		/// </summary>
		/// <param name="go">A <see cref="GameObject"/> to remove.</param>
		public void RemoveGameObject(GameObject go)
		{
			if (_outlineLayers)
			{
				_outlineLayers.Remove(go);
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			OutlineResources.LogSrpNotSupported(this);
			OutlineResources.LogPpNotSupported(this);
		}

		private void OnEnable()
		{
			InitCameraAndCommandBuffer();
		}

		private void OnDisable()
		{
			ReleaseCameraAndCommandBuffer();
		}

		private void OnPreRender()
		{
			FillCommandBuffer();
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

		//private void OnValidate()
		//{
		//	InitCameraAndCommandBuffer();
		//	FillCommandBuffer();
		//}

		private void Reset()
		{
			_outlineLayers = null;
		}

#endif

		#endregion

		#region implementation

		private void InitCameraAndCommandBuffer()
		{
			_camera = GetComponent<Camera>();

			if (_camera && _commandBuffer is null)
			{
				_commandBuffer = new CommandBuffer
				{
					name = string.Format("{0} - {1}", GetType().Name, name)
				};

				_camera.depthTextureMode |= DepthTextureMode.Depth;
				_camera.AddCommandBuffer(_cameraEvent, _commandBuffer);
			}
		}

		private void ReleaseCameraAndCommandBuffer()
		{
			if (_commandBuffer != null)
			{
				if (_camera)
				{
					_camera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
				}

				_commandBuffer.Dispose();
				_commandBuffer = null;
			}

			_camera = null;
		}

		private void FillCommandBuffer()
		{
			if (_camera && _outlineLayers && _commandBuffer != null)
			{
				_commandBuffer.Clear();

				if (_outlineResources && _outlineResources.IsValid)
				{
					using (var renderer = new OutlineRenderer(_commandBuffer, _outlineResources, _camera.actualRenderingPath))
					{
						_renderObjects.Clear();
						_outlineLayers.GetRenderObjects(_renderObjects);

						foreach (var renderObject in _renderObjects)
						{
							renderer.Render(renderObject);
						}
					}
				}
			}
		}

		private void CreateLayersIfNeeded()
		{
			if (_outlineLayers is null)
			{
				_outlineLayers = ScriptableObject.CreateInstance<OutlineLayerCollection>();
				_outlineLayers.name = "OutlineLayers";
			}
		}

		#endregion
	}
}
