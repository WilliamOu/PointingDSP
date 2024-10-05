// Description: This script manages the title screen UI elements and to load the selected scenes
// Also creates the file system that stores the CSV files
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

public class LegacyTitleScreenManager : MonoBehaviour
{
    // UI elements
    [SerializeField] private TMP_InputField participantField;
    [SerializeField] private TMP_InputField genderField;
    [SerializeField] private TMP_InputField dateField;
    [SerializeField] private TMP_InputField numberField;
    [SerializeField] private TMP_InputField timeField;
    [SerializeField] private TMP_InputField scaleField;

    [SerializeField] private TMP_InputField mouseSensitivity;
    [SerializeField] private TMP_InputField movementSpeed;

    [SerializeField] private Toggle allStagesToggle;
    [SerializeField] private Toggle trainingToggle;
    [SerializeField] private Toggle learningToggle;
    [SerializeField] private Toggle retracingToggle;
    [SerializeField] private Toggle pointingToggle;
    [SerializeField] private Toggle wayfindingToggle;

    [SerializeField] private Toggle experimentalMode;
    [SerializeField] private Toggle useShadows;
    [SerializeField] private Toggle limitCues;
    [SerializeField] private Toggle additionalWalls;
    [SerializeField] private Toggle heightScaling;

    [SerializeField] private GameObject heightScaleValue;
    [SerializeField] private GameObject trialsCompleted;

    [SerializeField] private Button desktopStartButton;
    [SerializeField] private Button virtualizerStartButton;
    [SerializeField] private Button roomscaleStartButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private TMP_Text errorMessageText;

    void Start()
    {
        // Initially disables the trials completed input field
        // Must be declared as a GameObject in order to enable/disable it
        trialsCompleted.SetActive(false);
        heightScaleValue.SetActive(false);

        // Add a listener to the start button
        desktopStartButton.onClick.AddListener(OnStartPressed);
        virtualizerStartButton.onClick.AddListener(OnStartPressedVirtualizer);
        roomscaleStartButton.onClick.AddListener(OnStartPressedRoomscale);
        returnButton.onClick.AddListener(ReturnToTitleScreen);

        // Toggles the "Trials Completed" text
        pointingToggle.onValueChanged.AddListener(delegate { UpdateInputPanels(); });
        wayfindingToggle.onValueChanged.AddListener(delegate { UpdateInputPanels(); });
        heightScaling.onValueChanged.AddListener(delegate { UpdateInputPanels(); });
    }

    void OnStartPressedRoomscale()
    {
        PersistentDataManager.Instance.IsRoomscale = true;
        OnStartPressedVirtualizer();
    }

    void OnStartPressedVirtualizer()
    {
        PersistentDataManager.Instance.IsVR = true;
        errorMessageText.color = Color.green;
        errorMessageText.text = "Initializing VR...";
        OnStartPressed();
    }

    void OnStartPressed()
    {
        // Requires all fields to be filled out
        if (string.IsNullOrWhiteSpace(participantField.text) ||
        string.IsNullOrWhiteSpace(genderField.text) ||
        string.IsNullOrWhiteSpace(dateField.text) ||
        string.IsNullOrWhiteSpace(numberField.text) ||
        string.IsNullOrWhiteSpace(timeField.text) ||
        string.IsNullOrWhiteSpace(scaleField.text))
        {
            errorMessageText.text = "Please fill out all fields";
            return;
        }

        // Ensures that only one stage is selected
        int checkedToggles = 0;
        checkedToggles += allStagesToggle.isOn ? 1 : 0;
        checkedToggles += trainingToggle.isOn ? 1 : 0;
        checkedToggles += learningToggle.isOn ? 1 : 0;
        checkedToggles += retracingToggle.isOn ? 1 : 0;
        checkedToggles += pointingToggle.isOn ? 1 : 0;
        checkedToggles += wayfindingToggle.isOn ? 1 : 0;
        if (checkedToggles < 1)
        {
            errorMessageText.text = "Please tick at least one stage checkbox";
            return;
        }
        else if (checkedToggles > 1)
        {
            errorMessageText.text = "Please only tick one stage checkbox";
            return;
        }

        // Parses the inputted data
        // TryParse prints errors if the data type provided is invalid
        string participantNumber = participantField.text;
        PersistentDataManager.Instance.ParticipantNumber = participantField.text;
        if (!int.TryParse(genderField.text, out int gender)) { errorMessageText.text = "Invalid gender value."; return; }
        PersistentDataManager.Instance.Gender = gender;
        string date = dateField.text;
        PersistentDataManager.Instance.Date = date;
        if (!int.TryParse(numberField.text, out int lapCount)) { errorMessageText.text = "Invalid lap count."; return; }
        PersistentDataManager.Instance.TotalLaps = lapCount;
        if (!float.TryParse(timeField.text, out float time)) { errorMessageText.text = "Invalid time value."; return; }
        PersistentDataManager.Instance.Time = time;
        if (!float.TryParse(scaleField.text, out float scale)) { errorMessageText.text = "Invalid scale value."; return; }
        PersistentDataManager.Instance.Scale = scale;
        float mouseSens;
        if (!float.TryParse(mouseSensitivity.text, out mouseSens)) { mouseSens = 1f; }
        PersistentDataManager.Instance.MouseSensitivity = mouseSens;
        float moveSpeed;
        if (!float.TryParse(movementSpeed.text, out moveSpeed)) { moveSpeed = 8f; }
        PersistentDataManager.Instance.MovementSpeed = moveSpeed;

        TMP_InputField trialsCompletedInputField = trialsCompleted.GetComponent<TMP_InputField>();
        if (!int.TryParse(trialsCompletedInputField.text, out int startingTrial)) { startingTrial = 0; }
        PersistentDataManager.Instance.StartingTrial = startingTrial;

        TMP_InputField heightScaleInputField = heightScaleValue.GetComponent<TMP_InputField>();
        if (!float.TryParse(heightScaleInputField.text, out float heightScale)) { heightScale = 1f; }
        PersistentDataManager.Instance.HeightScale = heightScale;

        if (experimentalMode.isOn) { PersistentDataManager.Instance.Experimental = true; }
        if (useShadows.isOn) { PersistentDataManager.Instance.UseShadows = true; }
        if (limitCues.isOn) { PersistentDataManager.Instance.LimitCues = true; }
        if (additionalWalls.isOn) { PersistentDataManager.Instance.AdditionalWallsForLegacyMap = true; }

        SetupParticipantDirectory(participantField.text, int.Parse(genderField.text), dateField.text);

        if (!PersistentDataManager.Instance.IsVR)
        {
            StartCoroutine(ControlXR.StopVR());
        }
        LoadSelectedScenes();
    }

    void UpdateInputPanels()
    {
        if (pointingToggle.isOn || wayfindingToggle.isOn)
        {
            trialsCompleted.SetActive(true);
        }
        else
        {
            trialsCompleted.SetActive(false);
        }

        if (heightScaling.isOn)
        {
            heightScaleValue.gameObject.SetActive(true);
        }
        else
        {
            heightScaleValue.gameObject.SetActive(false);
        }
    }

    void SetupParticipantDirectory(string participantNumber, int participantGender, string date)
    {
        // Create the "CSVs" directory if it doesn't exist
        string csvsDirectoryPath = Path.Combine(Application.persistentDataPath, "CSVs");
        if (!Directory.Exists(csvsDirectoryPath))
        {
            Directory.CreateDirectory(csvsDirectoryPath);
        }

        // Create a subdirectory for the participant
        string participantDirectoryPath = Path.Combine(csvsDirectoryPath, participantNumber);
        if (!Directory.Exists(participantDirectoryPath))
        {
            Directory.CreateDirectory(participantDirectoryPath);
        }

        // meta.txt
        string metaFilePath = Path.Combine(participantDirectoryPath, "meta.txt");
        if (!File.Exists(metaFilePath))
        {
            using (StreamWriter writer = new StreamWriter(metaFilePath, false)) // false to overwrite if file exists
            {
                // Write meta data
                WriteMetaData(writer, participantNumber, participantGender, date);
            }
        }

        // timeout.txt 
        string timeoutFilePath = Path.Combine(participantDirectoryPath, "timeout.txt");
        if (!File.Exists(timeoutFilePath))
        {
            File.Create(timeoutFilePath).Dispose();
        }

        // Prints the file path
        Debug.Log("Participant directory: " + participantDirectoryPath);
    }

    // TODO: Update function
    // Participant number and gender being passed in are relics from when these values were not stored in the PersistentDataManager
    void WriteMetaData(StreamWriter writer, string participantNumber, int participantGender, string date)
    {
        writer.WriteLine("Participant Number: " + participantNumber);
        writer.WriteLine("Participant Gender: " + participantGender);
        writer.WriteLine("Date: " + date);
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
        writer.WriteLine("Additional Walls: " + PersistentDataManager.Instance.AdditionalWallsForLegacyMap);
        writer.WriteLine("VR Mode: " + (PersistentDataManager.Instance.IsVR ? "On" : "Off"));
    }

    // using UnityEngine.SceneManagement;
    void LoadSelectedScenes()
    {
        // All stages simply loads the training stage
        // The remaining scene transition logic is handled scene-by-scene
        if (trainingToggle.isOn || allStagesToggle.isOn) {
            SceneManager.LoadScene("Training Stage");
        } else if (learningToggle.isOn) {
            SceneManager.LoadScene("Learning Stage");
        } else if (retracingToggle.isOn) {
            SceneManager.LoadScene("Retracing Stage");
        } else if (pointingToggle.isOn) {
            SceneManager.LoadScene("Pointing Stage");
        } else if (wayfindingToggle.isOn) {
            SceneManager.LoadScene("Wayfinding Stage");
        }
    }

    private void ReturnToTitleScreen()
    {
        if (PersistentDataManager.Instance != null)
        {
            Destroy(PersistentDataManager.Instance.gameObject);
        }

        SceneManager.LoadScene("Title Screen");
    }
}