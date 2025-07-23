using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SpaceConverter : MonoBehaviour
{
    [SerializeField] private LayerMask layermask;
    [SerializeField] private Camera minimapCam;
    [SerializeField] private List<GameObject> landmarks;
    [SerializeField] private List<GameObject> landmarksReplicas;

    [SerializeField] BodyFixedMap bodyFixedMap;

    // Enum for changing the minimap size
    public enum MiniMapSize
    {
        Custom
    }

    [SerializeField] private MiniMapSize layer = MiniMapSize.Custom;
    [SerializeField] private float customMiniMapSizeMultiplier = 1.0f;

    private float averageDistance;

    /// <summary>
    /// Record landmarks that already are visualized
    /// </summary>
    Dictionary<int, GameObject> VisualizedLandmarks = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Get BodyFixedMap
        bodyFixedMap = bodyFixedMap.GetComponent<BodyFixedMap>();

        // Calculate the average distance between landmarks
        averageDistance = CalculateAverageDistanceBetweenLandmarks(landmarks);

        // Adjust the mini-map camera size
        AdjustMiniMapCamera();
    }

    // Update is called once per frame
    void Update()
    {
        // For each target landmark
        for (int index = 0; index < landmarks.Count; index++)
        {
            // Landmark world space position -> viewport space position            
            Vector3 worldPos = landmarks[index].transform.position;
            Vector2 viewPos = minimapCam.WorldToViewportPoint(worldPos);

            // If the landmark is in the viewport
            bool InViewport = viewPos.x > 0.1 && viewPos.x < 0.9 && viewPos.y > 0.1 && viewPos.y < 0.9;
            bool InBoundary = viewPos.x > -0.1 && viewPos.x < 1.1 && viewPos.y > -0.1 && viewPos.y < 1.1;

            if (InViewport)
            {
                // If the landmark has not been visualized yet, clone it as a landmark replica
                if (!VisualizedLandmarks.ContainsKey(index))
                {
                    // Clone the landmark replica
                    GameObject LandmarkClone = bodyFixedMap.LandmarkVisualize(viewPos, landmarksReplicas[index]);
                    // Update dictionary
                    VisualizedLandmarks.Add(index, LandmarkClone);
                }

                // Update the landmark position on the map, if the "index" key exists in the dictionary
                if (VisualizedLandmarks.ContainsKey(index))
                {
                    bodyFixedMap.LandmarkPositionUpdate(viewPos, VisualizedLandmarks[index]);
                }
            }
            // If the landmark is in the boundary
            else if (InBoundary)
            {
                // Do nothing (for smoother transformation) - not make it appear or disappear

                // Update the landmark position on the map, if the "index" key exists in the dictionary
                if (VisualizedLandmarks.ContainsKey(index))
                {
                    bodyFixedMap.LandmarkPositionUpdate(viewPos, VisualizedLandmarks[index]);
                }
            }
            // If the landmark is out of the viewport
            else
            {
                if (VisualizedLandmarks.ContainsKey(index))
                {
                    Destroy(VisualizedLandmarks[index]);
                    VisualizedLandmarks.Remove(index);
                }
            }
        }
    }

    /// <summary>
    /// Calculate the average distance between all pairs of landmarks
    /// </summary>
    private float CalculateAverageDistanceBetweenLandmarks(List<GameObject> landmarks)
    {
        if (landmarks == null || landmarks.Count < 2)
        {
            UnityEngine.Debug.LogError("Not enough landmarks to calculate average distance!");
            return 0f;
        }

        float totalDistance = 0f;
        int pairCount = 0;

        // Calculate the total distance between all pairs of landmarks
        for (int i = 0; i < landmarks.Count; i++)
        {
            for (int j = i + 1; j < landmarks.Count; j++)
            {
                float distance = Vector3.Distance(landmarks[i].transform.position, landmarks[j].transform.position);
                totalDistance += distance;
                pairCount++;
            }
        }

        // Calculate the average distance
        float averageDistance = totalDistance / pairCount;
        UnityEngine.Debug.Log("Average distance between landmarks: " + averageDistance);
        return averageDistance;
    }

    /// <summary>
    /// Adjust the mini-map camera size based on the average distance and custom multiplier
    /// </summary>
    private void AdjustMiniMapCamera()
    {
        if (minimapCam == null)
        {
            UnityEngine.Debug.LogError("Mini-map camera is not assigned!");
            return;
        }

        // Adjust the camera height based on the average distance and custom multiplier
        float baseOrthographicSize = averageDistance / 2f; // default orthographic size
        minimapCam.orthographicSize = baseOrthographicSize * customMiniMapSizeMultiplier;

        UnityEngine.Debug.Log("Mini-map orthographic size set to: " + minimapCam.orthographicSize);
    }

    /// <summary>
    /// Update the mini-map size dynamically
    /// </summary>
    public void UpdateMiniMapSize(float customMultiplier)
    {
        customMiniMapSizeMultiplier = customMultiplier;
        AdjustMiniMapCamera();
    }
}