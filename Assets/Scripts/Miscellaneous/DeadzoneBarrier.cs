// Description: This script is attached to thin barriers, automatically fail the player if they take a wrong turn
// Used in conjunction with deadzone colliders
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadzoneBarrier : MonoBehaviour
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
                Debug.Log("Player collided with a barrier");
                // Notify the RetracingStageManager
                stageManager.EnteredDeadzoneBarrier();
            }
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
