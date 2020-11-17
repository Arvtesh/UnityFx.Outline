// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityFx.Outline
{
	[CustomEditor(typeof(OutlineLayerCollection))]
	public class OutlineLayerCollectionEditor : Editor
	{
		private OutlineLayerCollection _layers;

		private SerializedProperty _layersProp;
		private ReorderableList _layersList;

		private void OnEnable()
		{
			_layers = (OutlineLayerCollection)target;

			_layersProp = serializedObject.FindProperty("_layers");
			_layersList = new ReorderableList(serializedObject, _layersProp, true, true, true, true);
			_layersList.drawElementCallback += OnDrawLayer;
			_layersList.drawHeaderCallback += OnDrawHeader;
			_layersList.elementHeightCallback += OnGetElementHeight;
			_layersList.onAddCallback += OnAddLayer;
			_layersList.onRemoveCallback += OnRemoveLayer;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();

			var mask = EditorGUILayout.MaskField("Ignore layers", _layers.IgnoreLayerMask, InternalEditorUtility.layers);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_layers, "Change ignore mask");
				_layers.IgnoreLayerMask = mask;
			}

			EditorGUILayout.Space();

			_layersList.DoLayoutList();

			if (_layers.NumberOfObjects > 0)
			{
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("Read-only lists below represent game objects assigned to specific outline layers. Only non-empty layers are displayed.", MessageType.Info);

				foreach (var layer in _layers)
				{
					if (layer.Count > 0)
					{
						EditorGUILayout.LabelField(layer.Name, EditorStyles.boldLabel);
						EditorGUI.BeginDisabledGroup(true);
						EditorGUI.indentLevel += 1;

						var index = 0;

						foreach (var go in layer)
						{
							EditorGUILayout.ObjectField($"#{index++}", go, typeof(GameObject), true);
						}

						EditorGUI.indentLevel -= 1;
						EditorGUI.EndDisabledGroup();
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void OnDrawLayer(Rect rect, int index, bool isActive, bool isFocused)
		{
			var lineHeight = EditorGUIUtility.singleLineHeight;
			var lineSpacing = EditorGUIUtility.standardVerticalSpacing;
			var lineOffset = lineHeight + lineSpacing;
			var y = rect.y + lineSpacing;
			var layer = _layers[index];

			var obj = layer.OutlineSettings;
			var merge = layer.MergeLayerObjects;
			var enabled = layer.Enabled;
			var name = layer.NameTag;
			var color = layer.OutlineColor;
			var width = layer.OutlineWidth;
			var renderMode = layer.OutlineRenderMode;
			var blurIntensity = layer.OutlineIntensity;
			var alphaCutoff = layer.OutlineAlphaCutoff;

			EditorGUI.BeginChangeCheck();

			// Header
			{
				var rc = new Rect(rect.x, y, rect.width, lineHeight);
				var bgRect = new Rect(rect.x - 2, y - 2, rect.width + 3, lineHeight + 3);

				// Header background
				EditorGUI.DrawRect(rc, Color.gray);
				EditorGUI.DrawRect(new Rect(bgRect.x, bgRect.y, bgRect.width, 1), Color.gray);
				EditorGUI.DrawRect(new Rect(bgRect.x, bgRect.yMax, bgRect.width, 1), Color.gray);
				EditorGUI.DrawRect(new Rect(bgRect.x, bgRect.y, 1, bgRect.height), Color.gray);
				EditorGUI.DrawRect(new Rect(bgRect.xMax, bgRect.y, 1, bgRect.height), Color.gray);

				obj = (OutlineSettings)EditorGUI.ObjectField(rc, " ", obj, typeof(OutlineSettings), true);
				enabled = EditorGUI.ToggleLeft(rc, "Layer #" + index.ToString(), enabled, EditorStyles.boldLabel);
				y += lineOffset;
			}

			// Layer properties
			{
				name = EditorGUI.TextField(new Rect(rect.x, y, rect.width, lineHeight), "Name", name);
				y += lineOffset;

				merge = EditorGUI.Toggle(new Rect(rect.x, y, rect.width, lineHeight), "Merge Layer Objects", merge);
				y += lineOffset;
			}

			// Outline settings
			{
				EditorGUI.BeginDisabledGroup(obj != null);

				color = EditorGUI.ColorField(new Rect(rect.x, y, rect.width, lineHeight), "Color", color);
				y += lineOffset;

				width = EditorGUI.IntSlider(new Rect(rect.x, y, rect.width, lineHeight), "Width", width, OutlineResources.MinWidth, OutlineResources.MaxWidth);
				y += lineOffset;

				renderMode = (OutlineRenderFlags)EditorGUI.EnumFlagsField(new Rect(rect.x, y, rect.width, lineHeight), "Render Flags", renderMode);
				y += lineOffset;

				if ((renderMode & OutlineRenderFlags.Blurred) != 0)
				{
					blurIntensity = EditorGUI.Slider(new Rect(rect.x, y, rect.width, lineHeight), "Blur Intensity", blurIntensity, OutlineResources.MinIntensity, OutlineResources.MaxIntensity);
					y += lineOffset;
				}

				if ((renderMode & OutlineRenderFlags.EnableAlphaTesting) != 0)
				{
					alphaCutoff = EditorGUI.Slider(new Rect(rect.x, y, rect.width, lineHeight), "Alpha Cutoff", alphaCutoff, OutlineResources.MinAlphaCutoff, OutlineResources.MaxAlphaCutoff);
				}

				EditorGUI.EndDisabledGroup();
			}

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_layers, "Layers changed");
				EditorUtility.SetDirty(_layers);

				layer.OutlineSettings = obj;
				layer.Enabled = enabled;
				layer.NameTag = name;
				layer.MergeLayerObjects = merge;
				layer.OutlineWidth = width;
				layer.OutlineColor = color;
				layer.OutlineRenderMode = renderMode;
				layer.OutlineIntensity = blurIntensity;
				layer.OutlineAlphaCutoff = alphaCutoff;
			}
		}

		private void OnDrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Layer settings");
		}

		private float OnGetElementHeight(int index)
		{
			var numberOfLines = 6;

			if ((_layers[index].OutlineRenderMode & OutlineRenderFlags.Blurred) != 0)
			{
				++numberOfLines;
			}

			if ((_layers[index].OutlineRenderMode & OutlineRenderFlags.EnableAlphaTesting) != 0)
			{
				++numberOfLines;
			}

			return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * numberOfLines + EditorGUIUtility.standardVerticalSpacing;
		}

		private void OnAddLayer(ReorderableList list)
		{
			var layer = new OutlineLayer();

			Undo.RecordObject(_layers, "Add Layer");
			EditorUtility.SetDirty(_layers);

			_layers.Add(layer);
		}

		private void OnRemoveLayer(ReorderableList list)
		{
			var index = list.index;
			var layer = _layers[index];

			Undo.RecordObject(_layers, "Remove Layer");
			EditorUtility.SetDirty(_layers);

			_layers.RemoveAt(index);
		}
	}
}
