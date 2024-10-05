using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    // Store the checkpoints that the player has passed
    // HashSet allows us to add elements without checking for duplicates
    private HashSet<int> passedCheckpoints = new HashSet<int>();
    private int totalCheckpoints;
    private int currentLap = 1;

    void Start()
    {
        // Find the total number of checkpoints in the scene
        totalCheckpoints = FindObjectsOfType<Checkpoint>().Length;
    }

    public void PlayerPassedCheckpoint(int checkpointNumber)
    {
        // If the player has already passed this checkpoint, ignore it
        passedCheckpoints.Add(checkpointNumber);

        // If the player has passed all checkpoints, they have completed a lap
        if (passedCheckpoints.Count == totalCheckpoints)
        {
            Debug.Log("Lap " + currentLap + " completed.");
            currentLap++;
            passedCheckpoints.Clear(); // Reset for the next lap
        }
    }
}