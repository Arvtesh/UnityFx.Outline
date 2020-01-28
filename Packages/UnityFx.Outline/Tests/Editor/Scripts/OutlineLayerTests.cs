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
	public class OutlineLayerTests : IOutlineSettingsExTests, IDisposable
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
			Assert.IsTrue(_layer.IsChanged);
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
		public void Add_DoesNotSetChangedOnError()
		{
			_layer.AcceptChanges();

			try
			{
				_layer.Add(null);
			}
			catch
			{
			}

			Assert.IsFalse(_layer.IsChanged);
		}

		[Test]
		public void Add_SetsCount()
		{
			_layer.Add(new GameObject());

			Assert.AreEqual(1, _layer.Count);
		}

		[Test]
		public void Add_SetsChanged()
		{
			_layer.AcceptChanges();
			_layer.Add(new GameObject());

			Assert.IsTrue(_layer.IsChanged);
		}

		[Test]
		public void Add_FiltersRenderesByLayer()
		{
			var go = new GameObject("r1", typeof(MeshRenderer));
			var go2 = new GameObject("r2", typeof(MeshRenderer));

			go2.layer = LayerMask.NameToLayer("TransparentFX");
			go2.transform.SetParent(go.transform, false);

			ICollection<Renderer> r;

			_layer.Add(go, "TransparentFX");
			_layer.TryGetRenderers(go, out r);

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
		public void Remove_DoesNotSetChangedOnError()
		{
			_layer.AcceptChanges();
			_layer.Remove(null);

			Assert.IsFalse(_layer.IsChanged);
		}

		[Test]
		public void Remove_DoesNotSetChangedIfNotfound()
		{
			_layer.AcceptChanges();
			_layer.Remove(new GameObject());

			Assert.IsFalse(_layer.IsChanged);
		}

		[Test]
		public void Remove_SetsChanged()
		{
			var go = new GameObject();

			_layer.Add(go);
			_layer.AcceptChanges();
			_layer.Remove(go);

			Assert.IsTrue(_layer.IsChanged);
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
		public void Clear_SetsChanged()
		{
			_layer.Add(new GameObject());
			_layer.AcceptChanges();
			_layer.Clear();

			Assert.IsTrue(_layer.IsChanged);
		}

		[Test]
		public void Clear_DoesNotSetChangedIfEmpty()
		{
			_layer.AcceptChanges();
			_layer.Clear();

			Assert.IsFalse(_layer.IsChanged);
		}

		[Test]
		public void Contains_DoesNotThrowIfArgumentIsNull()
		{
			_layer.Contains(null);
		}

		[Test]
		public void Contains_DoesNotSetChangedOnError()
		{
			_layer.AcceptChanges();

			try
			{
				_layer.Contains(null);
			}
			catch
			{
			}

			Assert.IsFalse(_layer.IsChanged);
		}

		[Test]
		public void Contains_SearchesArgument()
		{
			var go = new GameObject();

			Assert.IsFalse(_layer.Contains(go));

			_layer.Add(go);

			Assert.IsTrue(_layer.Contains(go));
		}

		[Test]
		public void Contains_DoesNotSetChanged()
		{
			var go = new GameObject();

			_layer.AcceptChanges();
			_layer.Contains(go);

			Assert.IsFalse(_layer.IsChanged);

			_layer.Add(go);
			_layer.AcceptChanges();
			_layer.Contains(go);

			Assert.IsFalse(_layer.IsChanged);
		}

		[Test]
		public void AcceptChanges_ResetsChanged()
		{
			_layer.AcceptChanges();

			Assert.IsFalse(_layer.IsChanged);
		}
	}
}
