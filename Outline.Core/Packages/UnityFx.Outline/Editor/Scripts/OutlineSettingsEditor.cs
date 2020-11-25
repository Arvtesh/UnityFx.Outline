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
		private const string _filterModePropName = "_filterMode";
		private const string _layerMaskPropName = "_layerMask";
		private const string _renderingLayerMaskPropName = "_renderingLayerMask";
		private const string _settingsPropName = "_outlineSettings";
		private const string _colorPropName = "_outlineColor";
		private const string _widthPropName = "_outlineWidth";
		private const string _intensityPropName = "_outlineIntensity";
		private const string _cutoffPropName = "_outlineAlphaCutoff";
		private const string _renderModePropName = "_outlineMode";

		private static readonly GUIContent _filterModeContent = new GUIContent("Outline Filter Settings", "");
		private static readonly GUIContent _layerMaskContent = new GUIContent("Layer Mask", OutlineResources.OutlineLayerMaskTooltip);
		private static readonly GUIContent _renderingLayerMaskContent = new GUIContent("Rendering Layer Mask", OutlineResources.OutlineRenderingLayerMaskTooltip);
		private static readonly GUIContent _colorContent = new GUIContent("Color", "Outline color.");
		private static readonly GUIContent _widthContent = new GUIContent("Width", "Outline width in pixels.");
		private static readonly GUIContent _renderModeContent = new GUIContent("Render Flags", "Outline render flags. Multiple values can be selected at the same time.");
		private static readonly GUIContent _intensityContent = new GUIContent("Blur Intensity", "Outline intensity value. It is only usable for blurred outlines.");
		private static readonly GUIContent _cutoffContent = new GUIContent("Alpha Cutoff", "Outline alpha cutoff value. It is only usable when alpha testing is enabled and the material doesn't have _Cutoff property.");

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var colorProp = serializedObject.FindProperty(_colorPropName);
			var widthProp = serializedObject.FindProperty(_widthPropName);
			var intensityProp = serializedObject.FindProperty(_intensityPropName);
			var cutoffProp = serializedObject.FindProperty(_cutoffPropName);
			var renderModeProp = serializedObject.FindProperty(_renderModePropName);
			var renderMode = (OutlineRenderFlags)renderModeProp.intValue;

			EditorGUILayout.PropertyField(colorProp, _colorContent);
			EditorGUILayout.PropertyField(widthProp, _widthContent);

			//EditorGUILayout.PropertyField(renderModeProp, _renderModeContent);
			renderModeProp.intValue = (int)(OutlineRenderFlags)EditorGUILayout.EnumFlagsField(_renderModeContent, renderMode);

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				EditorGUILayout.PropertyField(intensityProp, _intensityContent);
			}

			if ((renderMode & OutlineRenderFlags.EnableAlphaTesting) != 0)
			{
				EditorGUILayout.PropertyField(cutoffProp, _cutoffContent);
			}

			serializedObject.ApplyModifiedProperties();
		}

		internal static float GetSettingsInstanceHeight(SerializedProperty property)
		{
			var lineCy = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			var renderModeProp = property.FindPropertyRelative(_renderModePropName);
			var renderMode = (OutlineRenderFlags)renderModeProp.intValue;
			var result = lineCy * 4;

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				result += lineCy;
			}

			if ((renderMode & OutlineRenderFlags.EnableAlphaTesting) != 0)
			{
				result += lineCy;
			}

			return result;
		}

		internal static float GetSettingsWithMaskHeight(SerializedProperty property)
		{
			var lineCy = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			var filterModeProp = property.FindPropertyRelative(_filterModePropName);
			var renderOutlineSettings = false;

			if (filterModeProp.intValue == (int)OutlineFilterMode.UseLayerMask)
			{
				var layerMaskProp = property.FindPropertyRelative(_layerMaskPropName);
				renderOutlineSettings = true;
			}
			else if (filterModeProp.intValue == (int)OutlineFilterMode.UseRenderingLayerMask)
			{
				var renderingLayerMaskProp = property.FindPropertyRelative(_renderingLayerMaskPropName);
				renderOutlineSettings = true;
			}

			if (renderOutlineSettings)
			{
				var renderModeProp = property.FindPropertyRelative(_renderModePropName);
				var renderMode = (OutlineRenderFlags)renderModeProp.intValue;
				var result = lineCy * 6;

				if ((renderMode & OutlineRenderFlags.Blurred) != 0)
				{
					result += lineCy;
				}

				if ((renderMode & OutlineRenderFlags.EnableAlphaTesting) != 0)
				{
					result += lineCy;
				}

				return result;
			}

			return lineCy;
		}

		internal static void DrawSettingsInstance(Rect rc, SerializedProperty property)
		{
			var settingsProp = property.FindPropertyRelative(_settingsPropName);

			EditorGUI.PropertyField(new Rect(rc.x, rc.y, rc.width, EditorGUIUtility.singleLineHeight), settingsProp);
			EditorGUI.indentLevel += 1;

			if (settingsProp.objectReferenceValue)
			{
				var obj = new SerializedObject(settingsProp.objectReferenceValue);
				var colorProp = obj.FindProperty(_colorPropName);
				var widthProp = obj.FindProperty(_widthPropName);
				var intensityProp = obj.FindProperty(_intensityPropName);
				var cutoffProp = obj.FindProperty(_cutoffPropName);
				var renderModeProp = obj.FindProperty(_renderModePropName);

				EditorGUI.BeginDisabledGroup(true);
				DrawSettingsInternal(rc, colorProp, widthProp, intensityProp, cutoffProp, renderModeProp);
				EditorGUI.EndDisabledGroup();
			}
			else
			{
				var colorProp = property.FindPropertyRelative(_colorPropName);
				var widthProp = property.FindPropertyRelative(_widthPropName);
				var intensityProp = property.FindPropertyRelative(_intensityPropName);
				var cutoffProp = property.FindPropertyRelative(_cutoffPropName);
				var renderModeProp = property.FindPropertyRelative(_renderModePropName);

				DrawSettingsInternal(rc, colorProp, widthProp, intensityProp, cutoffProp, renderModeProp);
			}

			EditorGUI.indentLevel -= 1;
		}

		internal static void DrawSettingsWithMask(Rect rc, SerializedProperty property)
		{
			var lineCy = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			var filterModeProp = property.FindPropertyRelative(_filterModePropName);

			EditorGUI.PropertyField(new Rect(rc.x, rc.y, rc.width, EditorGUIUtility.singleLineHeight), filterModeProp, _filterModeContent);

			if (filterModeProp.intValue == (int)OutlineFilterMode.UseLayerMask)
			{
				var layerMaskProp = property.FindPropertyRelative(_layerMaskPropName);

				EditorGUI.indentLevel += 1;
				EditorGUI.PropertyField(new Rect(rc.x, rc.y + lineCy, rc.width, EditorGUIUtility.singleLineHeight), layerMaskProp, _layerMaskContent);
				EditorGUI.indentLevel -= 1;

				DrawSettingsInstance(new Rect(rc.x, rc.y + lineCy * 2, rc.width, rc.height - lineCy), property);
			}
			else if (filterModeProp.intValue == (int)OutlineFilterMode.UseRenderingLayerMask)
			{
				var renderingLayerMaskProp = property.FindPropertyRelative(_renderingLayerMaskPropName);

				EditorGUI.indentLevel += 1;
				EditorGUI.PropertyField(new Rect(rc.x, rc.y + lineCy, rc.width, EditorGUIUtility.singleLineHeight), renderingLayerMaskProp, _renderingLayerMaskContent);
				EditorGUI.indentLevel -= 1;

				DrawSettingsInstance(new Rect(rc.x, rc.y + lineCy * 2, rc.width, rc.height - lineCy), property);	
			}
		}

		private static void DrawSettingsInternal(Rect rc, SerializedProperty colorProp, SerializedProperty widthProp, SerializedProperty intensityProp, SerializedProperty cutoffProp, SerializedProperty renderModeProp)
		{
			var renderMode = (OutlineRenderFlags)renderModeProp.intValue;
			var lineCy = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			var n = 4;

			EditorGUI.PropertyField(new Rect(rc.x, rc.y + 1 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), colorProp, _colorContent);
			EditorGUI.PropertyField(new Rect(rc.x, rc.y + 2 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), widthProp, _widthContent);

			// NOTE: EditorGUI.PropertyField doesn't allow multi-selection, have to use EnumFlagsField explixitly.
			renderModeProp.intValue = (int)(OutlineRenderFlags)EditorGUI.EnumFlagsField(new Rect(rc.x, rc.y + 3 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), _renderModeContent, renderMode);

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				EditorGUI.PropertyField(new Rect(rc.x, rc.y + n++ * lineCy, rc.width, EditorGUIUtility.singleLineHeight), intensityProp, _intensityContent);
			}

			if ((renderMode & OutlineRenderFlags.EnableAlphaTesting) != 0)
			{
				EditorGUI.PropertyField(new Rect(rc.x, rc.y + n * lineCy, rc.width, EditorGUIUtility.singleLineHeight), cutoffProp, _cutoffContent);
			}
		}
	}
}
