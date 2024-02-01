using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clock {

public class Clock : MonoBehaviour {

	[SerializeField] private float clockSpeed = 1.0f;
	[SerializeField] private float clockTickTime = 1.0f;

	[SerializeField] private ClockHandle secondHandle;
	[SerializeField] private ClockHandle minuteHandle;
	[SerializeField] private ClockHandle hourHandle;

	private float clockTime;
	private bool clockStopped;
	private Coroutine tickRoutine;

	private void Start() {
		clockTime = 0;
		ResetClock();
		StartClock();
	}

	public void StartClock() {
		clockStopped = false;
		tickRoutine = StartCoroutine(Tick());
	}

	public void StopClock() {
		clockStopped = true;
		StopCoroutine(tickRoutine);
		tickRoutine = null;
	}

	public void ResetClock() {
		SetTime(0, 0, 0);
	}

	public void SetTime(float second, float minute, float hour){
		secondHandle.SetTime(second);
		minuteHandle.SetTime(minute);
		hourHandle.SetTime(hour);
	}

	public IEnumerator Tick(){

		while(!clockStopped) {
			yield return new WaitForSeconds(clockTickTime);

			clockTime += 1f*clockSpeed;

			float seconds = clockTime;
			float minutes = seconds/60f;
			float hours = minutes/60f;

			secondHandle.SetTime(seconds);
			minuteHandle.SetTime(minutes);
			hourHandle.SetTime(hours);
		}
	}
}}
