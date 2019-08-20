// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// This asset is used to store references to shaders and other resources needed at runtime without having to use a Resources folder.
	/// </summary>
	[CreateAssetMenu(fileName = "OutlineResources", menuName = "UnityFx/Outline Resources")]
	public class OutlineResources : ScriptableObject
	{
		/// <summary>
		/// Gets or sets a <see cref="Shader"/> that renders objects outlined with a solid while color.
		/// </summary>
		public Shader RenderShader;

		/// <summary>
		/// Gets or sets a <see cref="Shader"/> that renders outline around the mask, that was generated with <see cref="RenderShader"/>.
		/// </summary>
		public Shader PostProcessShader;

		/// <summary>
		/// Gets a value indicating whether the instance is in valid state.
		/// </summary>
		public bool IsValid
		{
			get
			{
				return RenderShader && PostProcessShader;
			}
		}

		/// <summary>
		/// Resets the resources to defaults.
		/// </summary>
		public void ResetToDefaults()
		{
			RenderShader = Shader.Find("UnityFx/Outline/RenderColor");
			PostProcessShader = Shader.Find("UnityFx/Outline/PostProcess");
		}
	}
}
