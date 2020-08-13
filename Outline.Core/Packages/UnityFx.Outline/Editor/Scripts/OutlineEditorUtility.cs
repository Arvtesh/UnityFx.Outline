// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.Outline
{
	public static class OutlineEditorUtility
	{
		public static void RenderPreview(OutlineLayer layer, int layerIndex, bool showObjects)
		{
			if (layer != null)
			{
				var goIndex = 1;

				EditorGUILayout.BeginHorizontal();
				EditorGUI.indentLevel += 1;
				EditorGUILayout.PrefixLabel("Layer #" + layerIndex.ToString());
				EditorGUI.indentLevel -= 1;

				if (layer.Enabled)
				{
					EditorGUILayout.LabelField(layer.OutlineRenderMode == OutlineRenderFlags.None ? layer.OutlineRenderMode.ToString() : string.Format("Blurred ({0})", layer.OutlineIntensity), GUILayout.MaxWidth(70));
					EditorGUILayout.IntField(layer.OutlineWidth, GUILayout.MaxWidth(100));
					EditorGUILayout.ColorField(layer.OutlineColor, GUILayout.MinWidth(100));
				}
				else
				{
					EditorGUILayout.LabelField("Disabled.");
				}

				EditorGUILayout.EndHorizontal();

				if (showObjects)
				{
					if (layer.Count > 0)
					{
						foreach (var go in layer)
						{
							EditorGUI.indentLevel += 2;
							EditorGUILayout.ObjectField("#" + goIndex.ToString(), go, typeof(GameObject), true);
							EditorGUI.indentLevel -= 2;

							goIndex++;
						}
					}
					else
					{
						EditorGUI.indentLevel += 2;
						EditorGUILayout.LabelField("No objects.");
						EditorGUI.indentLevel -= 2;
					}
				}
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUI.indentLevel += 1;
				EditorGUILayout.PrefixLabel("Layer #" + layerIndex.ToString());
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.LabelField("Null");
				EditorGUILayout.EndHorizontal();
			}
		}

		public static void RenderPreview(IList<OutlineLayer> layers, bool showObjects)
		{
			EditorGUI.BeginDisabledGroup(true);

			if (layers.Count > 0)
			{
				for (var i = 0; i < layers.Count; ++i)
				{
					RenderPreview(layers[i], i, showObjects);
				}
			}
			else
			{
				EditorGUI.indentLevel += 1;
				EditorGUILayout.LabelField("No layers.");
				EditorGUI.indentLevel -= 1;
			}

			EditorGUI.EndDisabledGroup();
		}
	}
}
