// Description: Allows for the testing of roomscale spaces by enabling joystick movement in experimental mode
// Left joystick to walk, right joystick to sprint
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRControllerMovement : MonoBehaviour
{
    [SerializeField] private Transform playerHead;

    private SteamVR_Action_Vector2 leftJoystickInput;
    private SteamVR_Action_Vector2 rightJoystickInput;
    private float walkSpeed = 2;
    private float sprintSpeed = 4f;

    private void Start()
    {
        leftJoystickInput = SteamVR_Actions.default_ExperimentalModeWalk; 
        rightJoystickInput = SteamVR_Actions.default_ExperimentalModeSprint;
    }

    private void Update()
    {
        if (!PersistentDataManager.Instance.Experimental) { return; }

        Vector2 leftInputDirection = leftJoystickInput.GetAxis(SteamVR_Input_Sources.LeftHand);
        if (leftInputDirection != Vector2.zero)
        {
            MovePlayer(leftInputDirection, walkSpeed);
        }

        Vector2 rightInputDirection = rightJoystickInput.GetAxis(SteamVR_Input_Sources.RightHand);
        if (rightInputDirection != Vector2.zero)
        {
            MovePlayer(rightInputDirection, sprintSpeed);
        }
    }

    private void MovePlayer(Vector2 inputDirection, float speed)
    {
        Vector3 forward = playerHead.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = playerHead.right;
        right.y = 0;
        right.Normalize();

        Vector3 moveDirection = forward * inputDirection.y + right * inputDirection.x;

        transform.position += moveDirection * speed * Time.deltaTime;
    }
}