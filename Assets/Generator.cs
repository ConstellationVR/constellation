using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON; 	
using System;

public class Generator : MonoBehaviour {
	public GameObject cubePrefab;
	public GameObject originBody;
	public Text suggestText;

	private bool waitingForResult = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("space")) {
			//if(!Physics.Raycast (this.transform.position, this.transform.forward, 40))
			launchSpeech(); // Disabled for testing

			//generate("Steve Jobs");
		} else if (Input.GetKeyDown ("t")) {
			generate ("Steve Jobs");
		} else if (Input.GetKeyDown ("y")) {
			generate ("Hilary Clinton");
		}
	}

	public void generate(String text) {
		GameObject cube = (GameObject) Instantiate(cubePrefab, Vector3.zero + this.transform.forward.normalized / 5, this.transform.rotation);
		cube.GetComponent<CubeStart>().assocText = text;
		cube.GetComponent<CubeStart>().player = originBody;
		cube.GetComponent<CubeStart> ().generatorObj = this;
	}

	public void setSuggest(string text) {
		suggestText.text = text;
	}

	public void launchSpeech() {
		if (waitingForResult) return;
		StartCoroutine ("ProcessSpeech");
		waitingForResult = true;
	}

	/** 
	 * Makes a post request to the voice_to_text server to generate text to input into the scene.
	 */
	private IEnumerator ProcessSpeech() {
		string url = "http://fcb60fc2.ngrok.io/";
		WWW www = new WWW (url);
		string textFromSpeech;
		yield return www;
		waitingForResult = false;
		if (www.error == null) {
			Debug.Log (url);
			string jsonStr = www.data;
			JSONNode jsn = JSON.Parse (jsonStr);
			textFromSpeech = jsn ["result"];
			generate(textFromSpeech);
			return true; 
		} else {
			Debug.Log ("Error with speech processing" + www.error);
			return false; 
		}
	}
}
