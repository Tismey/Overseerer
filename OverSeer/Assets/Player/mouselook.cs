using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouselook : MonoBehaviour {
	public float Mousesensitivity =100f;
	public Transform Playerbody;
	 float xRotation = 0f;
	 float camerashake = 0f; 
	 float shakeangle = 0f;
	public float speedmultiplier= 2f;
	 float speedshake = 5f;
	public float speedset = 3f;
	
	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;

	}
	
	// Update is called once per frame
	void Update () {

	
		
		float mouseX = Input.GetAxis("Mouse X") * Mousesensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis ("Mouse Y") * Mousesensitivity * Time.deltaTime;

		Playerbody.Rotate (Vector3.up * mouseX); 

		xRotation = Mathf.Clamp (xRotation, -90f, 90f);
		xRotation -= mouseY;
		transform.localRotation = Quaternion.Euler (xRotation, 0f, Mathf.Cos(camerashake)*shakeangle);

	}
}
