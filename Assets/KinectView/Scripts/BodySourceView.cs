using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
	public GameObject Pointer;

	public GameObject generatorObject;

    private Dictionary<ulong, BodyData> _Bodies = new Dictionary<ulong, BodyData>();
    private BodySourceManager _BodyManager;

	private bool myoHandIsClosed = false;

	// Myo game object to connect with.
	// This object must have a ThalmicMyo script attached.
	public GameObject myo = null;
	
	// The pose from the last update. This is used to determine if the pose has changed
	// so that actions are only performed upon making them rather than every frame during
	// which they are active.
	private Pose _lastPose = Pose.Unknown;
    
    class BodyData {
		public GameObject gameObject;
		public HandStatus leftHand = new HandStatus();
		public HandStatus rightHand = new HandStatus();
		public GameObject leftPointer;
		public GameObject rightPointer;
		public SpringJoint leftHandObject;
		public SpringJoint rightHandObject;

		public BodyData(GameObject gameObject) {
			this.gameObject = gameObject;
		}
	}

	private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
   	
    void Update () 
    {
		// Access the ThalmicMyo component attached to the Myo game object.
		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();

		if (thalmicMyo.pose == Pose.Rest || thalmicMyo.pose == Pose.Fist) {
			myoHandIsClosed = thalmicMyo.pose == Pose.Fist;
		}
		
		// Check if the pose has changed since last update.
		// The ThalmicMyo component of a Myo game object has a pose property that is set to the
		// currently detected pose (e.g. Pose.Fist for the user making a fist). If no pose is currently
		// detected, pose will be set to Pose.Rest. If pose detection is unavailable, e.g. because Myo
		// is not on a user's arm, pose will be set to Pose.Unknown.
		if (thalmicMyo.pose != _lastPose) {
			Debug.Log (thalmicMyo.pose);
			_lastPose = thalmicMyo.pose;
			
			// Vibrate the Myo armband when a fist is made.
			if (thalmicMyo.pose == Pose.Fist) {
				thalmicMyo.Vibrate (VibrationType.Short);
				
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
				
				// Change material when wave in, wave out or double tap poses are made.
				//generator.generate();
			} else if (thalmicMyo.pose == Pose.WaveIn) {
				//GetComponent<Renderer>().material = waveInMaterial;
				
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			} else if (thalmicMyo.pose == Pose.WaveOut) {
				//GetComponent<Renderer>().material = waveOutMaterial;
				
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			} else if (thalmicMyo.pose == Pose.DoubleTap) {
				//GetComponent<Renderer>().material = doubleTapMaterial;
				
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
			}
		}


        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
				BodyData bd = _Bodies[trackingId];
                Destroy(bd.gameObject);
				Destroy(bd.leftPointer);
				if (bd.rightHandObject != null) Destroy(bd.rightHandObject);
				Destroy(bd.rightPointer);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }
    
    private BodyData CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
		BodyData bd = new BodyData (body);
        
		/*
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.005f, 0.005f);
            
            jointObj.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        */

		bd.rightPointer = Instantiate (Pointer);
		bd.rightPointer.name = "RightPointer";
		bd.rightPointer.transform.parent = body.transform;
        
		bd.leftPointer = Instantiate (Pointer);
		bd.leftPointer.name = "LeftPointer";
		bd.leftPointer.transform.parent = body.transform;

        return bd;
    }

	// Attempt to bind the object under the pointer
	void addSpringJoint(SpringJoint joint, Vector3 target) {
		Debug.Log ("Attaching spring");

		// 1: Try to get UI element
		PointerEventData pointer = new PointerEventData(EventSystem.current);
		// convert to a 2D position
		pointer.position = Camera.current.WorldToScreenPoint(this.transform.position);
		
		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, raycastResults);

		if (raycastResults.Count > 0) {
			RaycastResult result = raycastResults[0];
			switch (result.gameObject.tag) {
			case "MainButton":
				generatorObject.GetComponent<Generator>().generate("hello");
				break;
			}
			return;
		}

		// 2: Try to get text element
		RaycastHit hit;
		if (Physics.Raycast(this.transform.position, target, out hit)) {
			GameObject gameObject = hit.collider.gameObject;
			Debug.Log("Got object: " + gameObject);
			
			gameObject.GetComponent<CubeStart>().enterLinkingMode();
			
			joint.connectedBody = gameObject.GetComponent<Rigidbody>();
			joint.autoConfigureConnectedAnchor = false;
			joint.anchor = Vector3.zero;
			joint.connectedAnchor = Vector3.zero;
			joint.spring = 200f;
			joint.damper = 100f;
			joint.minDistance = 0;
			joint.maxDistance = 0;
			joint.breakForce = float.PositiveInfinity;
			joint.breakTorque = float.PositiveInfinity;
			joint.enableCollision = false;
			joint.enablePreprocessing = true;
		}
	}

	// Free the object if one is currently bound
	void destroySpringJoint(SpringJoint joint) {
		if (joint.connectedBody != null) {
			joint.connectedBody.gameObject.GetComponent<CubeStart> ().exitLinkingMode ();
		}
		
		Destroy(joint);
	}
    
    private void RefreshBodyObject(Kinect.Body body, BodyData bd)
    {
		Kinect.Joint headJoint = body.Joints [Kinect.JointType.Neck];
		Vector3 headPosition = GetVector3FromJoint (headJoint);

		bd.rightHand.update (Time.deltaTime, body.HandRightState);
		bd.leftHand.update (Time.deltaTime, body.HandLeftState);

		Kinect.Joint rightHandJoint = body.Joints [Kinect.JointType.HandRight];
		bd.rightPointer.transform.position = GetVector3FromJoint (rightHandJoint) - headPosition;
		bd.rightPointer.GetComponent<Renderer>().material.color = GetColorForState(myoHandIsClosed);

		Kinect.Joint leftHandJoint = body.Joints [Kinect.JointType.HandLeft];
		bd.leftPointer.transform.position = GetVector3FromJoint (leftHandJoint) - headPosition;
		bd.leftPointer.GetComponent<Renderer>().material.color = GetColorForState(bd.leftHand.isClosed);

		if (myoHandIsClosed) {
			if (bd.rightHandObject == null) {
				SpringJoint joint = bd.rightPointer.AddComponent<SpringJoint>();
				addSpringJoint(joint, bd.rightPointer.transform.position);
				bd.rightHandObject = joint;
			}
		} else {
			if (bd.rightHandObject != null) {
				destroySpringJoint(bd.rightHandObject);
				bd.rightHandObject = null;
			}
		}

		if (bd.leftHand.isClosed) {
			if (bd.leftHandObject == null) {
				SpringJoint joint = bd.leftPointer.AddComponent<SpringJoint>();
				addSpringJoint(joint, bd.leftPointer.transform.position);
				bd.leftHandObject = joint;
			}
		} else {
			if (bd.leftHandObject != null) {
				destroySpringJoint(bd.leftHandObject);
				bd.leftHandObject = null;
			}
		}

		/*
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
			jointObj.localPosition = GetVector3FromJoint(sourceJoint) - headPosition;
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
				lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value) - headPosition);
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }*/
    }

	class HandStatus {
		public HandStatus() {}

		public bool isClosed = false;
		bool nextIsClosed = false;
		float heldTime = 0;
		static float threshold = 0.1f;

		public void update(float timeDelta, Kinect.HandState nextState) {
			bool nextNextIsClosed;

			switch (nextState) {
			case Kinect.HandState.Open:
				nextNextIsClosed = false;
				break;
			case Kinect.HandState.Closed:
				nextNextIsClosed = true;
				break;
			default:
				return;
			}

			if (nextIsClosed == nextNextIsClosed) {
				heldTime += timeDelta;
				if (heldTime > threshold) {
					isClosed = nextIsClosed;
				}
			} else {
				heldTime = 0;
				nextIsClosed = nextNextIsClosed;
			}
		}
	}

	private static Color GetColorForState(bool state)
	{
		if (state) return Color.red;
		else return Color.green;
	}
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }

	private static Color GetColorForState(Kinect.HandState state) {
		switch (state) {
		case Kinect.HandState.Open:
			return Color.green;
		case Kinect.HandState.Lasso:
			return Color.yellow;
		case Kinect.HandState.NotTracked:
			return Color.black;
		case Kinect.HandState.Closed:
			return Color.blue;
		default:
			return Color.red;
		}
	}

    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X, joint.Position.Y, -joint.Position.Z) * 1;
    }

	// Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was
	// recognized.
	void ExtendUnlockAndNotifyUserAction (ThalmicMyo myo)
	{
		ThalmicHub hub = ThalmicHub.instance;
		
		if (hub.lockingPolicy == LockingPolicy.Standard) {
			myo.Unlock (UnlockType.Timed);
		}
		
		myo.NotifyUserAction ();
	}
}
