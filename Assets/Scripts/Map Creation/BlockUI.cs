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