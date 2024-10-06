// Description: The Player Manager assists with the initialization and the tracking of the player
// It utilizes the singleton pattern to ensure that there is only one instance of it
using UnityEngine;
using TMPro;
using Valve.VR;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public PlayerMovement PlayerMovement { get; private set; }
    public Camera PlayerCamera { get; private set; }
    public CVirtPlayerController CyberithVirtualizer { get; private set; }
    public TMP_Text VRUIText { get; private set; }
    public BlackoutController BlackoutController { get; private set; }
    public DotCrosshair DotCrosshair { get; private set; }
    public VROrienter VROrienter { get; private set; }
    public GazeHandler GazeHandler { get; private set; }
    public NearsightController NearsightController { get; private set; }
    public SteamVR_Action_Boolean TrackpadPressAction { get; private set; }
    public SteamVR_Action_Boolean TriggerPressAction { get; private set; }

    public void RunDiagnostics()
    {
        if (!PlayerMovement && !PersistentDataManager.Instance.IsVR) { Debug.LogError("Player Movement is null"); }
        if (!PlayerCamera) { Debug.LogError("Player Camera is null"); }
        if (!CyberithVirtualizer && PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.IsRoomscale) { Debug.LogError("Virtualizer is null"); }
        if (!BlackoutController) { Debug.LogError("Blackout Controller is null"); }
        if (!DotCrosshair) { Debug.LogError("Dot Crosshair is null"); }
        if (!VROrienter && PersistentDataManager.Instance.IsVR) { Debug.LogError("VR Orienter is null"); }
        if (!GazeHandler && PersistentDataManager.Instance.IsVR) { Debug.LogError("Gaze Handler is null"); }
        if (!NearsightController) { Debug.LogError("Nearsight Controller is null"); }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializePlayer();
    }

    private void InitializePlayer()
    {
        if (!PersistentDataManager.Instance.IsVR)
        {
            StartCoroutine(ControlXR.StopVR());
            PlayerMovement = GetComponentInChildren<PlayerMovement>();
            PlayerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            CyberithVirtualizer = null;
            VRUIText = null;
            TrackpadPressAction = null;
            TriggerPressAction = null;
        }
        else
        {
            PlayerMovement = null;
            PlayerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            CyberithVirtualizer = GetComponentInChildren<CVirtPlayerController>();
            if (CyberithVirtualizer) { CyberithVirtualizer.movementSpeedMultiplier = 0f; }
            VRUIText = GameObject.Find("VR UI Text").GetComponent<TMP_Text>();
            TrackpadPressAction = SteamVR_Actions.default_TrackpadPress;
            TriggerPressAction = SteamVR_Actions.default_TriggerPress;
        }

        BlackoutController = FindObjectOfType<BlackoutController>();
        DotCrosshair = FindObjectOfType<DotCrosshair>();
        VROrienter = FindObjectOfType<VROrienter>();
        GazeHandler = FindObjectOfType<GazeHandler>();
        NearsightController = FindObjectOfType<NearsightController>();
    }
}