// Description: This script controls the player's mouse control functionality
// Can be adjusted in the training stage with experimental mode on
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField] private float mouseSensitivity;

    private float xRotation = 0f;

    public float MouseSensitivity
    {
        get { return mouseSensitivity; }
        set { mouseSensitivity = value; }
    }

    // Convention is to initialize this in Start(), but other methods rely on this being initialized
    void Awake()
    {
        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mouseSensitivity = PersistentDataManager.Instance.MouseSensitivity;
    }

    void Update()
    {
        // Time.deltaTime allows rotation speed to be independent of framerate
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // We seperate Y Axis rotation from the X Axis so we can "clamp" it
        // I.e prevent the player from turning their head upside down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = (!PersistentDataManager.Instance.LockVerticalLook) ? Quaternion.Euler(xRotation, 0f, 0f) : Quaternion.Euler(PersistentDataManager.Instance.XRotationFix, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }

    // Used during teleports to set the camera rotation properly
    public void SetXRotation(float angle)
    {
        xRotation = angle;
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    // Training stage experimental mode allows you to modify sensitivity mid scene
    public void AdjustSensitivity(bool increase)
    {
        mouseSensitivity += increase ? 0.1f : -0.1f;
        PersistentDataManager.Instance.MouseSensitivity = mouseSensitivity;
        mouseSensitivity = Mathf.Clamp(mouseSensitivity, 0.1f, 10f);
    }
}
