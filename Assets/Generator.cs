using UnityEngine;
using System.Collections;
using SimpleJSON; 	
using System;

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
			StartCoroutine("ProcessSpeech"); // Disabled for testing

			//generate("Steve Jobs");
		}
	}

	public void generate(String text) {
		GameObject cube = (GameObject) Instantiate(cubePrefab, Vector3.zero + this.transform.forward.normalized / 5, this.transform.rotation);
		cube.GetComponent<CubeStart>().assocText = text;
		cube.GetComponent<CubeStart>().player = originBody;
	}

	/** 
	 * Makes a post request to the voice_to_text server to generate text to input into the scene.
	 */
	public IEnumerator ProcessSpeech() {
		string url = "http://192.168.103.30:5555/";
		WWW www = new WWW (url);
		string textFromSpeech;
		yield return www;
		if (www.error == null) {
			Debug.Log (url);
			string jsonStr = www.data;
			JSONNode jsn = JSON.Parse (jsonStr);
			textFromSpeech = jsn ["result"];
			generate(textFromSpeech);
			return true; 
		} else {
			Debug.Log ("Error with speech processing");
			return false; 
		}
	}
}
