﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Varjo.XR;
using System.Diagnostics;


public class EyeTrackingExploration : MonoBehaviour
{
    [Header("Gaze data")]
    public GazeDataSource gazeDataSource = GazeDataSource.InputSubsystem;

    [Header("Gaze calibration settings")]
    public VarjoEyeTracking.GazeCalibrationMode gazeCalibrationMode = VarjoEyeTracking.GazeCalibrationMode.Fast;
    public KeyCode calibrationRequestKey = KeyCode.Space;

    [Header("Gaze output filter settings")]
    public VarjoEyeTracking.GazeOutputFilterType gazeOutputFilterType = VarjoEyeTracking.GazeOutputFilterType.Standard;
    public KeyCode setOutputFilterTypeKey = KeyCode.RightShift;

    [Header("Gaze data output frequency")]
    public VarjoEyeTracking.GazeOutputFrequency frequency;

    [Header("Toggle gaze target visibility")]
    public KeyCode toggleGazeTarget = KeyCode.Return;

    [Header("Debug Gaze")]
    public KeyCode checkGazeAllowed = KeyCode.PageUp;
    public KeyCode checkGazeCalibrated = KeyCode.PageDown;

    [Header("Toggle fixation point indicator visibility")]
    public bool showFixationPoint = true;

    [Header("Visualization Transforms")]
    public Transform fixationPointTransform;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;

    [Header("XR camera")]
    public Camera xrCamera;

    [Header("Gaze point indicator")]
    public GameObject gazeTarget;

    [Header("Gaze ray radius")]
    public float gazeRadius = 0.01f;

    [Header("Gaze point distance if not hit anything")]
    public float floatingGazeTargetDistance = 5f;

    [Header("Gaze target offset towards viewer")]
    public float targetOffset = 0.2f;

    [Header("Amout of force give to freerotating objects at point where user is looking")]
    public float hitForce = 5f;

    [Header("Gaze data logging")]
    public KeyCode loggingToggleKey = KeyCode.RightControl;

    [Header("Default path is Logs under application data path.")]
    public bool useCustomLogPath = false;
    public string customLogPath = "";

    [Header("Print gaze data framerate while logging.")]
    public bool printFramerate = false;

    [Header("Map Raycast Settings")]
    [SerializeField] private LayerMask mapLayer;      // "Map" layer mask
    [SerializeField] private Transform mapTransform;  //to Transform to local coordinate  


    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;
    private Eyes eyes;
    private VarjoEyeTracking.GazeData gazeData;
    private List<VarjoEyeTracking.GazeData> dataSinceLastUpdate;
    private List<VarjoEyeTracking.EyeMeasurements> eyeMeasurementsSinceLastUpdate;
    private Vector3 leftEyePosition;
    private Vector3 rightEyePosition;
    private Quaternion leftEyeRotation;
    private Quaternion rightEyeRotation;
    private Vector3 fixationPoint;
    private Vector3 direction;
    private Vector3 rayOrigin;
    private RaycastHit hit;
    private float distance;
    private StreamWriter writer = null;
    private bool logging = false;
    private bool isLookingAtMap;
    private Vector3 mapLocalPos;
    private string mapLandmarkName;

    private static readonly string[] ColumnNames = { "Trial", "Time",
        "Frame", "CaptureTime", "LogTime",
        "HMDPosition", "HMDRotation", "GlobalCameraPosition", "CameraForward",
        "GazeStatus", "CombinedGazeForward", "CombinedGazePosition", "CombinedGazeForwardGlobal", "CombinedGazePositionGlobal", "GazeHitObject","GazeHitPosition", "GazeHitDistance",
        "InterPupillaryDistanceInMM",
        "LeftEyeStatus", "LeftEyeForward", "LeftEyePosition", "LeftPupilIrisDiameterRatio", "LeftPupilDiameterInMM", "LeftIrisDiameterInMM",
        "RightEyeStatus", "RightEyeForward", "RightEyePosition", "RightPupilIrisDiameterRatio", "RightPupilDiameterInMM", "RightIrisDiameterInMM",
        "FocusDistance", "FocusStability",
        "isInDirectionTrigger", "DirectionTriggerName",
        "isInLandmarkTrigger", "LandmarkTriggerName",
        "IsLookingAtMap", "MapLocalPos","MapLandmarkName"
        };
    private const string ValidString = "VALID";
    private const string InvalidString = "INVALID";

    int gazeDataCount = 0;
    float gazeTimer = 0f;

    //add
    private string hitGameObjectName;
    private Vector3 hitPoint;
    //add

    //add
    [SerializeField] DataManager dataManager;
    //add



    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(XRNode.CenterEye, devices);
        device = devices.FirstOrDefault();
        UnityEngine.Debug.Log(device + "GetDevice Done!");
    }

    void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }

    private void Start()
    {
        VarjoEyeTracking.SetGazeOutputFrequency(frequency);
        //Hiding the gazetarget if gaze is not available or if the gaze calibration is not done
        if (VarjoEyeTracking.IsGazeAllowed() && VarjoEyeTracking.IsGazeCalibrated())
        {
            gazeTarget.SetActive(true);
        }
        else
        {
            gazeTarget.SetActive(false);
        }

        if (showFixationPoint)
        {
            fixationPointTransform.gameObject.SetActive(true);
        }
        else
        {
            fixationPointTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // UnityEngine.Debug.Log("Gaze allowed: " + VarjoEyeTracking.IsGazeAllowed() + "Gaze calibrated: " + VarjoEyeTracking.IsGazeCalibrated());
        if (logging && printFramerate)
        {
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= 1.0f)
            {
                UnityEngine.Debug.Log("Gaze data rows per second: " + gazeDataCount);
                gazeDataCount = 0;
                gazeTimer = 0f;
            }
        }

        // Request gaze calibration
        if (Input.GetKeyDown(calibrationRequestKey))
        {
            VarjoEyeTracking.RequestGazeCalibration(gazeCalibrationMode);
        }

        // Set output filter type
        if (Input.GetKeyDown(setOutputFilterTypeKey))
        {
            VarjoEyeTracking.SetGazeOutputFilterType(gazeOutputFilterType);
            UnityEngine.Debug.Log("Gaze output filter type is now: " + VarjoEyeTracking.GetGazeOutputFilterType());
        }

        // Check if gaze is allowed
        if (Input.GetKeyDown(checkGazeAllowed))
        {
            UnityEngine.Debug.Log("Gaze allowed: " + VarjoEyeTracking.IsGazeAllowed());
        }

        // Check if gaze is calibrated
        if (Input.GetKeyDown(checkGazeCalibrated))
        {
            UnityEngine.Debug.Log("Gaze calibrated: " + VarjoEyeTracking.IsGazeCalibrated());
        }

        // Toggle gaze target visibility
        if (Input.GetKeyDown(toggleGazeTarget))
        {
            gazeTarget.GetComponentInChildren<MeshRenderer>().enabled = !gazeTarget.GetComponentInChildren<MeshRenderer>().enabled;
        }

        // Get gaze data if gaze is allowed and calibrated
        if (VarjoEyeTracking.IsGazeAllowed() && VarjoEyeTracking.IsGazeCalibrated())
        {
            //Get device if not valid
            if (!device.isValid)
            {
                GetDevice();
            }

            // Show gaze target (change default to: false)
            gazeTarget.SetActive(false);

            if (gazeDataSource == GazeDataSource.InputSubsystem)
            {
                // Get data for eye positions, rotations and the fixation point
                if (device.TryGetFeatureValue(CommonUsages.eyesData, out eyes))
                {
                    if (eyes.TryGetLeftEyePosition(out leftEyePosition))
                    {
                        leftEyeTransform.localPosition = leftEyePosition;
                    }

                    if (eyes.TryGetLeftEyeRotation(out leftEyeRotation))
                    {
                        leftEyeTransform.localRotation = leftEyeRotation;
                    }

                    if (eyes.TryGetRightEyePosition(out rightEyePosition))
                    {
                        rightEyeTransform.localPosition = rightEyePosition;
                    }

                    if (eyes.TryGetRightEyeRotation(out rightEyeRotation))
                    {
                        rightEyeTransform.localRotation = rightEyeRotation;
                    }

                    if (eyes.TryGetFixationPoint(out fixationPoint))
                    {
                        fixationPointTransform.localPosition = fixationPoint;
                    }
                }

                // Set raycast origin point to VR camera position
                rayOrigin = xrCamera.transform.position;

                // Direction from VR camera towards fixation point
                direction = (fixationPointTransform.position - xrCamera.transform.position).normalized;

            }
            else
            {
                gazeData = VarjoEyeTracking.GetGaze();

                if (gazeData.status != VarjoEyeTracking.GazeStatus.Invalid)
                {
                    // GazeRay vectors are relative to the HMD pose so they need to be transformed to world space
                    if (gazeData.leftStatus != VarjoEyeTracking.GazeEyeStatus.Invalid)
                    {
                        leftEyeTransform.position = xrCamera.transform.TransformPoint(gazeData.left.origin);
                        leftEyeTransform.rotation = Quaternion.LookRotation(xrCamera.transform.TransformDirection(gazeData.left.forward));
                    }

                    if (gazeData.rightStatus != VarjoEyeTracking.GazeEyeStatus.Invalid)
                    {
                        rightEyeTransform.position = xrCamera.transform.TransformPoint(gazeData.right.origin);
                        rightEyeTransform.rotation = Quaternion.LookRotation(xrCamera.transform.TransformDirection(gazeData.right.forward));
                    }

                    // Set gaze origin as raycast origin
                    rayOrigin = xrCamera.transform.TransformPoint(gazeData.gaze.origin);

                    // Set gaze direction as raycast direction
                    direction = xrCamera.transform.TransformDirection(gazeData.gaze.forward);

                    // Fixation point can be calculated using ray origin, direction and focus distance
                    fixationPointTransform.position = rayOrigin + direction * gazeData.focusDistance;
                }
            }

        }

        // Raycast to world from VR Camera position towards fixation point
        if (Physics.SphereCast(rayOrigin, gazeRadius, direction, out hit, 100f, mapLayer))
        {
            UnityEngine.Debug.Log($"Hit: {hit.collider.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            // if collide with map
            isLookingAtMap = true;

            // get local coordinates from the map
            mapLocalPos = mapTransform.InverseTransformPoint(hit.point);

            // Check if the hit object has the "targetLandmark" tag
            mapLandmarkName = hit.transform.name;

            // Put target on gaze raycast position with offset towards user
            gazeTarget.transform.position = hit.point - direction * targetOffset;

            // Make gaze target point towards user
            gazeTarget.transform.LookAt(rayOrigin, Vector3.up);

            // Scale gazetarget with distance so it apperas to be always same size
            distance = hit.distance;
            gazeTarget.transform.localScale = Vector3.one * distance;

            // * add *
            hitGameObjectName = hit.collider.gameObject.name;
            hitPoint = hit.point;
            // * add *

            // Prefer layers or tags to identify looked objects in your application
            // This is done here using GetComponent for the sake of clarity as an example
            RotateWithGaze rotateWithGaze = hit.collider.gameObject.GetComponent<RotateWithGaze>();
            if (rotateWithGaze != null)
            {
                rotateWithGaze.RayHit();
            }

            // Alternative way to check if you hit object with tag
            if (hit.transform.CompareTag("FreeRotating"))
            {
                AddForceAtHitPosition();
            }
        }
        else
        {
            // did not collide with map
            isLookingAtMap = false;
            // If gaze ray didn't hit anything, the gaze target is shown at fixed distance
            gazeTarget.transform.position = rayOrigin + direction * floatingGazeTargetDistance;
            gazeTarget.transform.LookAt(rayOrigin, Vector3.up);
            gazeTarget.transform.localScale = Vector3.one * floatingGazeTargetDistance;

            mapLocalPos = Vector3.zero;
            mapLandmarkName = "None";

            // * add *
            hitGameObjectName = "None";
            hitPoint = new Vector3(0, 0, 0);
            distance = -1f;
            // * add *
        }



        if (Input.GetKeyDown(loggingToggleKey)) //logging data 
        {
            if (!logging)
            {
                UnityEngine.Debug.Log("Start logging eye-tracking...");
                StartLogging();
            }
            else
            {
                UnityEngine.Debug.Log("stop logging eye-tracking...");
                StopLogging();
            }
            return;
        }

        if (logging)
        {
            int dataCount = VarjoEyeTracking.GetGazeList(out dataSinceLastUpdate, out eyeMeasurementsSinceLastUpdate);
            if (printFramerate) gazeDataCount += dataCount;
            for (int i = 0; i < dataCount; i++)
            {
                LogGazeData(dataSinceLastUpdate[i], eyeMeasurementsSinceLastUpdate[i]);
            }
        }
    }

    void AddForceAtHitPosition()
    {
        //Get Rigidbody form hit object and add force on hit position
        Rigidbody rb = hit.rigidbody;
        if (rb != null)
        {
            rb.AddForceAtPosition(direction * hitForce, hit.point, ForceMode.Force);
        }
    }

    void LogGazeData(VarjoEyeTracking.GazeData data, VarjoEyeTracking.EyeMeasurements eyeMeasurements)
    {
        string[] logData = new string[39];

        // * Trial *
        logData[0] = "";

        // * Time *
        logData[1] = DateTime.Now.ToString();

        // Gaze data frame number
        logData[2] = data.frameNumber.ToString();

        // Gaze data capture time (nanoseconds)
        logData[3] = data.captureTime.ToString();

        // Log time (milliseconds)
        logData[4] = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();

        // HMD
        logData[5] = xrCamera.transform.localPosition.ToString("F3");
        logData[6] = xrCamera.transform.localRotation.ToString("F3");

        // * Camera global Position *
        logData[7] = xrCamera.transform.position.ToString("F3");
        // * Camera Forward *
        logData[8] = xrCamera.transform.forward.ToString("F3");


        // Combined gaze
        bool invalid = data.status == VarjoEyeTracking.GazeStatus.Invalid;
        logData[9] = invalid ? InvalidString : ValidString;
        logData[10] = invalid ? "" : data.gaze.forward.ToString("F3");
        logData[11] = invalid ? "" : data.gaze.origin.ToString("F3");

        // * Combined gaze Global *
        logData[12] = invalid ? "" : direction.ToString("F3");
        logData[13] = invalid ? "" : rayOrigin.ToString("F3");

        // * Hit Object *
        logData[14] = invalid ? "" : hitGameObjectName;
        logData[15] = invalid ? "" : hitPoint.ToString("F3");
        logData[16] = invalid ? "" : distance.ToString("F3");

        // IPD
        logData[17] = invalid ? "" : eyeMeasurements.interPupillaryDistanceInMM.ToString("F3");

        // Left eye
        bool leftInvalid = data.leftStatus == VarjoEyeTracking.GazeEyeStatus.Invalid;
        logData[18] = leftInvalid ? InvalidString : ValidString;
        logData[19] = leftInvalid ? "" : data.left.forward.ToString("F3");
        logData[20] = leftInvalid ? "" : data.left.origin.ToString("F3");
        logData[21] = leftInvalid ? "" : eyeMeasurements.leftPupilIrisDiameterRatio.ToString("F3");
        logData[22] = leftInvalid ? "" : eyeMeasurements.leftPupilDiameterInMM.ToString("F3");
        logData[23] = leftInvalid ? "" : eyeMeasurements.leftIrisDiameterInMM.ToString("F3");

        // Right eye
        bool rightInvalid = data.rightStatus == VarjoEyeTracking.GazeEyeStatus.Invalid;
        logData[24] = rightInvalid ? InvalidString : ValidString;
        logData[25] = rightInvalid ? "" : data.right.forward.ToString("F3");
        logData[26] = rightInvalid ? "" : data.right.origin.ToString("F3");
        logData[27] = rightInvalid ? "" : eyeMeasurements.rightPupilIrisDiameterRatio.ToString("F3");
        logData[28] = rightInvalid ? "" : eyeMeasurements.rightPupilDiameterInMM.ToString("F3");
        logData[29] = rightInvalid ? "" : eyeMeasurements.rightIrisDiameterInMM.ToString("F3");

        // Focus
        logData[30] = invalid ? "" : data.focusDistance.ToString();
        logData[31] = invalid ? "" : data.focusStability.ToString();

        // isInDirectionTrigger
        logData[32] = SharedVariables.isInDirectionTrigger.ToString();
        logData[33] = SharedVariables.DirectionTriggerName;

        // isInLandmarkTrigger
        logData[34] = SharedVariables.isInLandmarkTrigger.ToString();
        logData[35] = SharedVariables.LandmarkTriggerName;

        //Which Landmark is user looking at
        logData[36] = isLookingAtMap.ToString();             // "True" or "False"
        logData[37] = mapLocalPos.ToString("F3");            // "(x.xx, y.yy, z.zz)"
        logData[38] = mapLandmarkName;                       // if there is no name for landmark (then "")


        Log(logData);
    }

    // Write given values in the log file
    void Log(string[] values)
    {
        if (!logging || writer == null)
            return;

        string line = "";
        for (int i = 0; i < values.Length; ++i)
        {
            values[i] = values[i].Replace("\r", "").Replace("\n", ""); // Remove new lines so they don't break csv
            line += values[i] + (i == (values.Length - 1) ? "" : ";"); // Do not add semicolon to last data string
            //line += values[i] + (i == (values.Length - 1) ? "" : ";"); // Do not add semicolon to last data string
        }
        writer.WriteLine(line);
    }

    public void StartLogging()
    {
        if (logging)
        {
            UnityEngine.Debug.LogWarning("Logging was on when StartLogging was called. No new log was started.");
            return;
        }

        logging = true;

        // * add
        string logPath = dataManager.folderPath + "/";

        //string logPath = useCustomLogPath ? customLogPath : Application.dataPath + "/Logs/";
        //Directory.CreateDirectory(logPath);

        DateTime now = DateTime.Now;
        string fileName = dataManager.fileName + "EyeTracking" + string.Format("{0}-{1:00}-{2:00}-{3:00}-{4:00}", now.Year, now.Month, now.Day, now.Hour, now.Minute);
        //string fileName = string.Format("{0}-{1:00}-{2:00}-{3:00}-{4:00}", now.Year, now.Month, now.Day, now.Hour, now.Minute);

        string path = logPath + fileName + ".csv";
        writer = new StreamWriter(path);

        Log(ColumnNames);
        UnityEngine.Debug.Log("Log file started at: " + path);
    }

    void StopLogging()
    {
        if (!logging)
            return;

        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
        logging = false;
        UnityEngine.Debug.Log("Logging ended");
    }

    void OnApplicationQuit()
    {
        StopLogging();
    }


    // ***************** add my func here ***************** 

    public Vector3 getDirection()
    {
        return direction;
    }
    public Vector3 getRayOrigin()
    {
        return rayOrigin;
    }

}