using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Shared {
public class DebugStatistics : MonoBehaviour {

	[SerializeField, Range(0.0166f, 1f)] private float FPSSampleDuration;
	[SerializeField] private TextMeshProUGUI debugDisplay;

	private int frameCount;
	private float frameTime;
	private float bestFpsTime = 1e10f;
	private float worstFpstime;

    // Update is called once per frame
    void Update() {
        float frameDuration = Time.unscaledDeltaTime;
		frameCount++;
		frameTime += frameDuration;

		if (frameTime < bestFpsTime && frameTime > 0.01f) {
			bestFpsTime = frameTime;
		}
		if (frameTime > worstFpstime) {
			worstFpstime = frameTime;
		}

		if (frameTime >= FPSSampleDuration) {
			float fps = frameCount/frameTime;
			float currentFrameTime = frameTime/frameCount;
			float bestFps = 1.0f/bestFpsTime;
			float worstFps = 1.0f/worstFpstime;
			string str = ConstructDisplayString(fps, currentFrameTime*1000f, bestFps, bestFpsTime*1000f, worstFps, worstFpstime*1000f);
			debugDisplay.SetText(str);
			frameCount = 0;
			frameTime = 0f;
		}
    }

    private string ConstructDisplayString(float fps, float time, float bestFps, float bestTime, float worstFps, float worstTime) {
    	return  $"FPS: {fps:0.0} ({time:0.00}ms)\n" +
				$"Best: {bestFps:0.0} ({bestTime:0.00}ms)\n" +
				$"Worst: {worstFps:0.0} ({worstTime:0.00}ms)";
    }
}}
