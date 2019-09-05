// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace UnityFx.Outline
{
	public abstract class IOutlineSettingsTests
	{
		private IOutlineSettings _settings;

		protected void Init(IOutlineSettings settings)
		{
			_settings = settings;
		}

		[Test]
		public void OutlineColor_SetsValuePassed()
		{
			var color = Color.blue;
			_settings.OutlineColor = color;

			Assert.AreEqual(color, _settings.OutlineColor);
		}

		[Test]
		public void OutlineWidth_DefaultValueIsValid()
		{
			Assert.LessOrEqual(OutlineRenderer.MinWidth, _settings.OutlineWidth);
			Assert.GreaterOrEqual(OutlineRenderer.MaxWidth, _settings.OutlineWidth);
		}

		[Test]
		public void OutlineWidth_SetsValuePassed()
		{
			var width = UnityEngine.Random.Range(OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);
			_settings.OutlineWidth = width;

			Assert.AreEqual(width, _settings.OutlineWidth);
		}

		[Test]
		public void OutlineWidth_ClampsValuePasset()
		{
			_settings.OutlineWidth = 1000;

			Assert.AreEqual(OutlineRenderer.MaxWidth, _settings.OutlineWidth);

			_settings.OutlineWidth = -1000;

			Assert.AreEqual(OutlineRenderer.MinWidth, _settings.OutlineWidth);
		}

		[Test]
		public void OutlineMode_SetsValuePassed()
		{
			_settings.OutlineMode = OutlineMode.Blurred;
			Assert.AreEqual(OutlineMode.Blurred, _settings.OutlineMode);

			_settings.OutlineMode = OutlineMode.Solid;
			Assert.AreEqual(OutlineMode.Solid, _settings.OutlineMode);
		}

		[Test]
		public void OutlineIntensity_DefaultValueIsValid()
		{
			Assert.LessOrEqual(OutlineRenderer.MinIntensity, _settings.OutlineIntensity);
			Assert.GreaterOrEqual(OutlineRenderer.MaxIntensity, _settings.OutlineIntensity);
		}

		[Test]
		public void OutlineIntensity_SetsValuePassed()
		{
			var intensity = UnityEngine.Random.Range(OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);

			_settings.OutlineIntensity = intensity;

			Assert.AreEqual(intensity, _settings.OutlineIntensity);
		}

		[Test]
		public void OutlineIntensity_ClampsValuePasset()
		{
			_settings.OutlineIntensity = 1000;

			Assert.AreEqual(OutlineRenderer.MaxIntensity, _settings.OutlineIntensity);

			_settings.OutlineIntensity = -1000;

			Assert.AreEqual(OutlineRenderer.MinIntensity, _settings.OutlineIntensity);
		}
	}
}
