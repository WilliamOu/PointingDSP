using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrialsUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown levelDropdown;
    [SerializeField] private TMP_InputField levelInputField;
    [SerializeField] private TMP_Dropdown startingDropdown;
    [SerializeField] private TMP_Dropdown targetDropdown;
    [SerializeField] private TMP_InputField startingPositionInputField;
    [SerializeField] private WorldSave worldSave;
    [SerializeField] private Button addButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private BirdsEyeCameraUI uiBase;

    private void Start()
    {
        PopulateLevelDropdown();
        PopulateTrialObjectDropdowns();
        OnLevelDropdownChanged(0);
        OnStartingDropdownChanged(0);
        OnTargetDropdownChanged(0);
        SetSpawnPointVisualizer();

        levelDropdown.onValueChanged.AddListener(OnLevelDropdownChanged);
        levelInputField.onValueChanged.AddListener(OnLevelNameChanged);
        startingDropdown.onValueChanged.AddListener(OnStartingDropdownChanged);
        targetDropdown.onValueChanged.AddListener(OnTargetDropdownChanged);
        startingPositionInputField.onValueChanged.AddListener(OnStartingPositionChanged);
        addButton.onClick.AddListener(OnAddButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    public void SetSpawnPointVisualizer()
    {
        if (worldSave.csvData.Count != 0) {
            uiBase.spawnPointVisualizer.SetActive(true);
            int index = levelDropdown.value;
            OnStartingPositionChanged(uiBase.FormatVector3(new Vector3(worldSave.csvData[index].StartingX, worldSave.csvData[index].StartingY, worldSave.csvData[index].StartingZ)));
        }
    }

    private void PopulateLevelDropdown()
    {
        levelDropdown.ClearOptions();

        foreach (var data in worldSave.csvData)
        {
            levelDropdown.options.Add(new TMP_Dropdown.OptionData(data.Level));
        }

        levelDropdown.RefreshShownValue();
    }

    private void PopulateTrialObjectDropdowns()
    {
        startingDropdown.ClearOptions();
        targetDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < worldSave.modelParents.Count; i++)
        {
            options.Add(worldSave.modelParents[i].name);
        }
        startingDropdown.AddOptions(options);
        targetDropdown.AddOptions(options);
    }

    private void OnLevelDropdownChanged(int index)
    {
        if (worldSave.csvData.Count != 0)
        {
            var selectedData = worldSave.csvData[index];

            levelInputField.text = selectedData.Level;
            startingDropdown.value = startingDropdown.options.FindIndex(option => option.text == selectedData.Starting);
            targetDropdown.value = targetDropdown.options.FindIndex(option => option.text == selectedData.Target);
            startingPositionInputField.text = uiBase.FormatVector3(new Vector3(selectedData.StartingX, selectedData.StartingY, selectedData.StartingZ));
        }
    }

    private void OnLevelNameChanged(string newName)
    {
        if (worldSave.csvData.Count != 0)
        {
            int index = levelDropdown.value;
            worldSave.csvData[index].Level = newName;
            levelDropdown.options[index].text = newName;
            levelDropdown.RefreshShownValue();
        }
    }

    private void OnStartingDropdownChanged(int index)
    {
        if (worldSave.csvData.Count != 0)
        {
            string selectedObject = startingDropdown.options[index].text;
            worldSave.csvData[levelDropdown.value].Starting = selectedObject;
        }
    }

    private void OnTargetDropdownChanged(int index)
    {
        if (worldSave.csvData.Count != 0)
        {
            string selectedObject = targetDropdown.options[index].text;
            worldSave.csvData[levelDropdown.value].Target = selectedObject;
        }
    }

    private void OnStartingPositionChanged(string newValue)
    {
        if (uiBase.TryParseVector3(newValue, out Vector3 result) && worldSave.csvData.Count != 0)
        {
            int index = levelDropdown.value;
            worldSave.csvData[index].StartingX = result.x;
            worldSave.csvData[index].StartingY = result.y;
            worldSave.csvData[index].StartingZ = result.z;
            uiBase.spawnPointVisualizer.transform.position = result + new Vector3(VoxelData.WorldOffset, 0f, VoxelData.WorldOffset);
        }
    }

    private void OnAddButtonClicked()
    {
        int index = worldSave.csvData.Count;

        worldSave.csvData.Add(new CSVData());
        worldSave.csvData[index].Level = "New Trial";

        PopulateLevelDropdown();
        levelDropdown.value = index;
        OnLevelDropdownChanged(index);
    }

    private void OnDeleteButtonClicked()
    {
        int index = levelDropdown.value;

        if (index >= 0 && index < worldSave.csvData.Count)
        {
            // Remove the selected CSVData element
            worldSave.csvData.RemoveAt(index);

            // Repopulate the level dropdown
            PopulateLevelDropdown();

            // Select the first element if any, otherwise clear the input fields
            if (worldSave.csvData.Count > 0)
            {
                levelDropdown.value = 0;
            }
            else
            {
                levelInputField.text = string.Empty;
                startingDropdown.value = 0;
                targetDropdown.value = 0;
                startingPositionInputField.text = string.Empty;
                uiBase.spawnPointVisualizer.SetActive(false);
            }
        }
    }
}
