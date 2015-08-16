using UnityEngine;
using System.Collections;
using SimpleJSON; 	
using System;

public class Generator : MonoBehaviour {
	public GameObject cubePrefab;
	public GameObject originBody;
	public TextMesh text;
	public TextMesh text2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("space")) {
			//if(!Physics.Raycast (this.transform.position, this.transform.forward, 40))
			StartCoroutine("ProcessSpeech");
		}
	}

	public void generate() {
		GameObject cube = (GameObject) Instantiate(cubePrefab, Vector3.zero + this.transform.forward.normalized / 5, Quaternion.identity);
		cube.GetComponent<CubeStart>().assocText = "hello";
		cube.GetComponent<CubeStart>().player = originBody;
	}

	IEnumerator ProcessSpeech() {
		string url = "http://192.168.103.30:5555/";
		WWW www = new WWW (url);
		string textFromSpeech;
		yield return www;
		if (www.error == null) {
			Debug.Log (url);
			string jsonStr = www.data;
			JSONNode jsn = JSON.Parse (jsonStr);
			textFromSpeech = jsn ["result"];
			text.text = textFromSpeech;
			text2.text = textFromSpeech;
			Instantiate (cubePrefab, this.transform.position, this.transform.rotation);
		} else {
			Debug.Log ("Error with speech processing");
		}
	}
}
