// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityFx.Outline
{
	[CustomEditor(typeof(OutlineBuilder))]
	public class OutlineBuilderEditor : Editor
	{
		private OutlineBuilder _builder;
		private ReorderableList _content;
		private List<ReorderableList> _lists;

		private void OnEnable()
		{
			_builder = (OutlineBuilder)target;

			if (EditorApplication.isPlaying)
			{
				if (_builder.OutlineLayers)
				{
					_lists = new List<ReorderableList>(_builder.OutlineLayers.Count);

					foreach (var layer in _builder.OutlineLayers)
					{
						var list0 = new ArrayList(layer.Count);

						foreach (var go in layer)
						{
							list0.Add(go);
						}

						var editorList = new ReorderableList(list0, typeof(GameObject), false, true, true, true);

						editorList.onAddCallback += (list) =>
						{
							list.list.Add(null);
						};

						editorList.onRemoveCallback += (list) =>
						{
							var go = list.list[list.index];
							list.list.RemoveAt(list.index);
							layer.Remove(go as GameObject);
						};

						editorList.drawElementCallback += (rect, index, isActive, isFocused) =>
						{
							var prevGo = list0[index] as GameObject;
							var go = (GameObject)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), $"#{index}", prevGo, typeof(GameObject), true);

							if (prevGo != go)
							{
								list0[index] = go;
								layer.Remove(prevGo);
								layer.Add(go);
							}
						};

						editorList.drawHeaderCallback += (rect) =>
						{
							EditorGUI.LabelField(rect, layer.Name);
						};

						editorList.elementHeightCallback += (index) =>
						{
							return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
						};

						_lists.Add(editorList);
					}
				}
			}
			else
			{
				_content = new ReorderableList(_builder.Content, typeof(OutlineBuilder.ContentItem), true, true, true, true);

				_content.drawElementCallback += (rect, index, isActive, isFocused) =>
				{
					if (_builder && _builder.Content != null && index < _builder.Content.Count)
					{
						var item = _builder.Content[index];

						if (item != null)
						{
							item.Go = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + rect.width * 0.3f + 1, rect.y, rect.width * 0.7f, EditorGUIUtility.singleLineHeight), item.Go, typeof(GameObject), true);
							item.LayerIndex = EditorGUI.IntField(new Rect(rect.x, rect.y, rect.width * 0.3f - 1, EditorGUIUtility.singleLineHeight), item.LayerIndex);
						}
					}
				};

				_content.drawHeaderCallback += (rect) =>
				{
					EditorGUI.LabelField(rect, "Content");
				};

				_content.elementHeightCallback += (index) =>
				{
					return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				};
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();

			if (_content != null)
			{
				EditorGUILayout.HelpBox("Game objectes listed below will be added to corresponding outline layers when application is started. Only scene references are allowed.", MessageType.Info);
				_content.DoLayoutList();
				EditorGUILayout.Space();

				if (GUILayout.Button("Clear"))
				{
					_builder.Content.Clear();
				}

				serializedObject.ApplyModifiedProperties();
			}
			else if (_lists != null && _lists.Count > 0)
			{
				EditorGUILayout.HelpBox("Settings below are not serialized, they only exist in runtime.", MessageType.Info);

				for (var i = 0; i < _lists.Count; ++i)
				{
					_lists[i].DoLayoutList();
					EditorGUILayout.Space();
				}

				if (GUILayout.Button("Clear"))
				{
					foreach (var list in _lists)
					{
						list.list.Clear();
					}

					_builder.Clear();
				}

				serializedObject.ApplyModifiedProperties();
			}
			else
			{
				// TODO
			}

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_builder, "Builder");

				if (!EditorApplication.isPlayingOrWillChangePlaymode)
				{
					EditorUtility.SetDirty(_builder.gameObject);
					EditorSceneManager.MarkSceneDirty(_builder.gameObject.scene);
				}
			}
		}
	}
}
