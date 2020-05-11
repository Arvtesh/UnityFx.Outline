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
		private readonly GUILayoutOption _layerButtonStyle = GUILayout.ExpandWidth(false);
		private OutlineLayerCollection _layers;

		//private SerializedProperty _layersProp;
		//private ReorderableList _layersList;

		private void OnEnable()
		{
			_layers = (OutlineLayerCollection)target;

			//_layersProp = serializedObject.FindProperty("_layers");
			//_layersList = new ReorderableList(serializedObject, _layersProp, true, true, true, true);
			//_layersList.drawElementCallback += OnDrawLayer;
			//_layersList.drawHeaderCallback += OnDrawHeader;
			//_layersList.elementHeightCallback += OnGetElementHeight;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			//_layersList.DoLayoutList();

			EditorGUI.BeginChangeCheck();

			var removeLayer = -1;
			var moveUpLayer = -1;
			var moveDownLayer = -1;

			// 1) Layers list.
			if (_layers.Count > 0)
			{
				for (var i = 0; i < _layers.Count; i++)
				{
					EditorGUILayout.Space();
					OutlineEditorUtility.RenderDivider(Color.gray);

					EditorGUILayout.BeginHorizontal();
					var enabled = EditorGUILayout.ToggleLeft("Layer #" + i.ToString(), _layers[i].Enabled, EditorStyles.boldLabel);

					if (enabled != _layers[i].Enabled)
					{
						if (enabled)
						{
							Undo.RecordObject(_layers, "Enable Layer");
						}
						else
						{
							Undo.RecordObject(_layers, "Disable Layer");
						}

						_layers[i].Enabled = enabled;
					}

					GUILayout.FlexibleSpace();
					EditorGUI.BeginDisabledGroup(i == 0);

					if (GUILayout.Button("Move Up", _layerButtonStyle))
					{
						moveUpLayer = i;
					}

					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup(i == _layers.Count - 1);

					if (GUILayout.Button("Move Down", _layerButtonStyle))
					{
						moveDownLayer = i;
					}

					EditorGUI.EndDisabledGroup();

					if (GUILayout.Button("Remove", _layerButtonStyle))
					{
						removeLayer = i;
					}

					EditorGUILayout.EndHorizontal();

					var name = EditorGUILayout.TextField("Layer Name", _layers[i].NameTag);

					if (name != _layers[i].NameTag)
					{
						Undo.RecordObject(_layers, "Set Layer Name");
						_layers[i].NameTag = name;
					}

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

			if (moveUpLayer > 0)
			{
				Undo.RecordObject(_layers, "Move Layer");
				var tmp = _layers[moveUpLayer - 1];
				_layers[moveUpLayer - 1] = _layers[moveUpLayer];
				_layers[moveUpLayer] = tmp;
			}

			if (moveDownLayer >= 0)
			{
				Undo.RecordObject(_layers, "Move Layer");
				var tmp = _layers[moveDownLayer + 1];
				_layers[moveDownLayer + 1] = _layers[moveDownLayer];
				_layers[moveDownLayer] = tmp;
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

			serializedObject.ApplyModifiedProperties();
		}

		private void OnDrawLayer(Rect rect, int index, bool isActive, bool isFocused)
		{
			var lineHeight = EditorGUIUtility.singleLineHeight;
			var lineSpacing = EditorGUIUtility.standardVerticalSpacing;
			var lineOffset = lineHeight + lineSpacing;
			var y = rect.y;
			var layer = _layers[index];

			EditorGUI.BeginChangeCheck();

			var obj = (OutlineSettings)EditorGUI.ObjectField(new Rect(rect.x, y, rect.width, lineHeight), " ", layer.OutlineSettings, typeof(OutlineSettings), true);

			if (layer.OutlineSettings != obj)
			{
				Undo.RecordObject(_layers, "Settings");
				layer.OutlineSettings = obj;
			}

			var enabled = EditorGUI.ToggleLeft(new Rect(rect.x, y, rect.width, lineHeight), "Layer #" + index.ToString(), layer.Enabled, EditorStyles.boldLabel);

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

			y += lineOffset;
			EditorGUI.DrawRect(new Rect(rect.x, y, rect.width, 1), Color.gray);
			EditorGUI.DrawRect(new Rect(rect.x, y + 1, rect.width, 1), Color.white);

			y += lineSpacing + 2;
			var name = EditorGUI.TextField(new Rect(rect.x, y, rect.width, lineHeight), "Tag", layer.NameTag);

			if (name != layer.NameTag)
			{
				Undo.RecordObject(_layers, "Layer Name");
				layer.NameTag = name;
			}

			EditorGUI.BeginDisabledGroup(obj != null);

			y += lineOffset;
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

			y += lineOffset;
			EditorGUI.DrawRect(new Rect(rect.x, y, rect.width, 1), Color.gray);
			EditorGUI.DrawRect(new Rect(rect.x, y + 1, rect.width, 1), Color.white);

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_layers);
			}
		}

		private void OnDrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Layers");
		}

		private float OnGetElementHeight(int index)
		{
			var numberOfLines = 5;

			if ((_layers[index].OutlineRenderMode & OutlineRenderFlags.Blurred) != 0)
			{
				++numberOfLines;
			}

			return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * numberOfLines + EditorGUIUtility.standardVerticalSpacing + 5;
		}
	}
}
