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
		public static void Render(IOutlineSettingsEx settings, UnityEngine.Object undoContext)
		{
			var obj = (OutlineSettings)EditorGUILayout.ObjectField("Outline Settings", settings.OutlineSettings, typeof(OutlineSettings), true);

			if (settings.OutlineSettings != obj)
			{
				Undo.RecordObject(undoContext, "Settings");
				settings.OutlineSettings = obj;
			}

			if (obj)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.indentLevel += 1;

				Render((IOutlineSettings)settings, undoContext);

				EditorGUILayout.HelpBox(string.Format("Settings are overriden with values from {0}.", obj.name), MessageType.Info, true);
				EditorGUI.indentLevel -= 1;
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				EditorGUI.indentLevel += 1;

				Render((IOutlineSettings)settings, undoContext);

				EditorGUI.indentLevel -= 1;
			}
		}

		public static void Render(IOutlineSettings settings, UnityEngine.Object undoContext)
		{
			var color = EditorGUILayout.ColorField("Color", settings.OutlineColor);

			if (settings.OutlineColor != color)
			{
				Undo.RecordObject(undoContext, "Color");
				settings.OutlineColor = color;
			}

			var width = EditorGUILayout.IntSlider("Width", settings.OutlineWidth, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);

			if (settings.OutlineWidth != width)
			{
				Undo.RecordObject(undoContext, "Width");
				settings.OutlineWidth = width;
			}

			var blurred = EditorGUILayout.Toggle("Blurred", settings.OutlineMode == OutlineMode.Blurred);

			if (blurred)
			{
				EditorGUI.indentLevel += 1;

				var i = EditorGUILayout.Slider("Blur Intensity", settings.OutlineIntensity, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);

				if (!Mathf.Approximately(settings.OutlineIntensity, i))
				{
					Undo.RecordObject(undoContext, "Blur Intensity");
					settings.OutlineIntensity = i;
				}

				EditorGUI.indentLevel -= 1;
			}

			if (blurred != (settings.OutlineMode == OutlineMode.Blurred))
			{
				Undo.RecordObject(undoContext, "Blur");
				settings.OutlineMode = blurred ? OutlineMode.Blurred : OutlineMode.Solid;
			}
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
