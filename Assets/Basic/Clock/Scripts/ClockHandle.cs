using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clock {

public class ClockHandle : MonoBehaviour {

	[SerializeField] private Transform pivot;

    public void SetTime(float time) {
    	pivot.localRotation = Quaternion.Euler(0, 0, -6*time);
    }
}}
