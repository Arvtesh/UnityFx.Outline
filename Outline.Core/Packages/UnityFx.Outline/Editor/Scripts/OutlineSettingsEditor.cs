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
		private OutlineSettings _settings;

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			OutlineEditorUtility.Render(_settings, _settings);

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_settings);
			}
		}

		private void OnEnable()
		{
			_settings = (OutlineSettings)target;
		}
	}
}
