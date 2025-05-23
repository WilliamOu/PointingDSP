/************************************************************************************

Filename    :   CVirtPlayerController.cs
Content     :   PlayerController takes input from a VirtDevice and moves the player respectively.
Created     :   August 8, 2014
Last Updated:	December 5, 2019
Authors     :   Lukas Pfeifhofer
				Stefan Radlwimmer

Copyright   :   Copyright 2019 Cyberith GmbH

Licensed under the AssetStore Free License and the AssetStore Commercial License respectively.

************************************************************************************/

// The underlying class of UVirtDevice is dependent on the platform used
#if UNITY_ANDROID || PLATFORM_ANDROID
using UVirtDevice = CybSDK.CBleVirtDevice;
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UVirtDevice = CybSDK.IVirtDevice;
#endif

using UnityEngine;
using CybSDK;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CVirtDeviceController))]
public class CVirtPlayerController : MonoBehaviour
{
	private CVirtDeviceController deviceController;
	private CharacterController characterController;

	[Tooltip("Reference to a GameObject that will be rotated according to the player’s orientation in the device. If not set will search for 'ForwardDirection' attached to GameObject.")]
	public Transform forwardDirection;

	[Tooltip("Movement Speed Multiplier, to fine tune the players speed.")]
	[Range(0, 10)]
	public float movementSpeedMultiplier = 1.2f;

	public Vector3 MotionVector { get; private set; }

	// Added in order to be able to extract the player rotation angle
    public Quaternion GlobalOrientation { get; private set; }

    // Required for HeadBasedDirection
    private Camera mainCamera;
	// Required for DeviceBasedDirection
    private Quaternion coupledRotationOffset;

    // Use this for initialization
    void Start()
	{
		// Find the forward direction        
		if (forwardDirection == null)
		{
			forwardDirection = transform.Find("ForwardDirection");
		}

		//Check if this object has a CVirtDeviceController attached
		deviceController = GetComponent<CVirtDeviceController>();
		if (deviceController == null)
		{
			CLogger.LogError(string.Format("CVirtPlayerController requires a CVirtDeviceController attached to gameobject '{0}'.", gameObject.name));
			enabled = false;
			return;
		}

		//Check if this object has a CharacterController attached
		characterController = GetComponent<CharacterController>();
		if (characterController == null)
		{
			CLogger.LogError(string.Format("CVirtPlayerController requires a CharacterController attached to gameobject '{0}'.", gameObject.name));
			enabled = false;
			return;
		}

        if (deviceController.directionCouplingType == DirectionCouplingType.HeadBasedDirection)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                CLogger.LogError("CVirtPlayerController: No main camera found. Please tag one camera as 'MainCamera'.");
                enabled = false;
                return;
            }
        } else if (deviceController.directionCouplingType == DirectionCouplingType.DeviceBasedDirection) 
		{
            coupledRotationOffset = transform.localRotation;
        }

    }

	// Update is called once per frame
	void Update()
	{
		UVirtDevice device = deviceController.GetDevice();

		if (device == null || !device.IsOpen())
			return;

		// MOVE
		///////////
		Vector3 movement = device.GetMovementVector() * movementSpeedMultiplier;

		// ROTATION
		///////////
		Quaternion localOrientation = device.GetPlayerOrientationQuaternion();

		// Determine global orientation for characterController Movement
		Quaternion globalOrientation;

        switch (deviceController.directionCouplingType)
        {
			// Camera and Forward Direction are decoupled
            case DirectionCouplingType.Decoupled:
                if (forwardDirection != null)
                {
                    forwardDirection.transform.localRotation = localOrientation;
                    globalOrientation = forwardDirection.transform.rotation;
                }
                else
                {
                    // Quaternions are applied right to left
                    globalOrientation = localOrientation * gameObject.transform.rotation;
                }
                break;
				// Camera and Forward Direction are based on the head direction
            case DirectionCouplingType.HeadBasedDirection:
                globalOrientation = mainCamera.transform.rotation;
                break;
				// Camera and Forward Direction are based on the Virtualizer device rotation
            case DirectionCouplingType.DeviceBasedDirection:
                gameObject.transform.rotation = localOrientation * coupledRotationOffset;
                globalOrientation = localOrientation;
                break;
            default:
                globalOrientation = localOrientation * gameObject.transform.rotation;
                break;
        }
		GlobalOrientation = globalOrientation;

		Vector3 motionVector = globalOrientation * movement;
		MotionVector = motionVector;
		characterController.SimpleMove(motionVector);
	}
}