// Description: This script manages the retracing functionality
// Manages the zones and barriers that compose the stage
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Valve.VR;

public class RetracingStageManager : MonoBehaviour
{
    [SerializeField] private GameObject defaultMap;
    [SerializeField] private GameObject defaultDeadzones;

    [SerializeField] private TMP_Text desktopUIText;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject virtualizerPlayer;
    [SerializeField] private GameObject roomscalePlayer;

    private Vector3 playerPosition;
    private Vector3 playerRotation;
    private float playerRotationY;
    private Vector2 gridPosition;
    private string grid;

    private StringBuilder dataBuffer = new StringBuilder();
    private float nextBufferWriteTime = 0f;
    private float nextLogTime = 0f;

    private string csvFileName;

    private bool playerIsMoving = false;
    private Vector3 currentPlayerPosition;
    private Vector3 lastPlayerPosition;
    private float movementThreshold = 0.001f;

    private float deadzoneTime = 0f;
    private float stillTime = 0f;
    private float timeToFail = 5f;

    public bool InDeadzone { get; set; } = false;

    private int totalCheckpoints;
    private HashSet<int> passedCheckpoints = new HashSet<int>();

    void Awake()
    {
        if (PersistentDataManager.Instance.SkipRetracing) { SceneManager.LoadScene("Pointing Stage"); }
        if (PersistentDataManager.Instance.Map != "Default Map") { DisableDefaultMapAndEnableCustomMap(); }
        ResetGameState.SetState();
        MazeFeatures.Initialize();
        if (!PlayerManager.Instance) { StartCoroutine(PlayerSpawner.SpawnPlayerCoroutine(player, virtualizerPlayer, roomscalePlayer)); }
        else { PlayerManager.Instance.RunDiagnostics(); }
        Teleport.PlayerTo(new Vector3(PersistentDataManager.Instance.SpawnPosition.x * PersistentDataManager.Instance.Scale, PersistentDataManager.Instance.SpawnPosition.y * PersistentDataManager.Instance.HeightScale, PersistentDataManager.Instance.SpawnPosition.z * PersistentDataManager.Instance.Scale), PersistentDataManager.Instance.SpawnRotation);
    }

    void Start()
    {
        RetracingSpecificInitializations();
        InitializeCheckpoints();
        InitializeDeadzones();
        CreateRetracingFile();
    }

    void Update()
    {
        HandleGameStart();
        HandlePlayerIdleTime();
        HandleDeadzoneTime();
        RetracingDataLogging();
    }

    // Data Logging
    private void CreateRetracingFile()
    {
        string header = "Global Time,Participant X,Participant Y,Participant Z,Rotation X,Rotation Y";
        if (PersistentDataManager.Instance.IsVR) { header += ",Rotation Z,Grid"; }
        if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.IsRoomscale) { header += ",Virtualizer X,Virtualizer Z,Virtualizer Angle"; }
        if (PersistentDataManager.Instance.IsVR) { header += PlayerManager.Instance.GazeHandler.appendedEyeTrackingInformation; }
        if (!PersistentDataManager.Instance.IsVR) { header += ",Grid\n"; }
        csvFileName = $"retrace{PersistentDataManager.Instance.RetraceCount + 1}.csv";
        PersistentDataManager.Instance.CreateAndWriteToFile(csvFileName, header);
    }

    private void RetracingDataLogging()
    {
        if (PersistentDataManager.Instance.GameStarted && !PersistentDataManager.Instance.GameEnded)
        {
            if (Time.timeSinceLevelLoad >= nextLogTime)
            {
                playerPosition = PlayerManager.Instance.PlayerCamera.transform.position;
                playerRotation = PlayerManager.Instance.PlayerCamera.transform.eulerAngles;
                gridPosition = GridCalculation.CalculateGridPosition(playerPosition, PersistentDataManager.Instance.Scale);
                grid = GridCalculation.GridPositionToLabel(gridPosition);

                // create the data line
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0:F2},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2}",
                    Time.timeSinceLevelLoad, playerPosition.x, playerPosition.y, playerPosition.z, playerRotation.x, playerRotation.y);

                if (PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0:F2},{1}", playerRotation.z, grid); }
                if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.IsRoomscale) { sb.AppendFormat(",{0:F2},{1:F2},{2:F2}", PlayerManager.Instance.CyberithVirtualizer.transform.position.x, PlayerManager.Instance.CyberithVirtualizer.transform.position.z, PlayerManager.Instance.CyberithVirtualizer.GlobalOrientation.eulerAngles.y); }
                if (PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0}", PlayerManager.Instance.GazeHandler.GetFullGazeInfo()); }
                if (!PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0}\n", grid); }

                dataBuffer.Append(sb.ToString());

                nextLogTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.Interval;
            }

            if (Time.timeSinceLevelLoad >= nextBufferWriteTime)
            {
                StartCoroutine(PersistentDataManager.Instance.AppendToFile(csvFileName, dataBuffer.ToString()));
                dataBuffer.Clear();
                nextBufferWriteTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.BufferWriteInterval;
            }
        }
    }

    // Custom maps use a checkpoint system of invisible arrows mirroring the learning stage's arrows
    // The default map still uses the legacy trigger system
    private void InitializeCheckpoints()
    {
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
        totalCheckpoints = checkpoints.Length;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.SetRetracingStageManager(this);
            checkpoint.RefreshCheckpoint();
        }

        // End the scene if there are no arrows
        if (totalCheckpoints == 0 && PersistentDataManager.Instance.Map != "Default Map") { PlayerPassedCheckpoint(0); }
    }

    private void InitializeDeadzones()
    {
        DeadzoneCollider[] colliders = FindObjectsOfType<DeadzoneCollider>();
        foreach (DeadzoneCollider collider in colliders)
        {
            collider.SetRetracingStageManager(this);
        }

        DeadzoneBarrier[] barriers = FindObjectsOfType<DeadzoneBarrier>();
        foreach (DeadzoneBarrier barrier in barriers)
        {
            barrier.SetRetracingStageManager(this);
        }
    }

    private void HandleGameStart()
    {
        if (!PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.GameStarted && Input.GetKeyDown(KeyCode.Return))
        {
            PersistentDataManager.Instance.GameStarted = true;
            desktopUIText.text = "";
            PlayerManager.Instance.PlayerMovement.EnableMovement();

            nextLogTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.Interval;
            string printLine = $"Scene Start,Time In Instruction:,{Time.timeSinceLevelLoad:F2}\n";
            StartCoroutine(PersistentDataManager.Instance.AppendToFile(csvFileName, printLine));
        }
        else if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.GameStarted && (PlayerManager.Instance.TrackpadPressAction.GetStateDown(SteamVR_Input_Sources.Any) || Input.GetKeyDown(KeyCode.Return)) && PersistentDataManager.Instance.PlayerIsOriented)
        {
            PersistentDataManager.Instance.GameStarted = true;
            desktopUIText.text = "";
            PlayerManager.Instance.VRUIText.text = "";
            if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = PersistentDataManager.Instance.MovementSpeed * PersistentDataManager.Instance.VRMovementSpeedMultiplier; }
            PlayerManager.Instance.BlackoutController.FadeOut();

            nextLogTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.Interval;
            string printLine = $"Scene Start,Time In Instruction:,{Time.timeSinceLevelLoad:F2}\n";
            StartCoroutine(PersistentDataManager.Instance.AppendToFile(csvFileName, printLine));
        }
    }

    private void HandlePlayerIdleTime()
    {
        if (!PersistentDataManager.Instance.IsVR) { currentPlayerPosition = PlayerManager.Instance.transform.position; }
        else if (!PersistentDataManager.Instance.IsRoomscale) { currentPlayerPosition = PlayerManager.Instance.CyberithVirtualizer.transform.position; }
        else { currentPlayerPosition = PlayerManager.Instance.PlayerCamera.transform.position; }

        // Checks distance rather than using an equality due to floating point imprecision
        // Tighten the movement threshold if, for some reason, the player is incorrectly being registered as not moving
        // Or if, conversely, the player is unable to be registered as not moving, loosen it
        if (Vector3.Distance(lastPlayerPosition, currentPlayerPosition) > movementThreshold)
        {
            playerIsMoving = true;
        }
        else
        {
            playerIsMoving = false;
        }
        lastPlayerPosition = currentPlayerPosition;

        if (!playerIsMoving && PersistentDataManager.Instance.GameStarted && !PersistentDataManager.Instance.GameEnded)
        {
            stillTime += Time.deltaTime;
            if (stillTime >= timeToFail)
            {
                Debug.Log("Failed retracing due to standing still");
                FailRetracing();
            }
        }
        else
        {
            stillTime = 0;
        }
    }

    private void HandleDeadzoneTime()
    {
        if (InDeadzone)
        {
            deadzoneTime += Time.deltaTime;
            if (deadzoneTime >= timeToFail)
            {
                Debug.Log("Failed retracing due to standing in a deadzone for too long");
                FailRetracing();
            }
        }
        else
        {
            deadzoneTime = 0f;
        }
    }

    public void EnteredDeadzoneBarrier()
    {
        if (PersistentDataManager.Instance.GameStarted && !PersistentDataManager.Instance.GameEnded)
        {
            FailRetracing();
        } 
    }

    public void PlayerPassedCheckpoint(int checkpointNumber)
    {
        if (!PersistentDataManager.Instance.GameEnded)
        {
            passedCheckpoints.Add(checkpointNumber);
            Debug.Log($"Passed Checkpoint {checkpointNumber}");

            if (passedCheckpoints.Count >= totalCheckpoints)
            {
                SucceedRetracing();
            }
        }
    }

    private void RetracingSpecificInitializations()
    {
        PlayerManager.Instance.DotCrosshair.Deactivate();
        
        if (!PersistentDataManager.Instance.IsVR)
        {
            currentPlayerPosition = PlayerManager.Instance.transform.position;
            lastPlayerPosition = PlayerManager.Instance.transform.position;
            PlayerManager.Instance.PlayerMovement.DisableMovement();
            PlayerManager.Instance.BlackoutController.FadeOut();
        }
        else
        {
            if (!PersistentDataManager.Instance.IsRoomscale) {
                PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = 0f;
                currentPlayerPosition = PlayerManager.Instance.CyberithVirtualizer.transform.position;
                lastPlayerPosition = PlayerManager.Instance.CyberithVirtualizer.transform.position;
            }
            else
            {
                currentPlayerPosition = PlayerManager.Instance.PlayerCamera.transform.position;
                lastPlayerPosition = PlayerManager.Instance.PlayerCamera.transform.position;
            }
            PersistentDataManager.Instance.PlayerIsOriented = false;
            if (PersistentDataManager.Instance.Map != "Default Map")
            {
                Vector3 playerSpawnPosition = PersistentDataManager.Instance.SpawnPosition;
                float playerRotationY = PersistentDataManager.Instance.SpawnRotation.y;

                Vector3 direction = Quaternion.Euler(0, playerRotationY, 0) * Vector3.forward;
                Vector3 pillarSpawn = playerSpawnPosition + direction * 2.5f;

                StartCoroutine(PlayerManager.Instance.VROrienter.BeginOrientation("- Retracing Phase -\nWait For Instructions", PersistentDataManager.Instance.SpawnPosition.x * PersistentDataManager.Instance.Scale, PersistentDataManager.Instance.SpawnPosition.z * PersistentDataManager.Instance.Scale, pillarSpawn.x * PersistentDataManager.Instance.Scale, pillarSpawn.z * PersistentDataManager.Instance.Scale, true));
            }
            else
            {
                StartCoroutine(PlayerManager.Instance.VROrienter.BeginOrientation("- Retracing Phase -\nWait For Instructions", 25 * PersistentDataManager.Instance.Scale, 25 * PersistentDataManager.Instance.Scale, 25 * PersistentDataManager.Instance.Scale, 20 * PersistentDataManager.Instance.Scale, true));
            }
        }
    }

    public void FailRetracing()
    {
        if (!PersistentDataManager.Instance.GameEnded)
        {
            Debug.Log("Player failed retracing");
            PersistentDataManager.Instance.RetraceCount++;
            PersistentDataManager.Instance.TotalLaps = 1;

            PersistentDataManager.Instance.GameEnded = true;
            if (dataBuffer.Length > 0) { PersistentDataManager.Instance.PushLeftoverDataInBufferToFile(csvFileName, dataBuffer.ToString()); }
            if (PersistentDataManager.Instance.Map != "Default Map") { MapManager.Instance.RetracingStageObjects.SetActive(false); }
            PlayerManager.Instance.BlackoutController.FadeIn("Learning Stage");
        }
    }

    public void SucceedRetracing()
    {
        if (!PersistentDataManager.Instance.GameEnded)
        {
            PersistentDataManager.Instance.GameEnded = true;
            if (dataBuffer.Length > 0) { PersistentDataManager.Instance.PushLeftoverDataInBufferToFile(csvFileName, dataBuffer.ToString()); }
            if (PersistentDataManager.Instance.Map != "Default Map") { MapManager.Instance.RetracingStageObjects.SetActive(false); }
            PlayerManager.Instance.BlackoutController.FadeIn("Pointing Stage");
        }
    }

    private void DisableDefaultMapAndEnableCustomMap()
    {
        defaultMap.SetActive(false);
        defaultDeadzones.SetActive(false);
        MapManager.Instance.Self.SetActive(true);
        MapManager.Instance.RetracingStageObjects.SetActive(true);
    }
}
