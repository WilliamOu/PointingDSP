// Description: Spawns a different player depending on the game mode
// Since the study has to be both VR and desktop compatible, the player cannot be a prefab that's already placed in the scene
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Valve.VR;

public class PlayerSpawnerDeprecated : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject virtualizerPlayer;
    [SerializeField] private GameObject roomscalePlayer;

    [SerializeField] private float x;
    [SerializeField] private float z;
    [SerializeField] private float playerRotation;
    [SerializeField] private float virtualizerPlayerRotation;

    public void InitializePlayer(bool fadeOut, out GameObject player, out PlayerMovement playerMovement, out Camera playerCamera, out CVirtPlayerController cyberithVirtualizer, out TMP_Text vrUIText, out BlackoutController blackoutController, out SteamVR_Action_Boolean trackpadPressAction, out SteamVR_Action_Boolean triggerPressAction)
    {
        if (!PersistentDataManager.Instance.IsVR)
        {
            StartCoroutine(ControlXR.StopVR());
            player = SpawnPCPlayer();
            playerMovement = player.GetComponentInChildren<PlayerMovement>();
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            cyberithVirtualizer = null;
            vrUIText = null;
            trackpadPressAction = null;
            triggerPressAction = null;
        }
        else
        {
            player = (PersistentDataManager.Instance.IsRoomscale) ? SpawnRoomscalePlayer() : SpawnVirtualizerPlayer();
            playerMovement = null;
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            cyberithVirtualizer = player.GetComponentInChildren<CVirtPlayerController>();
            if (cyberithVirtualizer) { cyberithVirtualizer.movementSpeedMultiplier = 0f; }
            vrUIText = GameObject.Find("VR UI Text").GetComponent<TMP_Text>();
            trackpadPressAction = SteamVR_Actions.default_TrackpadPress;
            triggerPressAction = SteamVR_Actions.default_TriggerPress;
        }

        blackoutController = FindObjectOfType<BlackoutController>();
        if (!PersistentDataManager.Instance.IsVR && fadeOut)
        {
            blackoutController.FadeOut();
        }
    }

    private GameObject SpawnPCPlayer()
    {
        x = SceneManager.GetActiveScene().name == "Training Stage" ? x : x * PersistentDataManager.Instance.Scale;
        z = SceneManager.GetActiveScene().name == "Training Stage" ? z : z * PersistentDataManager.Instance.Scale;

            // Create the position vector from the x and z coordinates
            Vector3 spawnPosition = new Vector3(x, 1.88f, z);

        // Create the rotation quaternion from the Y rotation
        Quaternion spawnRotation = Quaternion.Euler(0, playerRotation, 0);

        // Instantiate the player prefab at the specified position and rotation
        GameObject playerInstance = GameObject.Instantiate(player, spawnPosition, spawnRotation);

        // Adjusts the player height to the proper value
        playerInstance.transform.localScale = new Vector3(1f, 1.8f, 1f);

        return playerInstance;
    }

    private GameObject SpawnVirtualizerPlayer()
    {
        x = SceneManager.GetActiveScene().name == "Training Stage" ? x : x * PersistentDataManager.Instance.Scale;
        z = SceneManager.GetActiveScene().name == "Training Stage" ? z : z * PersistentDataManager.Instance.Scale;

        // Create the position vector from the x and z coordinates
        Vector3 spawnPosition = new Vector3(x, 0, z);

        // Create the rotation quaternion from the Y rotation
        Quaternion spawnRotation = Quaternion.Euler(0, virtualizerPlayerRotation, 0);

        GameObject vrPlayerInstance = GameObject.Instantiate(virtualizerPlayer, spawnPosition, spawnRotation);
        return vrPlayerInstance;
    }

    private GameObject SpawnRoomscalePlayer()
    {
        // Position and rotation are irrelevant because base stations track the player's actual location
        // For clarification, the parent prefab's position and rotation has no bearing on the camera's position or rotation
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);

        GameObject vrPlayerInstance = GameObject.Instantiate(roomscalePlayer, spawnPosition, spawnRotation);

        return vrPlayerInstance;
    }
}