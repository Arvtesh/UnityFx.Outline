// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
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
		private IOutlineSettings _settings;

		public override void OnInspectorGUI()
		{
			OutlineEditorUtility.Render(_settings, false);
		}

		private void OnEnable()
		{
			_settings = (IOutlineSettings)target;
		}
	}
}
