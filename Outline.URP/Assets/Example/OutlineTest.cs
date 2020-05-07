using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class OutlineTest : MonoBehaviour
{
#pragma warning disable 0649

	[SerializeField]
	private GameObject[] _layer0;
	[SerializeField]
	private GameObject[] _layer1;
	[SerializeField]
	private OutlineLayerCollection _layers;

#pragma warning restore 0649

	void Start()
	{
		foreach (var go in _layer0)
		{
			_layers[0].Add(go);
		}

		foreach (var go in _layer1)
		{
			_layers[1].Add(go);
		}
	}
}
