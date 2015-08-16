using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON; 	
using System;

public class CubeStart : MonoBehaviour {
	public static int speed = 142;
	public static int maxCharsPerLine = 15;
	public static float sphereRadius = 1.0f;

	public GameObject player;
	public SpringJoint radiusSpring;
	public TextMesh text;
	public TextMesh text2;
	public string assocText;
	public Text suggestText;

	private bool launchDone = false;
	private Rigidbody rb;
	private BoxCollider bc;

	private float totalTime = 0f;

	private bool shouldHighlightLinks = false;
	private GameObject currentCollidingElement = null;

	// Use this for initialization
	void Start () {	
		GetComponent<SpringJoint> ().connectedBody = player.GetComponent<Rigidbody> ();

		rb = GetComponent<Rigidbody> ();
		rb.AddForce (this.transform.forward * speed);
		radiusSpring.spring = 0f;
		radiusSpring.minDistance = 1.5f;
		radiusSpring.maxDistance = 1.5f;

		// format the text so that it fits in a nice box
		string finalText = FormatText (assocText);
		text.text = finalText;
		text2.text = "";
		//text2.text = finalText;

		// we want to add a collider that fits exactly around the text.
		Bounds textBounds = text.GetComponent<Renderer> ().bounds;

		bc = GetComponent<BoxCollider> ();
		bc.size = new Vector3 (textBounds.extents.x * 2 + .2f, textBounds.extents.y * 2 + .2f, .2f);
		launchDone = true;

		StartCoroutine("ProcessText");
	}
	
	// Update is called once per frame
	void Update () {
		if (totalTime > 1f) {
			radiusSpring.spring = 500f;
		} else {
			totalTime += Time.deltaTime;
		}

		// FOR TESTING ONLY
		if(Input.GetKeyDown("right")) {
			rb.AddForce(new Vector3(30, 0, 0));
		}
		// FOR TESTING ONLY
		if(Input.GetKeyDown("left")) {
			rb.AddForce(new Vector3(-30, 0, 0));
		}
		// FOR TESTING ONLY
		if(Input.GetKeyDown("up")) {
			rb.AddForce(new Vector3(0, 30, 0));
		}

		// FOR TESTING ONLY
		if(Input.GetKeyDown("down")) {
			rb.AddForce(new Vector3(0, -30, 0));
		}
	}

	// Methods called externally by sensor class to start/stop selection mode.
	public void enterLinkingMode() {
		shouldHighlightLinks = true;
	}

	public void exitLinkingMode() {
		shouldHighlightLinks = false;
		if (currentCollidingElement != null) {
			// TODO: on release, add a repulsive force between the objects -- or just use spring??

			// then add a spring that acts as a rigid rod to keep them tied together
			SpringJoint newSpring = this.transform.gameObject.AddComponent<SpringJoint> ();
			Debug.Log (currentCollidingElement.name);
			newSpring.anchor = Vector3.zero;
			newSpring.autoConfigureConnectedAnchor = false;
			newSpring.connectedAnchor = Vector3.zero;
			newSpring.connectedBody = currentCollidingElement.GetComponent<Rigidbody> ();
			newSpring.minDistance = 0.5f;
			newSpring.maxDistance = 0.8f;
			newSpring.spring = 100f;
			
			// TODO: also add a visible line that always connects the centers. the connection code is in 
			// the line object's script

			currentCollidingElement = null;
		}
	}

	void OnTriggerEnter(Collider col) {
		if (currentCollidingElement != null) {
			// TODO: while it's being held on top of the other object, change text color to green
			// TODO: wait until release
			// TODO: on release, change text color back to white
		} else {
			currentCollidingElement = col.gameObject;
		}
	}

	public static string FormatText(string assocText) {
		char[] delim = {' '};
		string[] words = assocText.Split (delim);
		int currLineLen = 0;
		string finalStr = "";
		foreach ( string word in words) {
			if (word.Length + currLineLen > maxCharsPerLine) {
				finalStr += "\n" + word + " ";
				currLineLen = 0;
			} else {
				finalStr += word + " ";
				currLineLen += word.Length;
			}
		}
		return finalStr;
	}

	IEnumerator ProcessText() {
		string url = "http://localhost:5000/interpret/" + System.Uri.EscapeUriString(assocText);
		WWW www = new WWW (url);
		string suggestStr;
		Debug.Log ("1" + url);
		yield return www;
		if (www.error == null) {
			Debug.Log(url);
			string jsonStr = www.data;
			JSONNode jsn = JSON.Parse(jsonStr);
			suggestStr = jsn["0"]["abstract"];
			if (suggestStr.Length < 1) {
				suggestStr = jsn["definitions"]["0"]["definition"];
			}
			suggestText.text = suggestStr;
		} else {
			Debug.Log("ERROR: " + www.error);
		}
	}
}
