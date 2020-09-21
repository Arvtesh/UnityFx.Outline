// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityFx.Outline
{
	[CustomEditor(typeof(OutlineBehaviour))]
	public class OutlineBehaviourEditor : Editor
	{
		private OutlineBehaviour _effect;
		private SerializedProperty _settingsProp;
		private bool _debugOpened;
		private bool _renderersOpened;
		private bool _camerasOpened;

		private void OnEnable()
		{
			_effect = (OutlineBehaviour)target;
			_settingsProp = serializedObject.FindProperty("_outlineSettings");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			// 1) Outline settings.
			EditorGUI.BeginChangeCheck();

			var mask = EditorGUILayout.MaskField("Ignore layers", _effect.IgnoreLayerMask, InternalEditorUtility.layers);

			if (_effect.IgnoreLayerMask != mask)
			{
				Undo.RecordObject(_effect, "Set Ignore Layers");
				_effect.IgnoreLayerMask = mask;
			}

			var e = (CameraEvent)EditorGUILayout.EnumPopup("Render Event", _effect.RenderEvent);

			if (e != _effect.RenderEvent)
			{
				Undo.RecordObject(_effect, "Set Render Event");
				_effect.RenderEvent = e;
			}

			var c = (Camera)EditorGUILayout.ObjectField("Target Camera", _effect.Camera, typeof(Camera), true);

			if (c != _effect.Camera)
			{
				Undo.RecordObject(_effect, "Set Target Camera");
				_effect.Camera = c;
			}

			EditorGUILayout.PropertyField(_settingsProp);
			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_effect.gameObject);

				if (!EditorApplication.isPlayingOrWillChangePlaymode)
				{
					EditorSceneManager.MarkSceneDirty(_effect.gameObject.scene);
				}
			}

			// 2) Renderers (read-only).
			_renderersOpened = EditorGUILayout.Foldout(_renderersOpened, "Renderers", true);

			if (_renderersOpened)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.indentLevel += 1;

				var rendererNumber = 1;

				foreach (var renderer in _effect.OutlineRenderers)
				{
					EditorGUILayout.ObjectField("#" + rendererNumber.ToString(), renderer, typeof(Renderer), true);
					rendererNumber++;
				}

				EditorGUI.indentLevel -= 1;
				EditorGUI.EndDisabledGroup();
			}

			// 3) Cameras (read-only).
			_camerasOpened = EditorGUILayout.Foldout(_camerasOpened, "Cameras", true);

			if (_camerasOpened)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.indentLevel += 1;

				var cameraNumber = 1;

				foreach (var camera in _effect.Cameras)
				{
					EditorGUILayout.ObjectField("#" + cameraNumber.ToString(), camera, typeof(Camera), true);
					cameraNumber++;
				}

				EditorGUI.indentLevel -= 1;
				EditorGUI.EndDisabledGroup();
			}
		}
	}
}
