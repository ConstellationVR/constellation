using UnityEngine;
using System.Collections;

public class CameraFacing : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody> ().freezeRotation = true;
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt (Vector3.zero);
	}
}
