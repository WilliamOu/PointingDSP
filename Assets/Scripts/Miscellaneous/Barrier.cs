// Description: General use case barrier script
// Used in conjuction with the OneWayExitTrigger and OneWayEntryTrigger scripts (not the deprecated versions)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    [SerializeField] BoxCollider boxCollider; 
    private bool activatedBuffer = true;
    private bool isInBarrier = false;

    public void Activate()
    {
        activatedBuffer = true;
        if (!isInBarrier)
        {
            boxCollider.isTrigger = false;
        }
    }

    public void Deactivate()
    {
        activatedBuffer = false;
        boxCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.gameObject.CompareTag("Player"))
        {
            isInBarrier = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.gameObject.CompareTag("Player"))
        {
            // If the barrier is activated, the trigger is off since that makes the collider solid
            boxCollider.isTrigger = !activatedBuffer;
            isInBarrier = false;
        }
    }
}
