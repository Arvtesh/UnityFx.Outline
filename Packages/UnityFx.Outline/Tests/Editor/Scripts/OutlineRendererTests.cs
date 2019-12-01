// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace UnityFx.Outline
{
	[Category("OutlineRenderer"), TestOf(typeof(OutlineRenderer))]
	public class OutlineRendererTests : IDisposable
	{
		private CommandBuffer _commandBuffer;
		private OutlineRenderer _renderer;

		[SetUp]
		public void Init()
		{
			_commandBuffer = new CommandBuffer();
			_renderer = new OutlineRenderer(_commandBuffer, BuiltinRenderTextureType.CameraTarget);
		}

		[TearDown]
		public void Dispose()
		{
			_commandBuffer.Dispose();
		}

		[Test]
		public void Dispose_CanBeCalledMultipleTimes()
		{
			_renderer.Dispose();
			_renderer.Dispose();
		}

		[Test]
		public void RenderSingleObject_ThrowsIfNullArguments()
		{
			Assert.Throws<ArgumentNullException>(() => _renderer.Render(default(IList<Renderer>), null, null));
			Assert.Throws<ArgumentNullException>(() => _renderer.Render(default(Renderer), null, null));
		}
	}
}
