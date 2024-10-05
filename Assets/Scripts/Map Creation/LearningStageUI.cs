using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LearningStageUI : MonoBehaviour
{
    [SerializeField] private BirdsEyeCameraUI uiBase;
    [SerializeField] private WorldSave worldSave;

    [SerializeField] private GameObject arrowUI;
    [SerializeField] private GameObject barrierUI;
    [SerializeField] private TMP_Dropdown selectDropdown;
    [SerializeField] private TMP_Dropdown arrowDropdown;
    [SerializeField] private TMP_Dropdown barrierDropdown;
    [SerializeField] private TMP_InputField arrowPositionInputField;
    [SerializeField] private TMP_InputField arrowRotationInputField;
    [SerializeField] private TMP_InputField arrowScaleInputField;
    [SerializeField] private TMP_InputField arrowHitboxPositionInputField;
    [SerializeField] private TMP_InputField arrowHitboxScaleInputField;
    [SerializeField] private TMP_InputField barrierPositionInputField;
    [SerializeField] private TMP_InputField barrierRotationInputField;
    [SerializeField] private TMP_InputField barrierScaleInputField;
    [SerializeField] private Toggle isOneWayBarrierToggle;
    [SerializeField] private Button addArrowButton;
    [SerializeField] private Button deleteArrowButton;
    [SerializeField] private Button addBarrierButton;
    [SerializeField] private Button deleteBarrierButton;

    private GameObject currentlySelectedObject;

    void Start()
    {
        arrowUI.SetActive(true);
        barrierUI.SetActive(false);
        PopulateBarrierDropdown();
        PopulateArrowDropdown();
        OnBarrierDropdownChanged(0);
        OnArrowDropdownChanged(0);

        selectDropdown.onValueChanged.AddListener(SelectSubMenu);
        arrowDropdown.onValueChanged.AddListener(OnArrowDropdownChanged);
        barrierDropdown.onValueChanged.AddListener(OnBarrierDropdownChanged);
        addArrowButton.onClick.AddListener(OnAddArrowButtonClicked);
        deleteArrowButton.onClick.AddListener(OnDeleteArrowButtonClicked);
        addBarrierButton.onClick.AddListener(OnAddBarrierButtonClicked);
        deleteBarrierButton.onClick.AddListener(OnDeleteBarrierButtonClicked);

        arrowPositionInputField.onValueChanged.AddListener(value => OnArrowInputFieldChanged(value, "position"));
        arrowRotationInputField.onValueChanged.AddListener(value => OnArrowInputFieldChanged(value, "rotation"));
        arrowScaleInputField.onValueChanged.AddListener(value => OnArrowInputFieldChanged(value, "scale"));
        arrowHitboxPositionInputField.onValueChanged.AddListener(value => OnArrowInputFieldChanged(value, "hitboxPosition"));
        arrowHitboxScaleInputField.onValueChanged.AddListener(value => OnArrowInputFieldChanged(value, "hitboxScale"));
        barrierPositionInputField.onValueChanged.AddListener(value => OnBarrierInputFieldChanged(value, "position"));
        barrierRotationInputField.onValueChanged.AddListener(value => OnBarrierInputFieldChanged(value, "rotation"));
        barrierScaleInputField.onValueChanged.AddListener(value => OnBarrierInputFieldChanged(value, "scale"));
        isOneWayBarrierToggle.onValueChanged.AddListener(MakeOneWayBarrier);
    }

    private void PopulateArrowDropdown()
    {
        arrowDropdown.ClearOptions();

        for (int i = 0; i < worldSave.arrows.Count; i++)
        {
            arrowDropdown.options.Add(new TMP_Dropdown.OptionData("Arrow " + (i + 1)));
        }

        arrowDropdown.RefreshShownValue();
    }

    private void PopulateBarrierDropdown()
    {
        barrierDropdown.ClearOptions();

        for (int i = 0; i < worldSave.barriers.Count; i++)
        {
            barrierDropdown.options.Add(new TMP_Dropdown.OptionData("Barrier " + (i + 1)));
        }

        barrierDropdown.RefreshShownValue();
    }

    private void OnArrowDropdownChanged(int index)
    {
        if (worldSave.arrows.Count != 0)
        {
            RemoveHighlight();
            LoadArrowData(index);
            AddHighlight();
        }
    }

    private void OnBarrierDropdownChanged(int index)
    {
        if (worldSave.barriers.Count != 0)
        {
            RemoveHighlight();
            LoadBarrierData(index);
            AddHighlight();
        }
    }

    private void LoadArrowData(int index)
    {
        if (worldSave.arrows.Count > 0 && index >= 0 && index < worldSave.arrows.Count)
        {
            var selectedData = worldSave.arrowData[index];
            currentlySelectedObject = worldSave.arrows[index];

            arrowPositionInputField.text = uiBase.FormatVector3(selectedData.position);
            arrowRotationInputField.text = uiBase.FormatQuaternion(selectedData.rotation);
            arrowScaleInputField.text = uiBase.FormatVector3(selectedData.scale);
            arrowHitboxPositionInputField.text = uiBase.FormatVector3(selectedData.colliderData.center);
            arrowHitboxScaleInputField.text = uiBase.FormatVector3(selectedData.colliderData.size);
        }
    }

    private void LoadBarrierData(int index)
    {
        if (worldSave.barriers.Count > 0 && index >= 0 && index < worldSave.barriers.Count)
        {
            var selectedData = worldSave.barrierData[index];
            currentlySelectedObject = worldSave.barriers[index];

            barrierPositionInputField.text = uiBase.FormatVector3(selectedData.position);
            barrierRotationInputField.text = uiBase.FormatQuaternion(selectedData.rotation);
            barrierScaleInputField.text = uiBase.FormatVector3(selectedData.scale);
            isOneWayBarrierToggle.isOn = worldSave.barrierData[index].isOneWayBarrier;
        }
    }

    private void OnArrowInputFieldChanged(string value, string fieldType)
    {
        int index = arrowDropdown.value;

        if (uiBase.TryParseVector3(value, out Vector3 result) && worldSave.arrows.Count != 0)
        {
            switch (fieldType)
            {
                case "position":
                    worldSave.arrows[index].transform.position = result + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset);
                    worldSave.arrowData[index].position = result;
                    if (uiBase.CurrentMenu == 4) { uiBase.DrawBoxColliderOutline(worldSave.arrows[index].GetComponent<BoxCollider>()); }
                    break;
                case "rotation":
                    worldSave.arrows[index].transform.rotation = Quaternion.Euler(result);
                    worldSave.arrowData[index].rotation = new SerializableQuaternion(Quaternion.Euler(result));
                    if (uiBase.CurrentMenu == 4) { uiBase.DrawBoxColliderOutline(worldSave.arrows[index].GetComponent<BoxCollider>()); }
                    break;
                case "scale":
                    worldSave.arrows[index].transform.localScale = result;
                    worldSave.arrowData[index].scale = new SerializableVector3(result);
                    if (uiBase.CurrentMenu == 4) { uiBase.DrawBoxColliderOutline(worldSave.arrows[index].GetComponent<BoxCollider>()); }
                    break;
                case "hitboxPosition":
                    worldSave.arrows[index].GetComponent<BoxCollider>().center = result;
                    worldSave.arrowData[index].colliderData.center = new SerializableVector3(result);
                    if (uiBase.CurrentMenu == 4) { uiBase.DrawBoxColliderOutline(worldSave.arrows[index].GetComponent<BoxCollider>()); }
                    break;
                case "hitboxScale":
                    worldSave.arrows[index].GetComponent<BoxCollider>().size = result;
                    worldSave.arrowData[index].colliderData.size = new SerializableVector3(result);
                    if (uiBase.CurrentMenu == 4) { uiBase.DrawBoxColliderOutline(worldSave.arrows[index].GetComponent<BoxCollider>()); }
                    break;
            }
        }
    }

    private void OnBarrierInputFieldChanged(string value, string fieldType)
    {
        int index = barrierDropdown.value;

        if (uiBase.TryParseVector3(value, out Vector3 result) && worldSave.barriers.Count != 0)
        {
            switch (fieldType)
            {
                case "position":
                    worldSave.barriers[index].transform.position = result + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset);
                    worldSave.barrierData[index].position = result;
                    if (uiBase.CurrentMenu == 4) { uiBase.DrawBoxColliderOutline(worldSave.barriers[index].GetComponent<BoxCollider>()); }
                    break;
                case "rotation":
                    worldSave.barriers[index].transform.rotation = Quaternion.Euler(result);
                    worldSave.barrierData[index].rotation = new SerializableQuaternion(Quaternion.Euler(result));
                    if (uiBase.CurrentMenu == 4) { uiBase.DrawBoxColliderOutline(worldSave.barriers[index].GetComponent<BoxCollider>()); }
                    break;
                case "scale":
                    worldSave.barriers[index].transform.localScale = result;
                    worldSave.barrierData[index].scale = new SerializableVector3(result);
                    if (uiBase.CurrentMenu == 4) { uiBase.DrawBoxColliderOutline(worldSave.barriers[index].GetComponent<BoxCollider>()); }
                    break;
            }
        }
    }

    private void OnAddArrowButtonClicked()
    {
        GameObject newArrow = Instantiate(worldSave.arrowPrefab, worldSave.arrowObjects.transform.position, Quaternion.identity);
        newArrow.transform.localScale = new SerializableVector3(75f, 2.5f, 22.5f);
        newArrow.transform.SetParent(worldSave.arrowObjects.transform);

        worldSave.arrows.Add(newArrow);
        worldSave.arrowData.Add(new ArrowData
        {
            position = new SerializableVector3(Vector3.zero),
            rotation = new SerializableQuaternion(Quaternion.identity),
            scale = new SerializableVector3(75f, 2.5f, 22.5f),
            colliderData = new BoxColliderData
            {
                center = new SerializableVector3(0f, 0.5f, 0f),
                size = new SerializableVector3(0.0334f, 1, 0.112f)
            }
        });

        PopulateArrowDropdown();
        arrowDropdown.value = worldSave.arrows.Count - 1;
        // Setting the value in the edge case where the list is empty does not invoke the dropdown change method so we need to manually call it
        OnArrowDropdownChanged(worldSave.arrows.Count - 1); 
    }

    private void OnAddBarrierButtonClicked()
    {
        GameObject newBarrier = Instantiate(worldSave.barrierPrefab, worldSave.barrierObjects.transform.position, Quaternion.identity);
        newBarrier.transform.localScale = new SerializableVector3(Vector3.one);
        newBarrier.transform.SetParent(worldSave.barrierObjects.transform);

        worldSave.barriers.Add(newBarrier);
        worldSave.barrierData.Add(new BarrierData
        {
            position = new SerializableVector3(Vector3.zero),
            rotation = new SerializableQuaternion(Quaternion.identity),
            scale = new SerializableVector3(Vector3.one),
            isOneWayBarrier = false
        });

        PopulateBarrierDropdown();
        barrierDropdown.value = worldSave.barriers.Count - 1;
        // Setting the value in the edge case where the list is empty does not invoke the dropdown change method so we need to manually call it
        OnBarrierDropdownChanged(worldSave.barriers.Count - 1);
    }

    private void OnDeleteArrowButtonClicked()
    {
        int index = arrowDropdown.value;
        if (index >= 0 && index < worldSave.arrows.Count)
        {
            RemoveHighlight();
            Destroy(worldSave.arrows[index]);
            worldSave.arrows.RemoveAt(index);
            worldSave.arrowData.RemoveAt(index);

            PopulateArrowDropdown();
            if (worldSave.arrows.Count > 0)
            {
                arrowDropdown.value = 0;
                currentlySelectedObject = worldSave.arrows[arrowDropdown.value];
                AddHighlight();
            }
            else
            {
                arrowPositionInputField.text = string.Empty;
                arrowRotationInputField.text = string.Empty;
                arrowScaleInputField.text = string.Empty;
                arrowHitboxPositionInputField.text = string.Empty;
                arrowHitboxScaleInputField.text = string.Empty;
            }
        }
    }

    private void OnDeleteBarrierButtonClicked()
    {
        int index = barrierDropdown.value;
        if (index >= 0 && index < worldSave.barriers.Count)
        {
            RemoveHighlight();
            Destroy(worldSave.barriers[index]);
            worldSave.barriers.RemoveAt(index);
            worldSave.barrierData.RemoveAt(index);

            PopulateBarrierDropdown();
            if (worldSave.barriers.Count > 0)
            {
                barrierDropdown.value = 0;
                currentlySelectedObject = worldSave.barriers[barrierDropdown.value];
                AddHighlight();
            }
            else
            {
                barrierPositionInputField.text = string.Empty;
                barrierRotationInputField.text = string.Empty;
                barrierScaleInputField.text = string.Empty;
                isOneWayBarrierToggle.isOn = false;
            }
        }
    }

    private void MakeOneWayBarrier(bool isOneWay)
    {
        int index = barrierDropdown.value;
        if (worldSave.barriers.Count > 0)
        {
            if (isOneWay)
            {
                worldSave.barriers[index].transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = true;
                worldSave.barriers[index].transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = true;
                worldSave.barrierData[index].isOneWayBarrier = true;
            }
            else
            {
                worldSave.barriers[index].transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = false;
                worldSave.barriers[index].transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = false;
                worldSave.barrierData[index].isOneWayBarrier = false;
            }
        }
    }

    public void AddHighlight()
    {
        if (currentlySelectedObject)
        {
            BoxCollider boxCollider = currentlySelectedObject.GetComponent<BoxCollider>();
            uiBase.DrawBoxColliderOutline(boxCollider);
        }
    }

    public void RemoveHighlight()
    {
        if (currentlySelectedObject)
        {
            uiBase.ClearBoxColliderOutline();
        }
    }

    private void SelectSubMenu(int index)
    {
        switch (index)
        {
            case 0:
                arrowUI.SetActive(true);
                barrierUI.SetActive(false);

                RemoveHighlight();
                if (worldSave.arrows.Count != 0)
                {
                    currentlySelectedObject = worldSave.arrows[arrowDropdown.value];
                    AddHighlight();
                }

                break;
            case 1:
                arrowUI.SetActive(false);
                barrierUI.SetActive(true);

                RemoveHighlight();
                if (worldSave.barriers.Count != 0)
                {
                    currentlySelectedObject = worldSave.barriers[barrierDropdown.value];
                    AddHighlight();
                }

                break;
        }
    }

    public void UpdatePrimaryTransformUI()
    {
        int selectedMenu = selectDropdown.value;

        if (selectedMenu == 0 && worldSave.arrows.Count > 0)
        {
            var selectedArrowData = worldSave.arrows[arrowDropdown.value].transform;

            arrowPositionInputField.text = uiBase.FormatVector3(selectedArrowData.position - new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset));
            arrowRotationInputField.text = uiBase.FormatQuaternion(selectedArrowData.rotation);
            arrowScaleInputField.text = uiBase.FormatVector3(selectedArrowData.localScale);
        }
        else if (selectedMenu == 1 && worldSave.barriers.Count > 0)
        {
            var selectedBarrierData = worldSave.barriers[barrierDropdown.value].transform;

            barrierPositionInputField.text = uiBase.FormatVector3(selectedBarrierData.position - new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset));
            barrierRotationInputField.text = uiBase.FormatQuaternion(selectedBarrierData.rotation);
            barrierScaleInputField.text = uiBase.FormatVector3(selectedBarrierData.localScale);
        }
    }
}
