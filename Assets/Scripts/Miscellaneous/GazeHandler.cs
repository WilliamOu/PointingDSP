// Description: This script tracks the eye position and provides data as needed
// Adapted from Justin Kasowski's SimpleXR with permission; see SimpleXR's GitHub page for more information
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
using Valve.VR;
using Varjo.XR;

// For future reference, could add SRANIPAL support here for use with Vives
public class GazeHandler : MonoBehaviour
{
    public string appendedEyeTrackingInformation = ",Screen Fixation X,Screen Fixation Y,Gaze Fixation X,Gaze Fixation Y,Gaze Fixation Z,Left Eye Position X,Left Eye Position Y,Left Eye Position Z,Right Eye Position X,Right Eye Position Y,Right Eye Position Z,Left Eye Rotation X,Left Eye Rotation Y,Left Eye Rotation Z,Right Eye Rotation X,Right Eye Rotation Y,Right Eye Rotation Z,Left Eye Open Amount,Right Eye Open Amount\n";

    private bool isVarjoHeadset;
    private VarjoEyeTracking.GazeData gazeData;

    private InputDevice eyeTracker;
    private Eyes eyesData;
    private Camera vrCamera;

    private void Start()
    {
        vrCamera = Camera.main; // Adjust this to get the correct VR camera if necessary
        InitializeEyeTracker();
    }

    private void InitializeEyeTracker()
    {
        isVarjoHeadset = VarjoEyeTracking.IsGazeAllowed();
        if (isVarjoHeadset)
        {
            Debug.Log("Varjo headset detected, using Varjo eye tracking.");
        }
        if (PersistentDataManager.Instance.IsVR)
        {
            List<InputDevice> inputDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, inputDevices);
            if (inputDevices.Count > 0)
            {
                eyeTracker = inputDevices[0];
                Debug.Log("Eye tracker initialized: " + eyeTracker.name);
            }
            else
            {
                Debug.LogWarning("No eye tracker found.");
            }
        }
    }

    public string GetFullGazeInfo()
    {
        Vector2 screenFixationPoint = GetScreenFixationPoint();
        return screenFixationPoint.x.ToString("F2") + "," +
               screenFixationPoint.y.ToString("F2") + "," + 
               GetGazeFixationPoint().x.ToString("F2") + "," + 
               GetGazeFixationPoint().y.ToString("F2") + "," + 
               GetGazeFixationPoint().z.ToString("F2") + "," +
               GetLeftEyePosition().x.ToString("F2") + "," + 
               GetLeftEyePosition().y.ToString("F2") + "," + 
               GetLeftEyePosition().z.ToString("F2") + "," +
               GetRightEyePosition().x.ToString("F2") + "," + 
               GetRightEyePosition().y.ToString("F2") + "," + 
               GetRightEyePosition().z.ToString("F2") + "," +
               GetLeftEyeRotation().eulerAngles.x.ToString("F2") + "," + 
               GetLeftEyeRotation().eulerAngles.y.ToString("F2") + "," + 
               GetLeftEyeRotation().eulerAngles.z.ToString("F2") + "," +
               GetRightEyeRotation().eulerAngles.x.ToString("F2") + "," + 
               GetRightEyeRotation().eulerAngles.y.ToString("F2") + "," + 
               GetRightEyeRotation().eulerAngles.z.ToString("F2") + "," +
               GetLeftEyeOpenAmount().ToString("F2") + "," + 
               GetRightEyeOpenAmount().ToString("F2") + "\n";
    }

    public Vector2 GetScreenFixationPoint()
    {
        Vector3 gazeFixationPoint = GetGazeFixationPoint();
        Vector3 screenPoint = vrCamera.WorldToScreenPoint(gazeFixationPoint);
        return new Vector2(screenPoint.x, screenPoint.y);
    }

    public Vector3 GetGazeFixationPoint()
    {
        if (isVarjoHeadset)
        {
            gazeData = VarjoEyeTracking.GetGaze();
            return gazeData.gaze.forward;
        }
        else if (eyeTracker.isValid && eyesData.TryGetFixationPoint(out Vector3 fixationPoint))
        {
            return fixationPoint;
        }
        return Vector3.zero;
    }

    public Vector3 GetLeftEyePosition()
    {
        if (eyeTracker.isValid && eyesData.TryGetLeftEyePosition(out Vector3 leftPos))
        {
            return leftPos;
        }
        return Vector3.zero;
    }

    public Vector3 GetRightEyePosition()
    {
        if (eyeTracker.isValid && eyesData.TryGetRightEyePosition(out Vector3 rightPos))
        {
            return rightPos;
        }
        return Vector3.zero;
    }

    public Quaternion GetLeftEyeRotation()
    {
        if (eyeTracker.isValid && eyesData.TryGetLeftEyeRotation(out Quaternion leftRot))
        {
            return leftRot;
        }
        return Quaternion.identity;
    }

    public Quaternion GetRightEyeRotation()
    {
        if (eyeTracker.isValid && eyesData.TryGetRightEyeRotation(out Quaternion rightRot))
        {
            return rightRot;
        }
        return Quaternion.identity;
    }

    public float GetLeftEyeOpenAmount()
    {
        if (eyeTracker.isValid && eyesData.TryGetLeftEyeOpenAmount(out float leftOpenAmount))
        {
            return leftOpenAmount;
        }
        return 0f;
    }

    public float GetRightEyeOpenAmount()
    {
        if (eyeTracker.isValid && eyesData.TryGetRightEyeOpenAmount(out float rightOpenAmount))
        {
            return rightOpenAmount;
        }
        return 0f;
    }
}