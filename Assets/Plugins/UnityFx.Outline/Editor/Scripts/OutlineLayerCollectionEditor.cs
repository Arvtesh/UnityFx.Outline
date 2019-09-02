// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.Outline
{
	[CustomEditor(typeof(OutlineLayerCollection))]
	public class OutlineLayerCollectionEditor : Editor
	{
		private OutlineLayerCollection _layers;
		private bool _previewOpened;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (_layers.Count > 0)
			{
				_previewOpened = EditorGUILayout.Foldout(_previewOpened, "Layers Preview", true);

				if (_previewOpened)
				{
					OutlineEditorUtility.RenderPreview(_layers, false);
				}
			}
		}

		private void OnEnable()
		{
			_layers = (OutlineLayerCollection)target;
		}
	}
}
