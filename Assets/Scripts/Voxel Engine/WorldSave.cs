using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using GLTFast;

[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float r, float s, float t)
    {
        x = r;
        y = s;
        z = t;
    }

    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public override string ToString()
    {
        return $"SerializableVector3(x={x}, y={y}, z={z})";
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        SerializableVector3 other = (SerializableVector3)obj;
        return x == other.x && y == other.y && z == other.z;
    }

    public static implicit operator Vector3(SerializableVector3 sVector)
    {
        return new Vector3(sVector.x, sVector.y, sVector.z);
    }

    public static implicit operator SerializableVector3(Vector3 vector)
    {
        return new SerializableVector3(vector);
    }
}

[System.Serializable]
public class SerializableQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public SerializableQuaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public SerializableQuaternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }

    public override string ToString()
    {
        return $"SerializableQuaternion(x={x}, y={y}, z={z}, w={w})";
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        SerializableQuaternion other = (SerializableQuaternion)obj;
        return x == other.x && y == other.y && z == other.z && w == other.w;
    }

    // Operator overloads
    public static implicit operator Quaternion(SerializableQuaternion sQuaternion)
    {
        return new Quaternion(sQuaternion.x, sQuaternion.y, sQuaternion.z, sQuaternion.w);
    }

    public static implicit operator SerializableQuaternion(Quaternion quaternion)
    {
        return new SerializableQuaternion(quaternion);
    }
}

[System.Serializable]
public class SerializableBlockType
{
    public string blockName;
    public bool isSolid;
    public bool isTransparent;
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public SerializableBlockType(BlockType blockType)
    {
        blockName = blockType.blockName;
        isSolid = blockType.isSolid;
        isTransparent = blockType.isTransparent;
        backFaceTexture = blockType.backFaceTexture;
        frontFaceTexture = blockType.frontFaceTexture;
        topFaceTexture = blockType.topFaceTexture;
        bottomFaceTexture = blockType.bottomFaceTexture;
        leftFaceTexture = blockType.leftFaceTexture;
        rightFaceTexture = blockType.rightFaceTexture;
    }

    public BlockType ToBlockType()
    {
        return new BlockType
        {
            blockName = blockName,
            isSolid = isSolid,
            isTransparent = isTransparent,
            backFaceTexture = backFaceTexture,
            frontFaceTexture = frontFaceTexture,
            topFaceTexture = topFaceTexture,
            bottomFaceTexture = bottomFaceTexture,
            leftFaceTexture = leftFaceTexture,
            rightFaceTexture = rightFaceTexture
        };
    }
}

[System.Serializable]
public class BlockData
{
    public float x;
    public float y;
    public float z;
    public byte blockType;

    public BlockData(float x, float y, float z, byte type)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        blockType = type;
    }

    public override string ToString()
    {
        return $"BlockData(x={x}, y={y}, z={z}, blockType={blockType})";
    }
}

[System.Serializable]
public class ModelData
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;
    public SerializableVector3 relativePosition;
    public SerializableQuaternion relativeRotation;
    public SerializableVector3 relativeScale;
    public BoxColliderData colliderData;
}

[System.Serializable]
public class BoxColliderData
{
    public SerializableVector3 center;
    public SerializableVector3 size;
}

[System.Serializable]
public class MetaData
{
    public SerializableVector3 spawnPoint;
    public SerializableQuaternion spawnRotation;
    public bool useFarPlane;

    public MetaData(SerializableVector3 spawnPoint, bool useFarPlane, SerializableQuaternion spawnRotation)
    {
        this.spawnPoint = spawnPoint;
        this.useFarPlane = useFarPlane;
        this.spawnRotation = spawnRotation;
    }

    public override string ToString()
    {
        return $"MetaData(spawnPoint={spawnPoint}, useFarPlane={useFarPlane})";
    }
}

[System.Serializable]
public class GameObjectData
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;
}

[System.Serializable]
public class BarrierData
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;
    public bool isOneWayBarrier;
}

[System.Serializable]
public class ArrowData
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;
    public BoxColliderData colliderData;
}

public class WorldSave : MonoBehaviour
{
    [HideInInspector] public bool inMapCreation = true;

    [SerializeField] private World world;
    [SerializeField] private ObjectUI objectUI;
    public GameObject arrowPrefab;
    public GameObject barrierPrefab;
    public GameObject deadzoneColliderPrefab;
    public GameObject deadzoneBarrierPrefab;
    public GameObject invisibleArrowPrefabForRetracingPhase;

    public Dictionary<SerializableVector3, BlockData> blockDataMap = new Dictionary<SerializableVector3, BlockData>();
    public List<GameObject> modelParents = new List<GameObject>();
    public List<GameObject> modelNested = new List<GameObject>();
    public List<CSVData> csvData = new List<CSVData>();
    public List<GameObject> invisibleArrows = new List<GameObject>();
    public List<GameObject> arrows = new List<GameObject>();
    public List<ArrowData> arrowData = new List<ArrowData>();
    public List<GameObject> barriers = new List<GameObject>();
    public List<BarrierData> barrierData = new List<BarrierData>();
    public List<GameObject> deadzoneColliders = new List<GameObject>();
    public List<BarrierData> deadzoneColliderData = new List<BarrierData>();
    public List<GameObject> deadzoneBarriers = new List<GameObject>();
    public List<BarrierData> deadzoneBarrierData = new List<BarrierData>();
    public MetaData metaData;

    public GameObject arrowObjects;
    public GameObject barrierObjects;
    public GameObject deadzoneColliderObjects;
    public GameObject deadzoneBarrierObjects;
    public GameObject retracingArrowObjects;

    private string mapPath;
    private GameObject globalMap;
    private GameObject mapObjects;

    private void Awake()
    {
        if (objectUI == null) { inMapCreation = false; }
        mapPath = Path.Combine(Application.persistentDataPath, "Maps", PersistentDataManager.Instance.Map);
        if (MapManager.Instance != null)
        {
            globalMap = MapManager.Instance.Self;
            mapObjects = MapManager.Instance.Objects;
        }
        else
        {
            globalMap = new GameObject("Map");
            mapObjects = new GameObject("Map Objects");
        }

        this.transform.SetParent(globalMap.transform);
        mapObjects.transform.SetParent(globalMap.transform);
    }

    public void AddBlock(float x, float y, float z, byte blockType)
    {
        SerializableVector3 blockCoords = new SerializableVector3(x, y, z);
        blockDataMap[blockCoords] = new BlockData(x, y, z, blockType);
    }

    public void RemoveBlock(float x, float y, float z)
    {
        SerializableVector3 blockCoords = new SerializableVector3(x, y, z);
        if (blockDataMap.ContainsKey(blockCoords))
        {
            blockDataMap.Remove(blockCoords);
        }
    }

    public bool IsBlockAt(float x, float y, float z)
    {
        SerializableVector3 blockCoords = new SerializableVector3(x, y, z);
        return blockDataMap.ContainsKey(blockCoords);
    }

    public BlockData GetBlockData(float x, float y, float z)
    {
        SerializableVector3 blockCoords = new SerializableVector3(x, y, z);
        if (blockDataMap.ContainsKey(blockCoords))
        {
            return blockDataMap[blockCoords];
        }
        return null;
    }

    public void SaveModelData()
    {
        for (int i = 0; i < modelParents.Count; i++)
        {
            GameObject parentObject = modelParents[i];
            GameObject nestedObject = modelNested[i];

            string directoryName = parentObject.name;
            string directoryPath = Path.Combine(mapPath, "Models", directoryName);
            string modelDataPath = Path.Combine(directoryPath, "model.dat");

            ModelData modelData = new ModelData
            {
                position = parentObject.transform.position,
                rotation = parentObject.transform.rotation,
                scale = parentObject.transform.localScale,
                relativePosition = nestedObject.transform.localPosition,
                relativeRotation = nestedObject.transform.localRotation,
                relativeScale = nestedObject.transform.localScale,
                colliderData = new BoxColliderData
                {
                    center = parentObject.GetComponent<BoxCollider>().center,
                    size = parentObject.GetComponent<BoxCollider>().size
                }
            };

            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Create(modelDataPath))
            {
                bf.Serialize(file, modelData);
            }
        }
    }

    public async Task LoadModelData()
    {
        string modelsPath = Path.Combine(mapPath, "Models");
        if (!Directory.Exists(modelsPath))
        {
            Directory.CreateDirectory(modelsPath);
        }

        var files = Directory.GetDirectories(modelsPath);
        foreach (var file in files)
        {
            await LoadModel(file);
        }

        if (inMapCreation) { objectUI.InitializeList(); }
        else { MapManager.Instance.PostInitialization(); }
    }

    private async Task LoadModel(string directoryPath)
    {
        string directoryName = Path.GetFileName(directoryPath);
        GameObject parentObject = new GameObject(directoryName);
        GameObject nestedObject = new GameObject(directoryName + "_nested");

        parentObject.transform.SetParent(mapObjects.transform);
        nestedObject.transform.SetParent(parentObject.transform);
        modelParents.Add(parentObject);
        modelNested.Add(nestedObject);

        if (inMapCreation) { parentObject.tag = "Draggable"; }

        var gltfFiles = Directory.GetFiles(directoryPath, "*.gltf");
        if (gltfFiles.Length == 0)
        {
            Debug.LogError("No .gltf file found in directory: " + directoryPath);
            Destroy(parentObject);
            return;
        }
        else if (gltfFiles.Length != 1)
        {
            Debug.LogError("You have too many .gltf files in this directory. Place one model in directory: " + directoryPath);
            Destroy(parentObject);
            return;
        }

        parentObject.transform.position = new Vector3(160f, 0f, 160f);

        var gltf = new GltfImport();
        if (await gltf.Load(gltfFiles[0]))
        {
            await gltf.InstantiateMainSceneAsync(nestedObject.transform);
            // OptimizeMesh(nestedObject);

            BoxCollider collider = parentObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            Rigidbody rigidbody = parentObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;

            Vector3 relativePosition = nestedObject.transform.localPosition;
            Quaternion relativeRotation = nestedObject.transform.localRotation;
            Vector3 relativeScale = nestedObject.transform.localScale;

            string modelDataPath = Path.Combine(directoryPath, "model.dat");
            ModelData modelData;
            if (!File.Exists(modelDataPath))
            {
                modelData = new ModelData
                {
                    position = parentObject.transform.position,
                    rotation = parentObject.transform.rotation,
                    scale = parentObject.transform.localScale,
                    relativePosition = relativePosition,
                    relativeRotation = relativeRotation,
                    relativeScale = relativeScale,
                    colliderData = new BoxColliderData
                    {
                        center = collider.center,
                        size = collider.size
                    }
                };
            }
            else
            {
                // Deserialize the model.dat file
                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream file = File.Open(modelDataPath, FileMode.Open))
                {
                    modelData = (ModelData)bf.Deserialize(file);
                }

                parentObject.transform.position = modelData.position;
                parentObject.transform.rotation = modelData.rotation;
                parentObject.transform.localScale = modelData.scale;
                nestedObject.transform.localPosition = modelData.relativePosition;
                nestedObject.transform.localRotation = modelData.relativeRotation;
                nestedObject.transform.localScale = modelData.relativeScale;
                collider.center = modelData.colliderData.center;
                collider.size = modelData.colliderData.size;
            }

            RenameChildObjects(parentObject);
        }
        else
        {
            Debug.LogError("Failed to load glTF model: " + directoryPath);
            Destroy(parentObject);
        }
    }

    private void OptimizeMesh(GameObject model)
    {
        MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh != null)
            {
                mesh.Optimize();
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
            }
        }
    }

    private void AddLODGroup(GameObject parent, GameObject nested)
    {
        LODGroup lodGroup = parent.AddComponent<LODGroup>();

        // Create LOD levels
        LOD[] lods = new LOD[3];
        lods[0] = new LOD(0.6f, nested.GetComponentsInChildren<Renderer>());
        lods[1] = new LOD(0.3f, nested.GetComponentsInChildren<Renderer>());
        lods[2] = new LOD(0.1f, new Renderer[0]);

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
    }

    private void RenameChildObjects(GameObject parentObject)
    {
        int counter = 0;
        foreach (Transform child in parentObject.transform)
        {
            if (child.name == parentObject.name)
            {
                child.name = parentObject.name + "_child_" + counter++;
            }
        }
    }

    public void SaveWorldData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, "world.dat");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(filePath))
        {
            bf.Serialize(file, blockDataMap);
        }
    }

    public void LoadWorldData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        string filePath = Path.Combine(directoryPath, "world.dat");

        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        BinaryFormatter bf = new BinaryFormatter();
        try
        {
            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                blockDataMap = (Dictionary<SerializableVector3, BlockData>)bf.Deserialize(file);
            }
        }
        catch (SerializationException ex)
        {
            Debug.LogWarning("Failed to deserialize file (if you are initializing this map for the first time, this error can be safely ignored). Initializing new block data map and model list. Error: " + ex.Message);
            blockDataMap = new Dictionary<SerializableVector3, BlockData>();
        }
        catch (Exception ex)
        {
            Debug.LogError("An unexpected error occurred while loading the file. Initializing new block data map and model list. Error: " + ex.Message);
            blockDataMap = new Dictionary<SerializableVector3, BlockData>();
        }
    }

    public void SaveBlockData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, "blocks.dat");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        List<SerializableBlockType> serializableBlockTypes = new List<SerializableBlockType>();
        foreach (var blockType in world.blocktypes)
        {
            serializableBlockTypes.Add(new SerializableBlockType(blockType));
        }

        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(filePath))
        {
            bf.Serialize(file, serializableBlockTypes);
        }
    }

    public void LoadBlockData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        string filePath = Path.Combine(directoryPath, "blocks.dat");

        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream file = File.Open(filePath, FileMode.Open))
                {
                    List<SerializableBlockType> serializableBlockTypes = (List<SerializableBlockType>)bf.Deserialize(file);
                    world.blocktypes = new List<BlockType>();
                    for (int i = 0; i < serializableBlockTypes.Count; i++)
                    {
                        world.blocktypes.Add(serializableBlockTypes[i].ToBlockType());
                    }
                }
            }
            catch (SerializationException ex)
            {
                Debug.LogWarning("Failed to deserialize file (if you are initializing this map for the first time, this error can be safely ignored). Initializing new block types array. Error: " + ex.Message);
                world.blocktypes = new List<BlockType>();
            }
            catch (Exception ex)
            {
                Debug.LogError("An unexpected error occurred while loading the file. Initializing new block types array. Error: " + ex.Message);
                world.blocktypes = new List<BlockType>();
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
            world.blocktypes = new List<BlockType>();
        }
    }

    public void SaveMetaData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, "meta.dat");
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(filePath))
        {
            bf.Serialize(file, metaData);
        }
    }

    public void LoadMetaData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        string filePath = Path.Combine(directoryPath, "meta.dat");

        if (!File.Exists(filePath))
        {
            metaData = new MetaData(new Vector3(0f, 0f, 0f), true, new SerializableQuaternion(Quaternion.identity));
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        try
        {
            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                metaData = (MetaData)bf.Deserialize(file);
            }
        }
        catch (SerializationException ex)
        {
            Debug.LogWarning("Failed to deserialize MetaData file. Initializing with default values. Error: " + ex.Message);
            metaData = new MetaData(new Vector3(0f, 0f, 0f), true, new SerializableQuaternion(Quaternion.identity));
        }
        catch (Exception ex)
        {
            Debug.LogError("An unexpected error occurred while loading the MetaData file. Initializing with default values. Error: " + ex.Message);
            metaData = new MetaData(new Vector3(0f, 0f, 0f), true, new SerializableQuaternion(Quaternion.identity));
        }
    }

    public void SaveCSVData()
    {
        string filePath = Path.Combine(mapPath, "trials.csv");

        Dictionary<string, int> indices = new Dictionary<string, int>();
        for (int i = 0; i < modelParents.Count; i++)
        {
            indices[modelParents[i].name] = i;
        }

        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Level,Starting,Target,StartingX,StartingY,StartingZ,TargetX,TargetY,TargetZ,StartingHorizontalAngle,StartingVerticalAngle,TrueHorizontalAngle,TrueVerticalAngle");
            Vector3 worldOffset = new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset);
            foreach (var data in csvData)
            {
                Vector3 startingPos = modelParents[indices[data.Starting]].transform.position - worldOffset;
                Vector3 targetPos = modelParents[indices[data.Target]].transform.position - worldOffset;

                data.TargetX = targetPos.x;
                data.TargetY = targetPos.y;
                data.TargetZ = targetPos.z;

                data.StartingHorizontalAngle = CalculateHorizontalAngle(data.StartingX, data.StartingZ, startingPos.x, startingPos.z);
                data.StartingVerticalAngle = CalculateVerticalAngle(data.StartingX, data.StartingY, data.StartingZ, startingPos.x, startingPos.y, startingPos.z);
                data.TrueHorizontalAngle = CalculateHorizontalAngle(data.StartingX, data.StartingZ, data.TargetX, data.TargetZ);
                data.TrueVerticalAngle = CalculateVerticalAngle(data.StartingX, data.StartingY, data.StartingZ, data.TargetX, data.TargetY, data.TargetZ);

                writer.WriteLine($"{data.Level},{data.Starting},{data.Target},{data.StartingX},{data.StartingY},{data.StartingZ},{data.TargetX},{data.TargetY},{data.TargetZ},{data.StartingHorizontalAngle},{data.StartingVerticalAngle},{data.TrueHorizontalAngle},{data.TrueVerticalAngle}");
            }
        }
    }

    private float CalculateHorizontalAngle(float startingX, float startingZ, float targetX, float targetZ)
    {
        Vector2 start = new Vector2(startingX, startingZ);
        Vector2 target = new Vector2(targetX, targetZ);
        Vector2 direction = target - start;
        float horizontalAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

        if (horizontalAngle < 0) { horizontalAngle += 360f; }

        return horizontalAngle;
    }

    private float CalculateVerticalAngle(float startingX, float startingY, float startingZ, float targetX, float targetY, float targetZ)
    {
        Vector3 start = new Vector3(startingX, startingY, startingZ);
        Vector3 target = new Vector3(targetX, targetY, targetZ);
        Vector3 direction = target - start;

        float horizontalDistance = new Vector2(direction.x, direction.z).magnitude;
        float verticalAngle = Mathf.Atan2(direction.y, horizontalDistance) * Mathf.Rad2Deg;

        return verticalAngle;
    }

    public void LoadCSVData()
    {
        PersistentDataManager.Instance.ReadCSV(Path.Combine(mapPath, "trials.csv"), csvData);
    }

    public void SaveLearningStageData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        string arrowFilePath = Path.Combine(directoryPath, "arrows.dat");
        string barrierFilePath = Path.Combine(directoryPath, "barriers.dat");

        if (!File.Exists(arrowFilePath))
        {
            File.Create(arrowFilePath).Dispose();
        }
        if (!File.Exists(barrierFilePath))
        {
            File.Create(barrierFilePath).Dispose();
        }

        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(arrowFilePath))
        {
            bf.Serialize(file, arrowData);
        }
        using (FileStream file = File.Create(barrierFilePath))
        {
            bf.Serialize(file, barrierData);
        }

    }

    public void LoadLearningStageData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        string arrowFilePath = Path.Combine(directoryPath, "arrows.dat");
        string barrierFilePath = Path.Combine(directoryPath, "barriers.dat");

        if (File.Exists(arrowFilePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream stream = new FileStream(arrowFilePath, FileMode.Open))
                {
                    arrowData = bf.Deserialize(stream) as List<ArrowData>;
                }

                for (int i = 0; i < arrowData.Count; i++)
                {
                    GameObject arrow = Instantiate(arrowPrefab, arrowData[i].position + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset), arrowData[i].rotation);
                    arrow.name = "Arrow " + (i + 1);
                    arrow.transform.localScale = arrowData[i].scale;
                    arrow.transform.SetParent(arrowObjects.transform);
                    arrow.GetComponent<BoxCollider>().center = arrowData[i].colliderData.center;
                    arrow.GetComponent<BoxCollider>().size = arrowData[i].colliderData.size;
                    if (inMapCreation) { arrow.tag = "Draggable"; }
                    arrows.Add(arrow);
                }
                if (!inMapCreation)
                {
                    for (int i = 0; i < arrows.Count; i++)
                    {
                        Checkpoint arrowCheckpoint = arrows[i].GetComponent<Checkpoint>();
                        arrowCheckpoint.checkpointNumber = i + 1;
                        arrowCheckpoint.nextCheckpoint = (i == arrowData.Count - 1) ? arrows[0].GetComponent<Checkpoint>() : arrows[i + 1].GetComponent<Checkpoint>();
                    }
                }
            }
            catch (SerializationException ex)
            {
                Debug.LogWarning("Failed to deserialize arrow data file. Error: " + ex.Message);
                arrowData.Clear();
                arrows.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError("An unexpected error occurred while loading the arrow data file. Error: " + ex.Message);
                arrowData.Clear();
                arrows.Clear();
            }
        }

        if (File.Exists(barrierFilePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream stream = new FileStream(barrierFilePath, FileMode.Open))
                {
                    barrierData = bf.Deserialize(stream) as List<BarrierData>;
                }

                for (int i = 0; i < barrierData.Count; i++)
                {
                    GameObject barrier = Instantiate(barrierPrefab, barrierData[i].position + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset), barrierData[i].rotation);
                    barrier.name = "Barrier " + (i + 1);
                    barrier.transform.localScale = barrierData[i].scale;
                    barrier.transform.SetParent(barrierObjects.transform);
                    if (inMapCreation) { barrier.tag = "Draggable"; }
                    barriers.Add(barrier);
                    if (barrierData[i].isOneWayBarrier && inMapCreation)
                    {
                        barrier.transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = true;
                        barrier.transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = true;
                    }
                }
                if (!inMapCreation)
                {
                    for (int i = 0; i < barriers.Count; i++)
                    {
                        barriers[i].GetComponent<MeshRenderer>().enabled = false;
                        if (!barrierData[i].isOneWayBarrier)
                        {
                            barriers[i].transform.Find("Entry Trigger").GetComponent<BoxCollider>().enabled = false;
                            barriers[i].transform.Find("Exit Trigger").GetComponent<BoxCollider>().enabled = false;
                        }
                        else
                        {
                            barriers[i].transform.Find("Entry Trigger").GetComponent<BoxCollider>().enabled = true;
                            barriers[i].transform.Find("Exit Trigger").GetComponent<BoxCollider>().enabled = true;
                        }
                    }
                }
            }
            catch (SerializationException ex)
            {
                Debug.LogWarning("Failed to deserialize barrier data file. Error: " + ex.Message);
                barrierData.Clear();
                barriers.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError("An unexpected error occurred while loading the barrier data file. Error: " + ex.Message);
                barrierData.Clear();
                barriers.Clear();
            }
        }
    }

    public void SaveRetracingStageData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        string deadzoneColliderFilePath = Path.Combine(directoryPath, "deadzone_colliders.dat");
        string deadzoneBarrierFilePath = Path.Combine(directoryPath, "deadzone_barriers.dat");

        if (!File.Exists(deadzoneColliderFilePath))
        {
            File.Create(deadzoneColliderFilePath).Dispose();
        }
        if (!File.Exists(deadzoneBarrierFilePath))
        {
            File.Create(deadzoneBarrierFilePath).Dispose();
        }

        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(deadzoneColliderFilePath))
        {
            bf.Serialize(file, deadzoneColliderData);
        }
        using (FileStream file = File.Create(deadzoneBarrierFilePath))
        {
            bf.Serialize(file, deadzoneBarrierData);
        }
    }

    public void LoadRetracingStageData()
    {
        string directoryPath = Path.Combine(mapPath, "World");
        string deadzoneColliderFilePath = Path.Combine(directoryPath, "deadzone_colliders.dat");
        string deadzoneBarrierFilePath = Path.Combine(directoryPath, "deadzone_barriers.dat");
        string arrowFilePath = Path.Combine(directoryPath, "arrows.dat");

        // Load invisible arrows for retracing
        for (int i = 0; i < arrowData.Count; i++)
        {
            GameObject invisibleArrow = Instantiate(invisibleArrowPrefabForRetracingPhase, arrowData[i].position + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset), arrowData[i].rotation);
            invisibleArrow.name = "Invisible Arrow " + (i + 1);
            invisibleArrow.transform.localScale = arrowData[i].scale;
            invisibleArrow.transform.SetParent(retracingArrowObjects.transform);
            invisibleArrow.GetComponent<BoxCollider>().center = arrowData[i].colliderData.center;
            invisibleArrow.GetComponent<BoxCollider>().size = arrowData[i].colliderData.size;
            if (inMapCreation) { invisibleArrow.tag = "Draggable"; }
            invisibleArrows.Add(invisibleArrow);
        }

        for (int i = 0; i < invisibleArrows.Count; i++)
        {
            Checkpoint invisibleArrowCheckpoint = invisibleArrows[i].GetComponent<Checkpoint>();
            invisibleArrowCheckpoint.checkpointNumber = i + 1;
            invisibleArrowCheckpoint.nextCheckpoint = (i == arrowData.Count - 1) ? invisibleArrows[0].GetComponent<Checkpoint>() : invisibleArrows[i + 1].GetComponent<Checkpoint>();
        }

        // Load deadzone colliders
        if (File.Exists(deadzoneColliderFilePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream stream = new FileStream(deadzoneColliderFilePath, FileMode.Open))
                {
                    deadzoneColliderData = bf.Deserialize(stream) as List<BarrierData>;
                }

                for (int i = 0; i < deadzoneColliderData.Count; i++)
                {
                    GameObject deadzoneCollider = Instantiate(deadzoneColliderPrefab, deadzoneColliderData[i].position + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset), deadzoneColliderData[i].rotation);
                    deadzoneCollider.name = "Deadzone Collider " + (i + 1);
                    deadzoneCollider.transform.localScale = deadzoneColliderData[i].scale;
                    deadzoneCollider.transform.SetParent(deadzoneColliderObjects.transform);
                    if (inMapCreation) { deadzoneCollider.tag = "Draggable"; }
                    deadzoneColliders.Add(deadzoneCollider);
                    if (deadzoneColliderData[i].isOneWayBarrier && inMapCreation)
                    {
                        deadzoneCollider.transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = true;
                        deadzoneCollider.transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = true;
                    }
                }
                if (!inMapCreation)
                {
                    for (int i = 0; i < deadzoneColliders.Count; i++)
                    {
                        deadzoneColliders[i].GetComponent<MeshRenderer>().enabled = false;
                        if (!deadzoneColliderData[i].isOneWayBarrier)
                        {
                            deadzoneColliders[i].GetComponent<DeadzoneCollider>().IsOneWay = false;
                        }
                        else
                        {
                            deadzoneColliders[i].GetComponent<DeadzoneCollider>().IsOneWay = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred while loading deadzone collider data: " + ex.Message);
                deadzoneColliderData.Clear();
                deadzoneColliders.Clear();
            }
        }

        // Load deadzone barriers
        if (File.Exists(deadzoneBarrierFilePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream stream = new FileStream(deadzoneBarrierFilePath, FileMode.Open))
                {
                    deadzoneBarrierData = bf.Deserialize(stream) as List<BarrierData>;
                }

                for (int i = 0; i < deadzoneBarrierData.Count; i++)
                {
                    GameObject deadzoneBarrier = Instantiate(deadzoneBarrierPrefab, deadzoneBarrierData[i].position + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset), deadzoneBarrierData[i].rotation);
                    deadzoneBarrier.name = "Deadzone Barrier " + (i + 1);
                    deadzoneBarrier.transform.localScale = deadzoneBarrierData[i].scale;
                    deadzoneBarrier.transform.SetParent(deadzoneBarrierObjects.transform);
                    if (inMapCreation) { deadzoneBarrier.tag = "Draggable"; }
                    deadzoneBarriers.Add(deadzoneBarrier);
                    if (deadzoneBarrierData[i].isOneWayBarrier && inMapCreation)
                    {
                        deadzoneBarrier.transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = true;
                        deadzoneBarrier.transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = true;
                    }
                }
                if (!inMapCreation)
                {
                    for (int i = 0; i < deadzoneBarriers.Count; i++)
                    {
                        deadzoneBarriers[i].GetComponent<MeshRenderer>().enabled = false;
                        if (!deadzoneBarrierData[i].isOneWayBarrier)
                        {
                            deadzoneBarriers[i].GetComponent<DeadzoneBarrier>().IsOneWay = false;
                        }
                        else
                        {
                            deadzoneBarriers[i].GetComponent<DeadzoneBarrier>().IsOneWay = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred while loading deadzone barrier data: " + ex.Message);
                deadzoneBarrierData.Clear();
                deadzoneBarriers.Clear();
            }
        }
    }
}