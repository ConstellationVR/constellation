using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
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
	public GameObject lrChild;

	private bool launchDone = false;
	private Rigidbody rb;
	private BoxCollider bc;

	// Map from external textCube game objects to the corresponding child that is generated just to create 
	// the line renderer for that connection. This favors a quick lookup to see if the external game
	// object has been paired with already.
	private Dictionary<GameObject, GameObject> links = new Dictionary<GameObject, GameObject >();

	private float totalTime = 0f;

	//public bool hasSprings = false;
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
		bc.size = new Vector3 (textBounds.extents.x * 2 + .2f, textBounds.extents.y * 2 + .2f, .5f);
		launchDone = true;

		StartCoroutine ("ProcessText");

	}
	
	// Update is called once per frame
	void Update () {
		if (totalTime > 1f) {
			radiusSpring.spring = 1000f;
		} else {
			totalTime += Time.deltaTime;
		}

		foreach (KeyValuePair<GameObject, GameObject> connection in links) {
			//connection.Value or connection.Key
			// Update the line renderer in the child object of this (connection.Value)
			LineRenderer currLr = connection.Value.GetComponent<LineRenderer>();
			currLr.SetPosition(0, this.transform.localPosition);
			currLr.SetPosition(1, connection.Key.transform.position);
		}
		// loop through all connections to update all necessary line renderers
		/*if (this.CompareTag("springNode")){
			SpringJoint[] joints = GetComponents<SpringJoint>();
			foreach (SpringJoint joint in joints) {
				if(joint.connectedBody.gameObject.CompareTag("springNode")){
					LineRenderer lr = this.transform.gameObject.AddComponent<LineRenderer> ();
					lr.SetPosition(0, this.transform.localPosition);
					lr.SetPosition(1, joint.connectedBody.transform.position);
				}
			}
		}*/



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
		Debug.Log ("Exiting linking mode");
		shouldHighlightLinks = false;
		if (currentCollidingElement != null) {
			Debug.Log ("Collided with element" + currentCollidingElement);

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

			// making a new connection
			if(!links.ContainsKey(currentCollidingElement)) {
				// create new child object of 'this'
				GameObject childElement = (GameObject) Instantiate(lrChild, transform.position, transform.rotation);
				childElement.transform.parent = this.gameObject.transform;

				// the stuff immediately below is actually not necessary because we can just use a child prefab with a line renderer
				// with the correct settings already
				/*// add initial line renderer to the child object we just created
				LineRenderer lr = this.transform.gameObject.AddComponent<LineRenderer> ();
				// set its persistent properties (material, etc.)
				*/

				// set the line renderer position stuff (temporary)
				LineRenderer lr = childElement.GetComponent<LineRenderer>();
				lr.SetPosition(0, this.transform.localPosition);
				lr.SetPosition(1, currentCollidingElement.transform.position);

				// add this key-value pair to the dictionary
				links.Add(currentCollidingElement, childElement);
			}
		
			// TODO: also add a visible line that always connects the centers. the connection code is in 
			// the line object's script

			currentCollidingElement = null;
		}
	}

	void OnTriggerEnter(Collider col) {
		Debug.Log ("OnTriggerEnter called with " + col.gameObject);
		if (currentCollidingElement != null) {
			// TODO: while it's being held on top of the other object, change text color to green
			// TODO: wait until release
			// TODO: on release, change text color back to white
		} else {
		}
		currentCollidingElement = col.gameObject;
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
		string url = "http://192.168.103.30:5000/interpret/" + System.Uri.EscapeUriString(assocText);
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
