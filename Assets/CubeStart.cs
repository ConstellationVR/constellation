using UnityEngine;
using System.Collections;

public class CubeStart : MonoBehaviour {
	public static int speed = 142;
	public static int maxCharsPerLine = 20;

	public SpringJoint radiusSpring;
	public TextMesh text;
	public TextMesh text2;
	private bool launchDone = false;
	private Rigidbody rb;
	public string assocText;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.AddForce (this.transform.forward * speed);
		radiusSpring.spring = 0f;

		// format the text so that it fits in a nice box
		string finalText = FormatText (assocText);
		text.text = finalText;
		text2.text = finalText;
		/*
		// we want to resize the parent to be a cube that fits exactly around the text mesh bounds
		Bounds textBounds = text.GetComponent<Renderer> ().bounds;
		this.transform.position = textBounds.center;
		this.transform.localScale = new Vector3 (textBounds.extents.x, textBounds.extents.y, 0f);*/
	}
	
	// Update is called once per frame
	void Update () {
		if (launchDone && rb.velocity.magnitude < 0.01) {
			radiusSpring.spring = 900f;
		}
	}

	public static string FormatText(string assocText) {
		int numLines = assocText.Length / maxCharsPerLine;
		string finalStr = "";
		for (int i = 0; i < numLines; i++) {
			if(assocText.Length - i * maxCharsPerLine > maxCharsPerLine) {
				finalStr += assocText.Substring(i * maxCharsPerLine, maxCharsPerLine) + "\n";
			} else {
				finalStr += assocText.Substring(i*maxCharsPerLine);
			}
		}
		return finalStr;
	}
}
