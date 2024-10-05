// Description: This script initializes additional features in the maze
// Options are set in the Title Screen
using UnityEngine;

public static class ResetGameState
{
    public static void SetState()
    {
        PersistentDataManager.Instance.GameStarted = false;
        PersistentDataManager.Instance.GameEnded = false;
        PersistentDataManager.Instance.PlayerIsOriented = false;
    } 
}