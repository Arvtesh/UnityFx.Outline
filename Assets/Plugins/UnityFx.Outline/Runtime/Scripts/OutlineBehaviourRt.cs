// Copyright (C) 2019 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Outline
{
	/// <summary>
	/// This should be attached to each <see cref="Renderer"/> for <see cref="OutlineBehaviour"/> to work as expected.
	/// </summary>
	/// <seealso cref="OutlineBehaviour"/>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	internal class OutlineBehaviourRt : MonoBehaviour
	{
		public OutlineBehaviour Parent;

		private void OnWillRenderObject()
		{
			if (isActiveAndEnabled && Parent)
			{
				Parent.OnWillRenderObjectRt();
			}
		}
	}
}
