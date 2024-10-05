// Description: This script fails the player if they enter the deadzone from the wrong direction and succeeds if they enter from the correct direction
// Ensures that the player can move on, but does not let them navigate the maze backwards
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OneWayDeadzoneBarrier : MonoBehaviour
{
    [SerializeField] private Vector3 deadzoneDirection = Vector3.forward;

    private RetracingStageManager stageManager;

    void Start()
    {
        // Find the RetracingStageManager in the scene
        stageManager = FindObjectOfType<RetracingStageManager>();
        if (stageManager == null)
        {
            Debug.LogError("RetracingStageManager not found in the scene.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Determine the player's entry direction relative to the deadzone's 'forward' direction
        Vector3 direction = other.transform.position - transform.position;
        float dotProduct = Vector3.Dot(direction.normalized, deadzoneDirection.normalized);

        if (dotProduct > 0)
        {
            stageManager.FailRetracing();
        }
        else
        {
            stageManager.SucceedRetracing();
        }
    }
}
