using UnityEngine;
using System.Collections;

public class CubeStart : MonoBehaviour {
	public int speed = 10000;
	// Use this for initialization
	void Start () {
		Rigidbody rb = GetComponent<Rigidbody> ();
		rb.AddForce (this.transform.forward * speed);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
