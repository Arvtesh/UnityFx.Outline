// Copyright (C) 2019-2021 Alexander Bogarsukov. All rights reserved.
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

		private static readonly string[] _renderingLayerMaskNames = new string[]
		{
			"Layer1",
			"Layer2",
			"Layer3",
			"Layer4",
			"Layer5",
			"Layer6",
			"Layer7",
			"Layer8",
			"Layer9",
			"Layer10",
			"Layer11",
			"Layer12",
			"Layer13",
			"Layer14",
			"Layer15",
			"Layer16",
			"Layer17",
			"Layer18",
			"Layer19",
			"Layer20",
			"Layer21",
			"Layer22",
			"Layer23",
			"Layer24",
			"Layer25",
			"Layer26",
			"Layer27",
			"Layer28",
			"Layer29",
			"Layer30",
			"Layer31",
			"Layer32",
		};

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var colorProp = serializedObject.FindProperty(_colorPropName);
			var widthProp = serializedObject.FindProperty(_widthPropName);
			var intensityProp = serializedObject.FindProperty(_intensityPropName);
			var cutoffProp = serializedObject.FindProperty(_cutoffPropName);
			var renderModeProp = serializedObject.FindProperty(_renderModePropName);
			var renderMode = (OutlineRenderFlags)renderModeProp.intValue;

			//EditorGUILayout.PropertyField(colorProp, _colorContent);
			colorProp.colorValue = EditorGUILayout.ColorField(OutlineEditorUtility.ColorContent, colorProp.colorValue, true, true, true);

			EditorGUILayout.PropertyField(widthProp, OutlineEditorUtility.WidthContent);

			//EditorGUILayout.PropertyField(renderModeProp, _renderModeContent);
			renderModeProp.intValue = (int)(OutlineRenderFlags)EditorGUILayout.EnumFlagsField(OutlineEditorUtility.RenderFlagsContent, renderMode);

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				EditorGUILayout.PropertyField(intensityProp, OutlineEditorUtility.BlurIntensityContent);
			}

			if ((renderMode & OutlineRenderFlags.EnableAlphaTesting) != 0)
			{
				EditorGUILayout.PropertyField(cutoffProp, OutlineEditorUtility.AlphaCutoffContent);
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

			EditorGUI.PropertyField(new Rect(rc.x, rc.y, rc.width, EditorGUIUtility.singleLineHeight), filterModeProp, OutlineEditorUtility.FilterSettingsContent);

			if (filterModeProp.intValue == (int)OutlineFilterMode.UseLayerMask)
			{
				var layerMaskProp = property.FindPropertyRelative(_layerMaskPropName);

				EditorGUI.indentLevel += 1;
				EditorGUI.PropertyField(new Rect(rc.x, rc.y + lineCy, rc.width, EditorGUIUtility.singleLineHeight), layerMaskProp, OutlineEditorUtility.LayerMaskContent);
				EditorGUI.indentLevel -= 1;

				DrawSettingsInstance(new Rect(rc.x, rc.y + lineCy * 2, rc.width, rc.height - lineCy), property);
			}
			else if (filterModeProp.intValue == (int)OutlineFilterMode.UseRenderingLayerMask)
			{
				var renderingLayerMaskProp = property.FindPropertyRelative(_renderingLayerMaskPropName);

				EditorGUI.indentLevel += 1;
				renderingLayerMaskProp.intValue = EditorGUI.MaskField(new Rect(rc.x, rc.y + lineCy, rc.width, EditorGUIUtility.singleLineHeight), OutlineEditorUtility.RenderingLayerMaskContent, renderingLayerMaskProp.intValue, _renderingLayerMaskNames);
				EditorGUI.indentLevel -= 1;

				DrawSettingsInstance(new Rect(rc.x, rc.y + lineCy * 2, rc.width, rc.height - lineCy), property);
			}
		}

		private static void DrawSettingsInternal(Rect rc, SerializedProperty colorProp, SerializedProperty widthProp, SerializedProperty intensityProp, SerializedProperty cutoffProp, SerializedProperty renderModeProp)
		{
			var renderMode = (OutlineRenderFlags)renderModeProp.intValue;
			var lineCy = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			var n = 4;

			//EditorGUI.PropertyField(new Rect(rc.x, rc.y + 1 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), colorProp, _colorContent);
			colorProp.colorValue = EditorGUI.ColorField(new Rect(rc.x, rc.y + 1 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), OutlineEditorUtility.ColorContent, colorProp.colorValue, true, true, true);

			EditorGUI.PropertyField(new Rect(rc.x, rc.y + 2 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), widthProp, OutlineEditorUtility.WidthContent);

			// NOTE: EditorGUI.PropertyField doesn't allow multi-selection, have to use EnumFlagsField explixitly.
			renderModeProp.intValue = (int)(OutlineRenderFlags)EditorGUI.EnumFlagsField(new Rect(rc.x, rc.y + 3 * lineCy, rc.width, EditorGUIUtility.singleLineHeight), OutlineEditorUtility.RenderFlagsContent, renderMode);

			if ((renderMode & OutlineRenderFlags.Blurred) != 0)
			{
				EditorGUI.PropertyField(new Rect(rc.x, rc.y + n++ * lineCy, rc.width, EditorGUIUtility.singleLineHeight), intensityProp, OutlineEditorUtility.BlurIntensityContent);
			}

			if ((renderMode & OutlineRenderFlags.EnableAlphaTesting) != 0)
			{
				EditorGUI.PropertyField(new Rect(rc.x, rc.y + n * lineCy, rc.width, EditorGUIUtility.singleLineHeight), cutoffProp, OutlineEditorUtility.AlphaCutoffContent);
			}
		}
	}
}
