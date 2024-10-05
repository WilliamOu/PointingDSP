// Description: This script marks areas that the player is not supposed to be in and fails them if in the area for too long
// Used in conjunction with deadzone barriers
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadzoneCollider : MonoBehaviour
{
    public bool IsOneWay { get; set; } = false;
    private RetracingStageManager stageManager;

    public void SetRetracingStageManager(RetracingStageManager manager)
    {
        stageManager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.gameObject.CompareTag("Player"))
        {
            if (!IsOneWay || DotProduct(other) > 0)
            {
                Debug.Log("Player entered deadzone");
                // Notify the RetracingStageManager
                stageManager.InDeadzone = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.gameObject.CompareTag("Player"))
        {
            // Notify the RetracingStageManager
            stageManager.InDeadzone = false;
        }
    }

    private float DotProduct(Collider other)
    {
        Transform exitTriggerTransform = transform.Find("Exit Trigger");
        if (exitTriggerTransform == null)
        {
            Debug.LogError("Exit Trigger transform not found");
            return 1;
        }

        Vector3 playerEntryDirection = other.transform.position - transform.position;
        Vector3 exitDirection = exitTriggerTransform.position - transform.position;
        float dotProduct = Vector3.Dot(playerEntryDirection.normalized, exitDirection.normalized);
        return dotProduct;
    }
}
