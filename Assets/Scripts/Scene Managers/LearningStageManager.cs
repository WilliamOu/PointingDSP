// Description: This script manages the learning functionality
// Controls the checkpoints in the scene as well as logging data
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Valve.VR;

public class LearningStageManager : MonoBehaviour
{
    [SerializeField] private GameObject defaultMap;
    [SerializeField] private GameObject defaultTriggerObjects;
    [SerializeField] private GameObject defaultArrows;
    [SerializeField] private GameObject defaultBarriers;

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

    private int totalCheckpoints;
    private int currentLap = 1;
    private int remainingLaps;

    // Store the checkpoints that the player has passed
    // HashSet allows us to add elements without checking for duplicates
    private HashSet<int> passedCheckpoints = new HashSet<int>();

    void Awake()
    {
        if (PersistentDataManager.Instance.Map != "Default Map") { DisableDefaultMapAndEnableCustomMap(); }
        ResetGameState.SetState();
        MazeFeatures.Initialize();
        if (!PlayerManager.Instance) { StartCoroutine(PlayerSpawner.SpawnPlayerCoroutine(player, virtualizerPlayer, roomscalePlayer)); }
        else { PlayerManager.Instance.RunDiagnostics(); }
        Teleport.PlayerTo(new Vector3(PersistentDataManager.Instance.SpawnPosition.x * PersistentDataManager.Instance.Scale, PersistentDataManager.Instance.SpawnPosition.y * PersistentDataManager.Instance.HeightScale, PersistentDataManager.Instance.SpawnPosition.z * PersistentDataManager.Instance.Scale), PersistentDataManager.Instance.SpawnRotation);
    }

    void Start()
    {
        LearningSpecificInitializations();
        InitializeCheckpoints();
        CreateLearningFile();
    }

    void Update()
    {
        HandleGameStart();
        LearningDataLogging();
    }

    private void CreateLearningFile()
    {
        string header = "Global Time,Participant X,Participant Y,Participant Z,Rotation X,Rotation Y";
        if (PersistentDataManager.Instance.IsVR) { header += ",Rotation Z,Grid,Lap"; }
        if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.IsRoomscale) { header += ",Virtualizer X,Virtualizer Z,Virtualizer Angle"; }
        if (PersistentDataManager.Instance.IsVR) { header += PlayerManager.Instance.GazeHandler.appendedEyeTrackingInformation;  }
        if (!PersistentDataManager.Instance.IsVR) { header += ",Grid,Lap\n"; }
        csvFileName = PersistentDataManager.Instance.RetraceCount == 0 ? "learning.csv" : $"review{PersistentDataManager.Instance.RetraceCount}.csv";
        PersistentDataManager.Instance.CreateAndWriteToFile(csvFileName, header);
    }

    private void LearningDataLogging()
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

                if (PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0:F2},{1},{2}", playerRotation.z, grid, currentLap); }
                if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.IsRoomscale) { sb.AppendFormat(",{0:F2},{1:F2},{2:F2}", PlayerManager.Instance.CyberithVirtualizer.transform.position.x, PlayerManager.Instance.CyberithVirtualizer.transform.position.z, PlayerManager.Instance.CyberithVirtualizer.GlobalOrientation.eulerAngles.y); }
                if (PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0}", PlayerManager.Instance.GazeHandler.GetFullGazeInfo()); }
                if (!PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0},{1}\n", grid, currentLap); }

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

    private void InitializeCheckpoints()
    {
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
        totalCheckpoints = checkpoints.Length;
        remainingLaps = PersistentDataManager.Instance.TotalLaps;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            checkpoint.SetLearningStageManager(this);
            checkpoint.RefreshCheckpoint();
        }

        Debug.Log($"Scene started. Total checkpoints: {totalCheckpoints}, Remaining laps: {remainingLaps}");

        // End the scene if there are no arrows. Remaining laps will intentionally show as -1
        if (totalCheckpoints == 0 && PersistentDataManager.Instance.Map != "Default Map") { remainingLaps = 0; PlayerPassedCheckpoint(0); }
    }

    private void HandleGameStart()
    {
        if (!PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.GameStarted && Input.GetKeyDown(KeyCode.Return))
        {
            PersistentDataManager.Instance.GameStarted = true;
            PlayerManager.Instance.PlayerMovement.EnableMovement();

            UpdateLapCountUI();

            nextLogTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.Interval;
            string printLine = $"Scene Start,Time In Instruction:,{Time.timeSinceLevelLoad:F2}\n";
            dataBuffer.Append(printLine);
        }   
        else if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.GameStarted && (PlayerManager.Instance.TrackpadPressAction.GetStateDown(SteamVR_Input_Sources.Any) || Input.GetKeyDown(KeyCode.Return)) && PersistentDataManager.Instance.PlayerIsOriented)
        {
            PersistentDataManager.Instance.GameStarted = true;
            if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = PersistentDataManager.Instance.MovementSpeed * PersistentDataManager.Instance.VRMovementSpeedMultiplier; }
            PlayerManager.Instance.BlackoutController.FadeOut();

            UpdateLapCountUI();

            nextLogTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.Interval;
            string printLine = $"Scene Start,Time In Instruction:,{Time.timeSinceLevelLoad:F2}\n";
            dataBuffer.Append(printLine);
        }
    }

    private void UpdateLapCountUI()
    {
        string plural = remainingLaps == 1 ? "" : "s";
        desktopUIText.text = $"{remainingLaps} lap{plural} remaining";
        if (PersistentDataManager.Instance.IsVR)
        {
            PlayerManager.Instance.VRUIText.text = $"{remainingLaps} lap{plural} remaining";
        }
    }

    private void LearningSpecificInitializations()
    {
        PlayerManager.Instance.DotCrosshair.Deactivate();
        if (!PersistentDataManager.Instance.IsVR)
        {
            PlayerManager.Instance.PlayerMovement.DisableMovement();
            PlayerManager.Instance.BlackoutController.FadeOut();
        }
        else
        {
            if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = 0f; }
            PersistentDataManager.Instance.PlayerIsOriented = false;
            if (PersistentDataManager.Instance.Map != "Default Map")
            {
                Vector3 playerSpawnPosition = PersistentDataManager.Instance.SpawnPosition;
                float playerRotationY = PersistentDataManager.Instance.SpawnRotation.y;

                Vector3 direction = Quaternion.Euler(0, playerRotationY, 0) * Vector3.forward;
                Vector3 pillarSpawn = playerSpawnPosition + direction * 2.5f;

                StartCoroutine(PlayerManager.Instance.VROrienter.BeginOrientation("- Learning Phase -\nWait For Instructions", PersistentDataManager.Instance.SpawnPosition.x * PersistentDataManager.Instance.Scale, PersistentDataManager.Instance.SpawnPosition.z * PersistentDataManager.Instance.Scale, pillarSpawn.x * PersistentDataManager.Instance.Scale, pillarSpawn.z * PersistentDataManager.Instance.Scale, true));
            }
            else
            {
                StartCoroutine(PlayerManager.Instance.VROrienter.BeginOrientation("- Learning Phase -\nWait For Instructions", 25 * PersistentDataManager.Instance.Scale, 25 * PersistentDataManager.Instance.Scale, 25 * PersistentDataManager.Instance.Scale, 20 * PersistentDataManager.Instance.Scale, true));
            }
        }
    }

    public void PlayerPassedCheckpoint(int checkpointNumber)
    {
        // If the player has already passed this checkpoint, ignore it
        passedCheckpoints.Add(checkpointNumber);
        Debug.Log("Passed Checkpoint " + checkpointNumber);

        // If the player has passed all checkpoints, they have completed a lap
        if (passedCheckpoints.Count >= totalCheckpoints && !PersistentDataManager.Instance.GameEnded)
        {
            Debug.Log("Lap " + currentLap + " completed.");
            currentLap++;
            passedCheckpoints.Clear();

            remainingLaps--;
            UpdateLapCountUI();

            if (remainingLaps <= 0)
            {
                PersistentDataManager.Instance.GameEnded = true;
                if (dataBuffer.Length > 0) { PersistentDataManager.Instance.PushLeftoverDataInBufferToFile(csvFileName, dataBuffer.ToString()); }
                if (PersistentDataManager.Instance.Map != "Default Map") { MapManager.Instance.LearningStageObjects.SetActive(false); }
                PlayerManager.Instance.BlackoutController.FadeIn("Retracing Stage");
            }
        }
    }

    private void DisableDefaultMapAndEnableCustomMap()
    {
        defaultMap.SetActive(false);
        defaultTriggerObjects.SetActive(false);
        defaultArrows.SetActive(false);
        defaultBarriers.SetActive(false);
        MapManager.Instance.Self.SetActive(true);
        MapManager.Instance.LearningStageObjects.SetActive(true);
    }
}
