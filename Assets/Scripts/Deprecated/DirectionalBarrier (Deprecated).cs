using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalBarrier : MonoBehaviour
{
    public Vector3 allowedDirection = Vector3.forward; // The direction from which entry is allowed
    private Collider barrierCollider;

    void Start()
    {
        barrierCollider = GetComponent<Collider>();
    }

    void Update()
    {
        // Assuming the player is the main camera, for example
        Vector3 playerDirection = (Camera.main.transform.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(playerDirection, allowedDirection.normalized);

        // Enable the collider if the player is approaching from the blocked direction
        if (dotProduct < 0)
        {
            barrierCollider.enabled = true;
        }
        else
        {
            barrierCollider.enabled = false;
        }
    }
}
