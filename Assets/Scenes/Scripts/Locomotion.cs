using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Locomotion : MonoBehaviour
{
    public Transform xrRig;
    public Transform head;
    // public Transform bodyTracker;
    [Header("Use controller.primary2DAxisTouch to move")]
    public float moveSpeed = 1.80f;

    // InputDevice device;
    InputDevice righthand;
    InputDevice lefthand;
    InputDevice tracker;
    bool ButtonState;

    Vector3 trackerPos;
    Quaternion trackerRot;
    Vector3 trackerForward;

    // Start is called before the first frame update
    void Start()
    {
        //// Find Controllers
        //var RightHandDevices = new List<InputDevice>();
        //InputDevices.GetDevicesAtXRNode(XRNode.RightHand, RightHandDevices);

        //if (RightHandDevices.Count == 1)
        //{
        //    device = RightHandDevices[0];
        //    Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
        //}
        //else if (RightHandDevices.Count > 1)
        //{
        //    Debug.Log("Found more than one right hand!");
        //}

        // Find Right Controller
        var RightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, RightHandDevices);

        if (RightHandDevices.Count == 1)
        {
            righthand = RightHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", righthand.name, righthand.role.ToString()));
        }
        else if (RightHandDevices.Count > 1)
        {
            Debug.Log("Found more than one right hand!");
        }

        // Find Left Controller
        var LeftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, LeftHandDevices);

        if (LeftHandDevices.Count == 1)
        {
            lefthand = LeftHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", lefthand.name, lefthand.role.ToString()));
        }
        else if (LeftHandDevices.Count > 1)
        {
            Debug.Log("Found more than one left hand!");
        }

        // Find Vive Tracker (Waist) - "GameController"
        var Trackers = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.GameController, Trackers);

        if (Trackers.Count == 1)
        {
            tracker = Trackers[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", tracker.name, tracker.role.ToString()));
        }
        else if (Trackers.Count > 1)
        {
            Debug.Log("Found more than one tracker!");
        }

    }

    // Update is called once per frame
    void Update()
    {
        //Find tracker rotation
        if (tracker.TryGetFeatureValue(CommonUsages.deviceRotation, out trackerRot))
        {
            trackerForward = trackerRot * Vector3.forward;
        }
        if (tracker.TryGetFeatureValue(CommonUsages.devicePosition, out trackerPos) && tracker.TryGetFeatureValue(CommonUsages.deviceRotation, out trackerRot))
        {
            Debug.DrawRay(trackerPos, trackerForward, Color.green);
        }




        //if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out ButtonState) && ButtonState) // using primary button
        //if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out ButtonState) && ButtonState) // using joystick
        if (righthand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisTouch, out ButtonState) && ButtonState) // using joystick
        {
            //Debug.Log("primary button is pressed.");
            Debug.Log("primary2DAxis button is pressed.");
            //Head-based steering
            //xrRig.transform.Translate(VectorYToZero(head.forward) * moveSpeed * Time.deltaTime, Space.World);

            //Body-based steering (Body rotation is tracked by a Vive Tracker)
            // Debug.DrawRay(transform.position, ProjectToXZPlane(bodyTracker.up), Color.green);
            //Debug.Log("trackerForward:"+trackerForward);
            // xrRig.transform.Translate(ProjectToXZPlane(trackerForward) * moveSpeed * Time.deltaTime, Space.World);

            //Joystick-based steering (rotation is determined by the controller)
            xrRig.transform.Translate(ProjectToXZPlane(this.transform.forward) * moveSpeed * Time.deltaTime, Space.World);
        }
        Debug.Log("ButtonState" + ButtonState);
    }

    Vector3 ProjectToXZPlane(Vector3 v)
    {
        return new Vector3(v.x, 0.0f, v.z).normalized;
        // for the specific projection to x-z plane, simply setting y to 0 works.
    }
}

