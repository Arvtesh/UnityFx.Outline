// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace UnityFx.Outline
{
	public abstract class IOutlineSettingsTests
	{
		private IOutlineSettings _settings;
		private IChangeTracking _changeTracking;

		protected void Init(IOutlineSettings settings)
		{
			_settings = settings;
			_changeTracking = settings as IChangeTracking;
		}

		[Test]
		public void OutlineColor_SetsValue()
		{
			var color = Color.blue;
			_settings.OutlineColor = color;

			Assert.AreEqual(color, _settings.OutlineColor);
		}

		[Test]
		public void OutlineColor_SetsChanged()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineColor = Color.blue;

				Assert.IsTrue(_changeTracking.IsChanged);
			}
		}

		[Test]
		public void OutlineColor_DoesNotSetsChangedOnSameValue()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineColor = _settings.OutlineColor;

				Assert.IsFalse(_changeTracking.IsChanged);
			}
		}

		[Test]
		public void OutlineWidth_DefaultValueIsValid()
		{
			Assert.LessOrEqual(OutlineResources.MinWidth, _settings.OutlineWidth);
			Assert.GreaterOrEqual(OutlineResources.MaxWidth, _settings.OutlineWidth);
		}

		[Test]
		public void OutlineWidth_SetsValue()
		{
			var width = UnityEngine.Random.Range(OutlineResources.MinWidth, OutlineResources.MaxWidth);
			_settings.OutlineWidth = width;

			Assert.AreEqual(width, _settings.OutlineWidth);
		}

		[Test]
		public void OutlineWidth_ClampsValue()
		{
			_settings.OutlineWidth = 1000;

			Assert.AreEqual(OutlineResources.MaxWidth, _settings.OutlineWidth);

			_settings.OutlineWidth = -1000;

			Assert.AreEqual(OutlineResources.MinWidth, _settings.OutlineWidth);
		}

		[Test]
		public void OutlineWidth_SetsChanged()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineWidth = 10;

				Assert.IsTrue(_changeTracking.IsChanged);
			}
		}

		[Test]
		public void OutlineWidth_DoesNotSetsChangedOnSameValue()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineWidth = _settings.OutlineWidth;

				Assert.IsFalse(_changeTracking.IsChanged);
			}
		}

		[Test]
		public void OutlineMode_SetsValue()
		{
			_settings.OutlineRenderMode = OutlineRenderFlags.Blurred;
			Assert.AreEqual(OutlineRenderFlags.Blurred, _settings.OutlineRenderMode);

			_settings.OutlineRenderMode = OutlineRenderFlags.None;
			Assert.AreEqual(OutlineRenderFlags.None, _settings.OutlineRenderMode);
		}

		[Test]
		public void OutlineMode_SetsChanged()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineRenderMode = OutlineRenderFlags.Blurred;

				Assert.IsTrue(_changeTracking.IsChanged);
			}
		}

		[Test]
		public void OutlineMode_DoesNotSetsChangedOnSameValue()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineRenderMode = _settings.OutlineRenderMode;

				Assert.IsFalse(_changeTracking.IsChanged);
			}
		}

		[Test]
		public void OutlineIntensity_DefaultValueIsValid()
		{
			Assert.LessOrEqual(OutlineResources.MinIntensity, _settings.OutlineIntensity);
			Assert.GreaterOrEqual(OutlineResources.MaxIntensity, _settings.OutlineIntensity);
		}

		[Test]
		public void OutlineIntensity_SetsValue()
		{
			var intensity = UnityEngine.Random.Range(OutlineResources.MinIntensity, OutlineResources.MaxIntensity);

			_settings.OutlineIntensity = intensity;

			Assert.AreEqual(intensity, _settings.OutlineIntensity);
		}

		[Test]
		public void OutlineIntensity_ClampsValue()
		{
			_settings.OutlineIntensity = 1000;

			Assert.AreEqual(OutlineResources.MaxIntensity, _settings.OutlineIntensity);

			_settings.OutlineIntensity = -1000;

			Assert.AreEqual(OutlineResources.MinIntensity, _settings.OutlineIntensity);
		}

		[Test]
		public void OutlineIntensity_SetsChanged()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineIntensity = 21;

				Assert.IsTrue(_changeTracking.IsChanged);
			}
		}

		[Test]
		public void OutlineIntensity_DoesNotSetsChangedOnSameValue()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineIntensity = _settings.OutlineIntensity;

				Assert.IsFalse(_changeTracking.IsChanged);
			}
		}
	}
}
