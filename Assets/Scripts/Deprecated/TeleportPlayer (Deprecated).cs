// Description: This script contains functions that teleport the player
// Used in Wayfinding and Pointing Stages
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TeleportPlayer
{
    public static void ToTrialStartPosition(GameObject player, CSVData trialData)
    {
        if (player != null)
        {
            // Get the Character Controller component 
            // Character controller can interfere with teleportation
            CharacterController characterController = player.GetComponentInChildren<CharacterController>();
            characterController.enabled = false;

            // VR Player has something else intended to get them to face the correct direction
            if (!PersistentDataManager.Instance.IsVR)
            {
                // Teleport the player and sets their rotation to face the object
                player.transform.position = new Vector3(trialData.StartingX * PersistentDataManager.Instance.Scale, player.transform.position.y, trialData.StartingZ * PersistentDataManager.Instance.Scale);
                player.transform.rotation = Quaternion.Euler(0, trialData.StartingAngle, 0);

                // Find and disable the Mouse Look component
                Camera playerCamera = player.GetComponentInChildren<Camera>();
                MouseLook mouseLookScript = playerCamera.GetComponent<MouseLook>();
                // So that the player's vertical look direction is reset as well
                mouseLookScript.SetXRotation(0);
            }

            else
            {
                CVirtPlayerController cyberithVirtualizer = player.GetComponentInChildren<CVirtPlayerController>();
                // Since the object with the Character Controller is a child, you can't teleport the player itself or you end up with a desynced Virtualizer player
                cyberithVirtualizer.transform.position = new Vector3(trialData.StartingX * PersistentDataManager.Instance.Scale, player.transform.position.y, trialData.StartingZ * PersistentDataManager.Instance.Scale);
            }

            characterController.enabled = true;
        }
    }
}*/