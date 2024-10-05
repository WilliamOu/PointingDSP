// Description: This script operates the checkpoints in the Learning Stage
// Each checkpoint can be enabled or disabled and stores a reference to the next checkpoint in the series
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointNumber = 1;
    public Checkpoint nextCheckpoint;
    private LearningStageManager learningStageManager;
    private RetracingStageManager retracingStageManager;
    private MeshRenderer meshRenderer;
    private Collider arrowCollider;
    private bool isLearningStage = true;

    void Awake()
    {
        arrowCollider = GetComponent<Collider>();
        if (arrowCollider == null)
        {
            Debug.LogError("Collider component not found.");
        }
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetLearningStageManager(LearningStageManager manager)
    {
        learningStageManager = manager;
        isLearningStage = true;
    }
    
    public void SetRetracingStageManager(RetracingStageManager manager)
    {
        retracingStageManager = manager;
        isLearningStage = false;
    }

    public void RefreshCheckpoint()
    {
        if (checkpointNumber == 1)
        {
            EnableCheckpoint();
        }
        else
        {
            DisableCheckpoint();
        }
    }

    public void EnableCheckpoint()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
        arrowCollider.enabled = true;
    }

    public void DisableCheckpoint()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        arrowCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!PersistentDataManager.Instance.IsRoomscale || PersistentDataManager.Instance.GameStarted))
        {
            // Tell the stage manager that the player has passed this checkpoint
            if (isLearningStage) { learningStageManager.PlayerPassedCheckpoint(checkpointNumber); }
            else { retracingStageManager.PlayerPassedCheckpoint(checkpointNumber); }
            if (nextCheckpoint != null)
            {
                nextCheckpoint.EnableCheckpoint();
            }
            DisableCheckpoint();
            // Debug.Log("Checkpoint " + checkpointNumber + " crossed.");
        }
    }
}
