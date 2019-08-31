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
	public sealed class OutlineBehaviour : MonoBehaviour, IOutlineSettings
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
		[SerializeField]
		[Range(OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity)]
		private float _outlineIntensity = 2;
		[SerializeField]
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

					_materials = _outlineResources.CreateMaterialSet();
					_materials.Reset(this);
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

		internal void OnWillRenderObjectRt()
		{
			OnWillRenderObject();
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			CreateRenderersIfNeeded();
			CreateMaterialsIfNeeded();
			CreateCommandBufferIfNeeded();
		}

		private void OnDestroy()
		{
			if (_commandBuffer != null)
			{
				_commandBuffer.Dispose();
				_commandBuffer = null;
			}
		}

		private void OnEnable()
		{
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
			CreateMaterialsIfNeeded();
			CreateCommandBufferIfNeeded();

			_changed = true;
		}

		private void Reset()
		{
			_renderers.Reset();
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

					if (_materials != null)
					{
						_materials.OutlineColor = value;
					}
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

					if (_materials != null)
					{
						_materials.OutlineWidth = value;
					}
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

					if (_materials != null)
					{
						_materials.OutlineIntensity = value;
					}
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

					if (_materials != null)
					{
						_materials.OutlineMode = value;
					}
				}
			}
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			if (_materials != null)
			{
				_materials.Reset(this);
			}

			_changed = true;
		}

		#endregion

		#region implementation

		private sealed class RendererCollection : IList<Renderer>
		{
			private readonly List<Renderer> _renderers = new List<Renderer>();
			private readonly OutlineBehaviour _parent;
			private readonly GameObject _go;

			public int Count
			{
				get
				{
					return _renderers.Count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}

			public Renderer this[int index]
			{
				get
				{
					return _renderers[index];
				}
				set
				{
					if (index < 0 || index >= _renderers.Count)
					{
						throw new ArgumentOutOfRangeException("index");
					}

					Validate(value);
					Release(_renderers[index]);
					Init(value);

					_renderers[index] = value;
				}
			}

			public RendererCollection(OutlineBehaviour parent)
			{
				Debug.Assert(parent);

				_parent = parent;
				_go = parent.gameObject;
			}

			public void Reset()
			{
				foreach (var r in _renderers)
				{
					Release(r);
				}

				_renderers.Clear();
				_parent.GetComponentsInChildren(true, _renderers);

				foreach (var r in _renderers)
				{
					Init(r);
				}
			}

			public void Add(Renderer renderer)
			{
				Validate(renderer);
				Init(renderer);

				_renderers.Add(renderer);
			}

			public bool Remove(Renderer renderer)
			{
				if (_renderers.Remove(renderer))
				{
					Release(renderer);
					return true;
				}

				return false;
			}

			public void Clear()
			{
				foreach (var r in _renderers)
				{
					Release(r);
				}

				_renderers.Clear();
			}

			public bool Contains(Renderer renderer)
			{
				return _renderers.Contains(renderer);
			}

			public int IndexOf(Renderer renderer)
			{
				return _renderers.IndexOf(renderer);
			}

			public void Insert(int index, Renderer renderer)
			{
				if (index < 0 || index >= _renderers.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}

				Validate(renderer);
				Init(renderer);

				_renderers.Insert(index, renderer);
			}

			public void RemoveAt(int index)
			{
				if (index >= 0 && index < _renderers.Count)
				{
					Release(_renderers[index]);
					_renderers.RemoveAt(index);
				}
			}

			public void CopyTo(Renderer[] array, int arrayIndex)
			{
				_renderers.CopyTo(array, arrayIndex);
			}

			public IEnumerator<Renderer> GetEnumerator()
			{
				return _renderers.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _renderers.GetEnumerator();
			}

			private void Validate(Renderer renderer)
			{
				if (renderer == null)
				{
					throw new ArgumentNullException("renderer");
				}

				if (!renderer.transform.IsChildOf(_go.transform))
				{
					throw new ArgumentException(string.Format("Only children of the {0} are allowed.", _go.name), "renderer");
				}
			}

			private void Init(Renderer r)
			{
				if (r && r.gameObject != _go)
				{
					var c = r.GetComponent<OutlineBehaviourRt>();

					if (c == null)
					{
						c = r.gameObject.AddComponent<OutlineBehaviourRt>();
					}

					c.Parent = _parent;
				}
			}

			private void Release(Renderer r)
			{
				if (r)
				{
					var c = r.GetComponent<OutlineBehaviourRt>();

					if (c)
					{
						Destroy(c);
					}
				}
			}
		}

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
				_changed = true;
			}
		}

		private void CreateMaterialsIfNeeded()
		{
			if (_outlineResources && (_materials == null || _materials.OutlineResources != _outlineResources))
			{
				_materials = _outlineResources.CreateMaterialSet();
			}

			if (_materials != null)
			{
				_materials.Reset(this);
			}
		}

		private void UpdateCommandBuffer()
		{
			if (_outlineResources != null && _renderers != null && _materials != null)
			{
				using (var renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget))
				{
					renderer.RenderSingleObject(_renderers, _materials);
				}

				_changed = false;
			}
		}

		private void CreateRenderersIfNeeded()
		{
			if (_renderers == null)
			{
				_renderers = new RendererCollection(this);
				_renderers.Reset();
			}
		}

		#endregion
	}
}
