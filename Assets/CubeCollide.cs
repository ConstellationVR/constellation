using UnityEngine;
using System.Collections;

public class CubeCollide : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void onCollisionEnter (Collision col) {
		// TODO (zliu): we probably have to add logic here to make sure the spring is only
		// added if the user lets go of the object (Kinect integration).
		
		// TODO: while it's being held on top of the other object, change text color to green
		// TODO: wait until release
		// TODO: on release, change text color back to white
		
		// on release, add a repulsive force between the objects -- or just use spring??
		
		Debug.Log ("hi");
		// then add a spring that acts as a rigid rod to keep them tied together
		SpringJoint newSpring = this.transform.parent.gameObject.AddComponent<SpringJoint> ();
		Debug.Log (col.gameObject.name);
		newSpring.connectedBody = col.gameObject.GetComponent<Rigidbody>();
		newSpring.minDistance = 0.02f;
		newSpring.maxDistance = 0.08f;
		newSpring.spring = 900f;
		
		// also add a visible line that always connects the centers. the connection code is in 
		// the line object's script
	}
}
