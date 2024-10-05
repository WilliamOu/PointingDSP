using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown objectDropdown;
    [SerializeField] private WorldSave worldSave;
    [SerializeField] private BirdsEyeCameraUI uiBase;

    [SerializeField] private TMP_InputField positionInputField;
    [SerializeField] private TMP_InputField rotationInputField;
    [SerializeField] private TMP_InputField scaleInputField;
    [SerializeField] private TMP_InputField relativePositionInputField;
    [SerializeField] private TMP_InputField relativeRotationInputField;
    [SerializeField] private TMP_InputField relativeScaleInputField;
    [SerializeField] private TMP_InputField hitboxPositionInputField;
    [SerializeField] private TMP_InputField hitboxScaleInputField;

    private BoxCollider boxCollider;
    private GameObject currentlySelectedModel = null;
    private Outline outline;
    private GameObject colliderVisualizer;

    public void InitializeList()
    {
        objectDropdown.ClearOptions();
        PopulateDropdown();

        if (worldSave.modelParents.Count > 0)
        {
            objectDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            InitializeFirstValue();
        }
        else
        {
            uiBase.VoidObjectAndTrialUIs();
            /*List<string> noOptions = new List<string> { "[NO OBJECTS]" };
            objectDropdown.AddOptions(noOptions);*/
        }

        positionInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "position"));
        rotationInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "rotation"));
        scaleInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "scale"));
        relativePositionInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "relativePosition"));
        relativeRotationInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "relativeRotation"));
        relativeScaleInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "relativeScale"));
        hitboxPositionInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "hitboxPosition"));
        hitboxScaleInputField.onValueChanged.AddListener(value => OnInputFieldChanged(value, "hitboxScale"));
    }

    public void InitializeFirstValue()
    {
        LoadModelData(0);
    }

    private void PopulateDropdown()
    {
        List<string> options = new List<string>();
        for (int i = 0; i < worldSave.modelParents.Count; i++)
        {
            options.Add(worldSave.modelParents[i].name);
        }
        objectDropdown.AddOptions(options);
    }

    private void OnDropdownValueChanged(int index)
    {
        if (currentlySelectedModel) { RemoveHighlight(); }
        LoadModelData(index);
        AddHighlight();
    }

    private void LoadModelData(int index)
    {
        currentlySelectedModel = worldSave.modelParents[index];
        boxCollider = worldSave.modelParents[index].GetComponent<BoxCollider>();
        positionInputField.text = uiBase.FormatVector3(worldSave.modelParents[index].transform.position - new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset));
        rotationInputField.text = uiBase.FormatQuaternion(worldSave.modelParents[index].transform.rotation);
        scaleInputField.text = uiBase.FormatVector3(worldSave.modelParents[index].transform.localScale);
        relativePositionInputField.text = uiBase.FormatVector3(worldSave.modelNested[index].transform.localPosition);
        relativeRotationInputField.text = uiBase.FormatQuaternion(worldSave.modelNested[index].transform.localRotation);
        relativeScaleInputField.text = uiBase.FormatVector3(worldSave.modelNested[index].transform.localScale);
        hitboxPositionInputField.text = uiBase.FormatVector3(boxCollider.center);
        hitboxScaleInputField.text = uiBase.FormatVector3(boxCollider.size);
    }

    private void OnInputFieldChanged(string value, string fieldType)
    {
        int index = objectDropdown.value;

        if (uiBase.TryParseVector3(value, out Vector3 result))
        {
            switch (fieldType)
            {
                case "position":
                    worldSave.modelParents[index].transform.position = result + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset);
                    break;
                case "rotation":
                    worldSave.modelParents[index].transform.rotation = Quaternion.Euler(result);
                    break;
                case "scale":
                    worldSave.modelParents[index].transform.localScale = result;
                    break;
                case "relativePosition":
                    worldSave.modelNested[index].transform.localPosition = result;
                    break;
                case "relativeRotation":
                    worldSave.modelNested[index].transform.localRotation = Quaternion.Euler(result);
                    break;
                case "relativeScale":
                    worldSave.modelNested[index].transform.localScale = result;
                    break;
                case "hitboxPosition":
                    worldSave.modelParents[index].GetComponent<BoxCollider>().center = result;
                    break;
                case "hitboxScale":
                    worldSave.modelParents[index].GetComponent<BoxCollider>().size = result;
                    break;
            }
            if (uiBase.CurrentMenu == 2) { uiBase.DrawBoxColliderOutline(boxCollider); }
        }
    }
    
    public void AddHighlight()
    {
        if (currentlySelectedModel == null || currentlySelectedModel.GetComponent<Outline>() != null) { return; }
        outline = currentlySelectedModel.AddComponent<Outline>();

        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.green;
        outline.OutlineWidth = 5f;

        uiBase.DrawBoxColliderOutline(boxCollider);
    }

    public void RemoveHighlight()
    {
        if (outline) { GameObject.Destroy(outline); }
        uiBase.ClearBoxColliderOutline();
    }

    public void UpdatePrimaryTransformUI()
    {
        int index = objectDropdown.value;
        positionInputField.text = uiBase.FormatVector3(worldSave.modelParents[index].transform.position - new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset));
        rotationInputField.text = uiBase.FormatQuaternion(worldSave.modelParents[index].transform.rotation);
        scaleInputField.text = uiBase.FormatVector3(worldSave.modelParents[index].transform.localScale);
    }
}