// Copyright (C) 2019-2021 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFx.Outline
{
	[CustomPropertyDrawer(typeof(OutlineSettingsWithLayerMask))]
	public class OutlineSettingsWithLayerMaskDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect rc, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(rc, label, property);
			OutlineSettingsEditor.DrawSettingsWithMask(rc, property);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return OutlineSettingsEditor.GetSettingsWithMaskHeight(property);
		}
	}
}
