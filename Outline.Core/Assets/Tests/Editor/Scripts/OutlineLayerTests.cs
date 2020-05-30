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
	[Category("OutlineLayer"), TestOf(typeof(OutlineLayer))]
	public class OutlineLayerTests : IOutlineSettingsTests, IDisposable
	{
		private OutlineLayer _layer;

		[SetUp]
		public void Init()
		{
			_layer = new OutlineLayer("TestLayer");
			Init(_layer);
		}

		[TearDown]
		public void Dispose()
		{
		}

		[Test]
		public void DefaultStateIsValid()
		{
			Assert.IsFalse(_layer.IsReadOnly);
			Assert.IsEmpty(_layer);
			Assert.Zero(_layer.Count);
			Assert.AreEqual("TestLayer", _layer.Name);
			Assert.AreEqual(-1, _layer.Index);
		}

		[Test]
		public void Add_ThrowsIfArgumentIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => _layer.Add(null));
		}

		[Test]
		public void Add_SetsCount()
		{
			_layer.Add(new GameObject());

			Assert.AreEqual(1, _layer.Count);
		}

		[Test]
		public void Add_FiltersRenderesByLayer()
		{
			var go = new GameObject("r1", typeof(MeshRenderer));
			var go2 = new GameObject("r2", typeof(MeshRenderer));
			var layers = new OutlineLayerCollection();
			layers.IgnoreLayerMask = 1 << LayerMask.NameToLayer("TransparentFX");
			layers.Add(_layer);

			go2.layer = LayerMask.NameToLayer("TransparentFX");
			go2.transform.SetParent(go.transform, false);

			_layer.Add(go);
			_layer.TryGetRenderers(go, out var r);

			Assert.AreEqual(1, r.Count);
			Assert.IsTrue(r.Contains(go.GetComponent<Renderer>()));
			Assert.IsFalse(r.Contains(go2.GetComponent<Renderer>()));
		}

		[Test]
		public void Remove_DoesNotThrowOnNullArgument()
		{
			Assert.DoesNotThrow(() => _layer.Remove(null));
		}

		[Test]
		public void Remove_SetsCount()
		{
			var go = new GameObject();

			_layer.Add(go);
			_layer.Remove(go);

			Assert.Zero(_layer.Count);
		}

		[Test]
		public void Clear_ResetsCount()
		{
			_layer.Add(new GameObject());
			_layer.Clear();

			Assert.Zero(_layer.Count);
		}

		[Test]
		public void Contains_DoesNotThrowIfArgumentIsNull()
		{
			_layer.Contains(null);
		}

		[Test]
		public void Contains_SearchesArgument()
		{
			var go = new GameObject();

			Assert.IsFalse(_layer.Contains(go));

			_layer.Add(go);

			Assert.IsTrue(_layer.Contains(go));
		}
	}
}
