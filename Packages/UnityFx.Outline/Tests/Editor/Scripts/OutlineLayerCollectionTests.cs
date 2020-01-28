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
	[Category("OutlineLayerCollection"), TestOf(typeof(OutlineLayerCollection))]
	public class OutlineLayerCollectionTests : IDisposable
	{
		private OutlineLayerCollection _layerCollection;

		[SetUp]
		public void Init()
		{
			_layerCollection = ScriptableObject.CreateInstance<OutlineLayerCollection>();
		}

		[TearDown]
		public void Dispose()
		{
			UnityEngine.Object.DestroyImmediate(_layerCollection);
		}

		[Test]
		public void DefaultStateIsValid()
		{
			Assert.IsTrue(_layerCollection.IsChanged);
			Assert.IsFalse(_layerCollection.IsReadOnly);
			Assert.IsEmpty(_layerCollection);
			Assert.Zero(_layerCollection.Count);
		}

		[Test]
		public void Add_ThrowsIfArgumentIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => _layerCollection.Add(null));
		}

		[Test]
		public void Add_ThrowsIfLayerBelongsToAnotherCollection()
		{
			var anotherLayerCollection = ScriptableObject.CreateInstance<OutlineLayerCollection>();
			var layer = new OutlineLayer();

			try
			{
				anotherLayerCollection.Add(layer);
				Assert.Throws<InvalidOperationException>(() => _layerCollection.Add(layer));
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(anotherLayerCollection);
			}
		}

		[Test]
		public void Add_DoesNotSetChangedOnError()
		{
			_layerCollection.AcceptChanges();

			try
			{
				_layerCollection.Add(null);
			}
			catch
			{
			}

			Assert.IsFalse(_layerCollection.IsChanged);
		}

		[Test]
		public void Add_SetsCount()
		{
			_layerCollection.Add(new OutlineLayer());

			Assert.AreEqual(1, _layerCollection.Count);
		}

		[Test]
		public void Add_SetsChanged()
		{
			_layerCollection.AcceptChanges();
			_layerCollection.Add(new OutlineLayer());

			Assert.IsTrue(_layerCollection.IsChanged);
		}

		[Test]
		public void Insert_ThrowsIfArgumentIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => _layerCollection.Insert(0, null));
		}

		[Test]
		public void Insert_ThrowsIfLayerBelongsToAnotherCollection()
		{
			var anotherLayerCollection = ScriptableObject.CreateInstance<OutlineLayerCollection>();
			var layer = new OutlineLayer();

			try
			{
				anotherLayerCollection.Add(layer);
				Assert.Throws<InvalidOperationException>(() => _layerCollection.Insert(0, layer));
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(anotherLayerCollection);
			}
		}

		[Test]
		public void Insert_DoesNotSetChangedOnError()
		{
			_layerCollection.AcceptChanges();

			try
			{
				_layerCollection.Insert(0, null);
			}
			catch
			{
			}

			Assert.IsFalse(_layerCollection.IsChanged);
		}

		[Test]
		public void Insert_SetsCount()
		{
			_layerCollection.Insert(0, new OutlineLayer());

			Assert.AreEqual(1, _layerCollection.Count);
		}

		[Test]
		public void Insert_SetsChanged()
		{
			_layerCollection.AcceptChanges();
			_layerCollection.Insert(0, new OutlineLayer());

			Assert.IsTrue(_layerCollection.IsChanged);
		}

		[Test]
		public void Remove_DoesNotThrowOnNullArgument()
		{
			Assert.DoesNotThrow(() => _layerCollection.Remove(null));
		}

		[Test]
		public void Remove_DoesNotSetChangedOnError()
		{
			_layerCollection.AcceptChanges();
			_layerCollection.Remove(null);

			Assert.IsFalse(_layerCollection.IsChanged);
		}

		[Test]
		public void Remove_DoesNotSetChangedIfNotfound()
		{
			_layerCollection.AcceptChanges();
			_layerCollection.Remove(new OutlineLayer());

			Assert.IsFalse(_layerCollection.IsChanged);
		}

		[Test]
		public void Remove_SetsChanged()
		{
			var layer = new OutlineLayer();

			_layerCollection.Add(layer);
			_layerCollection.AcceptChanges();
			_layerCollection.Remove(layer);

			Assert.IsTrue(_layerCollection.IsChanged);
		}

		[Test]
		public void Remove_SetsCount()
		{
			var layer = new OutlineLayer();

			_layerCollection.Add(layer);
			_layerCollection.Remove(layer);

			Assert.Zero(_layerCollection.Count);
		}

		[Test]
		public void Clear_ResetsCount()
		{
			_layerCollection.Add(new OutlineLayer());
			_layerCollection.Clear();

			Assert.Zero(_layerCollection.Count);
		}

		[Test]
		public void Clear_SetsChanged()
		{
			_layerCollection.Add(new OutlineLayer());
			_layerCollection.AcceptChanges();
			_layerCollection.Clear();

			Assert.IsTrue(_layerCollection.IsChanged);
		}

		[Test]
		public void Clear_DoesNotSetChangedIfEmpty()
		{
			_layerCollection.AcceptChanges();
			_layerCollection.Clear();

			Assert.IsFalse(_layerCollection.IsChanged);
		}

		[Test]
		public void Contains_DoesNotThrowIfArgumentIsNull()
		{
			_layerCollection.Contains(null);
		}

		[Test]
		public void Contains_DoesNotSetChangedOnError()
		{
			_layerCollection.AcceptChanges();

			try
			{
				_layerCollection.Contains(null);
			}
			catch
			{
			}

			Assert.IsFalse(_layerCollection.IsChanged);
		}

		[Test]
		public void Contains_SearchesArgument()
		{
			var layer = new OutlineLayer();

			Assert.IsFalse(_layerCollection.Contains(layer));

			_layerCollection.Add(layer);

			Assert.IsTrue(_layerCollection.Contains(layer));
		}

		[Test]
		public void Contains_DoesNotSetChanged()
		{
			var layer = new OutlineLayer();

			_layerCollection.AcceptChanges();
			_layerCollection.Contains(layer);

			Assert.IsFalse(_layerCollection.IsChanged);

			_layerCollection.Add(layer);
			_layerCollection.AcceptChanges();
			_layerCollection.Contains(layer);

			Assert.IsFalse(_layerCollection.IsChanged);
		}

		[Test]
		public void IsChanged_TracksLayerChanges()
		{
			var layer = new OutlineLayer();

			_layerCollection.Add(layer);
			_layerCollection.AcceptChanges();

			layer.OutlineWidth = 17;

			Assert.IsTrue(_layerCollection.IsChanged);
		}

		[Test]
		public void AcceptChanges_ResetsChanged()
		{
			_layerCollection.Add(new OutlineLayer());
			_layerCollection.AcceptChanges();

			Assert.IsFalse(_layerCollection.IsChanged);
		}

		[Test]
		public void SortedLayers_IsSortedByPriority()
		{
			var layer1 = new OutlineLayer()
			{
				Priority = 2
			};

			var layer2 = new OutlineLayer();
			var layer3 = new OutlineLayer();

			_layerCollection.Add(layer1);
			_layerCollection.Add(layer2);
			_layerCollection.Add(layer3);

			Assert.AreEqual(layer2, _layerCollection.SortedLayers[0]);
			Assert.AreEqual(layer3, _layerCollection.SortedLayers[1]);
			Assert.AreEqual(layer1, _layerCollection.SortedLayers[2]);
		}
	}
}
