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
	[Category("OutlineMaterialSet"), TestOf(typeof(OutlineMaterialSet))]
	public class OutlineMaterialSetTests : IOutlineSettingsTests
	{
		private OutlineResources _resources;
		private OutlineMaterialSet _materialSet;

		[SetUp]
		public void Init()
		{
			_resources = ScriptableObject.CreateInstance<OutlineResources>();
			_materialSet = _resources.CreateMaterialSet();
			Init(_materialSet);
		}

		[TearDown]
		public void Dispose()
		{
			_materialSet.Dispose();
			UnityEngine.Object.DestroyImmediate(_resources);
		}

		[Test]
		public void Dispose_CanBeCalledMultipleTimes()
		{
			_materialSet.Dispose();
			_materialSet.Dispose();
		}

		[Test]
		public void Reset_ThrowsIfDisposed()
		{
			_materialSet.Dispose();

			Assert.Throws<ObjectDisposedException>(() => _materialSet.Reset(_materialSet));
		}

		[Test]
		public void Reset_ThrowsIfArgumentIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => _materialSet.Reset(null));
		}

		[Test]
		public void Reset_SetsValuesPassed()
		{
			var settings = ScriptableObject.CreateInstance<OutlineSettings>();

			try
			{
				settings.OutlineColor = Color.yellow;
				settings.OutlineWidth = 20;
				settings.OutlineIntensity = 5;
				settings.OutlineMode = OutlineMode.Blurred;

				_materialSet.Reset(settings);

				Assert.AreEqual(settings.OutlineColor, _materialSet.OutlineColor);
				Assert.AreEqual(settings.OutlineWidth, _materialSet.OutlineWidth);
				Assert.AreEqual(settings.OutlineIntensity, _materialSet.OutlineIntensity);
				Assert.AreEqual(settings.OutlineMode, _materialSet.OutlineMode);
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(settings);
			}
		}

		[Test]
		public void OutlineColor_ThrowsIfDisposed()
		{
			_materialSet.Dispose();

			Assert.Throws<ObjectDisposedException>(() => _materialSet.OutlineColor = Color.magenta);
		}

		[Test]
		public void OutlineColor_SetsMaterialParams()
		{
			var color = Color.blue;
			_materialSet.OutlineColor = color;

			Assert.AreEqual(color, _materialSet.VPassBlendMaterial.GetColor(_materialSet.ColorNameId));
		}

		[Test]
		public void OutlineWidth_ThrowsIfDisposed()
		{
			_materialSet.Dispose();

			Assert.Throws<ObjectDisposedException>(() => _materialSet.OutlineWidth = 10);
		}

		[Test]
		public void OutlineWidth_SetsMaterialParams()
		{
			var width = UnityEngine.Random.Range(OutlineRenderer.MinWidth, OutlineRenderer.MaxWidth);
			_materialSet.OutlineWidth = width;

			Assert.AreEqual(width, _materialSet.HPassMaterial.GetInt(_materialSet.WidthNameId));
			Assert.AreEqual(width, _materialSet.VPassBlendMaterial.GetInt(_materialSet.WidthNameId));
		}

		[Test]
		public void OutlineMode_SetsMaterialParams()
		{
			_materialSet.OutlineMode = OutlineMode.Solid;
			Assert.AreEqual(OutlineRenderer.SolidIntensity, _materialSet.VPassBlendMaterial.GetFloat(_materialSet.IntensityNameId));
		}

		[Test]
		public void OutlineIntensity_SetsMaterialParams()
		{
			var intensity = UnityEngine.Random.Range(OutlineRenderer.MinIntensity, OutlineRenderer.MaxIntensity);

			_materialSet.OutlineIntensity = intensity;

			Assert.AreEqual(intensity, _materialSet.VPassBlendMaterial.GetFloat(_materialSet.IntensityNameId));
		}
	}
}
