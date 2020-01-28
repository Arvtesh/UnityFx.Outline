// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
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
	[Category("OutlineSettings"), TestOf(typeof(OutlineSettings))]
	public class OutlineSettingsTests : IOutlineSettingsTests, IDisposable
	{
		private OutlineSettings _settings;

		[SetUp]
		public void Init()
		{
			_settings = ScriptableObject.CreateInstance<OutlineSettings>();
			Init(_settings);
		}

		[TearDown]
		public void Dispose()
		{
			UnityEngine.Object.DestroyImmediate(_settings);
		}
	}
}
