using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    public bool Dragging { get; private set; } = false;
    public bool FirstPersonDrag { get; set; } = false;

    [SerializeField] private MapPlayer mapPlayer;
    [SerializeField] private ObjectUI objectUI;
    [SerializeField] private LearningStageUI learningStageUI;
    [SerializeField] private RetracingStageUI retracingStageUI;

    private Transform selectedObject;
    private Vector3 offset;
    private float originalYCoordinate;
    private float verticalTranslateAmount = 0.1f;
    private float rotateAmount = 1f;
    private float scaleAmount = 0.1f;
    private float fastVerticalTranslateAmount = 1f;
    private float fastRotateAmount = 15;
    private float fastScaleAmount = 0.5f;

    public void Update()
    {
        if (mapPlayer.CurrentMode != MapPlayer.Mode.MapEdit)
        {
            UseBirdsEyeDragAndDrop();
        }
        else
        {
            UseFirstPersonDragAndDrop();
        }
    }

    public void UseBirdsEyeDragAndDrop()
    {
        if (Input.GetMouseButtonDown(0) && selectedObject == null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            PickUpObject(ray);
        }

        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            DragObject(ray);
        }

        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            DropObject();
        }
    }

    public void UseFirstPersonDragAndDrop()
    {
        if (FirstPersonDrag)
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (selectedObject == null) { PickUpObject(ray); }
            else { DragObject(ray); }
        }
        else if (selectedObject != null)
        {
            DropObject();
        }
    }

    private void PickUpObject(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("Draggable"))
            {
                selectedObject = hit.transform;
                BoxCollider collider = selectedObject.GetComponent<BoxCollider>();

                // Calculate center offset using BoxCollider's size and center
                Vector3 objectCenter = collider.bounds.center;
                offset = selectedObject.position - objectCenter;

                originalYCoordinate = selectedObject.position.y;
                Dragging = true;
                Debug.Log("Picked up object: " + selectedObject.name);
            }
            else
            {
                Debug.Log("No Draggable object hit");
                FirstPersonDrag = false;
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything");
            FirstPersonDrag = false;
        }
    }

    private void DragObject(Ray ray)
    {
        Plane plane = new Plane(Vector3.up, new Vector3(selectedObject.position.x, originalYCoordinate, selectedObject.position.z));
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 point = ray.GetPoint(distance) + offset;

            if (mapPlayer.CurrentMode != MapPlayer.Mode.MapEdit)
            {
                // Round the X and Z coordinates
                float roundedX = Mathf.Round(point.x * 1000f) / 1000f;
                float roundedZ = Mathf.Round(point.z * 1000f) / 1000f;

                selectedObject.position = new Vector3(roundedX, selectedObject.position.y, roundedZ);
            }
            else
            {
                Vector3 playerPositionXZ = new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
                Vector3 pointXZ = new Vector3(point.x, 0, point.z);
                float maxDistance = 2f;

                if (Vector3.Distance(playerPositionXZ, pointXZ) > maxDistance)
                {
                    pointXZ = playerPositionXZ + (pointXZ - playerPositionXZ).normalized * maxDistance;
                }

                selectedObject.position = new Vector3(pointXZ.x, selectedObject.position.y, pointXZ.z);
            }
        }
        else
        {
            Vector3 playerPositionXZ = new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
            Vector3 cameraForwardXZ = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            float maxDistance = 2f;

            Vector3 pointXZ = playerPositionXZ + cameraForwardXZ * maxDistance;

            selectedObject.position = new Vector3(pointXZ.x, selectedObject.position.y, pointXZ.z);
        }


        HandleScrollInput();
        UpdateUIs();
    }

    private void DropObject()
    {
        Debug.Log("Dropped object: " + selectedObject.name);
        selectedObject = null;
        Dragging = false;
    }

    private void HandleScrollInput()
    {
        // Positive is scrolling up, negative is scrolling down
        float scroll = Input.mouseScrollDelta.y;
        bool fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        float currentVerticalTranslateAmount = fastMode ? fastVerticalTranslateAmount : verticalTranslateAmount;
        float currentRotateAmount = fastMode ? fastRotateAmount : rotateAmount;
        float currentScaleAmount = fastMode ? fastScaleAmount : scaleAmount;

        if (scroll != 0)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                selectedObject.Rotate(Vector3.up, scroll > 0 ? currentRotateAmount : -currentRotateAmount);
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                Vector3 newScale = selectedObject.localScale + (scroll > 0 ? Vector3.one * currentScaleAmount : -Vector3.one * currentScaleAmount);
                selectedObject.localScale = new Vector3(
                    Mathf.Max(0.1f, newScale.x),
                    Mathf.Max(0.1f, newScale.y),
                    Mathf.Max(0.1f, newScale.z)
                );
            }
            else
            {
                float newY = (scroll > 0) ? selectedObject.position.y + currentVerticalTranslateAmount : selectedObject.position.y - currentVerticalTranslateAmount;
                selectedObject.position = new Vector3(selectedObject.position.x, newY, selectedObject.position.z); 
            }
        }
    }

    private void UpdateUIs()
    {
        objectUI.UpdatePrimaryTransformUI();
        learningStageUI.UpdatePrimaryTransformUI();
        retracingStageUI.UpdatePrimaryTransformUI();
    }
}
