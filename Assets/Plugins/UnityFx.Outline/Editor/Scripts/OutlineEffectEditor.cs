// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.Outline
{
	[CustomEditor(typeof(OutlineEffect))]
	public class OutlineEffectEditor : Editor
	{
		private OutlineEffect _effect;
		private bool _previewOpened;

		private void OnEnable()
		{
			_effect = (OutlineEffect)target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (_effect.OutlineLayers.Count > 0)
			{
				_previewOpened = EditorGUILayout.Foldout(_previewOpened, "Preview", true);

				if (_previewOpened)
				{
					OutlineEditorUtility.RenderPreview(_effect.OutlineLayers, true);
				}
			}
		}
	}
}
