using UnityEngine;
using System.Collections;
using System.Collections.Generic; //for List
using SimpleJSON;

public class GenerateGraphJson : MonoBehaviour {

	private class Tuple {
		private string left; 
		private string right; 

		public Tuple(string left, string right){
			this.left = left; 
			this.right = right; 
		}

		public string getLeft() {
			return this.left; 
		}

		public void setLeft(string val) {
			this.left = val; 
		}

		public string getRight() {
			return this.right; 
		}
	
		public void setRight(string val) {
			this.right = val;
		}
	}

	/** 
	 * Call this function after a gesture indicating that you want to send the data to the 
	 * web server. 
	 * 
	 * Makes a POST request to endpoint, passing edges data in json format. 
	 */
	public void callEndPoint() {
		//TODO
	}

	/** 
	 * Returns a string of formatted Json data. 
	 * -- can be used to construct data structures in other formats i.e. d3 web view 
	 */
	public string returnEdgesJson() {
		// TODO
		//return JSONNode[(getAllEdges ())];
		return "";
	}

	/**
	 * Returns all edges of text strings in the graph connecting GameObject nodes. 
	 * -- this can be used later to reconstruct data/graphs to share overall mind map 
	 */
	private static List<Tuple> getAllEdges() {
		List<Tuple> results = new List<Tuple> ();

		GameObject[] gameobjects = GameObject.FindGameObjectsWithTag("textCube");
		foreach(GameObject go in gameobjects)
		{
			string thisText = go.GetComponent<CubeStart>().assocText;
			SpringJoint[] componentsList = go.GetComponents<SpringJoint>();
			foreach(SpringJoint sj in componentsList)
			{
				string otherText = sj.connectedBody.gameObject.GetComponent<CubeStart>().assocText;
				Tuple edge = new Tuple(thisText, otherText);
               	results.Add (edge);
			}
		}
		return results; 
	}
}