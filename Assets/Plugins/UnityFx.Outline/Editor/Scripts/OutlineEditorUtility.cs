// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.Outline
{
	internal static class OutlineEditorUtility
	{
		public static void Render(IOutlineSettingsEx settings)
		{
			settings.OutlineSettings = (OutlineSettings)EditorGUILayout.ObjectField("Outline Settings", settings.OutlineSettings, typeof(OutlineSettings), true);

			if (settings.OutlineSettings)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.indentLevel += 1;

				Render((IOutlineSettings)settings);

				EditorGUILayout.HelpBox(string.Format("Settings are overriden with values from {0}.", settings.OutlineSettings.name), MessageType.Info, true);
				EditorGUI.indentLevel -= 1;
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				EditorGUI.indentLevel += 1;

				Render((IOutlineSettings)settings);

				EditorGUI.indentLevel -= 1;
			}
		}

		public static void Render(IOutlineSettings settings)
		{
			settings.OutlineColor = EditorGUILayout.ColorField("Color", settings.OutlineColor);
			settings.OutlineWidth = EditorGUILayout.IntSlider("Width", settings.OutlineWidth, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);

			var blurred = EditorGUILayout.Toggle("Blurred", settings.OutlineMode == OutlineMode.Blurred);

			if (blurred)
			{
				EditorGUI.indentLevel += 1;
				settings.OutlineIntensity = EditorGUILayout.Slider("Blur Intensity", settings.OutlineIntensity, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);
				EditorGUI.indentLevel -= 1;
			}

			settings.OutlineMode = blurred ? OutlineMode.Blurred : OutlineMode.Solid;
		}

		public static void RenderPreview(OutlineLayer layer, int layerIndex, bool showObjects)
		{
			if (layer != null)
			{
				var goIndex = 1;

				EditorGUILayout.BeginHorizontal();
				EditorGUI.indentLevel += 1;
				EditorGUILayout.PrefixLabel("Layer #" + layerIndex.ToString());
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.IntField(layer.OutlineWidth, GUILayout.MaxWidth(100));
				EditorGUILayout.ColorField(layer.OutlineColor, GUILayout.MinWidth(100));
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
