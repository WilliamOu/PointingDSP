// Description: This script manages the title screen UI elements and to load the selected scenes
// Also creates the file system that stores the CSV files
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using UnityEngine.XR;
using TMPro;
using Valve.VR;
using System.Linq;
using System.Diagnostics;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject DefaultMap;
    [SerializeField] private GameObject AlternateMap1;
    [SerializeField] private GameObject AlternateMap2;
    [SerializeField] private GameObject AlternateMap3;
    [SerializeField] private GameObject mapManager;

    [SerializeField] private PresetController presetController;

    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject settingsCanvas;

    [SerializeField] private Toggle additionalWallsForLegacyMapToggle;
    [SerializeField] private TMP_InputField vrMovementSpeedMultiplierInput;
    [SerializeField] private TMP_InputField xRotationFixInput;
    [SerializeField] private TMP_InputField bufferWriteIntervalInput;
    [SerializeField] private TMP_InputField blackoutFadeDurationInput;
    [SerializeField] private TMP_InputField vrOrienterRequiredHeadAngleInput;
    [SerializeField] private TMP_InputField vrOrienterRequiredVirtualizerAngleInput;
    [SerializeField] private TMP_InputField vrOrienterRequiredProximityInput;
    [SerializeField] private TMP_InputField vrOrienterPillarResponsivenessInput;
    [SerializeField] private TMP_InputField nearsightTargetFogDensityInput;
    [SerializeField] private TMP_InputField nearsightTransitionTimeInput;

    [SerializeField] private TMP_InputField participantField;
    [SerializeField] private TMP_InputField genderField;
    [SerializeField] private TMP_InputField dateField;

    [SerializeField] private GameObject trialsCompleted;

    [SerializeField] private Button addPresetButton;
    [SerializeField] private Button editPresetButton;
    public TMP_Dropdown presetDropdown;
    [SerializeField] private TMP_Dropdown stageDropdown;

    [SerializeField] private Button legacyButton;

    [SerializeField] private Button desktopStartButton;
    [SerializeField] private Button virtualizerStartButton;
    [SerializeField] private Button roomscaleStartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button fileButton;
    [SerializeField] private Button returnToMainMenuButton;
    [SerializeField] private TMP_Text errorMessageText;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject virtualizerPlayer;
    [SerializeField] private GameObject roomscalePlayer;

    [SerializeField] private GameObject csvCalculationDummy;
    [SerializeField] private GameObject uiCamera;
    [SerializeField] private GameObject basicCameraRigOnlyVRPlayer;

    private void Awake()
    {
        LoadOrCreateFileAndUpdatePersistentDataManagerInternalSettings();
    }

    private void Start()
    {
        trialsCompleted.SetActive(false);

        settingsButton.onClick.AddListener(OpenSettingsMenu);
        fileButton.onClick.AddListener(OpenPersistentDataPath);
        settingsCanvas.SetActive(true);
        returnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
        settingsCanvas.SetActive(false);

        presetDropdown.onValueChanged.AddListener(delegate { OnPresetDropdownValueChanged(presetDropdown.value); });
        stageDropdown.onValueChanged.AddListener(delegate { ShowTrials(stageDropdown.value); });

        legacyButton.onClick.AddListener(OnLegacyPressed);

        desktopStartButton.onClick.AddListener(OnStartPressed);
        virtualizerStartButton.onClick.AddListener(OnStartPressedVirtualizer);
        roomscaleStartButton.onClick.AddListener(OnStartPressedRoomscale);

        addPresetButton.onClick.AddListener(() => OnAddPressed());
        editPresetButton.onClick.AddListener(() => OnEditPressed());

        presetController.PopulatePresetDropdown(presetDropdown);
        LoadLastUsedPreset();
    }

    private void OnAddPressed()
    {
        presetController.SwitchToPresetCanvas();
        presetController.GenerateUniquePresetNameAndLoadPreset();
    }

    private void OnEditPressed()
    {
        presetController.SwitchToPresetCanvas();
        presetController.LoadPresetIntoUI(presetController.Presets[presetDropdown.value].presetName);
    }

    private void OnLegacyPressed()
    {
        if (PersistentDataManager.Instance != null)
        {
            Destroy(PersistentDataManager.Instance.gameObject);
        }

        SceneManager.LoadScene("Legacy Title Screen");
    }

    private void OnStartPressedRoomscale()
    {
        PersistentDataManager.Instance.IsRoomscale = true;
        OnStartPressedVirtualizer();
    }

    private void OnStartPressedVirtualizer()
    {
        PersistentDataManager.Instance.IsVR = true;
        OnStartPressed();
    }

    private void OnStartPressed()
    {
        TMP_InputField trialsCompletedInputField = trialsCompleted.GetComponent<TMP_InputField>();
        if (!int.TryParse(trialsCompletedInputField.text, out int startingTrial)) { startingTrial = 0; }
        if (string.IsNullOrWhiteSpace(participantField.text)) { errorMessageText.color = Color.red; errorMessageText.text = "Participant field cannot be empty."; return; }
        if (!int.TryParse(genderField.text, out int gender)) { errorMessageText.color = Color.red; errorMessageText.text = "Invalid gender value."; return; }
        if (string.IsNullOrWhiteSpace(dateField.text)) { errorMessageText.color = Color.red; errorMessageText.text = "Date field cannot be empty."; return; }
        PersistentDataManager.Instance.StartingTrial = startingTrial;
        PersistentDataManager.Instance.ParticipantNumber = participantField.text;
        PersistentDataManager.Instance.Gender = gender;
        PersistentDataManager.Instance.Date = dateField.text;

        Preset selectedPreset = presetController.GetPresetByName(presetController.Presets[presetDropdown.value].presetName);
        LoadPresetData(selectedPreset);

        SetupParticipantDirectory();

        if (!PersistentDataManager.Instance.IsVR)
        {
            StartCoroutine(ControlXR.StopVR());
        }

        StartCoroutine(BeginLoading());
    }

    private IEnumerator BeginLoading()
    {
        errorMessageText.text = "<color=green>Loading Study</color>\n<color=red>Do not close the game</color>";

        yield return null;

        uiCamera.SetActive(false);
        basicCameraRigOnlyVRPlayer.SetActive(false);
        if (PersistentDataManager.Instance.Map != "Default Map")
        {
            mapManager.GetComponent<MapManager>().InitializeMap(stageDropdown.options[stageDropdown.value].text);
            PersistentDataManager.Instance.ReadCSV(Path.Combine(Application.persistentDataPath, "Maps", PersistentDataManager.Instance.Map, "trials.csv"), PersistentDataManager.Instance.DataList);
        }
        else
        {
            Destroy(mapManager);
            StartCoroutine(PlayerSpawner.SpawnPlayerCoroutine(player, virtualizerPlayer, roomscalePlayer, stageDropdown.options[stageDropdown.value].text));
        }
    }

    private void LoadPresetData(Preset preset)
    {
        PersistentDataManager.Instance.MouseSensitivity = preset.mouseSensitivity;
        PersistentDataManager.Instance.MovementSpeed = preset.movementSpeed;
        PersistentDataManager.Instance.TotalLaps = preset.laps;
        PersistentDataManager.Instance.Time = preset.time;
        PersistentDataManager.Instance.Scale = preset.scale;
        PersistentDataManager.Instance.HeightScale = preset.height;
        PersistentDataManager.Instance.Interval = preset.interval;
        PersistentDataManager.Instance.Experimental = preset.experimentalMode;
        PersistentDataManager.Instance.ShuffleObjects = preset.shuffleObjects;
        PersistentDataManager.Instance.LockVerticalLook = preset.lockVerticalLook;
        PersistentDataManager.Instance.LimitCues = preset.limitCues;
        PersistentDataManager.Instance.Nearsight = preset.nearsight;
        PersistentDataManager.Instance.UseShadows = preset.useShadows;
        PersistentDataManager.Instance.Pointsearch = preset.pointsearch;
        PersistentDataManager.Instance.SkipTraining = preset.skipTraining;
        PersistentDataManager.Instance.SkipRetracing = preset.skipRetracing;
        PersistentDataManager.Instance.Map = preset.map;
    }

    private void OnPresetDropdownValueChanged(int index)
    {
        string presetName = presetController.Presets[index].presetName;
        presetController.UpdateLastUsedPreset(presetName);
    }

    private void ShowTrials(int selectedIndex)
    {
        string selectedStageName = stageDropdown.options[selectedIndex].text;
        bool showTrials = selectedStageName == "Pointing Stage" || selectedStageName == "Wayfinding Stage";
        if (showTrials)
        {
            trialsCompleted.SetActive(true);
            stageDropdown.GetComponent<RectTransform>().sizeDelta = new Vector2(227, stageDropdown.GetComponent<RectTransform>().sizeDelta.y);
            stageDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(-86.5f, stageDropdown.GetComponent<RectTransform>().anchoredPosition.y);
        }
        else
        {
            trialsCompleted.SetActive(false);
            stageDropdown.GetComponent<RectTransform>().sizeDelta = new Vector2(400, stageDropdown.GetComponent<RectTransform>().sizeDelta.y);
            stageDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, stageDropdown.GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    private void LoadLastUsedPreset()
    {
        string lastUsedPresetName = presetController.LoadLastUsedPresetFromStorage();
        int presetIndex = presetController.Presets.FindIndex(p => p.presetName == lastUsedPresetName);
        if (presetIndex != -1)
        {
            presetDropdown.value = presetIndex;
        }
    }

    // Creates the directory that stores a unique participant's data
    private void SetupParticipantDirectory()
    {
        // Create the "CSVs" directory if it doesn't exist
        string csvsDirectoryPath = Path.Combine(Application.persistentDataPath, "CSVs");
        if (!Directory.Exists(csvsDirectoryPath))
        {
            Directory.CreateDirectory(csvsDirectoryPath);
        }

        // Create a subdirectory for the participant
        string participantDirectoryPath = Path.Combine(csvsDirectoryPath, PersistentDataManager.Instance.ParticipantNumber);
        if (!Directory.Exists(participantDirectoryPath))
        {
            Directory.CreateDirectory(participantDirectoryPath);
        }

        // meta.txt
        string metaFilePath = Path.Combine(participantDirectoryPath, "meta.txt");
        if (!File.Exists(metaFilePath))
        {
            using (StreamWriter writer = new StreamWriter(metaFilePath, false))
            {
                // Write meta data
                WriteMetaData(writer);
            }
        }

        // timeout.txt 
        string timeoutFilePath = Path.Combine(participantDirectoryPath, "timeout.txt");
        if (!File.Exists(timeoutFilePath))
        {
            File.Create(timeoutFilePath).Dispose();
        }

        // Prints the file path
        UnityEngine.Debug.Log("Participant directory: " + participantDirectoryPath);
    }

    private void WriteMetaData(StreamWriter writer)
    {
        writer.WriteLine("Participant Number: " + PersistentDataManager.Instance.ParticipantNumber);
        writer.WriteLine("Participant Gender: " + PersistentDataManager.Instance.Gender);
        writer.WriteLine("Date: " + PersistentDataManager.Instance.Date);
        writer.WriteLine("Map: " + PersistentDataManager.Instance.Map);
        writer.WriteLine("Time: " + PersistentDataManager.Instance.Time);
        writer.WriteLine("Laps: " + PersistentDataManager.Instance.TotalLaps);
        writer.WriteLine("Scale: " + PersistentDataManager.Instance.Scale);
        writer.WriteLine("Height Scale: " + PersistentDataManager.Instance.HeightScale);
        writer.WriteLine("Height Scale: " + PersistentDataManager.Instance.HeightScale);
        writer.WriteLine("Mouse Sensitivity: " + PersistentDataManager.Instance.MouseSensitivity);
        writer.WriteLine("Movement Speed: " + PersistentDataManager.Instance.MovementSpeed);
        writer.WriteLine("Experimental: " + PersistentDataManager.Instance.Experimental);
        writer.WriteLine("Use Shadows: " + PersistentDataManager.Instance.UseShadows);
        writer.WriteLine("Limit Cues: " + PersistentDataManager.Instance.LimitCues);
        writer.WriteLine("VR Mode: " + (PersistentDataManager.Instance.IsVR ? "On" : "Off"));
        writer.WriteLine("Additional Walls: " + PersistentDataManager.Instance.AdditionalWallsForLegacyMap);
    }

    private void LoadOrCreateFileAndUpdatePersistentDataManagerInternalSettings()
    {
        // Internal Settings
        bool additionalWallsForLegacyMap = true;
        float vrMovementSpeedMultiplier = 0.5f;
        float xRotationFix = 5f;
        float bufferWriteInterval = 1f;
        float blackoutFadeDuration = 0.75f;
        float vrOrienterRequiredHeadAngle = 20f;
        float vrOrienterRequiredVirtualizerAngle = 50f;
        float vrOrienterRequiredProximity = 1.5f;
        float vrOrienterPillarResponsiveness = 0.5f;
        float nearsightTargetFogDensity = 0.1f;
        float nearsightTransitionTime = 0.35f;

        // Load settings from Settings.txt if it exists, otherwise create it with default values
        string settingsFilePath = Path.Combine(Application.persistentDataPath, "settings.txt");
        if (File.Exists(settingsFilePath))
        {
            string[] lines = File.ReadAllLines(settingsFilePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    switch (key)
                    {
                        case "AdditionalWallsForLegacyMap":
                            additionalWallsForLegacyMap = ParseBoolean(value);
                            break;
                        case "VRMovementSpeedMultiplier":
                            float.TryParse(value, out vrMovementSpeedMultiplier);
                            break;
                        case "XRotationFix":
                            float.TryParse(value, out xRotationFix);
                            break;
                        case "BufferWriteInterval":
                            float.TryParse(value, out bufferWriteInterval);
                            break;
                        case "BlackoutFadeDuration":
                            float.TryParse(value, out blackoutFadeDuration);
                            break;
                        case "VROrienterRequiredHeadAngle":
                            float.TryParse(value, out vrOrienterRequiredHeadAngle);
                            break;
                        case "VROrienterRequiredVirtualizerAngle":
                            float.TryParse(value, out vrOrienterRequiredVirtualizerAngle);
                            break;
                        case "VROrienterRequiredProximity":
                            float.TryParse(value, out vrOrienterRequiredProximity);
                            break;
                        case "VROrienterPillarResponsiveness":
                            float.TryParse(value, out vrOrienterPillarResponsiveness);
                            break;
                        case "NearsightTargetFogDensity":
                            float.TryParse(value, out nearsightTargetFogDensity);
                            break;
                        case "NearsightTransitionTime":
                            float.TryParse(value, out nearsightTransitionTime);
                            break;
                    }
                }
            }
        }
        else
        {
            string[] lines = {
            "AdditionalWallsForLegacyMap = " + additionalWallsForLegacyMap,
            "VRMovementSpeedMultiplier = " + vrMovementSpeedMultiplier,
            "XRotationFix = " + xRotationFix,
            "BufferWriteInterval = " + bufferWriteInterval,
            "BlackoutFadeDuration = " + blackoutFadeDuration,
            "VROrienterRequiredHeadAngle = " + vrOrienterRequiredHeadAngle,
            "VROrienterRequiredVirtualizerAngle = " + vrOrienterRequiredVirtualizerAngle,
            "VROrienterRequiredProximity = " + vrOrienterRequiredProximity,
            "VROrienterPillarResponsiveness = " + vrOrienterPillarResponsiveness,
            "NearsightTargetFogDensity = " + nearsightTargetFogDensity,
            "NearsightTransitionTime = " + nearsightTransitionTime
        };
            File.WriteAllLines(settingsFilePath, lines);
        }

        // Save settings to PersistentDataManager
        PersistentDataManager.Instance.AdditionalWallsForLegacyMap = additionalWallsForLegacyMap;
        PersistentDataManager.Instance.VRMovementSpeedMultiplier = vrMovementSpeedMultiplier;
        PersistentDataManager.Instance.XRotationFix = xRotationFix;
        PersistentDataManager.Instance.BufferWriteInterval = bufferWriteInterval;
        PersistentDataManager.Instance.BlackoutFadeDuration = blackoutFadeDuration;
        PersistentDataManager.Instance.VROrienterRequiredHeadAngle = vrOrienterRequiredHeadAngle;
        PersistentDataManager.Instance.VROrienterRequiredVirtualizerAngle = vrOrienterRequiredVirtualizerAngle;
        PersistentDataManager.Instance.VROrienterRequiredProximity = vrOrienterRequiredProximity;
        PersistentDataManager.Instance.VROrienterPillarResponsiveness = vrOrienterPillarResponsiveness;
        PersistentDataManager.Instance.NearsightTargetFogDensity = nearsightTargetFogDensity;
        PersistentDataManager.Instance.NearsightTransitionTime = nearsightTransitionTime;
    }

    private void OpenSettingsMenu()
    {
        mainMenuCanvas.SetActive(false);
        settingsCanvas.SetActive(true);

        string settingsFilePath = Path.Combine(Application.persistentDataPath, "settings.txt");
        if (File.Exists(settingsFilePath))
        {
            string[] lines = File.ReadAllLines(settingsFilePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    switch (key)
                    {
                        case "AdditionalWallsForLegacyMap":
                            additionalWallsForLegacyMapToggle.isOn = ParseBoolean(value);
                            break;
                        case "VRMovementSpeedMultiplier":
                            vrMovementSpeedMultiplierInput.text = value;
                            break;
                        case "XRotationFix":
                            xRotationFixInput.text = value;
                            break;
                        case "BufferWriteInterval":
                            bufferWriteIntervalInput.text = value;
                            break;
                        case "BlackoutFadeDuration":
                            blackoutFadeDurationInput.text = value;
                            break;
                        case "VROrienterRequiredHeadAngle":
                            vrOrienterRequiredHeadAngleInput.text = value;
                            break;
                        case "VROrienterRequiredVirtualizerAngle":
                            vrOrienterRequiredVirtualizerAngleInput.text = value;
                            break;
                        case "VROrienterRequiredProximity":
                            vrOrienterRequiredProximityInput.text = value;
                            break;
                        case "VROrienterPillarResponsiveness":
                            vrOrienterPillarResponsivenessInput.text = value;
                            break;
                        case "NearsightTargetFogDensity":
                            nearsightTargetFogDensityInput.text = value;
                            break;
                        case "NearsightTransitionTime":
                            nearsightTransitionTimeInput.text = value;
                            break;
                    }
                }
            }
        }
    }

    private void ReturnToMainMenu()
    {
        bool hasInvalidInput = false;

        // Validate and collect input values
        if (!float.TryParse(vrMovementSpeedMultiplierInput.text, out float vrMovementSpeedMultiplier))
        {
            vrMovementSpeedMultiplierInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(xRotationFixInput.text, out float xRotationFix))
        {
            xRotationFixInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(bufferWriteIntervalInput.text, out float bufferWriteInterval))
        {
            bufferWriteIntervalInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(blackoutFadeDurationInput.text, out float blackoutFadeDuration))
        {
            blackoutFadeDurationInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(vrOrienterRequiredHeadAngleInput.text, out float vrOrienterRequiredHeadAngle))
        {
            vrOrienterRequiredHeadAngleInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(vrOrienterRequiredVirtualizerAngleInput.text, out float vrOrienterRequiredVirtualizerAngle))
        {
            vrOrienterRequiredVirtualizerAngleInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(vrOrienterRequiredProximityInput.text, out float vrOrienterRequiredProximity))
        {
            vrOrienterRequiredProximityInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(vrOrienterPillarResponsivenessInput.text, out float vrOrienterPillarResponsiveness))
        {
            vrOrienterPillarResponsivenessInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(nearsightTargetFogDensityInput.text, out float nearsightTargetFogDensity))
        {
            nearsightTargetFogDensityInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        if (!float.TryParse(nearsightTransitionTimeInput.text, out float nearsightTransitionTime))
        {
            nearsightTransitionTimeInput.text = "INVALID INPUT";
            hasInvalidInput = true;
        }

        // If any input is invalid, do not proceed
        if (hasInvalidInput)
        {
            return;
        }

        // Save the validated settings to settings.txt
        string settingsFilePath = Path.Combine(Application.persistentDataPath, "settings.txt");

        string[] lines = {
        "AdditionalWallsForLegacyMap = " + additionalWallsForLegacyMapToggle.isOn,
        "VRMovementSpeedMultiplier = " + vrMovementSpeedMultiplier,
        "XRotationFix = " + xRotationFix,
        "BufferWriteInterval = " + bufferWriteInterval,
        "BlackoutFadeDuration = " + blackoutFadeDuration,
        "VROrienterRequiredHeadAngle = " + vrOrienterRequiredHeadAngle,
        "VROrienterRequiredVirtualizerAngle = " + vrOrienterRequiredVirtualizerAngle,
        "VROrienterRequiredProximity = " + vrOrienterRequiredProximity,
        "VROrienterPillarResponsiveness = " + vrOrienterPillarResponsiveness,
        "NearsightTargetFogDensity = " + nearsightTargetFogDensity,
        "NearsightTransitionTime = " + nearsightTransitionTime
        };

        File.WriteAllLines(settingsFilePath, lines);

        // Switch back to the main menu
        mainMenuCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
    }


    public void OpenPersistentDataPath()
    {
        string folderPath = Application.persistentDataPath;

        // Ensure folder path is properly formatted and escaped
        folderPath = folderPath.Replace("/", "\\");

        // Windows
        #if UNITY_STANDALONE_WIN
        Process.Start(new ProcessStartInfo("explorer.exe", "\"" + folderPath + "\"") { UseShellExecute = true });
        #endif

        // MAC OS AND LINUX ARE UNTESTED
        // MacOS
        #if UNITY_STANDALONE_OSX
        Process.Start(new ProcessStartInfo("open", "\"" + folderPath + "\"") { UseShellExecute = true });
        #endif

        // Linux
        #if UNITY_STANDALONE_LINUX
        Process.Start(new ProcessStartInfo("xdg-open", "\"" + folderPath + "\"") { UseShellExecute = true });
        #endif
    }

    private bool ParseBoolean(string value)
    {
        return value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}