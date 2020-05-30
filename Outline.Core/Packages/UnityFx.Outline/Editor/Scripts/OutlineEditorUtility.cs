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
		public static void RenderDivider(Color color, int thickness = 1, int padding = 5)
		{
			var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));

			r.height = thickness;
			r.y += padding / 2;
			r.x -= 2;

			EditorGUI.DrawRect(r, color);
		}

		public static void Render(IOutlineSettings settings, UnityEngine.Object undoContext)
		{
			var color = EditorGUILayout.ColorField("Color", settings.OutlineColor);

			if (settings.OutlineColor != color)
			{
				Undo.RecordObject(undoContext, "Color");
				settings.OutlineColor = color;
			}

			var width = EditorGUILayout.IntSlider("Width", settings.OutlineWidth, OutlineResources.MinWidth, OutlineResources.MaxWidth);

			if (settings.OutlineWidth != width)
			{
				Undo.RecordObject(undoContext, "Width");
				settings.OutlineWidth = width;
			}

			var prevRenderMode = settings.OutlineRenderMode;
			var renderMode = (OutlineRenderFlags)EditorGUILayout.EnumFlagsField("Render Flags", prevRenderMode);

			if (renderMode != prevRenderMode)
			{
				Undo.RecordObject(undoContext, "Render Flags");
				settings.OutlineRenderMode = renderMode;
			}

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				var i = EditorGUILayout.Slider("Blur Intensity", settings.OutlineIntensity, OutlineResources.MinIntensity, OutlineResources.MaxIntensity);

				if (!Mathf.Approximately(settings.OutlineIntensity, i))
				{
					Undo.RecordObject(undoContext, "Blur Intensity");
					settings.OutlineIntensity = i;
				}
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
