using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbitTest : MonoBehaviour
{
    void Start()
    {
        // Rotation angle in degrees
        float angle = -45f;

        // Find the camera object
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        if (camera == null)
        {
            Debug.LogError("MainCamera not found. Please tag your camera as 'MainCamera'.");
            return;
        }

        Vector3 cameraPosition = camera.transform.position;
        Vector3 cameraForward = camera.transform.forward;
        Vector3 pivotPoint;

        // Calculate the focal point
        float distance = cameraPosition.y / -cameraForward.y;
        pivotPoint = cameraPosition + cameraForward * distance;

        Debug.Log("Focal Point: " + pivotPoint);

        // Convert angle to radians
        float angleRad = Mathf.Deg2Rad * angle;

        // Calculate the new position relative to the pivot point
        float x_relative = cameraPosition.x - pivotPoint.x;
        float z_relative = cameraPosition.z - pivotPoint.z;
        Debug.Log("Camera Position: " + cameraPosition);
        Debug.Log("Camera Forward: " + cameraForward);
        Debug.Log("Distance: " + distance);
        Debug.Log("Relative Position: " + new Vector3(x_relative, 0, z_relative));

        float x_new_relative = x_relative * Mathf.Cos(angleRad) - z_relative * Mathf.Sin(angleRad);
        float z_new_relative = x_relative * Mathf.Sin(angleRad) + z_relative * Mathf.Cos(angleRad);
        Debug.Log("New Relative Position: " + new Vector3(x_new_relative, 0, z_new_relative));

        // Calculate the final position
        float x_new = pivotPoint.x + x_new_relative;
        float z_new = pivotPoint.z + z_new_relative;

        Vector3 newCameraPosition = new Vector3(x_new, camera.transform.position.y, z_new);
        camera.transform.position = newCameraPosition;

        Debug.Log("New Camera Position: " + newCameraPosition);
    }
}
