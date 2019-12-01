// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEditor.Rendering.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityFx.Outline.PostProcessing
{
	[PostProcessEditor(typeof(Outline))]
	public sealed class OutlineEditor : PostProcessEffectEditor<Outline>
	{
		private SerializedParameterOverride _resources;
		private SerializedParameterOverride _layers;

		public override void OnEnable()
		{
			_resources = FindParameterOverride(x => x.Resources);
			_layers = FindParameterOverride(x => x.Layers);
		}

		public override void OnInspectorGUI()
		{
			PropertyField(_resources);
			PropertyField(_layers);
		}
	}
}
