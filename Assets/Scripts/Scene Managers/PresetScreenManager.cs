// Description: The preset screen manager provides listeners and functions that control the preset canvas's UI
// See preset controller for functionality pertaining to the saving, deletion, and management of presets themselves
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PresetScreenManager : MonoBehaviour
{
    [SerializeField] private PresetController presetController;

    // UI elements for Preset fields
    [SerializeField] private TMP_InputField presetNameInput;
    [SerializeField] private TMP_InputField mouseSensitivityInput;
    [SerializeField] private TMP_InputField movementSpeedInput;
    [SerializeField] private TMP_InputField lapsInput;
    [SerializeField] private TMP_InputField timeInput;
    [SerializeField] private TMP_InputField scaleInput;
    [SerializeField] private TMP_InputField heightInput;
    [SerializeField] private TMP_InputField intervalInput;
    [SerializeField] private TMP_InputField extraNotesInput;

    // UI elements for Preset toggles
    [SerializeField] private Toggle experimentalModeToggle;
    [SerializeField] private Toggle shuffleObjectsToggle;
    [SerializeField] private Toggle lockVerticalLookToggle;
    [SerializeField] private Toggle limitCuesToggle;
    [SerializeField] private Toggle nearsightToggle;
    [SerializeField] private Toggle useShadowsToggle;
    [SerializeField] private Toggle pointsearchToggle;
    [SerializeField] private Toggle skipTrainingToggle;
    [SerializeField] private Toggle skipRetracingToggle;

    // UI elements for Preset dropdowns and buttons
    public TMP_Dropdown presetDropdown;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text errorText;

    // UI elements for maps
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Button editMapButton;
    [SerializeField] private Button saveMapButton;

    private bool deleteConfirmed = false;

    private void Start()
    {
        presetDropdown.onValueChanged.AddListener(delegate { OnPresetDropdownValueChanged(presetDropdown.value); });

        saveButton.onClick.AddListener(OnSaveButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnPresetDropdownValueChanged(int index)
    {
        string presetName = presetController.Presets[index].presetName;
        presetController.LoadPresetIntoUI(presetName);
    }

    public void FillUIWithPresetDetails(Preset preset)
    {
        presetNameInput.text = preset.presetName;
        mouseSensitivityInput.text = preset.mouseSensitivity.ToString();
        movementSpeedInput.text = preset.movementSpeed.ToString();
        lapsInput.text = preset.laps.ToString();
        timeInput.text = preset.time.ToString();
        scaleInput.text = preset.scale.ToString();
        heightInput.text = preset.height.ToString();
        intervalInput.text = preset.interval.ToString();
        extraNotesInput.text = preset.extraNotes;

        experimentalModeToggle.isOn = preset.experimentalMode;
        shuffleObjectsToggle.isOn = preset.shuffleObjects;
        lockVerticalLookToggle.isOn = preset.lockVerticalLook;
        limitCuesToggle.isOn = preset.limitCues;
        nearsightToggle.isOn = preset.nearsight;
        useShadowsToggle.isOn = preset.useShadows;
        pointsearchToggle.isOn = preset.pointsearch;
        skipTrainingToggle.isOn = preset.skipTraining;
        skipRetracingToggle.isOn = preset.skipRetracing;

        int mapIndex = mapDropdown.options.FindIndex(option => option.text == preset.map);
        mapDropdown.value = mapIndex;

        int presetIndex = presetController.Presets.FindIndex(p => p.presetName == preset.presetName);
        presetDropdown.value = presetIndex;
        presetDropdown.RefreshShownValue();
    }

    private void OnSaveButtonClicked()
    {
        if (!ValidateInputFields())
        {
            return;
        }

        Preset newPreset = GetPresetFromUIInputs();
        presetController.SavePresetToList(newPreset);
        presetController.SwitchToTitleCanvas();
    }

    private bool ValidateInputFields()
    {
        if (string.IsNullOrWhiteSpace(presetNameInput.text) ||
            string.IsNullOrWhiteSpace(mouseSensitivityInput.text) ||
            string.IsNullOrWhiteSpace(movementSpeedInput.text) ||
            string.IsNullOrWhiteSpace(lapsInput.text) ||
            string.IsNullOrWhiteSpace(timeInput.text) ||
            string.IsNullOrWhiteSpace(scaleInput.text) ||
            string.IsNullOrWhiteSpace(heightInput.text) ||
            string.IsNullOrWhiteSpace(intervalInput.text))
        {
            errorText.text = "Please fill out all fields.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(presetNameInput.text)) { errorText.text = "Invalid preset name."; return false; }
        if (!float.TryParse(mouseSensitivityInput.text, out float mouseSensitivity) || mouseSensitivity <= 0) { errorText.text = "Invalid mouse sensitivity value. Please enter a positive number."; return false; }
        if (!float.TryParse(movementSpeedInput.text, out float movementSpeed) || movementSpeed <= 0) { errorText.text = "Invalid movement speed value. Please enter a positive number."; return false; }
        if (!int.TryParse(lapsInput.text, out int laps) || laps <= 0) { errorText.text = "Invalid laps value. Please enter a positive integer."; return false; }
        if (!float.TryParse(timeInput.text, out float time) || time <= 0) { errorText.text = "Invalid time value. Please enter a positive number."; return false; }
        if (!float.TryParse(scaleInput.text, out float scale) || scale <= 0) { errorText.text = "Invalid scale value. Please enter a positive number."; return false; }
        if (!float.TryParse(heightInput.text, out float height) || height <= 0) { errorText.text = "Invalid height value. Please enter a positive number."; return false; }
        if (!float.TryParse(intervalInput.text, out float interval) || interval <= 0) { errorText.text = "Invalid interval value. Please enter a positive number."; return false; }

        return true;
    }

    private Preset GetPresetFromUIInputs()
    {
        return new Preset
        {
            presetName = presetNameInput.text.Trim(),
            mouseSensitivity = float.Parse(mouseSensitivityInput.text),
            movementSpeed = float.Parse(movementSpeedInput.text),
            laps = int.Parse(lapsInput.text),
            time = float.Parse(timeInput.text),
            scale = float.Parse(scaleInput.text),
            height = float.Parse(heightInput.text),
            interval = float.Parse(intervalInput.text),
            extraNotes = extraNotesInput.text.Trim(),
            experimentalMode = experimentalModeToggle.isOn,
            shuffleObjects = shuffleObjectsToggle.isOn,
            lockVerticalLook = lockVerticalLookToggle.isOn,
            limitCues = limitCuesToggle.isOn,
            nearsight = nearsightToggle.isOn,
            useShadows = useShadowsToggle.isOn,
            pointsearch = pointsearchToggle.isOn,
            skipTraining = skipTrainingToggle.isOn,
            skipRetracing = skipRetracingToggle.isOn,
            map = mapDropdown.options[mapDropdown.value].text,
        };
    }

    private void OnDeleteButtonClicked()
    {
        string presetName = presetNameInput.text;
        if (presetName == "Default")
        {
            errorText.text = "The default preset cannot be deleted.";
            return;
        }

        if (deleteConfirmed)
        {
            presetController.DeletePresetFromList(presetName);
            deleteConfirmed = false;
            ClearErrorField();
        }
        else
        {
            errorText.text = "Press delete again to confirm.";
            deleteConfirmed = true;
            StartCoroutine(ResetDeleteConfirmation());
        }
    }

    private IEnumerator ResetDeleteConfirmation()
    {
        yield return new WaitForSeconds(1.25f);
        deleteConfirmed = false;
        ClearErrorField();
    }

    private void OnBackButtonClicked()
    {
        presetController.SwitchToTitleCanvas();
    }

    public void ClearErrorField()
    {
        errorText.text = "";
    }
}