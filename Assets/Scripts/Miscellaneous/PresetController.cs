// Description: The preset controller manages the functionality of presets
// It manages saving, deletion, memory management, etc
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

[System.Serializable]
public class LastUsedPreset
{
    public string presetName;
}

[System.Serializable]
public struct Preset
{
    public string presetName;
    public float mouseSensitivity;
    public float movementSpeed;
    public int laps;
    public float time;
    public float scale;
    public float height;
    public float interval;
    public string extraNotes;
    public bool experimentalMode;
    public bool shuffleObjects;
    public bool lockVerticalLook;
    public bool limitCues;
    public bool nearsight;
    public bool useShadows;
    public bool pointsearch;
    public bool skipTraining;
    public bool skipRetracing;
    public string map;
}

public class PresetController : MonoBehaviour
{
    [SerializeField] private TitleScreenManager titleScreenManager;
    [SerializeField] private PresetScreenManager presetScreenManager;
    [SerializeField] private GameObject titleScreenCanvas;
    [SerializeField] private GameObject presetScreenCanvas;

    public List<Preset> Presets { get; private set; }
    public string PresetFilePath { get; private set; }
    public string LastUsedPresetName { get; private set; }

    private void Awake()
    {
        presetScreenCanvas.SetActive(false);
        titleScreenCanvas.SetActive(true);

        PresetFilePath = Path.Combine(Application.persistentDataPath, "presets.json");
        LoadPresetsFromStorage();
    }

    public void SwitchToTitleCanvas()
    {
        presetScreenCanvas.SetActive(false);
        titleScreenCanvas.SetActive(true);
        PopulatePresetDropdown(titleScreenManager.presetDropdown);
    }

    public void SwitchToPresetCanvas()
    {
        titleScreenCanvas.SetActive(false);
        presetScreenCanvas.SetActive(true);
        PopulatePresetDropdown(presetScreenManager.presetDropdown);
        presetScreenManager.ClearErrorField();
    }

    public void SavePresetToList(Preset preset)
    {
        Presets = InsertDefaultPresetIfItDoesNotExist(Presets);

        int index = Presets.FindIndex(p => p.presetName == LastUsedPresetName);

        if (index != -1)
        {
            if (LastUsedPresetName == "Default" && preset.presetName != "Default")
            {
                Presets.Add(preset);
            }
            else
            {
                if (Presets.Any(p => p.presetName == preset.presetName && preset.presetName != LastUsedPresetName))
                {
                    Debug.LogError("A preset with that name already exists.");
                    return;
                }
                Presets[index] = preset;
            }
        }
        else
        {
            if (Presets.Any(p => p.presetName == preset.presetName))
            {
                Debug.LogError("A preset with that name already exists.");
                return;
            }
            Presets.Add(preset);
        }

        SavePresetsToStorage();
        LastUsedPresetName = preset.presetName;
        SaveLastUsedPresetToStorage(preset.presetName);
    }

    public void DeletePresetFromList(string presetName)
    {
        if (presetName == "Default")
        {
            Debug.LogError("The default preset cannot be deleted.");
            return;
        }

        int index = Presets.FindIndex(p => p.presetName == presetName);
        if (index != -1)
        {
            Presets.RemoveAt(index);
            SavePresetsToStorage();
            PopulatePresetDropdown(presetScreenManager.presetDropdown);
            LoadPresetIntoUI("Default");
        }
        else
        {
            Debug.LogError("Preset not found.");
        }
    }

    public void LoadPresetIntoUI(string presetName)
    {
        bool presetExists = Presets.Any(p => p.presetName == presetName);

        if (!presetExists)
        {
            Preset preset = GetDefaultPreset();
            preset.presetName = presetName;
            Presets.Add(preset);
            SavePresetsToStorage();
        }

        LastUsedPresetName = presetName;
        SaveLastUsedPresetToStorage(presetName);
        presetScreenManager.FillUIWithPresetDetails(GetPresetByName(presetName));
    }

    public void UpdateLastUsedPreset(string presetName)
    {
        LastUsedPresetName = presetName;
        SaveLastUsedPresetToStorage(presetName);
    }

    public void GenerateUniquePresetNameAndLoadPreset()
    {
        int counter = 0;
        string baseName = "New Preset";
        string newName = baseName;

        // Check if the newName already exists in the presets list, increment counter until a unique name is found
        while (Presets.Any(p => p.presetName.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            counter++;
            newName = $"{baseName} ({counter})";
        }

        //Debug.Log(newName);
        LoadPresetIntoUI(newName);
        LastUsedPresetName = newName;
        PopulatePresetDropdown(presetScreenManager.presetDropdown);
    }

    public void UpdateLastUsedPresetName(string presetName)
    {
        LastUsedPresetName = presetName;
    }

    public void PopulatePresetDropdown(TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        foreach (var preset in Presets)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(preset.presetName));
        }

        // Load the currently selected preset
        int index = Presets.FindIndex(p => p.presetName == LastUsedPresetName);
        if (index != -1)
        {
            dropdown.value = index;
        }

        dropdown.RefreshShownValue();
    }

    public Preset GetPresetByName(string presetName)
    {
        return Presets.Find(p => p.presetName == presetName);
    }

    public void SaveLastUsedPresetToStorage(string presetName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "LastUsedPreset.json");
        LastUsedPreset lastUsedPreset = new LastUsedPreset { presetName = presetName };
        string json = JsonUtility.ToJson(lastUsedPreset);
        File.WriteAllText(filePath, json);
    }

    public string LoadLastUsedPresetFromStorage()
    {
        string lastUsedPresetFilePath = Path.Combine(Application.persistentDataPath, "LastUsedPreset.json");
        if (File.Exists(lastUsedPresetFilePath))
        {
            string json = File.ReadAllText(lastUsedPresetFilePath);
            LastUsedPreset lastUsedPreset = JsonUtility.FromJson<LastUsedPreset>(json);
            return lastUsedPreset.presetName;
        }
        return "Default";
    }

    private void SavePresetsToStorage()
    {
        string jsonData = JsonUtility.ToJson(new PresetListWrapper { presets = Presets }, true);
        File.WriteAllText(PresetFilePath, jsonData);
    }

    private void LoadPresetsFromStorage()
    {
        if (File.Exists(PresetFilePath))
        {
            string jsonData = File.ReadAllText(PresetFilePath);
            PresetListWrapper wrapper = JsonUtility.FromJson<PresetListWrapper>(jsonData);
            Presets = wrapper.presets;
        }
        else
        {
            Presets = new List<Preset>();
        }

        Presets = InsertDefaultPresetIfItDoesNotExist(Presets);
    }

    private List<Preset> InsertDefaultPresetIfItDoesNotExist(List<Preset> presets)
    {
        if (!presets.Any(p => p.presetName == "Default"))
        {
            presets.Insert(0, GetDefaultPreset());
        }
        return presets;
    }

    private Preset GetDefaultPreset()
    {
        return new Preset
        {
            presetName = "Default",
            mouseSensitivity = 1f,
            movementSpeed = 8f,
            laps = 1,
            time = 20f,
            scale = 1f,
            height = 1f,
            interval = 0.1f,
            extraNotes = "",
            experimentalMode = false,
            shuffleObjects = false,
            lockVerticalLook = false,
            limitCues = false,
            nearsight = false,
            useShadows = false,
            pointsearch = false,
            skipTraining = false,
            skipRetracing = false,
            map = "Default Map",
        };
    }

    [System.Serializable]
    private class PresetListWrapper
    {
        public List<Preset> presets;
    }
}