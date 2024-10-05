using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;

public class MapController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private TMP_Text title;
    [SerializeField] private Button editMapButton;
    [SerializeField] private Button saveAndLoadButton;
    [SerializeField] private Button createMapButton;
    [SerializeField] private Button refreshMapButton;
    [SerializeField] private TMP_Text messageField;

    [SerializeField] private WorldSave worldSave;
    private bool inMapCreation = true;

    private void Awake()
    {
        if (worldSave == null) { inMapCreation = false; }
    }

    private void Start()
    {
        mapDropdown.ClearOptions();
        PopulateDropdown();
        mapDropdown.onValueChanged.AddListener(delegate { OnMapSelected(mapDropdown); });
        if (inMapCreation) { saveAndLoadButton.onClick.AddListener(SaveAndLoadMap); }
        else { editMapButton.onClick.AddListener(LoadMapForEditing); }
        createMapButton.onClick.AddListener(CreateMap);
        refreshMapButton.onClick.AddListener(PopulateDropdown);
    }

    private void PopulateDropdown()
    {
        string path = Path.Combine(Application.persistentDataPath, "Maps");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        List<string> mapNames = new List<string>
        {
            "Default Map"
        };

        string[] directories = Directory.GetDirectories(path);

        foreach (string directory in directories)
        {
            mapNames.Add(Path.GetFileName(directory));
        }

        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(mapNames);
        if (inMapCreation) {
            int index = mapDropdown.options.FindIndex(option => option.text == PersistentDataManager.Instance.Map);
            mapDropdown.value = index;
            title.text = mapDropdown.options[index].text;
        }
    }

    private void OnMapSelected(TMP_Dropdown dropdown)
    {
        string selectedMap = dropdown.options[dropdown.value].text;
        PersistentDataManager.Instance.Map = selectedMap;
        // Debug.Log("Selected map: " + selectedMap);
    }

    private void SaveAndLoadMap()
    {
        worldSave.SaveWorldData();
        worldSave.SaveBlockData();
        worldSave.SaveMetaData();
        worldSave.SaveCSVData();
        worldSave.SaveLearningStageData();
        worldSave.SaveRetracingStageData();
        worldSave.SaveModelData();
        LoadMapForEditing();
    }

    private void LoadMapForEditing()
    {
        if (mapDropdown.options[mapDropdown.value].text == "Default Map")
        {
            return;
        }

        messageField.text = "<color=green>Loading Map</color>\n<color=red>Do not close the game</color>";
        UnityEngine.SceneManagement.SceneManager.LoadScene("Map Creation");
    }

    private void CreateMap()
    {
        messageField.text = "<color=green>Creating New Map</color>\n<color=red>Do not close the game</color>";

        string mapPath = Path.Combine(Application.persistentDataPath, "Maps");
        string NewMapName = GenerateNewMapName(mapPath);

        string templatePath = Path.Combine(Application.dataPath, "Resources/Starter Map Template");
        string targetPath = Path.Combine(mapPath, NewMapName);

        CopyDirectory(templatePath, targetPath);

        messageField.text = "";
        PopulateDropdown();

        // Update the currently selected map dropdown value
        int newMapIndex = mapDropdown.options.FindIndex(option => option.text == NewMapName);
        if (newMapIndex != -1)
        {
            mapDropdown.value = newMapIndex;
            OnMapSelected(mapDropdown);
        }

        // Open the newly created directory
        Process.Start(new ProcessStartInfo()
        {
            FileName = targetPath,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    // Recursive template copy function
    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        // Copy each file into the new directory
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            if (Path.GetExtension(file) == ".meta")
            {
                continue;
            }

            string dest = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, dest);
        }

        // Copy each subdirectory
        foreach (var folder in Directory.GetDirectories(sourceDir))
        {
            string dest = Path.Combine(destinationDir, Path.GetFileName(folder));
            CopyDirectory(folder, dest);
        }
    }

    private string GenerateNewMapName(string mapPath)
    {
        string NewMapName = "New Map";
        Queue<int> indexQueue = new Queue<int>();

        foreach (var folder in Directory.GetDirectories(mapPath))
        {
            string folderName = Path.GetFileName(folder);
            if (folderName == NewMapName)
            {
                indexQueue.Enqueue(0);
            }
            else if (folderName.StartsWith(NewMapName + " (") && folderName.EndsWith(")"))
            {
                string indexPart = folderName.Substring(NewMapName.Length + 2, folderName.Length - NewMapName.Length - 3);
                if (int.TryParse(indexPart, out int index))
                {
                    indexQueue.Enqueue(index);
                }
            }
        }

        int availableIndex = 0;
        while (indexQueue.Count > 0)
        {
            int currentIndex = indexQueue.Dequeue();
            if (currentIndex == availableIndex)
            {
                availableIndex++;
            }
            else
            {
                break;
            }
        }

        string newMapNameWithIndex = availableIndex == 0 ? NewMapName : $"{NewMapName} ({availableIndex})";
        return newMapNameWithIndex;
    }

}
