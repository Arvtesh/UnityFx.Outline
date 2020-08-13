// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.Outline
{
    [CustomPropertyDrawer(typeof(OutlineSettingsInstance))]
    public class OutlineSettingsInstanceDrawer : PropertyDrawer
    {
		private static GUIContent _colorContent = new GUIContent("Color", "");
		private static GUIContent _widthContent = new GUIContent("Width", "");
		private static GUIContent _renderModeContent = new GUIContent("Render Flags", "");
		private static GUIContent _intensityContent = new GUIContent("Blur Intensity", "");

		public override void OnGUI(Rect rc, SerializedProperty property, GUIContent label)
		{
			var settingsProp = property.FindPropertyRelative("_outlineSettings");

			EditorGUI.BeginProperty(rc, label, property);
			EditorGUI.PropertyField(new Rect(rc.x, rc.y, rc.width, EditorGUIUtility.singleLineHeight), settingsProp);
			EditorGUI.indentLevel += 1;

			if (settingsProp.objectReferenceValue)
			{
				EditorGUI.BeginDisabledGroup(true);
				OutlineSettingsEditor.DrawSettings(rc, new SerializedObject(settingsProp.objectReferenceValue));
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				OutlineSettingsEditor.DrawSettings(rc, property);
			}

			EditorGUI.indentLevel -= 1;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return OutlineSettingsEditor.GetSettingsHeight(property);
		}
	}
}
