// Copyright (C) 2018-2020 Digimation. All rights reserved.
// See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

/// <summary>
/// An FPS counter.
/// </summary>
/// <seealso href="https://wiki.unity3d.com/index.php/FramesPerSecond"/>
public class FpsCounter : MonoBehaviour
{
	private const float _updateInterval = 0.5F;

	private float _accum;
	private int _frames;
	private float _timeleft;
	private float _fps;

	public float Fps => _fps;

	public static void RenderFps(float fps, string s, Rect rc)
	{
		var text = string.Format("{0}: {1:F2}", s, fps);

		if (fps < 10)
		{
			GUI.color = Color.red;
		}
		else if (fps < 30)
		{
			GUI.color = Color.yellow;
		}
		else
		{
			GUI.color = Color.green;
		}

		GUI.Label(rc, text);
	}

	private void OnEnable()
	{
		_accum = 0;
		_frames = 0;
		_timeleft = 0;
		_fps = 0;
	}

	private void Update()
	{
		_timeleft -= Time.deltaTime;
		_accum += Time.timeScale / Time.deltaTime;

		++_frames;

		if (_timeleft <= 0.0)
		{
			_fps = _accum / _frames;
			_timeleft = _updateInterval;
			_accum = 0.0F;
			_frames = 0;
		}
	}

	private void OnGUI()
	{
		RenderFps(_fps, "FPS", new Rect(Screen.width - 80, 0, 80, 20));
	}
}
