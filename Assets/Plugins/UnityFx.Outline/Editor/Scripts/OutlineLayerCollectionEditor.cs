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
		private readonly GUILayoutOption _layerButtonStyle = GUILayout.ExpandWidth(false);
		private OutlineLayerCollection _layers;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();

			var removeLayer = -1;

			// 1) Layers list.
			if (_layers.Count > 0)
			{
				for (var i = 0; i < _layers.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Layer #" + i.ToString());

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Remove", _layerButtonStyle))
					{
						removeLayer = i;
					}

					EditorGUILayout.EndHorizontal();

					EditorGUI.indentLevel += 1;
					OutlineEditorUtility.Render(_layers[i]);
					EditorGUI.indentLevel -= 1;
				}
			}

			// Add/remove processing.
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Add New", _layerButtonStyle))
			{
				_layers.Add(new OutlineLayer());
			}
			else if (removeLayer >= 0)
			{
				_layers.RemoveAt(removeLayer);
			}

			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_layers);
			}
		}

		private void OnEnable()
		{
			_layers = (OutlineLayerCollection)target;
		}
	}
}
