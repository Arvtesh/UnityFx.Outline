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

		public static void RenderOutlineSettings(IOutlineSettings settings, bool displayHeader)
		{
			if (displayHeader)
			{
				EditorGUILayout.LabelField("Outline Settings");
				EditorGUI.indentLevel += 1;
			}

			settings.OutlineColor = EditorGUILayout.ColorField("Color", settings.OutlineColor);
			settings.OutlineWidth = EditorGUILayout.IntSlider("Width", settings.OutlineWidth, OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);

			var blurred = EditorGUILayout.Toggle("Blured", settings.OutlineMode == OutlineMode.Blurred);

			if (blurred)
			{
				EditorGUI.indentLevel += 1;
				settings.OutlineIntensity = EditorGUILayout.Slider("Blur Intensity", settings.OutlineIntensity, OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);
				EditorGUI.indentLevel -= 1;
			}

			settings.OutlineMode = blurred ? OutlineMode.Blurred : OutlineMode.Solid;

			if (displayHeader)
			{
				EditorGUI.indentLevel -= 1;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (_effect.OutlineLayers.Count > 0)
			{
				_previewOpened = EditorGUILayout.Foldout(_previewOpened, "Preview", true);

				if (_previewOpened)
				{
					EditorGUI.BeginDisabledGroup(true);

					if (_effect.OutlineLayers.Count > 0)
					{
						for (var i = 0; i < _effect.OutlineLayers.Count; ++i)
						{
							var layer = _effect.OutlineLayers[i];
							var goIndex = 1;

							EditorGUILayout.BeginHorizontal();
							EditorGUI.indentLevel = 1;
							EditorGUILayout.PrefixLabel("Layer #" + i.ToString());
							EditorGUI.indentLevel = 0;
							EditorGUILayout.IntField(layer.OutlineWidth, GUILayout.MaxWidth(100));
							EditorGUILayout.ColorField(layer.OutlineColor, GUILayout.MinWidth(100));
							EditorGUILayout.EndHorizontal();

							if (layer.Count > 0)
							{
								foreach (var go in layer)
								{
									EditorGUI.indentLevel = 2;
									EditorGUILayout.ObjectField("#" + goIndex.ToString(), go, typeof(GameObject), true);
									EditorGUI.indentLevel = 0;

									goIndex++;
								}
							}
							else
							{
								EditorGUI.indentLevel = 2;
								EditorGUILayout.LabelField("No objects.");
								EditorGUI.indentLevel = 0;
							}
						}
					}
					else
					{
						EditorGUI.indentLevel = 1;
						EditorGUILayout.LabelField("No layers.");
						EditorGUI.indentLevel = 0;
					}

					EditorGUI.EndDisabledGroup();
				}
			}
		}
	}
}
