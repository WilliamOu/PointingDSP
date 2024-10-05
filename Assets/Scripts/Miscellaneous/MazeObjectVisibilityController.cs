// Description: This script is used to disable all maze objects except the current and starting targets
// These are specified in the stage managers that call this script
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeObjectVisibilityController : MonoBehaviour
{
    private Dictionary<string, GameObject> mazeObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        // Populate the dictionary with all child GameObjects
        foreach (Transform child in transform)
        {
            mazeObjects[child.name] = child.gameObject;
        }
    }

    public void SetActiveObjects(string startingObjectName, string targetObjectName)
    {
        // Enable the starting and target objects, disable others
        foreach (var item in mazeObjects)
        {
            item.Value.SetActive(item.Key == startingObjectName || item.Key == targetObjectName);
        }
    }
}
