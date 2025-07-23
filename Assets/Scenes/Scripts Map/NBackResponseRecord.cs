using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.XR;

// NBackResponseRecord: Responsible for logging user responses and performance.
public class NBackResponseRecord : MonoBehaviour
{
    // XR input variables
    bool buttonDown_XRInput;
    InputDevice lefthand;
    bool triggerValue;

    // Flag to ensure one response is recorded per trial.
    bool hasResponded = false;
    private bool responseLogged = false;

    // Flag to start the experiment.
    private bool experimentStarted = false;

    [SerializeField] DataManager dataManager; // For file path management.
    [SerializeField] NBackTask nBackTask;       // To access stimulus sequence and timing.
    private string Path = "";
    private string detailedFileName = "";
    private string summaryFileName = "";// Folder path for saving logs.


    // Performance metrics (summary)
    private int totalTargetTrials = 0;      // Number of target (2-back match) trials.
    private int totalNonTargetTrials = 0;   // Number of non-target trials.
    private int correctResponses = 0;       // Correct responses on target trials.
    private int falseAlarms = 0;            // Responses on non-target trials.
    private int missedMatches = 0;          // Target trials with no response.
    private float cumulativeReactionTime = 0f; // Sum of reaction times (ms) for correct responses.
    private int reactionCount = 0;          // For averaging reaction time.

    public float trialDuration = 2.5f;

    void Start()
    {
        // Find left-hand XR controller
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);

        if (devices.Count == 1)
        {
            lefthand = devices[0];
            UnityEngine.Debug.Log(string.Format("Left-hand controller found: Device name '{0}' with role '{1}'",
                        lefthand.name, lefthand.role.ToString()));
        }
        else if (devices.Count > 1)
        {
            UnityEngine.Debug.LogWarning("Found more than one left-hand controller! Using first device.");
            lefthand = devices[0]; // Use first device if multiple found
        }
        else
        {
            UnityEngine.Debug.LogError("Left-hand controller not found!");
        }

        // Initialize data logging system with timestamp
        DateTime now = DateTime.Now;
        string timestamp = string.Format("{0}-{1:00}-{2:00}-{3:00}-{4:00}",
                                      now.Year, now.Month, now.Day, now.Hour, now.Minute);

        Path = dataManager.folderPath + "/";
        Directory.CreateDirectory(Path); // Ensure directory exists

        // Create CSV files with timestamped filenames and headers
        detailedFileName = dataManager.fileName + $"NBackResponsesDetailed_{timestamp}.csv";
        summaryFileName = dataManager.fileName + $"NBackSummaryRealtime_{timestamp}.csv";

        // Detailed responses log
        RecordData.SaveData(Path, detailedFileName,
            "Timestamp(ms),DateTime,Stimulus,TargetStatus,ParticipantResponse," +
            "ReactionTime(ms),ResponseClassification,CorrectResponse\n");

        // Summary statistics log
        RecordData.SaveData(Path, summaryFileName,
            "BlockNumber,TaskAccuracy(%),AvgReactionTime(ms)," +
            "FalseAlarms(%),MissedMatches(%),TotalTrials,CorrectResponses\n");

        UnityEngine.Debug.Log($"Data will be saved to: {Path}");
        UnityEngine.Debug.Log($"Created log files: {detailedFileName} and {summaryFileName}");

        UnityEngine.Debug.Log("Press 'S' to begin the N-Back task");
    }
    void Update()
    {
        //UnityEngine.Debug.Log("PRESS S to start the N-Back experiment.");
        // Start the experiment on S key press.
        if (!experimentStarted && Input.GetKeyDown(KeyCode.S))
        {
            experimentStarted = true;
            UnityEngine.Debug.Log("Recording started for N-Back experiment.");
            // Start processing the first trial.
            StartCoroutine(ProcessTrial());


        }
    }



    IEnumerator ProcessTrial()
    {
        while (true)
        {
            // Reset flags for a new trial.
            hasResponded = false;
            responseLogged = false;
            float startTime = Time.time;
            UnityEngine.Debug.Log("New trial started at: " + startTime);

            // Process the trial for the given duration.
            while (Time.time - startTime < 3f)
            {
                if (!hasResponded && lefthand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue)
                {
                    UnityEngine.Debug.Log("Trigger button pressed. Handling user response.");
                    //Buzz feedback
                    StartCoroutine(TriggerHapticFeedback(lefthand, 0.2f, 0.8f));
                    HandleUserResponse();
                    hasResponded = true;
                    responseLogged = true;
                    break;
                }
                yield return null;
            }

            // If no response was recorded during the trial, log a missed response.
            if (!hasResponded)
            {
                UnityEngine.Debug.Log("Trial ENDED without a response. Logging missed response.");
                HandleMissedResponse();
                hasResponded = true;
                responseLogged = true;
            }

            // Wait until the trigger is released before starting the next trial.
            while (lefthand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue)
            {
                yield return null;
            }

            // Optionally, a brief pause between trials.
        }
    }



    // Process a valid user response.
    void HandleUserResponse()
    {
        hasResponded = true;
        AudioClip correctStimulus = GetCorrectStimulus();
        float reactionTime = (Time.time - nBackTask.responseStartTime) * 1000f; // ms
        bool missedResponse = false;
        bool isTargetTrial = IsTrialTarget();
        bool isCorrect = isTargetTrial ? true : false;
        LogResponse(correctStimulus, isCorrect, missedResponse, reactionTime, userTriggered: true);
    }

    // Process a missed response.
    void HandleMissedResponse()
    {
        hasResponded = true;
        float reactionTime = (Time.time - nBackTask.responseStartTime) * 1000f;
        bool missedResponse = true;
        bool isCorrect = false;
        AudioClip correctStimulus = GetCorrectStimulus();
        LogResponse(correctStimulus, isCorrect, missedResponse, reactionTime, userTriggered: false);
    }

    // Returns the target stimulus based on the n-back offset.
    AudioClip GetCorrectStimulus()
    {
        int currentStimulusIndex = nBackTask.stimulusSequence.Count - 1;
        if (currentStimulusIndex < 0)
            return null;
        int index = currentStimulusIndex - nBackTask.nBack;
        if (index < 0 || index >= nBackTask.stimulusSequence.Count)
            return null;
        return nBackTask.stimulusSequence[index];
    }

    // Determines if the current trial is a target (2-back match).
    bool IsTrialTarget()
    {
        int currentStimulusIndex = nBackTask.stimulusSequence.Count - 1;
        if (currentStimulusIndex < 0)
            return false;
        int index = currentStimulusIndex - nBackTask.nBack;
        if (index < 0 || index >= nBackTask.stimulusSequence.Count)
            return false;
        bool isTarget = (nBackTask.stimulusSequence[currentStimulusIndex].name == nBackTask.stimulusSequence[index].name);
        return isTarget;
    }
    //Buzz triger
    private IEnumerator TriggerHapticFeedback(InputDevice device, float duration, float amplitude)
    {
        if (device.isValid)
        {
            // start buzz
            device.SendHapticImpulse(0, amplitude, duration);

            
            yield return new WaitForSeconds(duration);
            // device.StopHaptics();
        }
        else
        {
            UnityEngine.Debug.LogWarning("Cannot send haptic feedback - device not valid");
        }
    }

    // Logs detailed response data and updates summary metrics.
    void LogResponse(AudioClip correctStimulus, bool isCorrect, bool missedResponse, float reactionTime, bool userTriggered)
    {
        double responseTimeStamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        int currentStimulusIndex = nBackTask.stimulusSequence.Count - 1;
        bool isTargetTrial = IsTrialTarget();

        string stimulus = (nBackTask.stimulusSequence[currentStimulusIndex] != null) ? nBackTask.stimulusSequence[currentStimulusIndex].name : "null";
        string targetStatus = isTargetTrial ? "Yes" : "No";
        string participantResponse = userTriggered ? "Yes" : "No";
        string classification = "";
        if (userTriggered)
        {
            classification = isTargetTrial ? "Hit" : "False Alarm";
        }
        else
        {
            classification = isTargetTrial ? "Miss" : "Correct Rejection";
        }

        string detailedLog = responseTimeStamp.ToString() + "," +
                             DateTime.Now.ToString() + "," +
                             stimulus + "," +
                             targetStatus + "," +
                             participantResponse + "," +
                             reactionTime.ToString("F2") + "," +
                             classification + "\n";

        RecordData.SaveData(Path, detailedFileName, detailedLog);
        UnityEngine.Debug.Log("Detailed response logged: " + detailedLog);

        // Update summary performance metrics.
        if (userTriggered)
        {
            if (classification == "Hit")
            {
                correctResponses++;
                totalTargetTrials++;
                cumulativeReactionTime += reactionTime;
                UnityEngine.Debug.Log("Target trial correct response. Total correct responses: " + correctResponses);
            }
            if (classification == "False Alarm")
            {
                falseAlarms++;
                totalNonTargetTrials++;
                UnityEngine.Debug.Log("False alarm on non-target trial. Total false alarms: " + falseAlarms);
            }
            reactionCount++;
        }
        else
        {
            if (classification == "Miss")
            {
                missedMatches++;
                totalTargetTrials++;
                UnityEngine.Debug.Log("Missed target trial. Total missed matches: " + missedMatches);
            }
            else
            {
                totalNonTargetTrials++;
            }
        }
    }

    //UpdateSummaryRealtime();





    // On scene exit or object destruction, log final summary metrics.
    void OnDestroy()
    {
        UnityEngine.Debug.Log("OnDestroy called. Logging final summary metrics.");
        LogFinalSummaryMetrics();
    }

    // Calculate and log final summary performance metrics.
    void LogFinalSummaryMetrics()
    {
        float taskAccuracy = totalTargetTrials > 0 ? ((float)correctResponses / totalTargetTrials) * 100f : 0f;
        float avgReactionTime = reactionCount > 0 ? (cumulativeReactionTime / reactionCount) : 0f;
        float falseAlarmRate = totalNonTargetTrials > 0 ? ((float)falseAlarms / totalNonTargetTrials) * 100f : 0f;
        float missedMatchRate = totalTargetTrials > 0 ? ((float)missedMatches / totalTargetTrials) * 100f : 0f;
        string finalSummary = "Final Summary - Task Accuracy (%): " + taskAccuracy.ToString("F2") +
                              ", Avg Reaction Time (ms): " + avgReactionTime.ToString("F2") +
                              ", False Alarms (%): " + falseAlarmRate.ToString("F2") +
                              ", Missed Matches (%): " + missedMatchRate.ToString("F2");
        UnityEngine.Debug.Log(finalSummary);
        RecordData.SaveData(Path, summaryFileName, finalSummary + "\n");
    }

}
