using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    public enum MiniMapSize
    {
        Small,   
        Medium, 
        Large,   
        Custom  
    }

    [SerializeField] private Camera minimapCam; 
    [SerializeField] private MiniMapSize miniMapSize = MiniMapSize.Medium; 
    [SerializeField] private float customMiniMapSizeMultiplier = 1.0f; 

    void Start()
    {
        AdjustMiniMap();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            miniMapSize = MiniMapSize.Small;
            AdjustMiniMap();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            miniMapSize = MiniMapSize.Medium;
            AdjustMiniMap();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            miniMapSize = MiniMapSize.Large;
            AdjustMiniMap();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            miniMapSize = MiniMapSize.Custom;
            AdjustMiniMap();
        }
    }

    void AdjustMiniMap()
    {
        AdjustCameraPosition();
        AdjustCameraAngle();
        AdjustClippingPlanes();
    }

    void AdjustCameraPosition()
    {
        Vector3 cameraPosition = minimapCam.transform.position;

        switch (miniMapSize)
        {
            case MiniMapSize.Small:
                cameraPosition.y = 50f; 
                break;
            case MiniMapSize.Medium:
                cameraPosition.y = 100f; 
                break;
            case MiniMapSize.Large:
                cameraPosition.y = 200f; 
                break;
            case MiniMapSize.Custom:
                cameraPosition.y = 100f * customMiniMapSizeMultiplier; 
                break;
        }

        minimapCam.transform.position = cameraPosition;
    }

    void AdjustCameraAngle()
    {
        Vector3 cameraRotation = minimapCam.transform.eulerAngles;

        switch (miniMapSize)
        {
            case MiniMapSize.Small:
                cameraRotation.x = 30f; 
                break;
            case MiniMapSize.Medium:
                cameraRotation.x = 45f; 
                break;
            case MiniMapSize.Large:
                cameraRotation.x = 60f; 
                break;
            case MiniMapSize.Custom:
                cameraRotation.x = 45f * customMiniMapSizeMultiplier; 
                break;
        }

        minimapCam.transform.eulerAngles = cameraRotation;
    }

    void AdjustClippingPlanes()
    {
        switch (miniMapSize)
        {
            case MiniMapSize.Small:
                minimapCam.farClipPlane = 500f; 
                break;
            case MiniMapSize.Medium:
                minimapCam.farClipPlane = 1000f; 
                break;
            case MiniMapSize.Large:
                minimapCam.farClipPlane = 2000f; 
                break;
            case MiniMapSize.Custom:
                minimapCam.farClipPlane = 1000f * customMiniMapSizeMultiplier; 
                break;
        }
    }
}