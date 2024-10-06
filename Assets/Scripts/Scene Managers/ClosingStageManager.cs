// Description: This script simply closes the game upon pressing the enter key
// Does not work in the Unity Editor; you have to build the project and run it
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using TMPro;

public class ClosingStageManager : MonoBehaviour
{
    [SerializeField] private GameObject defaultMap;
    [SerializeField] private GameObject vrCamera;
    [SerializeField] private VROrienter vrOrienter;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject virtualizerPlayer;
    [SerializeField] private GameObject roomscalePlayer;

    private bool fadedOut = false;

    void Awake()
    {
        // if (!PlayerManager.Instance && PersistentDataManager.Instance.IsVR) { PlayerSpawner.SpawnPlayerCoroutine(player, virtualizerPlayer, roomscalePlayer); }
        Teleport.PlayerTo(0f, 50f, 0f);

        if (PersistentDataManager.Instance.Map != "Default Map") {
            DisableDefaultMapAndEnableCustomMap();

            GameObject desktopPlayer = GameObject.Find("Player(Clone)");
            if (desktopPlayer != null) { Destroy(desktopPlayer); }

            /*GameObject virtualizerPlayer = GameObject.Find("Virtualizer Player(Clone)");
            if (virtualizerPlayer != null) { Destroy(virtualizerPlayer); }

            GameObject roomscalePlayer = GameObject.Find("Roomscale Player(Clone)");
            if (roomscalePlayer != null) { Destroy(roomscalePlayer); }*/
        }

        if (!PersistentDataManager.Instance.IsVR)
        {
            vrCamera.SetActive(false);
        }
    }

    void Start()
    {
        PlayerManager.Instance.BlackoutController.FadeOut();
        if (PersistentDataManager.Instance.Nearsight) { PlayerManager.Instance.NearsightController.FadeOut(); }
        // if (PersistentDataManager.Instance.IsRoomscale) { StartCoroutine(vrOrienter.BeginOrientation("", 0, 0, 5f, 5f, false)); }
    }

    void Update()
    {
        // Press any key to close the application
        if ((!PersistentDataManager.Instance.IsVR && Input.GetKeyDown(KeyCode.Return)) || PersistentDataManager.Instance.IsVR && (PlayerManager.Instance.TrackpadPressAction.GetStateDown(SteamVR_Input_Sources.Any) || PlayerManager.Instance.TriggerPressAction.GetStateDown(SteamVR_Input_Sources.Any)))
        {
            Application.Quit();
        }
    }

    private void DisableDefaultMapAndEnableCustomMap()
    {
        defaultMap.SetActive(false);
        MapManager.Instance.Self.SetActive(true);
    }
}
