using UnityEngine;
using System.Collections;

public class Generator : MonoBehaviour {
	public GameObject cubePrefab;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("space")) {
			//if(!Physics.Raycast (this.transform.position, this.transform.forward, 40))
				Instantiate(cubePrefab, this.transform.position, this.transform.rotation);
		}
	}
}
