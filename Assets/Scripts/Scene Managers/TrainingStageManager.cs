// Description: This script manages the training functionality
// Allows the adjustment of the mouse sensitivity and movement speed in experimental mode. Switch between them using the comma and period keys
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Valve.VR;

public class TrainingStageManager : MonoBehaviour
{
    [SerializeField] private GameObject mapWithArrows;
    [SerializeField] private TMP_Text desktopUIText;
    [SerializeField] private TMP_Text startUIText;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject virtualizerPlayer;
    [SerializeField] private GameObject roomscalePlayer;

    void Awake()
    {
        if (PersistentDataManager.Instance.SkipTraining) { SceneManager.LoadScene("Learning Stage"); }

        // Scale correction
        if (PersistentDataManager.Instance.Map != "Default Map")
        {
            MapManager.Instance.Self.SetActive(false); 
            mapWithArrows.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Teleport.PlayerTo(new Vector3(11.25f, 0f, 11.25f), new Vector3(0f, 180f, 0f));
        }
        else { Teleport.PlayerTo(new Vector3(22.5f, 0f, 22.5f), new Vector3(0f, 180f, 0f)); }

        MazeFeatures.Initialize();

        if (!PlayerManager.Instance) { StartCoroutine(PlayerSpawner.SpawnPlayerCoroutine(player, virtualizerPlayer, roomscalePlayer)); }
        else { PlayerManager.Instance.RunDiagnostics(); }
    }

    void Start()
    {
        TrainingSpecificInitializations();
    }

    void Update()
    {
        HandleGameStart();
        LoadNextScene();
    }

    void HandleGameStart()
    {
        if (!PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.GameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            PersistentDataManager.Instance.GameStarted = true;
            startUIText.text = "";
            desktopUIText.text = "- Training Stage -\nControls:\nWASD to move\nMouse to look";
            PlayerManager.Instance.PlayerMovement.EnableMovement();
            PlayerManager.Instance.PlayerCamera.GetComponentInChildren<MouseLook>().MouseSensitivity = PersistentDataManager.Instance.MouseSensitivity;
        }
        if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.GameStarted && PersistentDataManager.Instance.PlayerIsOriented)
        {
            PersistentDataManager.Instance.GameStarted = true;
            if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = PersistentDataManager.Instance.MovementSpeed * PersistentDataManager.Instance.VRMovementSpeedMultiplier; }
            PlayerManager.Instance.BlackoutController.FadeOut();
        }
    }

    void LoadNextScene()
    {
        // Uses enter key (Desktop) or side button (VR) to load the next scene
        if (PersistentDataManager.Instance.GameStarted && Input.GetKeyDown(KeyCode.Return) || PersistentDataManager.Instance.IsVR && PersistentDataManager.Instance.GameStarted && PlayerManager.Instance.TrackpadPressAction.GetStateDown(SteamVR_Input_Sources.Any))
        {
            PersistentDataManager.Instance.GameEnded = true;
            PlayerManager.Instance.BlackoutController.FadeIn("Learning Stage");
        }
    }

    void TrainingSpecificInitializations()
    {
        PlayerManager.Instance.DotCrosshair.Deactivate();
        if (!PersistentDataManager.Instance.IsVR)
        {
            PlayerManager.Instance.PlayerMovement.DisableMovement();
            PlayerManager.Instance.PlayerCamera.GetComponentInChildren<MouseLook>().MouseSensitivity = 0;
            PlayerManager.Instance.BlackoutController.FadeOut();
        }
        else
        {
            startUIText.text = "";
            if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = 0f; }
            if (PersistentDataManager.Instance.Map != "Default Map")
            {
                StartCoroutine(PlayerManager.Instance.VROrienter.BeginOrientation("Walk around! Look around!", 11.25f, 11.25f, 11.25f, 8.75f));
            }
            else
            {
                StartCoroutine(PlayerManager.Instance.VROrienter.BeginOrientation("Walk around! Look around!", 22.5f, 22.5f, 22.5f, 17.5f));
            }
        }
    }
}