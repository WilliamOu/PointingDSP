using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlockUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown blockDropdown;
    [SerializeField] private World world;
    [SerializeField] private WorldSave worldSave;

    [SerializeField] private TMP_InputField blockNameInputField;
    [SerializeField] private Toggle isSolidToggle;
    [SerializeField] private TMP_InputField backFaceTextureInputField;
    [SerializeField] private TMP_InputField frontFaceTextureInputField;
    [SerializeField] private TMP_InputField topFaceTextureInputField;
    [SerializeField] private TMP_InputField bottomFaceTextureInputField;
    [SerializeField] private TMP_InputField leftFaceTextureInputField;
    [SerializeField] private TMP_InputField rightFaceTextureInputField;

    [SerializeField] private Button addButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_Text errorField;

    [SerializeField] private TMP_InputField xFrom;
    [SerializeField] private TMP_InputField yFrom;
    [SerializeField] private TMP_InputField zFrom;
    [SerializeField] private TMP_InputField xTo;
    [SerializeField] private TMP_InputField yTo;
    [SerializeField] private TMP_InputField zTo;
    [SerializeField] private Button spawnButton;
    [SerializeField] private Button despawnButton;

    private static int WorldWidthInBlocks => VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth;
    private static int WorldHeightInBlocks => VoxelData.ChunkHeight;

    private Coroutine writeMessage;
    private Coroutine resetDelete;

    private bool deleteButtonPressed = false;
    private Queue<int> deletedBlocksQueue = new Queue<int>();
    private List<int> dropdownToBlockIndex = new List<int>();

    private void Start()
    {
        errorField.alignment = TextAlignmentOptions.Right;
        blockDropdown.ClearOptions();
        PopulateDropdown();

        blockDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        if (world.blocktypes.Count > 0)
        {
            InitializeFirstValue();
        }

        blockNameInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "blockName"));
        isSolidToggle.onValueChanged.AddListener(value => OnToggleChanged(value, "isSolid"));
        backFaceTextureInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "backFaceTexture"));
        frontFaceTextureInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "frontFaceTexture"));
        topFaceTextureInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "topFaceTexture"));
        bottomFaceTextureInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "bottomFaceTexture"));
        leftFaceTextureInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "leftFaceTexture"));
        rightFaceTextureInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "rightFaceTexture"));

        addButton.onClick.AddListener(OnAddButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);

        spawnButton.onClick.AddListener(OnSpawnButtonClicked);
        despawnButton.onClick.AddListener(OnDespawnButtonClicked);
    }

    public void InitializeFirstValue()
    {
        OnDropdownValueChanged(0);
    }

    private void PopulateDropdown()
    {
        blockDropdown.ClearOptions();
        dropdownToBlockIndex.Clear();

        List<string> options = new List<string>();
        for (int i = 0; i < world.blocktypes.Count; i++)
        {
            if (world.blocktypes[i].blockName != "%%DELETED_BLOCK%%")
            {
                options.Add(world.blocktypes[i].blockName);
                dropdownToBlockIndex.Add(i);
            }
        }
        blockDropdown.AddOptions(options);
    }

    private void OnDropdownValueChanged(int index)
    {
        int blockIndex = dropdownToBlockIndex[index];
        LoadBlockData(blockIndex);
    }

    private void LoadBlockData(int index)
    {
        BlockType block = world.blocktypes[index];
        blockNameInputField.text = block.blockName;
        isSolidToggle.isOn = block.isSolid;
        backFaceTextureInputField.text = block.backFaceTexture.ToString();
        frontFaceTextureInputField.text = block.frontFaceTexture.ToString();
        topFaceTextureInputField.text = block.topFaceTexture.ToString();
        bottomFaceTextureInputField.text = block.bottomFaceTexture.ToString();
        leftFaceTextureInputField.text = block.leftFaceTexture.ToString();
        rightFaceTextureInputField.text = block.rightFaceTexture.ToString();
    }

    private void OnInputFieldChanged(string value, string fieldType)
    {
        int dropdownIndex = blockDropdown.value;
        int blockIndex = dropdownToBlockIndex[dropdownIndex];
        BlockType block = world.blocktypes[blockIndex];

        if (fieldType == "blockName")
        {
            if (value == "%%DELETED_BLOCK%%")
            {
                blockNameInputField.text = "INVALID NAME";
                writeMessage = StartCoroutine(WriteErrorMessage("Block name '%%DELETED_BLOCK%%' is invalid."));
                return;
            }

            block.blockName = value;
            blockDropdown.options[dropdownIndex].text = value;
            blockDropdown.RefreshShownValue();
        }
        else if (int.TryParse(value, out int textureID))
        {
            // Error handling for extreme values
            textureID = Mathf.Clamp(textureID, 0, 255);

            switch (fieldType)
            {
                case "backFaceTexture":
                    block.backFaceTexture = textureID;
                    break;
                case "frontFaceTexture":
                    block.frontFaceTexture = textureID;
                    break;
                case "topFaceTexture":
                    block.topFaceTexture = textureID;
                    break;
                case "bottomFaceTexture":
                    block.bottomFaceTexture = textureID;
                    break;
                case "leftFaceTexture":
                    block.leftFaceTexture = textureID;
                    break;
                case "rightFaceTexture":
                    block.rightFaceTexture = textureID;
                    break;
            }
        }
    }

    private void OnToggleChanged(bool value, string fieldType)
    {
        int index = blockDropdown.value;
        BlockType block = world.blocktypes[index];

        if (fieldType == "isSolid")
        {
            block.isSolid = value;
        }
    }

    private void OnAddButtonClicked()
    {
        if (world.blocktypes.Count >= 256)
        {
            writeMessage = StartCoroutine(WriteErrorMessage("Cannot add more than 256 block types."));
            return;
        }

        BlockType newBlock = new BlockType
        {
            blockName = "New Block",
            isSolid = true,
            backFaceTexture = 0,
            frontFaceTexture = 0,
            topFaceTexture = 0,
            bottomFaceTexture = 0,
            leftFaceTexture = 0,
            rightFaceTexture = 0
        };

        if (deletedBlocksQueue.Count > 0)
        {
            int reuseIndex = deletedBlocksQueue.Dequeue();
            world.blocktypes[reuseIndex] = newBlock;
            PopulateDropdown();
            blockDropdown.value = dropdownToBlockIndex.IndexOf(reuseIndex);
            OnDropdownValueChanged(dropdownToBlockIndex.IndexOf(reuseIndex));
        }
        else
        {
            world.blocktypes.Add(newBlock);
            PopulateDropdown();
            blockDropdown.value = world.blocktypes.Count - 1;
            OnDropdownValueChanged(world.blocktypes.Count - 1);
        }

        blockDropdown.RefreshShownValue();
    }

    private void OnDeleteButtonClicked()
    {
        int dropdownIndex = blockDropdown.value;
        int blockIndex = dropdownToBlockIndex[dropdownIndex];

        if (blockIndex < 4)
        {
            writeMessage = StartCoroutine(WriteErrorMessage("Cannot delete default blocks."));
            return;
        }

        // Check if deleteButtonPressed is true
        // Cannot invoke an actual deletion as block indices must remain stable due to references in world data
        if (deleteButtonPressed)
        {
            // Set the block to "%%DELETED_BLOCK%%"
            BlockType deletedBlock = new BlockType
            {
                blockName = "%%DELETED_BLOCK%%",
                isSolid = true,
                backFaceTexture = 0,
                frontFaceTexture = 0,
                topFaceTexture = 0,
                bottomFaceTexture = 0,
                leftFaceTexture = 0,
                rightFaceTexture = 0
            };
            world.blocktypes[blockIndex] = deletedBlock;
            deletedBlocksQueue.Enqueue(blockIndex);

            PopulateDropdown();
            if (dropdownToBlockIndex.Count > 0)
            {
                blockDropdown.value = Mathf.Clamp(dropdownIndex, 0, dropdownToBlockIndex.Count - 1);
                OnDropdownValueChanged(blockDropdown.value);
            }

            deleteButtonPressed = false;
            errorField.text = "";
        }
        else
        {
            deleteButtonPressed = true;
            writeMessage = StartCoroutine(WriteErrorMessage("Press delete again to confirm deletion."));
            resetDelete = StartCoroutine(ResetDeleteButtonPressed());
        }
    }

    private void OnSpawnButtonClicked()
    {
        handleBlockSpawn(true);
    }

    private void OnDespawnButtonClicked()
    {
        handleBlockSpawn(false);
    }

    private void handleBlockSpawn(bool spawn)
    {
        // 1) Parse numbers
        int xFromVal, xToVal, yFromVal, yToVal, zFromVal, zToVal;
        bool ok = true;
        ok &= int.TryParse(xFrom.text, out xFromVal);
        ok &= int.TryParse(xTo.text, out xToVal);
        ok &= int.TryParse(yFrom.text, out yFromVal);
        ok &= int.TryParse(yTo.text, out yToVal);
        ok &= int.TryParse(zFrom.text, out zFromVal);
        ok &= int.TryParse(zTo.text, out zToVal);

        if (!ok)
        {
            writeMessage = StartCoroutine(WriteErrorMessage("At least one of the fields is not an integer."));
            return;
        }

        // 2) Normalize ranges
        int x1 = Mathf.Min(xFromVal, xToVal);
        int x2 = Mathf.Max(xFromVal, xToVal);
        int y1 = Mathf.Min(yFromVal, yToVal);
        int y2 = Mathf.Max(yFromVal, yToVal);
        int z1 = Mathf.Min(zFromVal, zToVal);
        int z2 = Mathf.Max(zFromVal, zToVal);

        // 3) World bounds check for BOTH corners (inclusive)
        if (!IsWithinWorldBounds(x1, y1, z1, x2, y2, z2))
        {
            writeMessage = StartCoroutine(WriteErrorMessage("Cannot write outside of world bounds"));
            return;
        }

        // 4) Resolve currently selected block id
        int ddIndex = blockDropdown.value;
        if (ddIndex < 0 || ddIndex >= dropdownToBlockIndex.Count)
        {
            writeMessage = StartCoroutine(WriteErrorMessage("No block type selected."));
            return;
        }
        int blockId = dropdownToBlockIndex[ddIndex];

        if (spawn && world.blocktypes[blockId].blockName == "%%DELETED_BLOCK%%")
        {
            writeMessage = StartCoroutine(WriteErrorMessage("Cannot place a deleted block type."));
            return;
        }

        // 5) Apply
        for (int x = x1; x <= x2; x++)
        {
            for (int y = y1; y <= y2; y++)
            {
                for (int z = z1; z <= z2; z++)
                {
                    // Bad practice to mix types, but I believe the block count is hard capped at 256
                    Vector3 pos = new Vector3(x, y, z) + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset);
                    if (spawn) 
                        PlaceBlock(pos, (byte)blockId);
                    else 
                        DeleteBlock(pos);
                }
            }
        }
    }

    private bool IsWithinWorldBounds(int x1, int y1, int z1, int x2, int y2, int z2)
    {
        bool InX(int v) => v >= 0 && v < WorldWidthInBlocks;
        bool InY(int v) => v >= 0 && v < WorldHeightInBlocks;
        bool InZ(int v) => v >= 0 && v < WorldWidthInBlocks;

        return InX(x1) && InX(x2) &&
               InY(y1) && InY(y2) &&
               InZ(z1) && InZ(z2);
    }

    private void PlaceBlock(Vector3 pos, byte blockId)
    {
        world.GetChunkFromVector3(pos).EditVoxel(pos, blockId);
        worldSave.AddBlock(pos.x, pos.y, pos.z, blockId);
    }

    private void DeleteBlock(Vector3 pos)
    {
        world.GetChunkFromVector3(pos).EditVoxel(pos, 0);
        worldSave.RemoveBlock(pos.x, pos.y, pos.z);
    }


    private IEnumerator ResetDeleteButtonPressed()
    {
        if (resetDelete != null) { StopCoroutine(resetDelete); }
        yield return new WaitForSeconds(3);
        deleteButtonPressed = false;
    }

    private IEnumerator WriteErrorMessage(string message)
    {
        if (writeMessage != null) { StopCoroutine(writeMessage); }
        errorField.text = message;
        yield return new WaitForSeconds(3);
        errorField.text = "";
    }
}