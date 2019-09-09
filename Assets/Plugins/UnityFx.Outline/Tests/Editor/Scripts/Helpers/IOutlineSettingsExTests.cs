// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace UnityFx.Outline
{
	public abstract class IOutlineSettingsExTests : IOutlineSettingsTests
	{
		private IOutlineSettingsEx _settings;
		private IChangeTracking _changeTracking;

		protected void Init(IOutlineSettingsEx settings)
		{
			_settings = settings;
			_changeTracking = settings as IChangeTracking;
			base.Init(settings);
		}

		[Test]
		public void OutlineSettings_SetsValue()
		{
			var settings = ScriptableObject.CreateInstance<OutlineSettings>();

			try
			{
				_settings.OutlineSettings = settings;

				Assert.AreEqual(settings, _settings.OutlineSettings);

				_settings.OutlineSettings = null;

				Assert.IsNull(_settings.OutlineSettings);
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(settings);
			}
		}

		[Test]
		public void OutlineSettings_SetsChanged()
		{
			if (_changeTracking != null)
			{
				var settings = ScriptableObject.CreateInstance<OutlineSettings>();

				try
				{
					_changeTracking.AcceptChanges();
					_settings.OutlineSettings = settings;

					Assert.IsTrue(_changeTracking.IsChanged);
				}
				finally
				{
					UnityEngine.Object.DestroyImmediate(settings);
				}
			}
		}

		[Test]
		public void OutlineSettings_DoesNotSetsChangedOnSameValue()
		{
			if (_changeTracking != null)
			{
				_changeTracking.AcceptChanges();
				_settings.OutlineSettings = _settings.OutlineSettings;

				Assert.IsFalse(_changeTracking.IsChanged);
			}
		}
	}
}
