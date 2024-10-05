// Description: The persistent data manager stores data that needs to be stored between scenes
// It utilizes the singleton pattern to ensure that there is only one instance of it
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PersistentDataManager : MonoBehaviour
{
    // Singleton pattern
    private static PersistentDataManager instance;
    public static PersistentDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("PersistentDataManager");
                instance = obj.AddComponent<PersistentDataManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    // Starting Scene
    public string StartingScene { get; set; } = "Training Stage";

    // Internal Settings
    public bool AdditionalWallsForLegacyMap { get; set; } = true;
    public float VRMovementSpeedMultiplier { get; set; } = 0.5f;
    public float XRotationFix { get; set; } = 5f;
    public float BufferWriteInterval { get; set; } = 1f;
    public float BlackoutFadeDuration { get; set; } = 0.75f;
    public float VROrienterRequiredHeadAngle { get; set; } = 20f;
    public float VROrienterRequiredVirtualizerAngle { get; set; } = 50f;
    public float VROrienterRequiredProximity { get; set; } = 1.5f;
    public float VROrienterPillarResponsiveness { get; set; } = 0.5f;
    public float NearsightTargetFogDensity { get; set; } = 0.1f;
    public float NearsightTransitionTime { get; set; } = 0.35f;

    // Data that needs to be stored and accessed in multiple scenes
    // CSV Data and retrace count, which stores the number of times the player has been sent back to the learning stage
    public List<CSVData> DataList { get; set; } = new List<CSVData>();
    public string CsvFilePath { get; set; }
    public int RetraceCount { get; set; } = 0;
    public bool PlayerIsOriented { get; set; } = false;
    public bool GameStarted { get; set; } = false;
    public bool GameEnded { get; set; } = false;
    public Vector3 SpawnPosition { get; set; } = new Vector3(25f, 0f, 25f);
    public Vector3 SpawnRotation { get; set; } = new Vector3(0f, 180f, 0f);
    public float VerticalPlayerOffset { get; set; } = 0.00f;

    // Title Screen data
    public string ParticipantNumber { get; set; } = "UnityEditorTests";
    public int Gender { get; set; }
    public string Date { get; set; }
    public bool IsVR { get; set; } = false;
    public bool IsRoomscale { get; set; } = false;

    // Preset Screen data
    public float MouseSensitivity { get; set; } = 1f;
    public float MovementSpeed { get; set; } = 8f;
    public int TotalLaps { get; set; } = 1;
    public float Time { get; set; } = 15f;
    public float Scale { get; set; } = 1f;
    public float HeightScale { get; set; } = 1f;
    public float Interval { get; set; } = 0.1f;
    public bool Experimental { get; set; } = false;
    public bool ShuffleObjects { get; set; } = false;
    public bool LockVerticalLook { get; set; } = false;
    public bool LimitCues { get; set; } = false;
    public bool Nearsight { get; set; } = true;
    public bool UseShadows { get; set; } = false;
    public bool Pointsearch { get; set; } = false;
    public bool SkipTraining { get; set; } = false;
    public bool SkipRetracing { get; set; } = false;
    public string Map { get; set; } = "Default Map";

    // Allows you to start on a certain trial in case the game crashes, but resets it for the next stage to avoid skipping trials
    private int startingTrial = 0;
    public int StartingTrial
    {
        get
        {
            int onlySkipTrialsForFirstScenes = startingTrial;
            startingTrial = 0;
            return onlySkipTrialsForFirstScenes;
        }
        set { startingTrial = value; }
    }

    void Awake()
    {
        // Makes sure that there is only one instance of this class, and that it persists between scenes
        if (instance == null)
        {
            // If not, sets this object as the singleton instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // If another instance exists, destroys this new instance
            Destroy(gameObject);
        }

        // Load CSV data if not already loaded
        if (DataList.Count == 0)
        {
            ReadCSV(Path.Combine(Application.streamingAssetsPath, "trials.csv"), DataList);
        }
    }

    public void CreateAndWriteToFile(string fileName, string data)
    {
        // Construct the directory path for the participant
        string participantDirectoryPath = Path.Combine(Application.persistentDataPath, "CSVs", ParticipantNumber);

        // Ensure the directory exists
        if (!Directory.Exists(participantDirectoryPath))
        {
            Directory.CreateDirectory(participantDirectoryPath);
        }

        // Construct the full file path within the participant's directory
        string filePath = Path.Combine(participantDirectoryPath, fileName);

        // Check if the file already exists
        bool fileExists = File.Exists(filePath);

        // Append a new line at the beginning of the data if the file already exists
        if (fileExists)
        {
            data = "\n" + data;
        }

        // Write or append the data to the file
        using (StreamWriter writer = new StreamWriter(filePath, fileExists))
        {
            writer.Write(data);
        }
    }

    public IEnumerator AppendToFile(string fileName, string data)
    {
        // Construct the directory path for the participant
        string participantDirectoryPath = Path.Combine(Application.persistentDataPath, "CSVs", ParticipantNumber);

        // Ensure the directory exists
        // Assuming the file creation logic is robust enough, this is commented out to improve data logging efficiency given how heavy it is
        /*if (!Directory.Exists(participantDirectoryPath))
        {
            Directory.CreateDirectory(participantDirectoryPath);
        }*/

    // Construct the full file path within the participant's directory
    string filePath = Path.Combine(participantDirectoryPath, fileName);

        // Append the data to the file
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.Write(data);
        }

        yield return null;
    }

    public void PushLeftoverDataInBufferToFile(string fileName, string data)
    {
        // Construct the directory path for the participant
        string participantDirectoryPath = Path.Combine(Application.persistentDataPath, "CSVs", ParticipantNumber);

        // Construct the full file path within the participant's directory
        string filePath = Path.Combine(participantDirectoryPath, fileName);

        // Append the data to the file
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.Write(data);
        }
    }

    // Seeded Shuffle
    // Allows the same shuffle to be reproduced given the same user ID
    public void ShuffleDataList(string stageIdentifier)
    {
        // Gets the participant ID
        string participantID = PersistentDataManager.Instance.ParticipantNumber;
        // Since we need two distinct shuffles, one for the Pointing and Wayfinding stages
        // We get a unique modifier in order to get two unique shuffles within the seed
        string seedString = participantID + stageIdentifier;
        int seed = seedString.GetHashCode();
        System.Random random = new System.Random(seed);

        for (int i = 0; i < DataList.Count; i++)
        {
            CSVData temp = DataList[i];
            int randomIndex = random.Next(i, DataList.Count);
            DataList[i] = DataList[randomIndex];
            DataList[randomIndex] = temp;
        }
    }

    // Reads the CSV file and stores the data in the provided dataList
    public void ReadCSV(string fullPath, List<CSVData> dataList)
    {
        // Clear the data list to avoid appending to an already non-empty list when refreshing the list
        dataList.Clear();

        if (!File.Exists(fullPath))
        {
            File.Create(fullPath).Dispose();
            return;
        }

        using (var reader = new StreamReader(fullPath))
        {
            bool isFirstLine = true;
            while (!reader.EndOfStream)
            {
                // Read each line in the CSV file
                var line = reader.ReadLine();
                if (isFirstLine)
                {
                    // Skip the header line
                    isFirstLine = false;
                    continue;
                }

                var values = line.Split(',');
                try
                {
                    // Parse the CSV line into a CSVData object
                    CSVData data = new CSVData
                    {
                        Level = values[0].Trim(),
                        Starting = values[1].Trim(),
                        Target = values[2].Trim(),
                        StartingX = float.Parse(values[3]),
                        StartingY = float.Parse(values[4]),
                        StartingZ = float.Parse(values[5]),
                        TargetX = float.Parse(values[6]),
                        TargetY = float.Parse(values[7]),
                        TargetZ = float.Parse(values[8]),
                        StartingHorizontalAngle = float.Parse(values[9]),
                        StartingVerticalAngle = float.Parse(values[10]),
                        TrueHorizontalAngle = float.Parse(values[11]),
                        TrueVerticalAngle = float.Parse(values[12])
                    };

                    dataList.Add(data);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing line: " + line + "\nError: " + e.Message);
                }
            }
        }
    }
}
