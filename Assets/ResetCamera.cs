using UnityEngine;
using System.Collections;

public class ResetCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		try {
			transform.position = -Camera.current.transform.localPosition;
		} catch (System.NullReferenceException e) {
		}
	}
}
