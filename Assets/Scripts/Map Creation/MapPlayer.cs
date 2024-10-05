// Description: Controls the player that can view and edit maps
// The three modes are map edit view, which is controlled like Minecraft Creative, isometric view which is controlled like XCOM 2, and top-down view
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MapPlayer : MonoBehaviour
{
    public enum Mode { Isometric, TopDown, MapEdit }
    public Mode CurrentMode = Mode.Isometric;
    public Mode LastBirdsEyeViewMode = Mode.Isometric;
    public bool IsZoomed;

    [SerializeField] private BirdsEyeCameraUI uiBase;
    [SerializeField] private GameObject mapEditUI;
    [SerializeField] private GameObject birdsEyeUI;
    [SerializeField] private WorldSave worldSave;
    [SerializeField] private Button backButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button qButton;
    [SerializeField] private Button rButton;
    [SerializeField] private Button eButton;
    [SerializeField] private Button centerCameraButton;
    [SerializeField] private Button editMapButton;
    [SerializeField] private Button showMenuButton;
    [SerializeField] private GameObject leftMenu;
    [SerializeField] private GameObject rightMenu;
    [SerializeField] private DragAndDrop dragAndDrop;

    [SerializeField] private CharacterController controller;
    
    [SerializeField] private float forceOfGravity = /* -9.81f */ -19.62f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float shiftSpeed = 25f;
    [SerializeField] private float speed = 9f;
    [SerializeField] private float shiftCameraPanSpeed = 16f;
    [SerializeField] private float cameraPanSpeed = 6f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 400f;
    [SerializeField] private float cameraAngle = 45f;

    [SerializeField] private GameObject highlightBlock;
    [SerializeField] private GameObject placeBlock;

    private Vector3 velocity;
    private float verticalVelocity = 0f;
    private float firstPersonCameraOffset = 0.65f;
    private Camera mapCamera;
    private bool isFlying = false;
    private bool isMenuOpen = false;
    private Vector3 birdEyeViewPosition;
    private Quaternion birdEyeViewRotation;
    private Quaternion trueBirdEyeViewRotation;

    private Coroutine screenLerpCoroutine;
    private Coroutine rotateCoroutine;

    private Vector3 lastPlayerPosition;
    private Quaternion lastPlayerRotation;
    private Quaternion lastCameraRotation;

    private float zoomedFOV = 15f;
    private float defaultFOV = 60f;
    private float hasJumped = 0f;

    private Vector3 leftMenuOnScreenPosition;
    private Vector3 rightMenuOnScreenPosition;
    private Vector3 leftMenuOffScreenPosition;
    private Vector3 rightMenuOffScreenPosition;

    void Start()
    {
        backButton.onClick.AddListener(ReturnToTitleScreen);
        saveButton.onClick.AddListener(SaveMap);

        qButton.onClick.AddListener(() => RotateCamera(45f, 0.2f));
        rButton.onClick.AddListener(SwitchBirdsEyeViewModes);
        eButton.onClick.AddListener(() => RotateCamera(-45f, 0.2f));
        centerCameraButton.onClick.AddListener(() => CenterCamera());
        editMapButton.onClick.AddListener(SwitchToMapEditView);
        showMenuButton.onClick.AddListener(ToggleMenu);

        mapCamera = Camera.main;
        birdEyeViewPosition = new Vector3(160f, 20f, 140f);
        birdEyeViewRotation = Quaternion.Euler(cameraAngle, 0f, 0f);
        trueBirdEyeViewRotation = Quaternion.Euler(90f, 0f, 0f);
        transform.position = birdEyeViewPosition;
        mapCamera.transform.rotation = birdEyeViewRotation;

        mapEditUI.SetActive(false);
        birdsEyeUI.SetActive(true);

        leftMenuOnScreenPosition = leftMenu.transform.position;
        rightMenuOnScreenPosition = rightMenu.transform.position;

        leftMenu.transform.Translate(new Vector3(-1000f, 0f, 0f));
        rightMenu.transform.Translate(new Vector3(1000f, 0f, 0f));

        leftMenuOffScreenPosition = leftMenu.transform.position;
        rightMenuOffScreenPosition = rightMenu.transform.position;
    }

    void Update()
    {
        if (hasJumped > 0f) { hasJumped -= Time.deltaTime; }

        float xDirection = (CurrentMode == Mode.MapEdit || !isMenuOpen) ? Input.GetAxis("Horizontal") : 0f;
        float zDirection = (CurrentMode == Mode.MapEdit || !isMenuOpen) ? Input.GetAxis("Vertical") : 0f;

        if (xDirection == 0f && zDirection == 0f && CurrentMode != Mode.MapEdit)
        {
            // Check if the mouse is near the edges of the screen
            float mouseX = Input.mousePosition.x;
            float mouseY = Input.mousePosition.y;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (mouseX <= 0f)
            {
                xDirection = -0.5f;
            }
            else if (mouseX >= screenWidth - 1f)
            {
                xDirection = 0.5f;
            }

            if (mouseY <= 0f)
            {
                zDirection = -0.5f;
            }
            else if (mouseY >= screenHeight - 1f)
            {
                zDirection = 0.5f;
            }
        }

        Vector3 move = transform.right * xDirection + transform.forward * zDirection;

        if (Input.GetKey(KeyCode.LeftShift)) { move *= shiftSpeed; }
        else { move *= speed; }

        if (!dragAndDrop.Dragging)
        {
            if (CurrentMode != Mode.MapEdit) { BirdsEyeViewSpecificMovement(); move *= 4f; }
            else { MapEditViewSpecificMovement(); }
        }
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        SwitchBetweenEditAndBirdsEyeViewModes();
        CheckForOtherBirdsEyeViewInputs();
        CheckForOtherMapEditViewInputs();
        Zoom();
    }

    private void Zoom()
    {
        if (Input.GetKey(KeyCode.F) && !isMenuOpen)
        {
            IsZoomed = true;
            mapCamera.fieldOfView = zoomedFOV;
        }
        else
        {
            IsZoomed = false;
            mapCamera.fieldOfView = defaultFOV;
        }
    }

    private void CenterCamera()
    {
        transform.position = birdEyeViewPosition;
    }

    private void SwitchBetweenEditAndBirdsEyeViewModes()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.B))
        {
            if (CurrentMode == Mode.MapEdit)
            {
                SwitchToBirdsEyeView();
            }
            else if (!isMenuOpen)
            {
                SwitchToMapEditView();
            }
        }
    }

    private void CheckForOtherBirdsEyeViewInputs()
    {
        if ((CurrentMode == Mode.Isometric || CurrentMode == Mode.TopDown))
        {
            if (!isMenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    RotateCamera(45f, 0.2f);
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    RotateCamera(-45f, 0.2f);
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    SwitchBirdsEyeViewModes();
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    CenterCamera();
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                ToggleMenu();
            }
        }
    }

    private void CheckForOtherMapEditViewInputs()
    {
        if (CurrentMode == Mode.MapEdit)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                dragAndDrop.FirstPersonDrag = !dragAndDrop.FirstPersonDrag;
            }
        }
    }

    private void MapEditViewSpecificMovement()
    {
        if (!isFlying)
        {
            ApplyGravity();

            if (Input.GetKey(KeyCode.Space))
            {
                if (controller.isGrounded) { verticalVelocity = jumpForce; }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (hasJumped > 0f)
                {
                    isFlying = true;
                    hasJumped = 0f;
                    verticalVelocity = 0;
                }
                else { hasJumped = 0.25f; }
            }

            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            if (Input.GetKey(KeyCode.Space)) { verticalVelocity = speed; }
            else if (Input.GetKey(KeyCode.LeftControl)) { verticalVelocity = -speed; } 
            else { verticalVelocity = 0; }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (hasJumped > 0f)
                {
                    isFlying = false;
                    hasJumped = 0f;
                    verticalVelocity = 0;
                }
                else { hasJumped = 0.25f; }
            }

            controller.Move(velocity * Time.deltaTime);
        }
    }

    private void BirdsEyeViewSpecificMovement()
    {
        controller.enabled = false;

        // Zoom in and out with scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        float currentZoomSpeed = Input.GetKey(KeyCode.LeftShift) ? zoomSpeed * shiftCameraPanSpeed : zoomSpeed * cameraPanSpeed;

        if (CurrentMode == Mode.TopDown)
        {
            // In top-down mode, scroll up and down
            float newCameraHeight = Mathf.Clamp(transform.position.y - scrollInput * currentZoomSpeed, minZoom, maxZoom);
            transform.position = new Vector3(transform.position.x, newCameraHeight, transform.position.z);
        }
        else
        {
            // In isometric view, scroll towards or away from the focal point
            Vector3 cameraPosition = mapCamera.transform.position;
            Vector3 cameraForward = mapCamera.transform.forward;

            // Calculate the focal point
            float distance = cameraForward.y == 0 ? 0.0001f : cameraPosition.y / -cameraForward.y;
            Vector3 focalPoint = cameraPosition + cameraForward * distance;

            // Calculate the new camera position based on the scroll input
            Vector3 direction = (cameraPosition - focalPoint).normalized;
            float newDistance = Mathf.Clamp(distance - scrollInput * currentZoomSpeed, minZoom, maxZoom);
            Vector3 newCameraPosition = focalPoint + direction * newDistance;

            // Update the camera position
            transform.position = newCameraPosition;
        }

        controller.enabled = true;
    }

    private IEnumerator LerpBetweenBirdsEyeViews(float duration, float endRotationX)
    {
        float startRotationX = mapCamera.transform.rotation.eulerAngles.x;
        float time = 0;

        while (time < duration)
        {
            float newRotationX = Mathf.Lerp(startRotationX, endRotationX, time / duration);
            Vector3 newRotation = new Vector3(newRotationX, mapCamera.transform.rotation.eulerAngles.y, mapCamera.transform.rotation.eulerAngles.z);
            mapCamera.transform.rotation = Quaternion.Euler(newRotation);
            time += Time.deltaTime;
            yield return null;
        }

        Vector3 finalRotation = new Vector3(endRotationX, mapCamera.transform.rotation.eulerAngles.y, mapCamera.transform.rotation.eulerAngles.z);
        mapCamera.transform.rotation = Quaternion.Euler(finalRotation);
    }

    // Switches between the two bird's eye view modes
    private void SwitchBirdsEyeViewModes()
    {
        if (CurrentMode == Mode.Isometric)
        {
            LastBirdsEyeViewMode = CurrentMode;
            CurrentMode = Mode.TopDown;
            StartCoroutine(LerpBetweenBirdsEyeViews(0.2f, 90f));
        }
        else if (CurrentMode == Mode.TopDown)
        {
            LastBirdsEyeViewMode = CurrentMode;
            CurrentMode = Mode.Isometric;
            StartCoroutine(LerpBetweenBirdsEyeViews(0.2f, 45f));
        }
    }

    // Switches from the map edit view (first person camera) to bird's eye view
    private void SwitchToBirdsEyeView()
    {
        highlightBlock.SetActive(false);
        placeBlock.SetActive(false);
        mapEditUI.SetActive(false);
        birdsEyeUI.SetActive(true);
        dragAndDrop.FirstPersonDrag = false;
        verticalVelocity = 0;
        CurrentMode = LastBirdsEyeViewMode;

        controller.enabled = false;
        mapCamera.transform.position = new Vector3(mapCamera.transform.position.x, mapCamera.transform.position.y - firstPersonCameraOffset, mapCamera.transform.position.z);
        transform.position = new Vector3(transform.position.x, transform.position.y + firstPersonCameraOffset, transform.position.z);
        controller.enabled = true;

        StartCoroutine(LerpToLastBirdEyeView(0.2f));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Switches to the map edit view from birds' eye views
    private void SwitchToMapEditView()
    {
        mapEditUI.SetActive(true);
        birdsEyeUI.SetActive(false);
        verticalVelocity = 0;
        LastBirdsEyeViewMode = CurrentMode;
        CurrentMode = Mode.MapEdit;

        lastPlayerPosition = transform.position;
        lastPlayerRotation = transform.rotation;
        lastCameraRotation = mapCamera.transform.rotation;

        controller.enabled = false;
        mapCamera.transform.position = new Vector3(mapCamera.transform.position.x, mapCamera.transform.position.y + firstPersonCameraOffset, mapCamera.transform.position.z);
        transform.position = new Vector3(transform.position.x, transform.position.y - firstPersonCameraOffset, transform.position.z);
        controller.enabled = true;

        isFlying = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator LerpToLastBirdEyeView(float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion startCameraRotation = mapCamera.transform.rotation;
        float time = 0;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, lastPlayerPosition, time / duration);
            transform.rotation = Quaternion.Lerp(startRotation, lastPlayerRotation, time / duration);
            mapCamera.transform.rotation = Quaternion.Lerp(startCameraRotation, lastCameraRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = lastPlayerPosition;
        transform.rotation = lastPlayerRotation;
        mapCamera.transform.rotation = lastCameraRotation;
    }

    private void RotateCamera(float angle, float duration)
    {
        // Stop the previous coroutine if it's running
        // This prevents strange behavior when stacking slerps
        if (rotateCoroutine != null) { StopCoroutine(rotateCoroutine); }

        // Calculate the target rotation angle
        float currentAngle = transform.rotation.eulerAngles.y;
        float targetAngle = currentAngle + angle;

        // Round the target angle to the closest 45-degree angle
        float roundedAngle = Mathf.Round(targetAngle / 45f) * 45f;

        // Calculate the actual rotation angle needed to reach the rounded target angle
        float actualAngle = roundedAngle - currentAngle;

        // Start a new coroutine for the rotation
        rotateCoroutine = StartCoroutine(RotateAroundPoint(actualAngle, duration));
    }

    private IEnumerator RotateAroundPoint(float angle, float duration)
    {
        controller.enabled = false;

        Vector3 cameraPosition = mapCamera.transform.position;
        Vector3 cameraForward = mapCamera.transform.forward;
        Vector3 pivotPoint;

        float distance = cameraPosition.y / -cameraForward.y;
        pivotPoint = cameraPosition + cameraForward * distance;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up) * startRotation;

        Vector3 initialOffset = cameraPosition - pivotPoint;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float interpolatedAngle = Mathf.Lerp(0f, angle, t);

            // Rotate the initial offset by the interpolated angle
            Vector3 rotatedOffset = Quaternion.AngleAxis(interpolatedAngle, Vector3.up) * initialOffset;

            // Calculate the new camera position based on the rotated offset
            Vector3 newCameraPosition = pivotPoint + rotatedOffset;

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            transform.position = newCameraPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        transform.position = pivotPoint + Quaternion.AngleAxis(angle, Vector3.up) * initialOffset;

        controller.enabled = true;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += forceOfGravity * Time.deltaTime;
    }

    private void ToggleMenu()
    {
        if (screenLerpCoroutine != null)
        {
            StopCoroutine(screenLerpCoroutine);
        }

        if (isMenuOpen) { uiBase.HideMenus(); }
        else { uiBase.ReloadMenus(); }
        screenLerpCoroutine = StartCoroutine(SmoothLerpMenus());
        isMenuOpen = !isMenuOpen;
    }

    private IEnumerator SmoothLerpMenus()
    {
        Vector3 leftStartPos = leftMenu.transform.position;
        Vector3 rightStartPos = rightMenu.transform.position;

        Vector3 leftEndPos = isMenuOpen ? leftMenuOffScreenPosition : leftMenuOnScreenPosition;
        Vector3 rightEndPos = isMenuOpen ? rightMenuOffScreenPosition : rightMenuOnScreenPosition;

        float duration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);
            leftMenu.transform.position = Vector3.Lerp(leftStartPos, leftEndPos, t);
            rightMenu.transform.position = Vector3.Lerp(rightStartPos, rightEndPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        leftMenu.transform.position = leftEndPos;
        rightMenu.transform.position = rightEndPos;
    }

    private void SaveMap()
    {
        worldSave.SaveWorldData();
        worldSave.SaveBlockData();
        worldSave.SaveMetaData();
        worldSave.SaveCSVData();
        worldSave.SaveLearningStageData();
        worldSave.SaveRetracingStageData();
        worldSave.SaveModelData();
        ReturnToTitleScreen();
    }

    private void ReturnToTitleScreen()
    {
        if (PersistentDataManager.Instance != null)
        {
            Destroy(PersistentDataManager.Instance.gameObject);
        }

        SceneManager.LoadScene("Title Screen");
    }
}
