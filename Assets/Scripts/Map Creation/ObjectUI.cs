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

    private struct ObjRef { public bool isTrial; public int idx; }
    private List<ObjRef> flatIndex = new List<ObjRef>();

    private GameObject GetParentAt(int flat)
    {
        return flatIndex[flat].isTrial ? worldSave.modelParents[flatIndex[flat].idx]
                                       : worldSave.nonTrialParents[flatIndex[flat].idx];
    }
    private GameObject GetNestedAt(int flat)
    {
        return flatIndex[flat].isTrial ? worldSave.modelNested[flatIndex[flat].idx]
                                       : worldSave.nonTrialNested[flatIndex[flat].idx];
    }
    private BoxCollider GetColliderAt(int flat)
    {
        return GetParentAt(flat).GetComponent<BoxCollider>();
    }

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
        flatIndex.Clear();
        List<string> options = new List<string>();

        // 1) trial objects
        for (int i = 0; i < worldSave.modelParents.Count; i++)
        {
            options.Add(worldSave.modelParents[i].name);
            flatIndex.Add(new ObjRef { isTrial = true, idx = i });
        }

        // 2) non-trial objects, with suffix to distinguish in UI
        for (int i = 0; i < worldSave.nonTrialParents.Count; i++)
        {
            options.Add("[NON-TRIAL] " + worldSave.nonTrialParents[i].name);
            flatIndex.Add(new ObjRef { isTrial = false, idx = i });
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
        currentlySelectedModel = GetParentAt(index);
        boxCollider = GetColliderAt(index);

        positionInputField.text = uiBase.FormatVector3(GetParentAt(index).transform.position - new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset));
        rotationInputField.text = uiBase.FormatQuaternion(GetParentAt(index).transform.rotation);
        scaleInputField.text = uiBase.FormatVector3(GetParentAt(index).transform.localScale);
        relativePositionInputField.text = uiBase.FormatVector3(GetNestedAt(index).transform.localPosition);
        relativeRotationInputField.text = uiBase.FormatQuaternion(GetNestedAt(index).transform.localRotation);
        relativeScaleInputField.text = uiBase.FormatVector3(GetNestedAt(index).transform.localScale);
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
                    GetParentAt(index).transform.position = result + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset);
                    break;
                case "rotation":
                    GetParentAt(index).transform.rotation = Quaternion.Euler(result);
                    break;
                case "scale":
                    GetParentAt(index).transform.localScale = result;
                    break;
                case "relativePosition":
                    GetNestedAt(index).transform.localPosition = result;
                    break;
                case "relativeRotation":
                    GetNestedAt(index).transform.localRotation = Quaternion.Euler(result);
                    break;
                case "relativeScale":
                    GetNestedAt(index).transform.localScale = result;
                    break;
                case "hitboxPosition":
                    GetColliderAt(index).center = result;
                    break;
                case "hitboxScale":
                    GetColliderAt(index).size = result;
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
        positionInputField.text = uiBase.FormatVector3(GetParentAt(index).transform.position - new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset));
        rotationInputField.text = uiBase.FormatQuaternion(GetParentAt(index).transform.rotation);
        scaleInputField.text = uiBase.FormatVector3(GetParentAt(index).transform.localScale);
    }
}