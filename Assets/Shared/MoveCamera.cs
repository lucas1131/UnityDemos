using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shared {

public class MoveCamera : MonoBehaviour {
	
    [SerializeField] float speed = 0.015f;
    [SerializeField] float runMultiplier = 4f;
    [SerializeField] float crouchMultiplier = 0.25f;
    [SerializeField] float sensitivity = 1.0f;
 
    private Camera cam;
    private Vector3 anchorPoint;
    private Quaternion anchorRot;
 
    private void Awake(){
        cam = GetComponent<Camera>();
    }
   
    void Update(){
        Vector3 move = Vector3.zero;
        float unitsPerSecond = speed * Time.deltaTime * 100;
        if(Input.GetKey(KeyCode.W))
            move += Vector3.forward * unitsPerSecond;
        if (Input.GetKey(KeyCode.S))
            move -= Vector3.forward * unitsPerSecond;
        if (Input.GetKey(KeyCode.D))
            move += Vector3.right * unitsPerSecond;
        if (Input.GetKey(KeyCode.A))
            move -= Vector3.right * unitsPerSecond;
        if (Input.GetKey(KeyCode.E))
            move += Vector3.up * unitsPerSecond;
        if (Input.GetKey(KeyCode.Q))
            move -= Vector3.up * unitsPerSecond;

        if (Input.GetKey(KeyCode.LeftShift))
        	move *= runMultiplier;
        if (Input.GetKey(KeyCode.LeftAlt))
            move *= crouchMultiplier;

        transform.Translate(move);
 
        if (Input.GetMouseButtonDown(1)){
            anchorPoint = new Vector3(Input.mousePosition.y, -Input.mousePosition.x);
            anchorRot = transform.rotation;
        }

        if (Input.GetMouseButton(1)){
            Quaternion rot = anchorRot;
            Vector3 dif = anchorPoint - new Vector3(Input.mousePosition.y, -Input.mousePosition.x);
            rot.eulerAngles += dif * sensitivity;
            transform.rotation = rot;
        }
    }
}}
