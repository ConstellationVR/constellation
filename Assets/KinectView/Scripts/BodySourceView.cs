using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
	public GameObject Pointer;
    
	private HandStatus leftHand = new HandStatus();
	private HandStatus rightHand = new HandStatus();

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    
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
                Destroy(_Bodies[trackingId]);
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
    
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        
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

		GameObject rightPointer = Instantiate (Pointer);
		rightPointer.name = "RightPointer";
		rightPointer.transform.parent = body.transform;
        
		GameObject leftPointer = Instantiate (Pointer);
		leftPointer.name = "LeftPointer";
		leftPointer.transform.parent = body.transform;

        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
		Kinect.Joint headJoint = body.Joints [Kinect.JointType.Neck];
		Vector3 headPosition = GetVector3FromJoint (headJoint);

		rightHand.update (Time.deltaTime, body.HandRightState);
		leftHand.update (Time.deltaTime, body.HandLeftState);

		Kinect.Joint rightHandJoint = body.Joints [Kinect.JointType.HandRight];
		Transform rightPointer = bodyObject.transform.FindChild ("RightPointer");
		rightPointer.localPosition = GetVector3FromJoint (rightHandJoint) - headPosition;
		rightPointer.GetComponent<Renderer>().material.color = GetColorForState(rightHand.isClosed);

		Kinect.Joint leftHandJoint = body.Joints [Kinect.JointType.HandLeft];
		Transform leftPointer = bodyObject.transform.FindChild ("LeftPointer");
		leftPointer.localPosition = GetVector3FromJoint (leftHandJoint) - headPosition;
		leftPointer.GetComponent<Renderer>().material.color = GetColorForState(leftHand.isClosed);

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
        }
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
}
