using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BirdsEyeCameraUI : MonoBehaviour
{
    public int CurrentMenu { get; private set; }
    public GameObject spawnPointVisualizer;

    [SerializeField] private MeshRenderer spawnPointAngleArrow;

    [SerializeField] private TMP_Dropdown settingsDropdown;
    [SerializeField] private TMP_Text submenuTitle;
    [SerializeField] private WorldSave worldSave;
    [SerializeField] private AtlasPacker atlasPacker;
    [SerializeField] private GameObject farPlane;

    [SerializeField] private GameObject mapSettingsMenu;
    [SerializeField] private GameObject trialsMenu;
    [SerializeField] private GameObject objectsMenu;
    [SerializeField] private GameObject blocksMenu;
    [SerializeField] private GameObject learningStageMenu;
    [SerializeField] private GameObject retracingStageMenu;
    [SerializeField] private GameObject learningStageObjects;
    [SerializeField] private GameObject retracingStageObjects;

    [SerializeField] private GameObject colliderVisualizer;
    [SerializeField] private Shader colliderShader;

    // Map Settings Menu
    [SerializeField] private TMP_InputField mouseSensitivityInputField;
    [SerializeField] private MapPlayerLook mapPlayerLook;

    [SerializeField] private TMP_InputField spawnPointInputField;
    [SerializeField] private TMP_InputField spawnRotationInputField;

    [SerializeField] private Button stichAtlas;
    [SerializeField] private Toggle useFarPlane;

    private Regex vector3Regex = new Regex(@"\(([-+]?[0-9]*\.?[0-9]+),\s*([-+]?[0-9]*\.?[0-9]+),\s*([-+]?[0-9]*\.?[0-9]+)\)");
    private List<LineRenderer> lines = new List<LineRenderer>();
    private BoxCollider visualizerHitbox;
    private bool menusAreVoidedDueToNoObjects = false;

    private void Start()
    {
        settingsDropdown.onValueChanged.AddListener(LoadMenu);
        mouseSensitivityInputField.onValueChanged.AddListener(OnMouseSensitivityChanged);
        stichAtlas.onClick.AddListener(() => atlasPacker.StitchAtlas());
        useFarPlane.onValueChanged.AddListener(OnUseFarPlaneChanged);
        spawnPointInputField.onValueChanged.AddListener(OnSpawnPointChanged);
        spawnRotationInputField.onValueChanged.AddListener(OnSpawnRotationChanged);

        LoadMenu(0);
        LoadInitialMapSettingsIntoUI();
        LoadMetaDataIntoUI();

        // visualizerHitbox = colliderVisualizer.GetComponent<BoxCollider>();

        learningStageObjects.SetActive(false);
        retracingStageObjects.SetActive(false);
    }

    public void HideMenus()
    {
        mapSettingsMenu.SetActive(false);
        trialsMenu.SetActive(false);
        objectsMenu.SetActive(false);
        blocksMenu.SetActive(false);
        learningStageMenu.SetActive(false);
        retracingStageMenu.SetActive(false);
    }

    public void ReloadMenus()
    {
        LoadMenu(settingsDropdown.value);
    }

    private void LoadMenu(int index)
    {
        string selectedOption = settingsDropdown.options[index].text;
        submenuTitle.text = selectedOption;
        CurrentMenu = index;

        HideMenus();

        // Scene specific adjustments
        spawnPointAngleArrow.enabled = false;
        spawnPointVisualizer.SetActive(false);
        learningStageObjects.SetActive(false);
        retracingStageObjects.SetActive(false);

        // Show the selected menu
        switch (index)
        {
            case 0:
                objectsMenu.GetComponent<ObjectUI>().RemoveHighlight();
                mapSettingsMenu.SetActive(true);
                spawnPointVisualizer.SetActive(true);
                spawnPointAngleArrow.enabled = true;
                OnSpawnPointChanged(FormatVector3(worldSave.metaData.spawnPoint));
                break;
            case 1:
                objectsMenu.GetComponent<ObjectUI>().RemoveHighlight();
                if (menusAreVoidedDueToNoObjects) { break; }
                trialsMenu.SetActive(true);
                trialsMenu.GetComponent<TrialsUI>().SetSpawnPointVisualizer();
                break;
            case 2:
                if (menusAreVoidedDueToNoObjects) { break; }
                objectsMenu.SetActive(true);
                objectsMenu.GetComponent<ObjectUI>().AddHighlight();
                break;
            case 3:
                objectsMenu.GetComponent<ObjectUI>().RemoveHighlight();
                blocksMenu.SetActive(true);
                break;
            case 4:
                objectsMenu.GetComponent<ObjectUI>().RemoveHighlight();
                learningStageMenu.SetActive(true);
                learningStageObjects.SetActive(true);
                learningStageMenu.GetComponent<LearningStageUI>().AddHighlight();
                break;
            case 5:
                objectsMenu.GetComponent<ObjectUI>().RemoveHighlight();
                retracingStageMenu.SetActive(true);
                retracingStageObjects.SetActive(true);
                retracingStageMenu.GetComponent<RetracingStageUI>().AddHighlight();
                break;
        }
    }

    public void VoidObjectAndTrialUIs()
    {
        menusAreVoidedDueToNoObjects = true;
        trialsMenu.SetActive(false);
        objectsMenu.SetActive(false);
        settingsDropdown.options[1].text = "Trials [VOID]";
        settingsDropdown.options[2].text = "Objects [VOID]";
    }

    private void OnMouseSensitivityChanged(string value)
    {
        if (float.TryParse(value, out float sensitivity))
        {
            mapPlayerLook.mouseSensitivity = sensitivity;
        }
        else
        {
            mapPlayerLook.mouseSensitivity = 3f;
            mouseSensitivityInputField.text = "3";
        }
    }

    private void OnUseFarPlaneChanged(bool isOn)
    {
        worldSave.metaData.useFarPlane = isOn;
        farPlane.SetActive(isOn);
    }

    private void OnSpawnPointChanged(string value)
    {
        if (TryParseVector3(value, out Vector3 result))
        {
            worldSave.metaData.spawnPoint = result;
            spawnPointVisualizer.transform.position = result + new Vector3(VoxelData.WorldOffset, 0f, VoxelData.WorldOffset);
        }
    }

    private void OnSpawnRotationChanged(string value)
    {
        if (TryParseVector3(value, out Vector3 result))
        {
            worldSave.metaData.spawnRotation = Quaternion.Euler(result);
            spawnPointVisualizer.transform.rotation = Quaternion.Euler(result);
        }
    }

    private void LoadInitialMapSettingsIntoUI()
    {
        mouseSensitivityInputField.text = mapPlayerLook.mouseSensitivity.ToString();
    }

    private void LoadMetaDataIntoUI()
    {
        useFarPlane.isOn = worldSave.metaData.useFarPlane;
        farPlane.SetActive(worldSave.metaData.useFarPlane);
        spawnPointInputField.text = FormatVector3(worldSave.metaData.spawnPoint);
        spawnRotationInputField.text = FormatQuaternion(worldSave.metaData.spawnRotation);
        spawnPointVisualizer.transform.position = worldSave.metaData.spawnPoint + new Vector3(VoxelData.WorldOffset, 0f, VoxelData.WorldOffset);
    }

    public bool TryParseVector3(string input, out Vector3 result)
    {
        var match = vector3Regex.Match(input);
        if (match.Success)
        {
            result = new Vector3(
                float.Parse(match.Groups[1].Value),
                float.Parse(match.Groups[2].Value),
                float.Parse(match.Groups[3].Value)
            );
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    public void DrawBoxColliderOutline(BoxCollider boxCollider)
    {
        Material colliderMaterial = new Material(colliderShader);
        Color color = Color.green;
        colliderMaterial.color = color;
        float width = 0.03f;

        Vector3 size = boxCollider.size;
        Vector3 center = boxCollider.center;

        Vector3 worldCenter = boxCollider.transform.TransformPoint(center);
        Vector3 halfExtents = Vector3.Scale(size / 2f, boxCollider.transform.lossyScale);

        ClearBoxColliderOutline();

        // Get the corners of the box in world space
        Vector3[] corners = new Vector3[8];
        corners[0] = worldCenter + boxCollider.transform.TransformDirection(new Vector3(halfExtents.x, halfExtents.y, halfExtents.z));
        corners[1] = worldCenter + boxCollider.transform.TransformDirection(new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z));
        corners[2] = worldCenter + boxCollider.transform.TransformDirection(new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z));
        corners[3] = worldCenter + boxCollider.transform.TransformDirection(new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z));
        corners[4] = worldCenter + boxCollider.transform.TransformDirection(new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z));
        corners[5] = worldCenter + boxCollider.transform.TransformDirection(new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z));
        corners[6] = worldCenter + boxCollider.transform.TransformDirection(new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z));
        corners[7] = worldCenter + boxCollider.transform.TransformDirection(new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z));

        // Draw the edges of the box
        DrawLine(corners[0], corners[1], color, colliderMaterial, width);
        DrawLine(corners[0], corners[2], color, colliderMaterial, width);
        DrawLine(corners[0], corners[4], color, colliderMaterial, width);
        DrawLine(corners[1], corners[3], color, colliderMaterial, width);
        DrawLine(corners[1], corners[5], color, colliderMaterial, width);
        DrawLine(corners[2], corners[3], color, colliderMaterial, width);
        DrawLine(corners[2], corners[6], color, colliderMaterial, width);
        DrawLine(corners[3], corners[7], color, colliderMaterial, width);
        DrawLine(corners[4], corners[5], color, colliderMaterial, width);
        DrawLine(corners[4], corners[6], color, colliderMaterial, width);
        DrawLine(corners[5], corners[7], color, colliderMaterial, width);
        DrawLine(corners[6], corners[7], color, colliderMaterial, width);
    }

    public void ClearBoxColliderOutline()
    {
        foreach (var line in lines)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        lines.Clear();
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color, Material colliderMaterial, float width = 0.01f)
    {
        LineRenderer line = new GameObject("Line_" + start.ToString() + "_" + end.ToString()).AddComponent<LineRenderer>();
        line.material = colliderMaterial;
        line.startColor = color;
        line.endColor = color;
        line.startWidth = width;
        line.endWidth = width;
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.transform.SetParent(transform);
        lines.Add(line);
    }

    private void SetLinesColor(Color color)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].material.color = color;
            lines[i].startColor = color;
            lines[i].endColor = color;
        }
    }

    /*private void MatchVisualizer(BoxCollider boxCollider)
    {
        colliderVisualizer.transform.SetParent(boxCollider.transform.parent);

        // Match the transform properties of the box collider
        colliderVisualizer.transform.localPosition = boxCollider.transform.localPosition;
        colliderVisualizer.transform.localRotation = boxCollider.transform.localRotation;
        colliderVisualizer.transform.localScale = boxCollider.transform.localScale;

        // Set the local position to match the collider's offset
        visualizerHitbox.center = boxCollider.center;
        visualizerHitbox.size = boxCollider.size;
    }*/

    public string FormatVector3(Vector3 vector)
    {
        return $"({vector.x:G}, {vector.y:G}, {vector.z:G})";
    }

    public string FormatQuaternion(Quaternion quaternion)
    {
        Vector3 euler = quaternion.eulerAngles;
        return $"({euler.x:G}, {euler.y:G}, {euler.z:G})";
    }
}