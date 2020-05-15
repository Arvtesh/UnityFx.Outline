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
	[CustomEditor(typeof(OutlineBuilder))]
	public class OutlineBuilderEditor : Editor
	{
		private OutlineBuilder _builder;
		private List<ReorderableList> _lists = new List<ReorderableList>();

		private void OnEnable()
		{
			_builder = (OutlineBuilder)target;

			if (_builder.OutlineLayers)
			{
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

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (_lists.Count > 0)
			{
				EditorGUILayout.Space();

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
		}
	}
}
