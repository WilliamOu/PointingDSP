// Description: This script is used in the wayfinding scene and tells the WayfindingStageManager if the player is touching a certain target
// The WayfindingStageManager will then confirm if it is the correct object
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : MonoBehaviour
{
    public WayfindingStageManager StageManager { get; set; }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.gameObject.CompareTag("Player"))
        {
            // Notify the WayfindingStageManager
            if (!StageManager)
            {
                Debug.Log("TargetObject.cs Failsafe activated. There is an issue with the target object script.");
                StageManager = FindObjectOfType<WayfindingStageManager>();
            }
            StageManager.PlayerReachedTarget(gameObject);
            // Debug.Log(this.name);
        }
    }
}