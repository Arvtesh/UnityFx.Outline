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
					EditorGUILayout.Space();
					var rect = EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Layer #" + i.ToString());

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Remove", _layerButtonStyle))
					{
						removeLayer = i;
					}

					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();

					rect.xMin -= 2;
					rect.xMax += 2;
					rect.yMin -= 2;
					rect.yMax += 2;

					GUI.Box(rect, GUIContent.none);

					OutlineEditorUtility.Render(_layers[i], _layers);
				}
			}
			else
			{
				EditorGUILayout.HelpBox("The layer collection is empty.", MessageType.Info, true);
			}

			// Add/remove processing.
			OutlineEditorUtility.RenderDivider(Color.gray);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Add New", _layerButtonStyle))
			{
				Undo.RecordObject(_layers, "Add Layer");
				_layers.Add(new OutlineLayer());
			}

			if (GUILayout.Button("Remove All", _layerButtonStyle))
			{
				Undo.RecordObject(_layers, "Remove All Layers");
				_layers.Clear();
			}

			if (removeLayer >= 0)
			{
				Undo.RecordObject(_layers, "Remove Layer");
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
