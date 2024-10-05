using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RetracingStageUI : MonoBehaviour
{
    [SerializeField] private BirdsEyeCameraUI uiBase;
    [SerializeField] private WorldSave worldSave;

    [SerializeField] private GameObject deadzoneColliderUI;
    [SerializeField] private GameObject deadzoneBarrierUI;
    [SerializeField] private TMP_Dropdown selectDropdown;
    [SerializeField] private TMP_Dropdown deadzoneColliderDropdown;
    [SerializeField] private TMP_Dropdown deadzoneBarrierDropdown;
    [SerializeField] private TMP_InputField deadzoneColliderPositionInputField;
    [SerializeField] private TMP_InputField deadzoneColliderRotationInputField;
    [SerializeField] private TMP_InputField deadzoneColliderScaleInputField;
    [SerializeField] private Toggle deadzoneColliderIsOneWayBarrierToggle;
    [SerializeField] private TMP_InputField deadzoneBarrierPositionInputField;
    [SerializeField] private TMP_InputField deadzoneBarrierRotationInputField;
    [SerializeField] private TMP_InputField deadzoneBarrierScaleInputField;
    [SerializeField] private Toggle deadzoneBarrierIsOneWayBarrierToggle;
    [SerializeField] private Button addDeadzoneColliderButton;
    [SerializeField] private Button deleteDeadzoneColliderButton;
    [SerializeField] private Button addDeadzoneBarrierButton;
    [SerializeField] private Button deleteDeadzoneBarrierButton;

    private GameObject currentlySelectedObject;

    void Start()
    {
        deadzoneColliderUI.SetActive(true);
        deadzoneBarrierUI.SetActive(false);
        PopulateDeadzoneBarrierDropdown();
        PopulateDeadzoneColliderDropdown();
        OnDeadzoneBarrierDropdownChanged(0);
        OnDeadzoneColliderDropdownChanged(0);

        selectDropdown.onValueChanged.AddListener(SelectSubMenu);
        deadzoneColliderDropdown.onValueChanged.AddListener(OnDeadzoneColliderDropdownChanged);
        deadzoneBarrierDropdown.onValueChanged.AddListener(OnDeadzoneBarrierDropdownChanged);
        addDeadzoneColliderButton.onClick.AddListener(OnAddDeadzoneColliderButtonClicked);
        deleteDeadzoneColliderButton.onClick.AddListener(OnDeleteDeadzoneColliderButtonClicked);
        addDeadzoneBarrierButton.onClick.AddListener(OnAddDeadzoneBarrierButtonClicked);
        deleteDeadzoneBarrierButton.onClick.AddListener(OnDeleteDeadzoneBarrierButtonClicked);

        deadzoneColliderPositionInputField.onValueChanged.AddListener(value => OnDeadzoneColliderInputFieldChanged(value, "position"));
        deadzoneColliderRotationInputField.onValueChanged.AddListener(value => OnDeadzoneColliderInputFieldChanged(value, "rotation"));
        deadzoneColliderScaleInputField.onValueChanged.AddListener(value => OnDeadzoneColliderInputFieldChanged(value, "scale"));
        deadzoneColliderIsOneWayBarrierToggle.onValueChanged.AddListener(MakeOneWayDeadzoneCollider);
        deadzoneBarrierPositionInputField.onValueChanged.AddListener(value => OnDeadzoneBarrierInputFieldChanged(value, "position"));
        deadzoneBarrierRotationInputField.onValueChanged.AddListener(value => OnDeadzoneBarrierInputFieldChanged(value, "rotation"));
        deadzoneBarrierScaleInputField.onValueChanged.AddListener(value => OnDeadzoneBarrierInputFieldChanged(value, "scale"));
        deadzoneBarrierIsOneWayBarrierToggle.onValueChanged.AddListener(MakeOneWayDeadzoneBarrier);
    }

    private void PopulateDeadzoneColliderDropdown()
    {
        deadzoneColliderDropdown.ClearOptions();

        for (int i = 0; i < worldSave.deadzoneColliders.Count; i++)
        {
            deadzoneColliderDropdown.options.Add(new TMP_Dropdown.OptionData("Deadzone Collider " + (i + 1)));
        }

        deadzoneColliderDropdown.RefreshShownValue();
    }

    private void PopulateDeadzoneBarrierDropdown()
    {
        deadzoneBarrierDropdown.ClearOptions();

        for (int i = 0; i < worldSave.deadzoneBarriers.Count; i++)
        {
            deadzoneBarrierDropdown.options.Add(new TMP_Dropdown.OptionData("Deadzone Barrier " + (i + 1)));
        }

        deadzoneBarrierDropdown.RefreshShownValue();
    }

    private void OnDeadzoneColliderDropdownChanged(int index)
    {
        if (worldSave.deadzoneColliders.Count != 0)
        {
            RemoveHighlight();
            LoadDeadzoneColliderData(index);
            AddHighlight();
        }
    }

    private void OnDeadzoneBarrierDropdownChanged(int index)
    {
        if (worldSave.deadzoneBarriers.Count != 0)
        {
            RemoveHighlight();
            LoadDeadzoneBarrierData(index);
            AddHighlight();
        }
    }

    private void LoadDeadzoneColliderData(int index)
    {
        if (worldSave.deadzoneColliders.Count > 0 && index >= 0 && index < worldSave.deadzoneColliders.Count)
        {
            var selectedData = worldSave.deadzoneColliderData[index];
            currentlySelectedObject = worldSave.deadzoneColliders[index];

            deadzoneColliderPositionInputField.text = uiBase.FormatVector3(selectedData.position);
            deadzoneColliderRotationInputField.text = uiBase.FormatQuaternion(selectedData.rotation);
            deadzoneColliderScaleInputField.text = uiBase.FormatVector3(selectedData.scale);
            deadzoneColliderIsOneWayBarrierToggle.isOn = worldSave.deadzoneColliderData[index].isOneWayBarrier;
        }
    }

    private void LoadDeadzoneBarrierData(int index)
    {
        if (worldSave.deadzoneBarriers.Count > 0 && index >= 0 && index < worldSave.deadzoneBarriers.Count)
        {
            var selectedData = worldSave.deadzoneBarrierData[index];
            currentlySelectedObject = worldSave.deadzoneBarriers[index];

            deadzoneBarrierPositionInputField.text = uiBase.FormatVector3(selectedData.position);
            deadzoneBarrierRotationInputField.text = uiBase.FormatQuaternion(selectedData.rotation);
            deadzoneBarrierScaleInputField.text = uiBase.FormatVector3(selectedData.scale);
            deadzoneBarrierIsOneWayBarrierToggle.isOn = worldSave.deadzoneBarrierData[index].isOneWayBarrier;
        }
    }

    private void OnDeadzoneColliderInputFieldChanged(string value, string fieldType)
    {
        int index = deadzoneColliderDropdown.value;

        if (uiBase.TryParseVector3(value, out Vector3 result) && worldSave.deadzoneColliders.Count != 0)
        {
            switch (fieldType)
            {
                case "position":
                    worldSave.deadzoneColliders[index].transform.position = result + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset);
                    worldSave.deadzoneColliderData[index].position = result;
                    if (uiBase.CurrentMenu == 5) { uiBase.DrawBoxColliderOutline(worldSave.deadzoneColliders[index].GetComponent<BoxCollider>()); }
                    break;
                case "rotation":
                    worldSave.deadzoneColliders[index].transform.rotation = Quaternion.Euler(result);
                    worldSave.deadzoneColliderData[index].rotation = new SerializableQuaternion(Quaternion.Euler(result));
                    if (uiBase.CurrentMenu == 5) { uiBase.DrawBoxColliderOutline(worldSave.deadzoneColliders[index].GetComponent<BoxCollider>()); }
                    break;
                case "scale":
                    worldSave.deadzoneColliders[index].transform.localScale = result;
                    worldSave.deadzoneColliderData[index].scale = new SerializableVector3(result);
                    if (uiBase.CurrentMenu == 5) { uiBase.DrawBoxColliderOutline(worldSave.deadzoneColliders[index].GetComponent<BoxCollider>()); }
                    break;
            }
        }
    }

    private void OnDeadzoneBarrierInputFieldChanged(string value, string fieldType)
    {
        int index = deadzoneBarrierDropdown.value;

        if (uiBase.TryParseVector3(value, out Vector3 result) && worldSave.deadzoneBarriers.Count != 0)
        {
            switch (fieldType)
            {
                case "position":
                    worldSave.deadzoneBarriers[index].transform.position = result + new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset);
                    worldSave.deadzoneBarrierData[index].position = result;
                    if (uiBase.CurrentMenu == 5) { uiBase.DrawBoxColliderOutline(worldSave.deadzoneBarriers[index].GetComponent<BoxCollider>()); }
                    break;
                case "rotation":
                    worldSave.deadzoneBarriers[index].transform.rotation = Quaternion.Euler(result);
                    worldSave.deadzoneBarrierData[index].rotation = new SerializableQuaternion(Quaternion.Euler(result));
                    if (uiBase.CurrentMenu == 5) { uiBase.DrawBoxColliderOutline(worldSave.deadzoneBarriers[index].GetComponent<BoxCollider>()); }
                    break;
                case "scale":
                    worldSave.deadzoneBarriers[index].transform.localScale = result;
                    worldSave.deadzoneBarrierData[index].scale = new SerializableVector3(result);
                    if (uiBase.CurrentMenu == 5) { uiBase.DrawBoxColliderOutline(worldSave.deadzoneBarriers[index].GetComponent<BoxCollider>()); }
                    break;
            }
        }
    }

    private void OnAddDeadzoneColliderButtonClicked()
    {
        GameObject newDeadzoneCollider = Instantiate(worldSave.deadzoneColliderPrefab, worldSave.deadzoneColliderObjects.transform.position, Quaternion.identity);
        newDeadzoneCollider.transform.localScale = new SerializableVector3(Vector3.one);
        newDeadzoneCollider.transform.SetParent(worldSave.deadzoneColliderObjects.transform);

        worldSave.deadzoneColliders.Add(newDeadzoneCollider);
        worldSave.deadzoneColliderData.Add(new BarrierData
        {
            position = new SerializableVector3(Vector3.zero),
            rotation = new SerializableQuaternion(Quaternion.identity),
            scale = new SerializableVector3(Vector3.one),
            isOneWayBarrier = false
        });

        PopulateDeadzoneColliderDropdown();
        deadzoneColliderDropdown.value = worldSave.deadzoneColliders.Count - 1;
        // Setting the value in the edge case where the list is empty does not invoke the dropdown change method so we need to manually call it
        OnDeadzoneColliderDropdownChanged(worldSave.deadzoneColliders.Count - 1);
    }

    private void OnAddDeadzoneBarrierButtonClicked()
    {
        GameObject newDeadzoneBarrier = Instantiate(worldSave.deadzoneBarrierPrefab, worldSave.deadzoneBarrierObjects.transform.position, Quaternion.identity);
        newDeadzoneBarrier.transform.localScale = new SerializableVector3(Vector3.one);
        newDeadzoneBarrier.transform.SetParent(worldSave.deadzoneBarrierObjects.transform);

        worldSave.deadzoneBarriers.Add(newDeadzoneBarrier);
        worldSave.deadzoneBarrierData.Add(new BarrierData
        {
            position = new SerializableVector3(Vector3.zero),
            rotation = new SerializableQuaternion(Quaternion.identity),
            scale = new SerializableVector3(Vector3.one),
            isOneWayBarrier = false
        });

        PopulateDeadzoneBarrierDropdown();
        deadzoneBarrierDropdown.value = worldSave.deadzoneBarriers.Count - 1;
        // Setting the value in the edge case where the list is empty does not invoke the dropdown change method so we need to manually call it
        OnDeadzoneBarrierDropdownChanged(worldSave.deadzoneBarriers.Count - 1);
    }

    private void OnDeleteDeadzoneColliderButtonClicked()
    {
        int index = deadzoneColliderDropdown.value;
        if (index >= 0 && index < worldSave.deadzoneColliders.Count)
        {
            RemoveHighlight();
            Destroy(worldSave.deadzoneColliders[index]);
            worldSave.deadzoneColliders.RemoveAt(index);
            worldSave.deadzoneColliderData.RemoveAt(index);

            PopulateDeadzoneColliderDropdown();
            if (worldSave.deadzoneColliders.Count > 0)
            {
                deadzoneColliderDropdown.value = 0;
                currentlySelectedObject = worldSave.deadzoneColliders[deadzoneColliderDropdown.value];
                AddHighlight();
            }
            else
            {
                deadzoneColliderPositionInputField.text = string.Empty;
                deadzoneColliderRotationInputField.text = string.Empty;
                deadzoneColliderScaleInputField.text = string.Empty;
                deadzoneColliderIsOneWayBarrierToggle.isOn = false;
            }
        }
    }

    private void OnDeleteDeadzoneBarrierButtonClicked()
    {
        int index = deadzoneBarrierDropdown.value;
        if (index >= 0 && index < worldSave.deadzoneBarriers.Count)
        {
            RemoveHighlight();
            Destroy(worldSave.deadzoneBarriers[index]);
            worldSave.deadzoneBarriers.RemoveAt(index);
            worldSave.deadzoneBarrierData.RemoveAt(index);

            PopulateDeadzoneBarrierDropdown();
            if (worldSave.deadzoneBarriers.Count > 0)
            {
                deadzoneBarrierDropdown.value = 0;
                currentlySelectedObject = worldSave.deadzoneBarriers[deadzoneBarrierDropdown.value];
                AddHighlight();
            }
            else
            {
                deadzoneBarrierPositionInputField.text = string.Empty;
                deadzoneBarrierRotationInputField.text = string.Empty;
                deadzoneBarrierScaleInputField.text = string.Empty;
                deadzoneBarrierIsOneWayBarrierToggle.isOn = false;
            }
        }
    }

    private void MakeOneWayDeadzoneCollider(bool isOneWay)
    {
        int index = deadzoneColliderDropdown.value;
        if (worldSave.deadzoneColliders.Count > 0)
        {
            if (isOneWay)
            {
                worldSave.deadzoneColliders[index].transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = true;
                worldSave.deadzoneColliders[index].transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = true;
                worldSave.deadzoneColliderData[index].isOneWayBarrier = true;
            }
            else
            {
                worldSave.deadzoneColliders[index].transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = false;
                worldSave.deadzoneColliders[index].transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = false;
                worldSave.deadzoneColliderData[index].isOneWayBarrier = false;
            }
        }
    }

    private void MakeOneWayDeadzoneBarrier(bool isOneWay)
    {
        int index = deadzoneBarrierDropdown.value;
        if (worldSave.deadzoneBarriers.Count > 0)
        {
            if (isOneWay)
            {
                worldSave.deadzoneBarriers[index].transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = true;
                worldSave.deadzoneBarriers[index].transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = true;
                worldSave.deadzoneBarrierData[index].isOneWayBarrier = true;
            }
            else
            {
                worldSave.deadzoneBarriers[index].transform.Find("Entry Trigger").GetComponent<MeshRenderer>().enabled = false;
                worldSave.deadzoneBarriers[index].transform.Find("Exit Trigger").GetComponent<MeshRenderer>().enabled = false;
                worldSave.deadzoneBarrierData[index].isOneWayBarrier = false;
            }
        }
    }

    public void AddHighlight()
    {
        if (currentlySelectedObject)
        {
            BoxCollider boxDeadzoneCollider = currentlySelectedObject.GetComponent<BoxCollider>();
            uiBase.DrawBoxColliderOutline(boxDeadzoneCollider);
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
                deadzoneColliderUI.SetActive(true);
                deadzoneBarrierUI.SetActive(false);

                RemoveHighlight();
                if (worldSave.deadzoneColliders.Count != 0)
                {
                    currentlySelectedObject = worldSave.deadzoneColliders[deadzoneColliderDropdown.value];
                    AddHighlight();
                }

                break;
            case 1:
                deadzoneColliderUI.SetActive(false);
                deadzoneBarrierUI.SetActive(true);

                RemoveHighlight();
                if (worldSave.deadzoneBarriers.Count != 0)
                {
                    currentlySelectedObject = worldSave.deadzoneBarriers[deadzoneBarrierDropdown.value];
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
            var selectedDeadzoneColliderData = worldSave.deadzoneColliders[deadzoneColliderDropdown.value].transform;

            deadzoneColliderPositionInputField.text = uiBase.FormatVector3(selectedDeadzoneColliderData.position - new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset));
            deadzoneColliderRotationInputField.text = uiBase.FormatQuaternion(selectedDeadzoneColliderData.rotation);
            deadzoneColliderScaleInputField.text = uiBase.FormatVector3(selectedDeadzoneColliderData.localScale);
        }
        else if (selectedMenu == 1 && worldSave.barriers.Count > 0)
        {
            var selectedDeadzoneBarrierData = worldSave.deadzoneBarriers[deadzoneBarrierDropdown.value].transform;

            deadzoneBarrierPositionInputField.text = uiBase.FormatVector3(selectedDeadzoneBarrierData.position - new Vector3(VoxelData.WorldOffset, 0, VoxelData.WorldOffset));
            deadzoneBarrierRotationInputField.text = uiBase.FormatQuaternion(selectedDeadzoneBarrierData.rotation);
            deadzoneBarrierScaleInputField.text = uiBase.FormatVector3(selectedDeadzoneBarrierData.localScale);
        }
    }
}
