using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Teleport
{
    public static void PlayerToTrialStart(CSVData trial)
    {
        if (!PersistentDataManager.Instance.IsVR || !PersistentDataManager.Instance.IsRoomscale)
        {
            float startingX = trial.StartingX * PersistentDataManager.Instance.Scale;
            float startingY = trial.StartingY * PersistentDataManager.Instance.HeightScale;
            float startingZ = trial.StartingZ * PersistentDataManager.Instance.Scale;
            float startingHorizontalAngle = trial.StartingHorizontalAngle;
            float startingVerticalAngle = trial.StartingVerticalAngle;
            PlayerTo(new Vector3(startingX, startingY, startingZ), new Vector3(startingVerticalAngle, startingHorizontalAngle, 0f));
        }
    }

    public static void PlayerTo(float x, float z)
    {
        if (!PersistentDataManager.Instance.IsRoomscale)
        {
            CharacterController characterController = PlayerManager.Instance.GetComponentInChildren<CharacterController>();
            characterController.enabled = false;

            if (!PersistentDataManager.Instance.IsVR)
            {
                PlayerManager.Instance.transform.position = new Vector3(x, 0f, z);

                Camera playerCamera = PlayerManager.Instance.GetComponentInChildren<Camera>();
                MouseLook mouseLookScript = playerCamera.GetComponent<MouseLook>();
                mouseLookScript.SetXRotation(0);
            }
            else if (!PersistentDataManager.Instance.IsRoomscale)
            {
                PlayerManager.Instance.CyberithVirtualizer.transform.position = new Vector3(x, PlayerManager.Instance.transform.position.y, z);
            }

            characterController.enabled = true;
        }
    }

    public static void PlayerTo(float x, float z, float horizontalRotation)
    {
        PlayerTo(new Vector3(x, 0f, z), new Vector3(0f, horizontalRotation, 0f));
    }

    public static void PlayerTo(Vector3 position, Vector3 rotation)
    {
        if (!PersistentDataManager.Instance.IsRoomscale)
        {
            CharacterController characterController = PlayerManager.Instance.GetComponentInChildren<CharacterController>();
            characterController.enabled = false;

            if (!PersistentDataManager.Instance.IsVR)
            {
                PlayerManager.Instance.transform.position = position + new Vector3(0f, PersistentDataManager.Instance.VerticalPlayerOffset, 0f);
                PlayerManager.Instance.transform.rotation = Quaternion.Euler(rotation);

                Camera playerCamera = PlayerManager.Instance.GetComponentInChildren<Camera>();
                MouseLook mouseLookScript = playerCamera.GetComponent<MouseLook>();
                mouseLookScript.SetXRotation(0);
            }
            else
            {
                PlayerManager.Instance.CyberithVirtualizer.transform.position = new Vector3(position.x, PlayerManager.Instance.transform.position.y, position.z);
            }

            characterController.enabled = true;
        }
    }
}