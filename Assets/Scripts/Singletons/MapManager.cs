using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public GameObject Self;
    public GameObject Objects;
    public GameObject Map;
    public GameObject LearningStageObjects;
    public GameObject RetracingStageObjects;

    [SerializeField] private World world;
    [SerializeField] private WorldSave worldSave;
    [SerializeField] private GameObject closePlane;
    [SerializeField] private GameObject farPlane;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject virtualizerPlayer;
    [SerializeField] private GameObject roomscalePlayer;

    private string stage;

    public void InitializeMap(string stage)
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        this.stage = stage;

        if (Map == null || world == null)
        {
            Debug.LogError("Map or World reference is missing.");
            return;
        }

        world.player = transform;
        Map.SetActive(true);

        if (!worldSave.metaData.useFarPlane)
        {
            farPlane.SetActive(false);
        }
        PersistentDataManager.Instance.SpawnPosition = worldSave.metaData.spawnPoint;
        PersistentDataManager.Instance.SpawnRotation = new Vector3(0f, worldSave.metaData.spawnRotation.ToQuaternion().eulerAngles.y, 0f);
    }

    public void PostInitialization()
    {
        world.transform.Translate(-VoxelData.WorldOffset, 0f, -VoxelData.WorldOffset);
        Objects.transform.Translate(-VoxelData.WorldOffset, 0f, -VoxelData.WorldOffset);
        LearningStageObjects.transform.Translate(-VoxelData.WorldOffset, 0f, -VoxelData.WorldOffset);
        RetracingStageObjects.transform.Translate(-VoxelData.WorldOffset, 0f, -VoxelData.WorldOffset);

        closePlane.AddComponent<MapScaler>();
        farPlane.AddComponent<MapScaler>();
        farPlane.AddComponent<FloorScaler>();
        closePlane.AddComponent<FloorScaler>();
        Map.AddComponent<MapScaler>();

        foreach (Transform child in LearningStageObjects.transform.Find("Arrow Objects").transform)
        {
            child.gameObject.AddComponent<HitboxOnlyScaler>();
            child.gameObject.AddComponent<PositionScaler>();
        }

        foreach (Transform child in RetracingStageObjects.transform.Find("Retracing Arrow Objects").transform)
        {
            child.gameObject.AddComponent<HitboxOnlyScaler>();
            child.gameObject.AddComponent<PositionScaler>();
        }

        // Probably better to use standard scaling for everything. 
        /*foreach (Transform child in Objects.transform)
        {
            child.gameObject.AddComponent<PositionScaler>();
        }*/

        Objects.AddComponent<MapScaler>();
        LearningStageObjects.transform.Find("Barrier Objects").gameObject.AddComponent<MapScaler>();
        RetracingStageObjects.transform.Find("Deadzone Colliders").gameObject.AddComponent<MapScaler>();
        RetracingStageObjects.transform.Find("Deadzone Barriers").gameObject.AddComponent<MapScaler>();

        LearningStageObjects.SetActive(false);
        RetracingStageObjects.SetActive(false);

        StartCoroutine(PlayerSpawner.SpawnPlayerCoroutine(player, virtualizerPlayer, roomscalePlayer, stage));
    }
}