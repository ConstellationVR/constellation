using UnityEngine;
using System.Collections;

public class ResetCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.position = -Camera.current.transform.localPosition;
	}
}
