// Description: This script manages the wayfinding functionality
// Works in conjunction with the TargetObject script in order to determine when the player has reached the target
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Valve.VR;

public class WayfindingStageManager : MonoBehaviour
{
    [SerializeField] private GameObject defaultMap;

    [SerializeField] private TMP_Text desktopUIText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private MazeObjectVisibilityController mazeObjects;

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

    private List<CSVData> trials;
    private CSVData currentTrial;
    private GameObject currentTarget;

    private bool trialStarted = false;
    private bool targetReached = false;
    private int trialCount = 0;

    private float trialTimer;

    private float elapsedTime = 0f;

    public int StartingTrial { get; set; } = 0;

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
        WayfindingSpecificInitializations();
        InitializeTrials();
        CreateWayfindingFile();
        if (PersistentDataManager.Instance.Pointsearch) { CreatePointingFile(); }
    }

    void Update()
    {
        HandleGameStart();
        WayfindingDataLogging();
    }

    // Data Logging
    private void CreateWayfindingFile()
    {
        string header = "Global Time,Elapsed Time,Trial Number,Trial Name,Starting Horizontal Angle,Starting Vertical Angle,True Horizontal Angle,True Vertical Angle,Participant X,Participant Y,Participant Z,Rotation X,Rotation Y";
        if (PersistentDataManager.Instance.IsVR) { header += ",Rotation Z,Grid"; }
        if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.IsRoomscale) { header += ",Virtualizer X,Virtualizer Z,Virtualizer Angle"; }
        if (PersistentDataManager.Instance.IsVR) { header += PlayerManager.Instance.GazeHandler.appendedEyeTrackingInformation; }
        if (!PersistentDataManager.Instance.IsVR) { header += ",Grid\n"; }
        PersistentDataManager.Instance.CreateAndWriteToFile("wayfinding.csv", header);
    }

    private void CreatePointingFile()
    {
        string header = "Global Time,Elapsed Time,Trial Number,Trial Name,Starting,Target,Starting Angle,True Angle,Participant X,Participant Z,Rotation X,Rotation Y";
        header += (PersistentDataManager.Instance.IsVR) ? ",Rotation Z,Grid\n" : ",Grid\n";
        // Pointing Stage does not track eye position, so gaze handler information is not appended
        PersistentDataManager.Instance.CreateAndWriteToFile("pointing.csv", header);
    }

    private void WayfindingDataLogging()
    {
        if (PersistentDataManager.Instance.GameStarted && !PersistentDataManager.Instance.GameEnded)
        {
            if (currentTarget != null && trialStarted)
            {
                trialTimer -= Time.deltaTime;
                elapsedTime += Time.deltaTime;
                UpdateTimerUI();

                if (Time.timeSinceLevelLoad >= nextLogTime)
                {
                    playerPosition = PlayerManager.Instance.PlayerCamera.transform.position;
                    playerRotation = PlayerManager.Instance.PlayerCamera.transform.eulerAngles;
                    gridPosition = GridCalculation.CalculateGridPosition(playerPosition, PersistentDataManager.Instance.Scale);
                    grid = GridCalculation.GridPositionToLabel(gridPosition);

                    // Build the data line
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0:F2},{1:F2},{2},{3},{4},{5},{6:F2},{7:F2},{8:F2},{9:F2},{10:F2}",
                        Time.timeSinceLevelLoad, elapsedTime, trialCount, currentTrial.Level, currentTrial.Starting,
                        currentTrial.Target, playerPosition.x, playerPosition.y, playerPosition.z, playerRotation.x, playerRotation.y);

                    if (PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0:F2},{1}", playerRotation.z, grid); }
                    if (PersistentDataManager.Instance.IsVR && !PersistentDataManager.Instance.IsRoomscale) { sb.AppendFormat(",{0:F2},{1:F2},{2:F2}", PlayerManager.Instance.CyberithVirtualizer.transform.position.x, PlayerManager.Instance.CyberithVirtualizer.transform.position.z, PlayerManager.Instance.CyberithVirtualizer.GlobalOrientation.eulerAngles.y); }
                    if (PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0}", PlayerManager.Instance.GazeHandler.GetFullGazeInfo()); }
                    if (!PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0}\n", grid); }

                    dataBuffer.Append(sb.ToString());

                    nextLogTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.Interval;
                }

                if (Time.timeSinceLevelLoad >= nextBufferWriteTime)
                {
                    StartCoroutine(PersistentDataManager.Instance.AppendToFile("wayfinding.csv", dataBuffer.ToString()));
                    dataBuffer.Clear();
                    nextBufferWriteTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.BufferWriteInterval;
                }
            }
        }
    }

    private void InitializeTrials()
    {
        PersistentDataManager.Instance.ShuffleDataList("W");
        trials = PersistentDataManager.Instance.DataList;
        StartingTrial = PersistentDataManager.Instance.StartingTrial;
    }

    private void HandleGameStart()
    {
        if (!PersistentDataManager.Instance.GameStarted && (Input.GetKeyDown(KeyCode.Return) || PersistentDataManager.Instance.IsVR && PlayerManager.Instance.TrackpadPressAction.GetStateDown(SteamVR_Input_Sources.Any)))
        {
            StartCoroutine(HandleWayfindingTrials());
            PersistentDataManager.Instance.GameStarted = true;

            nextLogTime = Time.timeSinceLevelLoad + PersistentDataManager.Instance.Interval;
            string printLine = $"Scene Start,Time In Instruction:,{Time.timeSinceLevelLoad:F2}\n";
            dataBuffer.Append(printLine);
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null && PersistentDataManager.Instance.Experimental == true)
        {
            // Prints the time remaining in Experimental Mode
            timerText.text = Mathf.Ceil(trialTimer).ToString();
        }
    }

    private IEnumerator HandleWayfindingTrials()
    {
        for (int i = StartingTrial; i < trials.Count; i++)
        {
            elapsedTime = 0f;

            // Wayfinding scene logs in frame intervals unlike pointing scene which only logs after each trial
            // It needs a way to track which trial we're on
            trialCount = i + 1;

            if (PersistentDataManager.Instance.LimitCues) { mazeObjects.SetActiveObjects(trials[i].Starting, trials[i].Target); }

            currentTrial = trials[i];
            currentTarget = FindTargetObjectInScene(trials[i].Target);
            if (!PersistentDataManager.Instance.IsRoomscale) { Teleport.PlayerToTrialStart(trials[i]); }

            // Not calling fadeout on the first trial is to prevent potentially undefined behavior
            if (PersistentDataManager.Instance.Nearsight && (trialCount - StartingTrial > 1)) { PlayerManager.Instance.NearsightController.FadeOut(); }

            desktopUIText.text = "Press space to begin the trial";
            if (PersistentDataManager.Instance.IsVR)
            {
                desktopUIText.text = "VR Player is orienting";
                yield return StartCoroutine(PlayerManager.Instance.VROrienter.BeginOrientation(trials[i]));

                PlayerManager.Instance.VRUIText.text = "Press the trackpad to begin the trial";
                desktopUIText.text = "Press the trackpad to begin the trial\nTrial " + (i + 1);
            }

            // Fadeout for the opening blackout and trial transition blackouts; skip timer for experimental mode
            PlayerManager.Instance.BlackoutController.FadeOut();
            if (!PersistentDataManager.Instance.Experimental) { yield return new WaitForSeconds(0.15f); }

            trialTimer = PersistentDataManager.Instance.Time;
            targetReached = false;

            // Start the Pointing Stage subsection if using Pointsearch option (combined Pointing and Wayfinding Stage)
            if (PersistentDataManager.Instance.Pointsearch) { StartCoroutine(BeginPointingStageSubTask(trialCount - 1)); }
            else {
                // Prompt the player to begin the trial
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || PersistentDataManager.Instance.IsVR && PlayerManager.Instance.TrackpadPressAction.GetStateDown(SteamVR_Input_Sources.Any));
                trialStarted = true;
            }

            yield return new WaitUntil(() => trialStarted);

            if (PersistentDataManager.Instance.Nearsight) { PlayerManager.Instance.NearsightController.FadeIn(); }

            if (!PersistentDataManager.Instance.IsVR) { PlayerManager.Instance.PlayerMovement.EnableMovement(); }
            else if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = PersistentDataManager.Instance.MovementSpeed * PersistentDataManager.Instance.VRMovementSpeedMultiplier; }

            desktopUIText.text = "Find the " + trials[i].Target;
            if (PersistentDataManager.Instance.IsVR) { 
                PlayerManager.Instance.VRUIText.text = "Find the " + trials[i].Target;
                desktopUIText.text += "\nTrial " + (i + 1);
            }

            yield return new WaitUntil(() => trialTimer <= 0 || targetReached);

            trialStarted = false;

            if (!PersistentDataManager.Instance.Experimental) { PlayerManager.Instance.BlackoutController.FadeIn(); }

            if (trialTimer <= 0 && !targetReached)
            {
                // Log to timeout.txt
                string timeoutLine = $"{trials[i].Level},{trials[i].Starting},{trials[i].Target}\n";
                StartCoroutine(PersistentDataManager.Instance.AppendToFile("timeout.txt", timeoutLine));
            }

            // i + 1 since the loop is zero indexed
            string printLine = $"Trial {i+1} Complete,Time:,{Time.timeSinceLevelLoad:F2},{elapsedTime:F2}\n";
            dataBuffer.Append(printLine);

            if (!PersistentDataManager.Instance.IsVR) { PlayerManager.Instance.PlayerMovement.DisableMovement(); }
            else if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = 0f; }

            if (!PersistentDataManager.Instance.Experimental) { yield return new WaitForSeconds(PersistentDataManager.Instance.BlackoutFadeDuration); }
        }

        PersistentDataManager.Instance.GameEnded = true;
        if (dataBuffer.Length > 0) { PersistentDataManager.Instance.PushLeftoverDataInBufferToFile("wayfinding.csv", dataBuffer.ToString()); }
        PlayerManager.Instance.BlackoutController.FadeIn("VR Closing Stage");
    }

    private GameObject FindTargetObjectInScene(string targetName)
    {
        GameObject targetObject = GameObject.Find(targetName);
        if (targetObject != null)
        {
            // Check if the TargetObject component is already attached
            TargetObject targetComponent = targetObject.GetComponent<TargetObject>();
            if (targetComponent == null)
            {
                // Add the TargetObject component to the target object
                targetComponent = targetObject.AddComponent<TargetObject>();
            }
            // Initialize the TargetObject component with the WayfindingStageManager instance
            targetComponent.StageManager = this;
            return targetObject;
        }
        else
        {
            Debug.LogError("Unknown target or target not found in scene: " + targetName);
            return null;
        }
    }

    private void WayfindingSpecificInitializations()
    {
        if (!PersistentDataManager.Instance.Pointsearch) { PlayerManager.Instance.DotCrosshair.Deactivate(); }
        if (!PersistentDataManager.Instance.IsVR)
        {
            PlayerManager.Instance.PlayerMovement.DisableMovement();
        }
        else {
            if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = 0f; }
            desktopUIText.text = "- Wayfinding Phase -\nWait For Instructions";
            PlayerManager.Instance.VRUIText.text = "- Wayfinding Phase -\nWait For Instructions";
        }
    }

    private IEnumerator BeginPointingStageSubTask(int trialCount)
    {
        PlayerManager.Instance.DotCrosshair.Activate();

        // Display the instruction text
        desktopUIText.text = "Point to the " + trials[trialCount].Target;
        if (PersistentDataManager.Instance.IsVR) { PlayerManager.Instance.VRUIText.text = "Look at the " + trials[trialCount].Target; }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || PersistentDataManager.Instance.IsVR && PlayerManager.Instance.TriggerPressAction.GetStateDown(SteamVR_Input_Sources.Any));

        Vector3 playerPosition = PlayerManager.Instance.PlayerCamera.transform.position;
        Vector3 playerRotation = PlayerManager.Instance.PlayerCamera.transform.eulerAngles;
        Vector2 gridPosition = GridCalculation.CalculateGridPosition(playerPosition, PersistentDataManager.Instance.Scale);
        string grid = GridCalculation.GridPositionToLabel(gridPosition);

        string printLine = $"{Time.timeSinceLevelLoad:F2},{elapsedTime:F2},{trialCount + 1},{trials[trialCount].Level},{trials[trialCount].Starting},{trials[trialCount].Target},{trials[trialCount].StartingHorizontalAngle},{trials[trialCount].StartingVerticalAngle},{trials[trialCount].TrueHorizontalAngle},{trials[trialCount].TrueVerticalAngle},{playerPosition.x:F0},{playerPosition.y:F0},{playerPosition.z:F0},{playerRotation.x:F2},{playerRotation.y:F2}";
        printLine += (PersistentDataManager.Instance.IsVR) ? $",{playerRotation.z},{grid}\n" : $",{grid}\n";

        StartCoroutine(PersistentDataManager.Instance.AppendToFile("pointing.csv", printLine));

        PlayerManager.Instance.DotCrosshair.Deactivate();

        trialStarted = true;
    }

    // Called by the TargetObject script when the player is in contact with a certain object
    public void PlayerReachedTarget(GameObject target)
    {
        if (target.name == currentTarget.name)
        {
            targetReached = true;
        }
    }

    private void DisableDefaultMapAndEnableCustomMap()
    {
        defaultMap.SetActive(false);
        MapManager.Instance.Self.SetActive(true);
    }
}