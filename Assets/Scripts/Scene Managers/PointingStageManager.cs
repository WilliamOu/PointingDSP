// Description: This script manages the pointing functionality
// Unlike other stages, pointing only cares about logging end of trial data, so it does not provide real time data
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Valve.VR;

public class PointingStageManager : MonoBehaviour
{
    [SerializeField] private GameObject defaultMap;
    [SerializeField] private TMP_Text desktopUIText;
    [SerializeField] private MazeObjectVisibilityController mazeObjects;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject virtualizerPlayer;
    [SerializeField] private GameObject roomscalePlayer;

    private List<CSVData> trials;

    private float elapsedTime = 0f;

    private bool trialStarted = false;

    private int startingTrial = 0;

    void Awake()
    {
        if (PersistentDataManager.Instance.Map != "Default Map") { DisableDefaultMapAndEnableCustomMap(); }
        if (PersistentDataManager.Instance.Pointsearch) { SceneManager.LoadScene("Wayfinding Stage"); }
        ResetGameState.SetState();
        MazeFeatures.Initialize();
        if (!PlayerManager.Instance) { StartCoroutine(PlayerSpawner.SpawnPlayerCoroutine(player, virtualizerPlayer, roomscalePlayer)); }
        else { PlayerManager.Instance.RunDiagnostics(); }
        Teleport.PlayerTo(new Vector3(PersistentDataManager.Instance.SpawnPosition.x * PersistentDataManager.Instance.Scale, PersistentDataManager.Instance.SpawnPosition.y * PersistentDataManager.Instance.HeightScale, PersistentDataManager.Instance.SpawnPosition.z * PersistentDataManager.Instance.Scale), PersistentDataManager.Instance.SpawnRotation);
    }

    void Start()
    {
        PointingSpecificInitializations();
        InitializeTrials();
        CreatePointingFile();
    }

    void Update()
    {
        HandleGameStart();
        HandleTrialProgress();
    }

    private void InitializeTrials()
    {
        PersistentDataManager.Instance.ShuffleDataList("P");
        trials = PersistentDataManager.Instance.DataList;
        startingTrial = PersistentDataManager.Instance.StartingTrial;
    }

    private void CreatePointingFile()
    {
        string header = "Global Time,Elapsed Time,Trial Number,Trial Name,Starting,Target,Starting Horizontal Angle,Starting Vertical Angle,True Horizontal Angle,True Vertical Angle,Participant X,Participant Y,Participant Z,Rotation X,Rotation Y";
        header += (PersistentDataManager.Instance.IsVR) ? ",Rotation Z,Grid\n" : ",Grid\n";
        // Pointing Stage does not track eye position, so gaze handler information is not appended
        PersistentDataManager.Instance.CreateAndWriteToFile("pointing.csv", header);
    }

    private void HandleGameStart()
    {
        if (!PersistentDataManager.Instance.GameStarted && (Input.GetKeyDown(KeyCode.Return) || PersistentDataManager.Instance.IsVR && PlayerManager.Instance.TrackpadPressAction.GetStateDown(SteamVR_Input_Sources.Any)))
        {
            StartCoroutine(HandlePointingTrials());
            PersistentDataManager.Instance.GameStarted = true;
            string printLine = $"Scene Start,Time In Instruction:,{Time.timeSinceLevelLoad:F2}\n";
            StartCoroutine(PersistentDataManager.Instance.AppendToFile("pointing.csv", printLine));
        }
    }

    private void HandleTrialProgress()
    {
        if (PersistentDataManager.Instance.GameStarted && trialStarted)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    private IEnumerator HandlePointingTrials()
    {
        for (int i = startingTrial; i < trials.Count; i++)
        {
            trialStarted = false;
            elapsedTime = 0f;

            if (PersistentDataManager.Instance.LimitCues) { mazeObjects.SetActiveObjects(trials[i].Starting, trials[i].Target); }

            if (!PersistentDataManager.Instance.IsRoomscale) { Teleport.PlayerToTrialStart(trials[i]); }

            desktopUIText.text = "Press enter to begin the trial";
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

            // Prompt the player to begin the trial
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || PersistentDataManager.Instance.IsVR && PlayerManager.Instance.TrackpadPressAction.GetStateDown(SteamVR_Input_Sources.Any));

            trialStarted = true;

            // Display the instruction text
            desktopUIText.text = "Point to the " + trials[i].Target;
            if (PersistentDataManager.Instance.IsVR) { 
                PlayerManager.Instance.VRUIText.text = "Look at the " + trials[i].Target;
                desktopUIText.text += "\nTrial " + (i + 1);
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || PersistentDataManager.Instance.IsVR && PlayerManager.Instance.TriggerPressAction.GetStateDown(SteamVR_Input_Sources.Any));

            if (!PersistentDataManager.Instance.Experimental) { PlayerManager.Instance.BlackoutController.FadeIn(); }

            Vector3 playerPosition = PlayerManager.Instance.PlayerCamera.transform.position;
            Vector3 playerRotation = PlayerManager.Instance.PlayerCamera.transform.eulerAngles;
            Vector2 gridPosition = GridCalculation.CalculateGridPosition(playerPosition, PersistentDataManager.Instance.Scale);
            string grid = GridCalculation.GridPositionToLabel(gridPosition);

            string printLine = $"{Time.timeSinceLevelLoad:F2},{elapsedTime:F2},{i+1},{trials[i].Level},{trials[i].Starting},{trials[i].Target},{trials[i].StartingHorizontalAngle},{trials[i].StartingVerticalAngle},{trials[i].TrueHorizontalAngle},{trials[i].TrueVerticalAngle},{playerPosition.x:F0},{playerPosition.y:F0},{playerPosition.z:F0},{playerRotation.x:F2},{playerRotation.y:F2}";
            printLine += (PersistentDataManager.Instance.IsVR) ? $",{playerRotation.z},{grid}\n" : $",{grid}\n";

            StartCoroutine(PersistentDataManager.Instance.AppendToFile("pointing.csv", printLine));

            if (!PersistentDataManager.Instance.Experimental) { yield return new WaitForSeconds(PersistentDataManager.Instance.BlackoutFadeDuration); }
        }

        PlayerManager.Instance.BlackoutController.FadeIn("Wayfinding Stage");
    }

    private void PointingSpecificInitializations()
    {
        PlayerManager.Instance.DotCrosshair.Activate();
        if (!PersistentDataManager.Instance.IsVR)
        {
            PlayerManager.Instance.PlayerMovement.DisableMovement();
        }
        else {
            if (!PersistentDataManager.Instance.IsRoomscale) { PlayerManager.Instance.CyberithVirtualizer.movementSpeedMultiplier = 0f; }
            desktopUIText.text = "- Pointing Phase -\nWait For Instructions";
            PlayerManager.Instance.VRUIText.text = "- Pointing Phase -\nWait For Instructions";
        }
    }

    private void DisableDefaultMapAndEnableCustomMap()
    {
        defaultMap.SetActive(false);
        MapManager.Instance.Self.SetActive(true);
    }
}
