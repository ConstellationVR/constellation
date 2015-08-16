using UnityEngine;
using System.Collections;

public class Generator : MonoBehaviour {
	public GameObject cubePrefab;
	public GameObject originBody;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("space")) {
			//if(!Physics.Raycast (this.transform.position, this.transform.forward, 40))
			generate();
		}
	}

	public void generate() {
		GameObject cube = (GameObject) Instantiate(cubePrefab, Vector3.zero + this.transform.forward.normalized / 5, Quaternion.identity);
		cube.GetComponent<CubeStart>().assocText = "hello";
		cube.GetComponent<CubeStart>().player = originBody;
	}
}
