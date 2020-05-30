// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace UnityFx.Outline
{
	[Category("OutlineBehaviour"), TestOf(typeof(OutlineBehaviour))]
	public class OutlineBehaviourTests : IOutlineSettingsTests, IDisposable
	{
		private GameObject _go;
		private OutlineBehaviour _outlineEffect;

		[SetUp]
		public void Init()
		{
			_go = new GameObject();
			_outlineEffect = _go.AddComponent<OutlineBehaviour>();
			Init(_outlineEffect);
		}

		[TearDown]
		public void Dispose()
		{
			UnityEngine.Object.DestroyImmediate(_go);
		}
	}
}
