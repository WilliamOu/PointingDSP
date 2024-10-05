using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;
    public WorldSave worldSave;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;

    public ChunkCoord playerChunkCoord;

    public List<BlockType> blocktypes = new List<BlockType>();

    [SerializeField] private AtlasPacker atlasPacker;
    [SerializeField] private AtlasScaler atlasScaler;
    // [SerializeField] private ChunkScaler chunkScaler;

    private Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    private ChunkCoord playerLastChunkCoord;
    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    private List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    // private bool isCreatingChunks;

    private void Awake()
    {
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

        worldSave = GetComponent<WorldSave>();
        worldSave.LoadWorldData();
        worldSave.LoadBlockData();
        worldSave.LoadMetaData();
        worldSave.LoadCSVData();
        worldSave.LoadLearningStageData();
        worldSave.LoadRetracingStageData();
    }

    private void Start()
    {
        InitializeTextures();
        GenerateWorld();
        InitializeModelsAsync();
    }

    private void InitializeTextures()
    {
        string atlasPath = Path.Combine(Application.persistentDataPath, "Maps", PersistentDataManager.Instance.Map, "Atlas", "Packed_Atlas.png");
        // string atlasPath = Path.Combine(Application.persistentDataPath, "Maps", PersistentDataManager.Instance.Map, "Atlas", "UVChecker.png");

        if (!File.Exists(atlasPath))
        {
            Debug.LogError("Texture atlas file not found at " + atlasPath);
            return;
        }

        byte[] fileData = File.ReadAllBytes(atlasPath);
        Texture2D atlasTexture = new Texture2D(2, 2);

        if (!atlasTexture.LoadImage(fileData))
        {
            Debug.LogError("Failed to load texture atlas from file data.");
        }

        atlasTexture.wrapMode = TextureWrapMode.Clamp;
        atlasTexture.filterMode = FilterMode.Point;
        atlasTexture.mipMapBias = 0f;

        material = new Material(Shader.Find("Standard"));
        material.mainTexture = atlasTexture;
    }

    private async void InitializeModelsAsync()
    {
        await worldSave.LoadModelData();
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void GenerateWorld()
    {
        for (int x = 0; x < VoxelData.WorldSizeInChunks; x++)
        {
            for (int z = 0; z < VoxelData.WorldSizeInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                // activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        /*if (!worldSave.inMapCreation)
        {
            foreach (Chunk chunk in chunks)
            {
                chunkScaler.ScaleChunk(chunk);
            }
        }*/
    }

    private bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return
                false;
    }

    private bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
    }

    public ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blocktypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;

        return blocktypes[GetVoxel(pos)].isSolid;
    }

    public bool CheckIfVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blocktypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent;

        return blocktypes[GetVoxel(pos)].isTransparent;
    }


    public byte GetVoxel(Vector3 pos)
    {
        SerializableVector3 serializedPos = new SerializableVector3(pos);

        if (worldSave.blockDataMap.ContainsKey(serializedPos))
        {
            return worldSave.blockDataMap[serializedPos].blockType;
        }
        else
        {
            // Air
            return 0;
        }
    }

    /* IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }*/

    /*void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        // Loop through all chunks currently within view distance of the player.
        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                // If the current chunk is in the world...
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    // Check if it active, if not, activate it.
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(x, z));
                }

                // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (ChunkCoord c in previouslyActiveChunks)
            chunks[c.x, c.z].isActive = false;
    }*/
}

[System.Serializable]
public class BlockType
{
    // Unused
    public VoxelMeshData meshData;
    public string blockName;
    public bool isSolid;
    // Unused
    public bool isTransparent;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}