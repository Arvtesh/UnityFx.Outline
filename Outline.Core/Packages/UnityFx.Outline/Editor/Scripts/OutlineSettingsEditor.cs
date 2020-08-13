// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.Outline
{
	[CustomEditor(typeof(OutlineSettings))]
	public class OutlineSettingsEditor : Editor
	{
		private const string _colorPropName = "_outlineColor";
		private const string _widthPropName = "_outlineWidth";
		private const string _intensityPropName = "_outlineIntensity";
		private const string _renderModePropName = "_outlineMode";

		private static readonly GUIContent _colorContent = new GUIContent("Color", "Outline color.");
		private static readonly GUIContent _widthContent = new GUIContent("Width", "Outline width in pixels.");
		private static readonly GUIContent _renderModeContent = new GUIContent("Render Flags", "Outline render flags. Multiple values can be selected at the same time.");
		private static readonly GUIContent _intensityContent = new GUIContent("Blur Intensity", "Outline intensity value. It is only usable for blurred outlines.");

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var colorProp = serializedObject.FindProperty(_colorPropName);
			var widthProp = serializedObject.FindProperty(_widthPropName);
			var intensityProp = serializedObject.FindProperty(_intensityPropName);
			var renderModeProp = serializedObject.FindProperty(_renderModePropName);
			var renderMode = (OutlineRenderFlags)renderModeProp.intValue;

			EditorGUILayout.PropertyField(colorProp, _colorContent);
			EditorGUILayout.PropertyField(widthProp, _widthContent);
			EditorGUILayout.PropertyField(renderModeProp, _renderModeContent);

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				EditorGUILayout.PropertyField(intensityProp, _intensityContent);
			}

			serializedObject.ApplyModifiedProperties();
		}

		internal static float GetSettingsHeight(SerializedProperty property)
		{
			var lineCy = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			var renderModeProp = property.FindPropertyRelative(_renderModePropName);
			var renderMode = (OutlineRenderFlags)renderModeProp.intValue;

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				return lineCy * 5;
			}

			return lineCy * 4;
		}

		internal static void DrawSettings(Rect rc, SerializedProperty property)
		{
			var colorProp = property.FindPropertyRelative(_colorPropName);
			var widthProp = property.FindPropertyRelative(_widthPropName);
			var intensityProp = property.FindPropertyRelative(_intensityPropName);
			var renderModeProp = property.FindPropertyRelative(_renderModePropName);

			DrawSettingsInternal(rc, colorProp, widthProp, intensityProp, renderModeProp);
		}

		internal static void DrawSettings(Rect rc, SerializedObject obj)
		{
			var colorProp = obj.FindProperty(_colorPropName);
			var widthProp = obj.FindProperty(_widthPropName);
			var intensityProp = obj.FindProperty(_intensityPropName);
			var renderModeProp = obj.FindProperty(_renderModePropName);

			DrawSettingsInternal(rc, colorProp, widthProp, intensityProp, renderModeProp);
		}

		private static void DrawSettingsInternal(Rect rc, SerializedProperty colorProp, SerializedProperty widthProp, SerializedProperty intensityProp, SerializedProperty renderModeProp)
		{
			var renderMode = (OutlineRenderFlags)renderModeProp.intValue;
			var lineCy = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			EditorGUI.PropertyField(new Rect(rc.x, rc.y + 1 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), colorProp, _colorContent);
			EditorGUI.PropertyField(new Rect(rc.x, rc.y + 2 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), widthProp, _widthContent);
			EditorGUI.PropertyField(new Rect(rc.x, rc.y + 3 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), renderModeProp, _renderModeContent);

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				EditorGUI.PropertyField(new Rect(rc.x, rc.y + 4 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), intensityProp, _intensityContent);
			}
		}
	}
}
