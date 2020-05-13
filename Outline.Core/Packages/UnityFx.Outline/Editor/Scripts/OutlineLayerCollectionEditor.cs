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

		private Dictionary<OutlineLayer, ReorderableList> _layerLists = new Dictionary<OutlineLayer, ReorderableList>();

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

			foreach (var layer in _layers)
			{
				AddLayerList(layer);
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			_layersList.DoLayoutList();

			EditorGUILayout.Space();

			foreach (var list in _layerLists.Values)
			{
				if (list.count > 0)
				{
					list.DoLayoutList();
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
			var settingsDisabled = false;

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

				var obj = (OutlineSettings)EditorGUI.ObjectField(rc, " ", layer.OutlineSettings, typeof(OutlineSettings), true);

				if (layer.OutlineSettings != obj)
				{
					Undo.RecordObject(_layers, "Settings");
					layer.OutlineSettings = obj;
				}

				var enabled = EditorGUI.ToggleLeft(rc, "Layer #" + index.ToString(), layer.Enabled, EditorStyles.boldLabel);

				if (layer.Enabled != enabled)
				{
					if (enabled)
					{
						Undo.RecordObject(_layers, "Enable Layer");
					}
					else
					{
						Undo.RecordObject(_layers, "Disable Layer");
					}

					layer.Enabled = enabled;
				}

				settingsDisabled = obj != null;
				y += lineOffset;
			}

			// Layer properties
			{
				var name = EditorGUI.TextField(new Rect(rect.x, y, rect.width, lineHeight), "Name", layer.NameTag);

				if (name != layer.NameTag)
				{
					Undo.RecordObject(_layers, "Layer Name");
					layer.NameTag = name;
				}

				y += lineOffset;
			}

			// Outline settings
			{
				EditorGUI.BeginDisabledGroup(settingsDisabled);

				var color = EditorGUI.ColorField(new Rect(rect.x, y, rect.width, lineHeight), "Color", layer.OutlineColor);

				if (layer.OutlineColor != color)
				{
					Undo.RecordObject(_layers, "Color");
					layer.OutlineColor = color;
				}

				y += lineOffset;
				var width = EditorGUI.IntSlider(new Rect(rect.x, y, rect.width, lineHeight), "Width", layer.OutlineWidth, OutlineResources.MinWidth, OutlineResources.MaxWidth);

				if (layer.OutlineWidth != width)
				{
					Undo.RecordObject(_layers, "Width");
					layer.OutlineWidth = width;
				}

				y += lineOffset;
				var renderMode = (OutlineRenderFlags)EditorGUI.EnumFlagsField(new Rect(rect.x, y, rect.width, lineHeight), "Render Flags", layer.OutlineRenderMode);

				if (layer.OutlineRenderMode != renderMode)
				{
					Undo.RecordObject(_layers, "Render Flags");
					layer.OutlineRenderMode = renderMode;
				}

				if ((renderMode & OutlineRenderFlags.Blurred) != 0)
				{
					y += lineOffset;
					var i = EditorGUI.Slider(new Rect(rect.x, y, rect.width, lineHeight), "Blur Intensity", layer.OutlineIntensity, OutlineResources.MinIntensity, OutlineResources.MaxIntensity);

					if (!Mathf.Approximately(layer.OutlineIntensity, i))
					{
						Undo.RecordObject(_layers, "Blur Intensity");
						layer.OutlineIntensity = i;
					}
				}

				EditorGUI.EndDisabledGroup();
			}

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_layers);
			}
		}

		private void OnDrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Layer settings");
		}

		private float OnGetElementHeight(int index)
		{
			var numberOfLines = 5;

			if ((_layers[index].OutlineRenderMode & OutlineRenderFlags.Blurred) != 0)
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
			AddLayerList(layer);
		}

		private void OnRemoveLayer(ReorderableList list)
		{
			var index = list.index;
			var layer = _layers[index];

			Undo.RecordObject(_layers, "Remove Layer");
			EditorUtility.SetDirty(_layers);

			_layers.RemoveAt(index);
			_layerLists.Remove(layer);
		}

		private void AddLayerList(OutlineLayer layer)
		{
			var list = new ReorderableList(layer.GetList(), typeof(GameObject), false, true, false, false);

			list.drawElementCallback += (rect, index, isActive, isFocused) =>
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "#" + index, list.list[index] as GameObject, typeof(GameObject), true);
				EditorGUI.EndDisabledGroup();
			};

			list.drawHeaderCallback += (rect) =>
			{
				EditorGUI.LabelField(rect, layer.Name);
			};

			list.elementHeightCallback += (index) =>
			{
				return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			};

			_layerLists.Add(layer, list);
		}
	}
}
