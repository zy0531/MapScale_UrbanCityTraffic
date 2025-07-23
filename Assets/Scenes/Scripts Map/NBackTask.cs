using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;
using UnityEngine.XR;
using System.IO;

public class NBackTask : MonoBehaviour
{
    [Header("Settings")]
    public int nBack = 2; // N-back level (e.g., 2-back)
    public float stimulusInterval = 1.5f; // Interval between stimuli (seconds)
    public AudioClip[] possibleAuditoryStimuli; // Array of audio clips for letters/alphabets
    public AudioSource auditoryStimuli; // AudioSource component for playing sounds
    public AudioClip correctResponseSound;
    public AudioClip incorrectResponseSound;

    [SerializeField] DataManager dataManager; // Reference to DataManager for log management
    string Path;
    string FileName;

    // Public variables accessible from NBackResponseRecord
    public List<AudioClip> stimulusSequence = new List<AudioClip>(); // History of presented stimuli
    public float responseStartTime; // Response start time for each trial

    bool buttonDown_XRInput;
    InputDevice lefthand;
    bool triggerValue;

    // Flag to check if experiment has started
    private bool experimentStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        // Find left hand controller
        var leftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);

        if (leftHandDevices.Count == 1)
        {
            lefthand = leftHandDevices[0];
            UnityEngine.Debug.Log(string.Format("Device name '{0}' with role '{1}'", lefthand.name, lefthand.role.ToString()));
        }
        else if (leftHandDevices.Count > 1)
        {
            UnityEngine.Debug.Log("Found more than one left hand!");
        }

        // Initialize data log file path with timestamp
        DateTime now = DateTime.Now;
        Path = dataManager.folderPath + "/";
        FileName = dataManager.fileName + "NBack" + string.Format("{0}-{1:00}-{2:00}-{3:00}-{4:00}", now.Year, now.Month, now.Day, now.Hour, now.Minute) + ".csv";

        // Ensure directory exists
        Directory.CreateDirectory(Path);

        UnityEngine.Debug.Log("Press 'S' to start the N-Back experiment.");
        UnityEngine.Debug.Log("Log file will be saved to: " + Path + FileName);
    }

    // Update is called once per frame
    void Update()
    {
        // Press 'S' key to start experiment
        if (!experimentStarted && Input.GetKeyDown(KeyCode.S))
        {
            experimentStarted = true;
            UnityEngine.Debug.Log("Starting N-Back experiment.");
            StartCoroutine(StartExperiment());
        }
    }

    // Coroutine to run N-back experiment
    IEnumerator StartExperiment()
    {
        // Write header to the log file
        RecordData.SaveData(Path, FileName, "Timestamp,DateTime,StimulusName,StimulusInterval\n");

        // Infinite loop to continuously present stimuli
        while (true)
        {
            // Randomly select one audio clip from possibleAuditoryStimuli array
            int randomIndex = UnityEngine.Random.Range(0, possibleAuditoryStimuli.Length);
            AudioClip currentStimulus = possibleAuditoryStimuli[randomIndex];

            // Record selected stimulus (store in stimulusSequence)
            stimulusSequence.Add(currentStimulus);

            // Log stimulus presentation time
            double audioTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            RecordData.SaveData(Path, FileName,
                audioTime.ToString() + "," +
                DateTime.Now.ToString() + "," +
                currentStimulus.name + "," +
                stimulusInterval.ToString() + "\n");

            // Play audio clip
            auditoryStimuli.clip = currentStimulus;
            auditoryStimuli.Play();

            // Record response start time for this trial
            responseStartTime = Time.time;

            // Wait for next stimulus (stimulusInterval seconds)
            yield return new WaitForSeconds(stimulusInterval);
        }
    }
}