using UnityEngine;
using System.Collections;

public class CubeStart : MonoBehaviour {
	public static int speed = 142;
	public static int maxCharsPerLine = 15;
	public static float sphereRadius = 1.0f;

	public GameObject player;
	public SpringJoint radiusSpring;
	public TextMesh text;
	public TextMesh text2;
	public string assocText;

	private bool launchDone = false;
	private Rigidbody rb;
	private BoxCollider bc;
	

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.AddForce (this.transform.forward * speed);
		radiusSpring.spring = 0f;

		// format the text so that it fits in a nice box
		string finalText = FormatText (assocText);
		text.text = finalText;
		text2.text = finalText;

		// we want to add a collider that fits exactly around the text.
		Bounds textBounds = text.GetComponent<Renderer> ().bounds;
		this.transform.position = textBounds.center;

		bc = GetComponent<BoxCollider> ();
		bc.size = new Vector3 (textBounds.extents.x * 2, textBounds.extents.y * 2, 0f);
		launchDone = true;

	}
	
	// Update is called once per frame
	void Update () {
		if ((player.transform.position - this.transform.position).magnitude > sphereRadius) {
			radiusSpring.minDistance = sphereRadius - .05f;
			radiusSpring.maxDistance = sphereRadius + .05f;
			radiusSpring.spring = 900f;
		}

		// FOR TESTING ONLY
		if(Input.GetKeyDown("right")) {
			rb.AddForce(new Vector3(15, 0, 0));
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
}
